using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Quartz.Controls
{
    public class CircularImageButton : Button
    {
        public string ButtonText { get; set; } = string.Empty;

        private Image _circularImage;
        public Image CircularImage
        {
            get => _circularImage;
            set
            {
                if (_circularImage != value)
                {
                    _circularImage = value;
                    OnCircularImageChanged(EventArgs.Empty);
                    Invalidate(); // Redraw the control
                }
            }
        }

        public int CircularImageToTextGapping { get; set; } = 5;
        public float CircularImageBorderSize { get; set; } = 0;
        public int CircularImageSize { get; set; } = 64;
        public Color CircularImageBorderColor { get; set; } = Color.Black;

        public Image ActionButtonImage { get; set; }
        public Image ActionButtonHoverImage { get; set; }
        public int ActionButtonGapping { get; set; } = 5;

        private bool isMouseOver = false;
        private bool isMouseOverActionButton = false;
        private Rectangle actionButtonRect;

        public event EventHandler ActionButtonClick;
        public event EventHandler CircularImageChanged;

        protected virtual void OnCircularImageChanged(EventArgs e)
        {
            CircularImageChanged?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            Rectangle clientRect = this.ClientRectangle;

            int textHeight = 0;
            if (!string.IsNullOrEmpty(ButtonText))
            {
                textHeight = TextRenderer.MeasureText(ButtonText, this.Font, new Size(clientRect.Width, int.MaxValue), TextFormatFlags.WordBreak).Height;
            }

            int totalHeight = CircularImageSize + (string.IsNullOrEmpty(ButtonText) ? 0 : CircularImageToTextGapping + textHeight);
            int startY = (clientRect.Height - totalHeight) / 2;

            int imageX = (clientRect.Width - CircularImageSize) / 2;
            int imageY = startY;
            Rectangle imageRect = new Rectangle(imageX, imageY, CircularImageSize, CircularImageSize);

            if (CircularImage != null)
            {
                Image downscaledImage = DownscaleImage(CircularImage, CircularImageSize, CircularImageSize);
                Image circularImage = CreateUltraSmoothCircularImage(downscaledImage, CircularImageSize);
                g.DrawImage(circularImage, imageRect);

                if (CircularImageBorderSize > 0)
                {
                    using (Pen borderPen = new Pen(CircularImageBorderColor, CircularImageBorderSize))
                    {
                        g.DrawEllipse(borderPen, imageRect);
                    }
                }
            }

            if (!string.IsNullOrEmpty(ButtonText))
            {
                int textY = CircularImage != null ? imageY + CircularImageSize + CircularImageToTextGapping : (clientRect.Height - textHeight) / 2;
                Rectangle textRect = new Rectangle(clientRect.X, textY, clientRect.Width, textHeight);
                TextRenderer.DrawText(g, ButtonText, this.Font, textRect, this.ForeColor, TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter);
            }

            if (isMouseOver && ActionButtonImage != null)
            {
                int size = ActionButtonImage.Width;
                int x = clientRect.Right - size - ActionButtonGapping;
                int y = clientRect.Top + ActionButtonGapping;

                actionButtonRect = new Rectangle(x, y, size, size);

                if (isMouseOverActionButton && ActionButtonHoverImage != null)
                    g.DrawImage(ActionButtonHoverImage, actionButtonRect);
                else
                    g.DrawImage(ActionButtonImage, actionButtonRect);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool wasOverActionButton = isMouseOverActionButton;

            isMouseOver = true;
            isMouseOverActionButton = actionButtonRect.Contains(e.Location);

            if (wasOverActionButton != isMouseOverActionButton)
                Invalidate(); // Redraw only if hover state changed
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isMouseOver = false;
            isMouseOverActionButton = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (isMouseOverActionButton)
            {
                ActionButtonClick?.Invoke(this, EventArgs.Empty);
                return; // prevent base click behavior
            }

            base.OnMouseDown(e);
        }

        private Image CreateUltraSmoothCircularImage(Image img, int size)
        {
            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, size, size);
                    using (Brush brush = new TextureBrush(img))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
            return bmp;
        }

        private Image DownscaleImage(Image originalImage, int width, int height)
        {
            Bitmap downscaledBitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(downscaledBitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(originalImage, 0, 0, width, height);
            }
            return downscaledBitmap;
        }
    }
}
