// Chromium-derived hover-card layout and image transitions. See TabHoverCards.md.
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Win32Interop.Enums;
using Win32Interop.Methods;
using Win32Interop.Structs;

namespace EasyTabs
{
    internal sealed class TabHoverCard : Form
    {
        internal const int CardWidth = 256, PreviewHeight = 144, ShadowMargin = 12;
        private readonly LayeredWindowBuffer _surface = new LayeredWindowBuffer();
        private TabFrameScheduler _frames;
        private Bitmap _header, _oldHeader, _thumbnail, _oldThumbnail, _footer, _oldFooter;
        private bool _showPreview;
        private double _imageStart;
        private float _scale = 1;
        private Color _background, _foreground;
        internal event EventHandler Frame;
        internal int HeaderHeight => _header?.Height ?? 0;
        internal bool HasImageAnimation => _oldThumbnail != null;
        internal int FooterHeight => _footer?.Height ?? 0;
        internal Size CardSize => new Size(Pixel(CardWidth), HeaderHeight + (_showPreview ? Pixel(PreviewHeight) : 0) + FooterHeight);

        internal TabHoverCard()
        {
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            AccessibleRole = AccessibleRole.None;
        }

        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                // Nonactivating, click-through, per-pixel alpha popup. Never steals
                // a tab click, keyboard focus, or another application's foreground.
                parameters.ExStyle |= 0x80000 | 0x08000000 | 0x80 | 0x20;
                return parameters;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _frames = new TabFrameScheduler(Handle);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _frames?.Dispose(); _frames = null;
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == TabFrameScheduler.Message)
            {
                _frames?.Acknowledge();
                Frame?.Invoke(this, EventArgs.Empty);
                return;
            }
            if (message.Msg == 0x21) { message.Result = new IntPtr(3); return; } // MA_NOACTIVATE
            if (message.Msg == 0x84) { message.Result = new IntPtr(-1); return; } // HTTRANSPARENT
            base.WndProc(ref message);
        }

        internal void SetAnimating(bool enabled)
        {
            if (_frames != null) _frames.Enabled = enabled;
        }

        internal void SetContent(string title, string domain, Image preview, bool showPreview, bool crashed,
            ChromiumTabTheme theme, float scale, bool transition, double now, double textProgress = 1, string footer = null)
        {
            if (_scale != scale) { transition = false; ReleaseImages(); }
            _scale = scale;
            _background = SystemInformation.HighContrast ? SystemColors.Info : theme.ActiveTab;
            _foreground = SystemInformation.HighContrast ? SystemColors.InfoText : theme.ActiveForeground;
            // Capture the actual blend, not the previous destination. During a
            // fast A->B->C reversal B may barely have been visible at all.
            ReplaceOutgoing(ref _header, ref _oldHeader, transition, textProgress);
            ReplaceOutgoing(ref _footer, ref _oldFooter, transition, textProgress);
            _header = MakeHeader(title, domain);
            _footer = string.IsNullOrEmpty(footer) ? null : MakeFooter(footer);
            _showPreview = showPreview;
            SetThumbnail(showPreview ? MakeThumbnail(preview, crashed) : null, transition, now);
        }

        internal void UpdateThumbnail(Image preview, bool crashed, bool animate, double now)
        {
            if (_showPreview) SetThumbnail(MakeThumbnail(preview, crashed), animate, now);
        }

        internal void UpdateFooter(string text)
        {
            _footer?.Dispose();
            _footer = string.IsNullOrEmpty(text) ? null : MakeFooter(text);
        }

        private void ReplaceOutgoing(ref Bitmap current, ref Bitmap outgoing, bool transition, double progress)
        {
            Bitmap snapshot = null;
            if (transition && (current != null || outgoing != null))
            {
                int height = Math.Max(1, (int)Math.Round((outgoing?.Height ?? current?.Height ?? 0) * (1 - progress)
                    + (current?.Height ?? 0) * progress));
                snapshot = new Bitmap(Pixel(CardWidth), height, PixelFormat.Format32bppPArgb);
                using (var graphics = Graphics.FromImage(snapshot))
                {
                    graphics.Clear(_background);
                    if (current != null) graphics.DrawImageUnscaled(current, 0, 0);
                    if (outgoing != null && progress < 1) DrawAlpha(graphics, outgoing, 0, 0, 1 - progress);
                }
            }
            outgoing?.Dispose(); current?.Dispose();
            outgoing = snapshot; current = null;
        }

        private void SetThumbnail(Bitmap next, bool animate, double now)
        {
            if (!animate)
            {
                _oldThumbnail?.Dispose(); _oldThumbnail = null;
                _thumbnail?.Dispose(); _thumbnail = next;
                return;
            }
            // ThumbnailView's three-way crossfade: keep the occluding image in
            // the first half; after halfway, promote the old target and rewind.
            if (_oldThumbnail != null)
            {
                double progress = TabHoverCardAnimation.Clamp((now - _imageStart) / 200);
                if (progress <= .5)
                {
                    _thumbnail?.Dispose(); _thumbnail = next;
                    return;
                }
                _oldThumbnail.Dispose();
                _oldThumbnail = _thumbnail;
                _imageStart = now - (1 - progress) * 200;
            }
            else
            {
                _oldThumbnail = _thumbnail;
                _imageStart = now;
            }
            _thumbnail = next;
        }

        private int Pixel(float dip) => Math.Max(1, ChromiumTabMetrics.Pixel(dip * _scale));

        private Bitmap MakeFooter(string text)
        {
            using (var font = new Font("Segoe UI", 12 * _scale, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var scratch = new Bitmap(1, 1))
            using (var measure = Graphics.FromImage(scratch))
            using (var format = new StringFormat(StringFormat.GenericTypographic))
            {
                int textLeft = Pixel(12 + 16 + 8), textWidth = Pixel(CardWidth - 12) - textLeft;
                int lines = measure.MeasureString(text, font, textWidth, format).Height > Pixel(16) + _scale ? 2 : 1;
                var footer = new Bitmap(Pixel(CardWidth), Pixel(24 + lines * 16), PixelFormat.Format32bppPArgb);
                using (var graphics = Graphics.FromImage(footer))
                using (var foreground = new SolidBrush(ChromiumTabTheme.Blend(_background, _foreground, .75f)))
                using (var pen = new Pen(_foreground, 1.5f * _scale))
                {
                    graphics.Clear(ChromiumTabTheme.Blend(_background, _foreground, .055f));
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    // Chromium's 16 DIP performance speedometer, with an 8 DIP
                    // icon/label gap and 12 DIP footer margins.
                    float x = Pixel(12), y = Pixel(12);
                    graphics.DrawArc(pen, x + _scale, y + 2 * _scale, 14 * _scale, 14 * _scale, 145, 250);
                    graphics.DrawLine(pen, x + 8 * _scale, y + 9 * _scale, x + 12 * _scale, y + 5 * _scale);
                    using (var dot = new SolidBrush(_foreground))
                        graphics.FillEllipse(dot, x + 6.5f * _scale, y + 7.5f * _scale, 3 * _scale, 3 * _scale);
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    graphics.DrawString(text, font, foreground, new RectangleF(textLeft, y, textWidth, Pixel(16 * lines)), format);
                }
                return footer;
            }
        }

        private Bitmap MakeHeader(string title, string domain)
        {
            int margin = Pixel(12), textWidth = Pixel(CardWidth) - margin * 2;
            using (var titleFont = new Font("Segoe UI Semibold", 14 * _scale, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var domainFont = new Font("Segoe UI", 12 * _scale, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var scratch = new Bitmap(1, 1))
            using (var measure = Graphics.FromImage(scratch))
            using (var format = new StringFormat(StringFormat.GenericTypographic))
            {
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags |= StringFormatFlags.LineLimit;
                title = string.IsNullOrEmpty(title) ? "Untitled" : title;
                int lineHeight = Pixel(20);
                int lines = measure.MeasureString(title, titleFont, textWidth, format).Height > lineHeight + _scale ? 2 : 1;
                int titleHeight = lineHeight * lines;
                int domainHeight = string.IsNullOrEmpty(domain) ? 0 : Pixel(4) + Pixel(16);
                var result = new Bitmap(Pixel(CardWidth), margin * 2 + titleHeight + domainHeight, PixelFormat.Format32bppPArgb);
                using (var graphics = Graphics.FromImage(result))
                using (var titleBrush = new SolidBrush(_foreground))
                using (var domainBrush = new SolidBrush(ChromiumTabTheme.Blend(_background, _foreground, .75f)))
                {
                    graphics.Clear(_background);
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    graphics.DrawString(title, titleFont, titleBrush,
                        new RectangleF(margin, margin, textWidth, titleHeight), format);
                    if (domainHeight > 0)
                    {
                        format.FormatFlags |= StringFormatFlags.NoWrap;
                        // Chrome elides the head of a long domain to retain its suffix.
                        while (domain.Length > 1 && graphics.MeasureString(domain, domainFont, int.MaxValue, format).Width > textWidth)
                        {
                            string tail = domain[0] == '\u2026' ? domain.Substring(1) : domain;
                            int skip = char.IsHighSurrogate(tail[0]) && tail.Length > 1 ? 2 : 1;
                            domain = "\u2026" + tail.Substring(skip);
                        }
                        graphics.DrawString(domain, domainFont, domainBrush,
                            new RectangleF(margin, margin + titleHeight + Pixel(4), textWidth, Pixel(16)), format);
                    }
                }
                return result;
            }
        }

        private Bitmap MakeThumbnail(Image preview, bool crashed)
        {
            var result = new Bitmap(Pixel(CardWidth), Pixel(PreviewHeight), PixelFormat.Format32bppPArgb);
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.Clear(ChromiumTabTheme.Blend(_background, _foreground, .055f));
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (preview != null)
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    double ratio = (double)preview.Width / preview.Height / (CardWidth / (double)PreviewHeight);
                    Rectangle target = new Rectangle(Point.Empty, result.Size);
                    if (ratio < .667 || ratio > 1.5)
                    {
                        double fit = Math.Min(result.Width / (double)preview.Width, result.Height / (double)preview.Height);
                        target.Size = new Size(Math.Max(1, (int)(preview.Width * fit)), Math.Max(1, (int)(preview.Height * fit)));
                        target.X = (result.Width - target.Width) / 2; // leading vertically, centered horizontally
                    }
                    graphics.DrawImage(preview, target);
                }
                else
                {
                    float cx = result.Width / 2f, cy = result.Height / 2f, radius = 26 * _scale;
                    using (var pen = new Pen(ChromiumTabTheme.Blend(_background, _foreground, .65f), 3 * _scale))
                    {
                        if (crashed)
                        {
                            graphics.DrawRectangle(pen, cx - radius * .7f, cy - radius, radius * 1.4f, radius * 2);
                            graphics.DrawLine(pen, cx - 10 * _scale, cy - 7 * _scale, cx - 6 * _scale, cy - 3 * _scale);
                            graphics.DrawLine(pen, cx + 6 * _scale, cy - 3 * _scale, cx + 10 * _scale, cy - 7 * _scale);
                            graphics.DrawArc(pen, cx - 10 * _scale, cy + 8 * _scale, 20 * _scale, 12 * _scale, 180, 180);
                        }
                        else
                        {
                            graphics.DrawEllipse(pen, cx - radius, cy - radius, radius * 2, radius * 2);
                            graphics.DrawEllipse(pen, cx - radius * .45f, cy - radius, radius * .9f, radius * 2);
                            graphics.DrawLine(pen, cx - radius, cy, cx + radius, cy);
                            graphics.DrawArc(pen, cx - radius, cy - radius * .65f, radius * 2, radius * .8f, 0, 180);
                            graphics.DrawArc(pen, cx - radius, cy - radius * .15f, radius * 2, radius * .8f, 180, 180);
                        }
                    }
                }
            }
            return result;
        }

        internal Bitmap RenderFrame(Size size, double textProgress, double now)
        {
            int shadow = Pixel(ShadowMargin);
            var frame = new Bitmap(size.Width + shadow * 2, size.Height + shadow * 2, PixelFormat.Format32bppPArgb);
            using (var graphics = Graphics.FromImage(frame)) PaintFrame(graphics, size, textProgress, now);
            return frame;
        }

        private void PaintFrame(Graphics graphics, Size size, double textProgress, double now)
        {
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int shadow = Pixel(ShadowMargin);
            var cardBounds = new RectangleF(shadow, shadow, size.Width, size.Height);
            // Two soft elevation shadows, rendered inside the reusable alpha surface.
            for (int i = Pixel(8); i >= 1; --i)
            {
                var bounds = cardBounds;
                bounds.Offset(0, 2 * _scale);
                bounds.Inflate(i / 2f, i / 2f);
                using (var path = Rounded(bounds, 8 * _scale + i / 2f))
                using (var brush = new SolidBrush(Color.FromArgb(Math.Max(1, (int)(5 / _scale)), Color.Black)))
                    graphics.FillPath(brush, path);
            }
            using (var path = Rounded(cardBounds, 8 * _scale))
            using (var background = new SolidBrush(_background))
            {
                graphics.FillPath(background, path);
                var state = graphics.Save();
                graphics.SetClip(path);
                int headerHeight = (int)Math.Round((_oldHeader?.Height ?? HeaderHeight) * (1 - textProgress) + HeaderHeight * textProgress);
                int footerHeight = (int)Math.Round((_oldFooter?.Height ?? FooterHeight) * (1 - textProgress) + FooterHeight * textProgress);
                var headerClip = graphics.Save();
                graphics.SetClip(new Rectangle(shadow, shadow, size.Width, headerHeight), CombineMode.Intersect);
                if (_header != null) graphics.DrawImageUnscaled(_header, shadow, shadow);
                if (_oldHeader != null && textProgress < 1)
                    DrawAlpha(graphics, _oldHeader, shadow, shadow, 1 - textProgress);
                graphics.Restore(headerClip);
                int thumbnailHeight = Math.Max(0, size.Height - headerHeight - footerHeight);
                if ((_thumbnail != null || _oldThumbnail != null) && thumbnailHeight > 0)
                {
                    int y = shadow + headerHeight;
                    var imageClip = graphics.Save();
                    graphics.SetClip(new Rectangle(shadow, y, size.Width, thumbnailHeight), CombineMode.Intersect);
                    if (_thumbnail != null) graphics.DrawImageUnscaled(_thumbnail, shadow, y);
                    double progress = TabHoverCardAnimation.Clamp((now - _imageStart) / 200);
                    if (_oldThumbnail != null && progress < 1)
                        DrawAlpha(graphics, _oldThumbnail, shadow, y, 1 - progress);
                    graphics.Restore(imageClip);
                }
                int footerY = shadow + size.Height - footerHeight;
                if (_footer != null) graphics.DrawImageUnscaled(_footer, shadow, footerY);
                if (_oldFooter != null && textProgress < 1)
                    DrawAlpha(graphics, _oldFooter, shadow, footerY, 1 - textProgress);
                graphics.Restore(state);
                using (var border = new Pen(Color.FromArgb(35, _foreground), _scale)) graphics.DrawPath(border, path);
            }
            if (textProgress >= 1)
            {
                _oldHeader?.Dispose(); _oldHeader = null;
                _oldFooter?.Dispose(); _oldFooter = null;
            }
            if (now - _imageStart >= 200) { _oldThumbnail?.Dispose(); _oldThumbnail = null; }
        }

        private static void DrawAlpha(Graphics graphics, Bitmap bitmap, int x, int y, double opacity)
        {
            using (var attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(new ColorMatrix { Matrix33 = (float)opacity });
                graphics.DrawImage(bitmap, new Rectangle(x, y, bitmap.Width, bitmap.Height), 0, 0,
                    bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        private static GraphicsPath Rounded(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float diameter = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        internal void Present(Form owner, TabHoverCardAnimation animation, double now)
        {
            if (animation.Opacity <= 0 && !animation.IsAnimating)
            {
                Hide(); SetAnimating(false); ReleaseImages(); return;
            }
            Rectangle bounds = Rectangle.Round(animation.Bounds);
            int shadow = Pixel(ShadowMargin);
            _surface.EnsureSize(bounds.Width + shadow * 2, bounds.Height + shadow * 2);
            PaintFrame(_surface.Graphics, bounds.Size, animation.TextProgress, now);
            _surface.Graphics.Flush(FlushIntention.Sync);
            SIZE size = new SIZE { cx = _surface.Bitmap.Width, cy = _surface.Bitmap.Height };
            POINT source = new POINT();
            POINT position = new POINT { x = bounds.X - shadow, y = bounds.Y - shadow };
            BLENDFUNCTION blend = new BLENDFUNCTION { BlendOp = 0, AlphaFormat = 1,
                SourceConstantAlpha = (byte)Math.Round(Math.Max(.01, animation.Opacity) * 255) };
            if (!User32.UpdateLayeredWindow(Handle, IntPtr.Zero, ref position, ref size,
                _surface.DeviceContext, ref source, 0, ref blend, ULW.ULW_ALPHA))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            if (!Visible) Show(owner);
            SetAnimating(animation.IsAnimating || HasImageAnimation);
        }

        internal void Clear()
        {
            Hide(); SetAnimating(false); ReleaseImages();
        }

        private void ReleaseImages()
        {
            _header?.Dispose(); _header = null;
            _oldHeader?.Dispose(); _oldHeader = null;
            _thumbnail?.Dispose(); _thumbnail = null;
            _oldThumbnail?.Dispose(); _oldThumbnail = null;
            _footer?.Dispose(); _footer = null;
            _oldFooter?.Dispose(); _oldFooter = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _frames?.Dispose(); ReleaseImages(); _surface.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
