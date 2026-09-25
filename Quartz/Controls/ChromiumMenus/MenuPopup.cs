using SkiaSharp;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz.Controls.ChromiumMenus
{
    internal sealed class MenuPopup : MenuLayeredWindow
    {
        internal readonly ChromiumMenu Menu;
        internal readonly MenuText TextMetrics;
        internal readonly MenuPainter Painter;
        internal MenuLayout LayoutData;
        internal readonly bool Submenu;
        internal float Scale;
        internal int BodyHeight, Scroll;
        internal ChromiumMenuItem Selected;
        internal ChromiumMenuItem ParentItem;
        internal int Part = -1;
        internal bool Cues;
        private int lastZoomPercent;
        private readonly MenuFadeAnimation fade = new MenuFadeAnimation();
        internal bool Retired { get; private set; }
        internal void ShowMenu(Form owner)
        {
            SurfaceAlpha = Painter.Appearance.Animations && MenuFadeAnimation.SystemEnabled ? (byte)0 : (byte)255;
            Show(owner); Render();
            if (SurfaceAlpha == 0) fade.Start(0, 1, value => { SurfaceAlpha = (byte)Math.Round(value * 255); Present(); });
        }
        internal void Retire()
        {
            Retired = true; MakeInputTransparent();
            if (!Painter.Appearance.Animations || !MenuFadeAnimation.SystemEnabled) { Dispose(); return; }
            fade.Start(SurfaceAlpha / 255.0, 0, value => { SurfaceAlpha = (byte)Math.Round(value * 255); Present(); }, Dispose);
        }
        internal int LeftInset => Submenu ? 16 : 24;
        internal int TopInset => Submenu ? 16 : 12;
        internal int BottomInset => Submenu ? 28 : 36;
        internal int ScrollArrowHeight => LayoutData.Height > BodyHeight ? TextMetrics.Height + 12 : 0;
        internal readonly int MaximumWidth, MaximumHeight;
        internal MenuPopup(ChromiumMenu menu, MenuAppearance appearance, bool submenu, float scale, int maxHeight, int maxWidth = 800)
        {
            Menu = menu; Submenu = submenu; Scale = scale;
            TextMetrics = new MenuText(); Painter = new MenuPainter(TextMetrics, appearance);
            MaximumWidth = Math.Min(800 - 2 * LeftInset, maxWidth);
            MaximumHeight = maxHeight;
            LayoutData = new MenuLayout(menu, TextMetrics, MaximumWidth);
            BodyHeight = Math.Min(LayoutData.Height, MaximumHeight);
            FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; StartPosition = FormStartPosition.Manual;
            AutoScaleMode = AutoScaleMode.None;
            Size = new Size(Pixel(LayoutData.Width + 2 * LeftInset), Pixel(BodyHeight + TopInset + BottomInset));
            Cues = SystemInformation.MenuAccessKeysUnderlined;
            lastZoomPercent = menu.Items.OfType<ChromiumZoomMenuItem>().Select(z => z.Percent).DefaultIfEmpty(-1).First();
            AccessibleName = "Menu"; AccessibleRole = AccessibleRole.MenuPopup;
        }
        internal int Pixel(float dip) => (int)Math.Round(dip * Scale, MidpointRounding.AwayFromZero);
        internal PointF BodyPoint(Point screen) => new PointF((screen.X - Left) / Scale - LeftInset, (screen.Y - Top) / Scale - TopInset);
        internal Rectangle BodyBounds => new Rectangle(Left + Pixel(LeftInset), Top + Pixel(TopInset), Pixel(LayoutData.Width), Pixel(BodyHeight));
        internal Rectangle RowBounds(MenuRow row)
        {
            return new Rectangle(Left + Pixel(LeftInset), Top + Pixel(TopInset + row.Y - Scroll + ScrollArrowHeight), Pixel(LayoutData.Width), Pixel(row.Height));
        }
        internal MenuRow Hit(Point screen, out int part)
        {
            part = -1; PointF p = BodyPoint(screen);
            if (p.Y < ScrollArrowHeight || p.Y >= BodyHeight - ScrollArrowHeight) return null;
            float y = p.Y + Scroll - ScrollArrowHeight;
            float x = Painter.Appearance.RightToLeft ? LayoutData.Width - p.X : p.X;
            var row = LayoutData.Hit(x, y);
            if (row?.Buttons != null)
                for (int i = 0; i < 3; i++) if (row.Buttons[i].Contains(x, y)) { part = i; break; }
            return row;
        }
        internal void EnsureVisible(MenuRow row)
        {
            if (ScrollArrowHeight == 0) return;
            int viewport = BodyHeight - 2 * ScrollArrowHeight;
            if (row.Y < Scroll) Scroll = row.Y;
            else if (row.Y + row.Height > Scroll + viewport) Scroll = row.Y + row.Height - viewport;
        }
        internal void Render()
        {
            if (IsDisposed || Retired || !IsHandleCreated) return;
            using (var bitmap = new SKBitmap(Width, Height, SKColorType.Bgra8888, SKAlphaType.Premul))
            using (var props = MenuPlatform.SurfaceProperties())
            using (var surface = SKSurface.Create(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, props))
            {
                Painter.Paint(surface.Canvas, LayoutData, Submenu, Scale, BodyHeight, Scroll, Selected, Part, Cues);
                SetImage(bitmap);
            }
            int percent = Menu.Items.OfType<ChromiumZoomMenuItem>().Select(z => z.Percent).DefaultIfEmpty(-1).First();
            if (percent != lastZoomPercent) { lastZoomPercent = percent; AnnounceZoom(); }
        }
        internal void AnnounceFocus()
        {
            int index = LayoutData.Rows.FindIndex(r => r.Item == Selected);
            if (index < 0) return;
            if (Part < 0) AccessibilityNotifyClients(AccessibleEvents.Focus, index);
            else Native.NotifyWinEvent(0x8005, Handle, 0x1000 + index * 4 + (Part == 0 ? 0 : Part + 1), 0);
        }
        internal void AnnounceZoom()
        {
            int index = LayoutData.Rows.FindIndex(r => r.Item is ChromiumZoomMenuItem);
            if (index < 0) return;
            int objectId = 0x1000 + index * 4 + 1;
            Native.NotifyWinEvent(0x800c, Handle, objectId, 0); // Name changed on the percentage itself.
            Native.NotifyWinEvent(0x0002, Handle, objectId, 0); // EVENT_SYSTEM_ALERT, ROLE_SYSTEM_ALERT.
        }
        protected override AccessibleObject CreateAccessibilityInstance() => new PopupAccessible(this);
        protected override void WndProc(ref Message m)
        {
            // UIA uses negative DWORD object IDs, sometimes zero-extended to 64
            // bits. Only decode our bounded positive range before converting.
            if (m.Msg == 0x003d && (long)m.LParam >= 0x1000 && (long)m.LParam < 0x1000L + LayoutData.Rows.Count * 4)
            {
                int id = (int)m.LParam - 0x1000;
                var child = AccessibilityObject.GetChild(id / 4)?.GetChild(id % 4);
                if (child != null)
                {
                    var iid = new Guid("618736e0-3c3d-11cf-810c-00aa00389b71");
                    IntPtr unknown = Marshal.GetIUnknownForObject(child);
                    try { m.Result = Native.LresultFromObject(ref iid, m.WParam, unknown); }
                    finally { Marshal.Release(unknown); }
                    return;
                }
            }
            if (m.Msg == 0x21) { m.Result = new IntPtr(3); return; }
            if (!Retired && m.Msg == 0x215 && m.LParam != Handle && MenuSession.Current?.Contains(Menu) == true) MenuSession.Current?.CaptureLost(m.LParam);
            if (!Retired && m.Msg == 0x02e0) { MenuSession.Current?.Close(Menu, true); return; }
            base.WndProc(ref m);
        }
        protected override void Dispose(bool disposing) { if (disposing) { fade.Dispose(); TextMetrics.Dispose(); } base.Dispose(disposing); }

        private sealed class PopupAccessible : ControlAccessibleObject
        {
            private readonly MenuPopup popup;
            private readonly Dictionary<ChromiumMenuItem, RowAccessible> children = new Dictionary<ChromiumMenuItem, RowAccessible>();
            internal PopupAccessible(MenuPopup popup) : base(popup) { this.popup = popup; }
            public override AccessibleRole Role => AccessibleRole.MenuPopup;
            public override int GetChildCount() => popup.LayoutData.Rows.Count;
            public override AccessibleObject GetChild(int index)
            {
                if (index < 0 || index >= GetChildCount()) return null;
                var row = popup.LayoutData.Rows[index];
                if (!children.TryGetValue(row.Item, out var node)) children.Add(row.Item, node = new RowAccessible(popup, row, this));
                node.Row = row; return node;
            }
            public override AccessibleObject GetFocused()
            {
                int index = popup.LayoutData.Rows.FindIndex(r => r.Item == popup.Selected);
                var row = GetChild(index); return popup.Part < 0 ? row : row?.GetChild(popup.Part == 0 ? 0 : popup.Part + 1);
            }
            public override AccessibleObject HitTest(int x, int y)
            {
                var row = popup.Hit(new Point(x, y), out int part);
                if (row == null) return this;
                var node = GetChild(popup.LayoutData.Rows.IndexOf(row)); return part < 0 ? node : node.GetChild(part == 0 ? 0 : part + 1);
            }
        }
        private sealed class RowAccessible : AccessibleObject
        {
            internal readonly MenuPopup Popup; internal MenuRow Row; private readonly AccessibleObject parent;
            private readonly AccessibleObject[] children = new AccessibleObject[4];
            internal RowAccessible(MenuPopup popup, MenuRow row, AccessibleObject parent) { Popup = popup; Row = row; this.parent = parent; }
            public override AccessibleObject Parent => parent;
            public override string Name { get => MenuText.Label(Row.Item.Text); set { } }
            public override string KeyboardShortcut => Row.Item.ShortcutKeyDisplayString;
            public override AccessibleRole Role => Row.Item is ChromiumMenuSeparator ? AccessibleRole.Separator : Row.Item is ChromiumZoomMenuItem ? AccessibleRole.MenuPopup : AccessibleRole.MenuItem;
            public override Rectangle Bounds => Popup.RowBounds(Row);
            public override AccessibleStates State => (Row.Item is ChromiumMenuSeparator ? AccessibleStates.None : Row.Item.Enabled ? AccessibleStates.Focusable : AccessibleStates.Unavailable) | (Row.Item.Checked ? AccessibleStates.Checked : 0) | (Popup.Selected == Row.Item ? AccessibleStates.Focused | AccessibleStates.Selected : 0) | (Row.Item.HasSubmenu ? AccessibleStates.HasPopup : 0);
            public override string DefaultAction => Row.Item.HasSubmenu ? "Open" : "Press";
            public override void DoDefaultAction() { MenuSession.Current?.InvokeAccessible(Popup, Row, -1); }
            public override int GetChildCount() => Row.Item is ChromiumZoomMenuItem ? 4 : 0;
            public override AccessibleObject GetChild(int index) => index >= 0 && index < GetChildCount() ? children[index] ?? (children[index] = new ZoomAccessible(this, index)) : null;
            public override AccessibleObject GetFocused() => Popup.Selected != Row.Item ? null : Popup.Part < 0 ? this : GetChild(Popup.Part == 0 ? 0 : Popup.Part + 1);
        }
        private sealed class ZoomAccessible : AccessibleObject
        {
            private readonly RowAccessible row; private readonly int index;
            private ChromiumZoomMenuItem Zoom => (ChromiumZoomMenuItem)row.Row.Item;
            private int Part => index == 0 ? 0 : index == 2 ? 1 : 2;
            internal ZoomAccessible(RowAccessible row, int index) { this.row = row; this.index = index; }
            public override AccessibleObject Parent => row;
            public override AccessibleRole Role => index == 1 ? AccessibleRole.Alert : AccessibleRole.MenuItem;
            public override string Name
            {
                get => index == 0 ? "Make Text Smaller" : index == 1 ? MenuText.Percent(Zoom.Percent) : index == 2 ? "Make Text Larger" : (Zoom.IsFullscreen() ? "Exit full screen" : Zoom.CanFullscreen() ? "Full screen" : "Full screen disabled") + " F11";
                set { }
            }
            public override AccessibleStates State => index == 1 ? AccessibleStates.ReadOnly : (Zoom.ButtonEnabled(Part) ? AccessibleStates.Focusable : AccessibleStates.Unavailable) | (row.Popup.Selected == Zoom && row.Popup.Part == Part ? AccessibleStates.Focused | AccessibleStates.Selected : 0);
            public override Rectangle Bounds
            {
                get
                {
                    var r = index == 1 ? row.Row.Percentage : row.Row.Buttons[Part]; var p = row.Popup; var b = row.Bounds;
                    float x = p.Painter.Appearance.RightToLeft ? p.LayoutData.Width - r.Right : r.Left;
                    return new Rectangle(b.X + p.Pixel(x), b.Y, p.Pixel(r.Width), b.Height);
                }
            }
            public override string DefaultAction => index == 1 ? "" : "Press";
            public override void DoDefaultAction() { if (index != 1) MenuSession.Current?.InvokeAccessible(row.Popup, row.Row, Part); }
        }
        [StructLayout(LayoutKind.Sequential)] internal struct BitmapInfo { internal int Size, Width, Height; internal short Planes, BitCount; internal int Compression, SizeImage, XPels, YPels, Used, Important; }
        [StructLayout(LayoutKind.Sequential, Pack = 1)] internal struct Blend { internal byte Op, Flags, SourceConstantAlpha, AlphaFormat; }
        internal static class Native
        {
            [DllImport("user32.dll")] internal static extern void NotifyWinEvent(uint eventId, IntPtr hwnd, int objectId, int childId);
            [DllImport("oleacc.dll")] internal static extern IntPtr LresultFromObject(ref Guid iid, IntPtr wparam, IntPtr unknown);
            [DllImport("user32.dll")] internal static extern IntPtr GetDC(IntPtr hwnd);
            [DllImport("user32.dll")] internal static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
            [DllImport("gdi32.dll")] internal static extern IntPtr CreateCompatibleDC(IntPtr dc);
            [DllImport("gdi32.dll")] internal static extern bool DeleteDC(IntPtr dc);
            [DllImport("gdi32.dll")] internal static extern IntPtr CreateDIBSection(IntPtr dc, ref BitmapInfo info, uint usage, out IntPtr bits, IntPtr section, uint offset);
            [DllImport("gdi32.dll")] internal static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
            [DllImport("gdi32.dll")] internal static extern bool DeleteObject(IntPtr obj);
            [DllImport("user32.dll", SetLastError = true)] internal static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr dst, ref Point pos, ref Size size, IntPtr src, ref Point origin, uint key, ref Blend blend, uint flags);
        }
    }
}
