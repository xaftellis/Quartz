using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Quartz.Controls.ChromiumMenus
{
    internal sealed class MenuSession : IMessageFilter, IDisposable
    {
        internal static MenuSession Current { get; private set; }
        private readonly List<MenuPopup> popups = new List<MenuPopup>();
        private readonly Control source;
        private readonly Form owner;
        private readonly MenuAppearance appearance;
        private readonly IntPtr initialForeground;
        private readonly Timer timer = new Timer { Interval = 30 };
        private readonly MenuTooltip tooltip = new MenuTooltip();
        private Point lastPoint, openingPoint;
        private DateTime opened, hoverAt, scrollAt;
        private ChromiumMenuItem pending;
        private MenuPopup pendingPopup;
        private bool closing, dragging;
        private int delay;
        private MenuSession(Control source, MenuAppearance appearance)
        {
            this.source = source; owner = source?.TopLevelControl as Form ?? source?.FindForm(); this.appearance = appearance;
            initialForeground = GetForegroundWindow();
            delay = 400; SystemParametersInfo(0x006a, 0, ref delay, 0);
            timer.Tick += Tick;
        }
        internal static void Open(ChromiumMenu menu, Control source, Point anchor)
        {
            Current?.Dispose();
            if (menu.IsDisposed || source == null || source.IsDisposed || !menu.Prepare(source)) return;
            using (new DpiScope())
            {
                // PointToScreen may be virtualized by Quartz's owner. Convert that point explicitly.
                LogicalToPhysicalPointForPerMonitorDPI(source.Handle, ref anchor);
                var session = new MenuSession(source, menu.Appearance ?? MenuAppearance.ForApplication()); Current = session;
                try
                {
                    session.openingPoint = session.lastPoint = anchor; session.opened = DateTime.UtcNow;
                    var area = WorkArea(anchor, out float scale);
                    var popup = new MenuPopup(menu, session.appearance, false, scale, (int)(area.Height / scale), (int)(area.Width / scale));
                    int x = anchor.X - popup.Pixel(popup.LeftInset);
                    if (session.appearance.RightToLeft) x = anchor.X - popup.Width + popup.Pixel(popup.LeftInset);
                    int y = anchor.Y - popup.Pixel(popup.TopInset);
                    if (y + popup.Pixel(popup.TopInset + popup.BodyHeight) > area.Bottom) y = anchor.Y - popup.Height + popup.Pixel(popup.BottomInset);
                    session.Place(popup, x, y, area); session.popups.Add(popup); popup.ShowMenu(session.owner);
                    menu.DidOpen(); Application.AddMessageFilter(session); SetCapture(popup.Handle); session.timer.Start();
                }
                catch { session.Dispose(); throw; }
            }
        }
        internal bool Contains(ChromiumMenu menu) => popups.Any(p => p.Menu == menu);
        internal static bool OwnsHandle(IntPtr handle) => Current?.popups.Any(p => p.Handle == handle) == true;
        internal static bool FilterKeys(ref Message message) => Current != null && Current.KeyMessage(ref message);
        internal void CaptureLost(IntPtr next) { if (!closing && !OwnsHandle(next)) Dispose(); }
        internal void Refresh(ChromiumMenu menu)
        {
            using (new DpiScope())
                foreach (var popup in popups.Where(p => p.Menu == menu).ToArray())
                {
                    popup.LayoutData = new MenuLayout(menu, popup.TextMetrics, popup.MaximumWidth);
                    popup.BodyHeight = Math.Min(popup.LayoutData.Height, popup.MaximumHeight);
                    popup.Size = new Size(popup.Pixel(popup.LayoutData.Width + 2 * popup.LeftInset), popup.Pixel(popup.BodyHeight + popup.TopInset + popup.BottomInset));
                    popup.Render();
                }
        }
        internal void Close(ChromiumMenu menu, bool force = false)
        {
            int index = popups.FindIndex(p => p.Menu == menu);
            if (index < 0) return;
            if (!force && !menu.WillClose()) { Refresh(menu); return; }
            if (index == 0) Dispose(); else CloseAfter(index - 1);
        }
        private void CloseAfter(int index)
        {
            for (int i = popups.Count - 1; i > index; --i)
            { var popup = popups[i]; popups.RemoveAt(i); popup.Menu.DidClose(); popup.Retire(); }
        }
        private void Place(MenuPopup p, int x, int y, Rectangle work)
        {
            int l = p.Pixel(p.LeftInset), t = p.Pixel(p.TopInset), bw = p.Pixel(p.LayoutData.Width), bh = p.Pixel(p.BodyHeight);
            x = Math.Max(work.Left - l, Math.Min(work.Right - bw - l, x));
            y = Math.Max(work.Top - t, Math.Min(work.Bottom - bh - t, y)); p.Location = new Point(x, y);
        }
        private void OpenSubmenu(MenuPopup parent, MenuRow row, bool keyboard)
        {
            pending = null;
            int depth = popups.IndexOf(parent);
            if (depth < 0 || !row.Item.Enabled || !row.Item.HasSubmenu) return;
            if (depth + 1 < popups.Count && popups[depth + 1].Menu == row.Item.DropDown && popups[depth + 1].ParentItem == row.Item) return;
            CloseAfter(depth); row.Item.PrepareDropDown(); var menu = row.Item.DropDown;
            if (menu == null || !menu.Prepare(source)) return;
            menu.OwnerItem = row.Item;
            using (new DpiScope())
            {
                Rectangle r = parent.RowBounds(row); var work = WorkArea(new Point(r.Right, r.Top), out float scale);
                var popup = new MenuPopup(menu, appearance, true, scale, (int)(work.Height / scale), (int)(work.Width / scale)) { ParentItem = row.Item };
                bool left = appearance.RightToLeft || (parent.Submenu && parent.Left < popups[depth - 1].Left);
                int x = left ? r.Left - popup.Width + popup.Pixel(popup.LeftInset) : r.Right - popup.Pixel(popup.LeftInset);
                if (!left && x + popup.Pixel(popup.LeftInset + popup.LayoutData.Width) > work.Right) x = r.Left - popup.Width + popup.Pixel(popup.LeftInset);
                else if (left && x + popup.Pixel(popup.LeftInset) < work.Left) x = r.Right - popup.Pixel(popup.LeftInset);
                int y = r.Top - popup.Pixel(popup.TopInset + 12);
                Place(popup, x, y, work); popups.Add(popup); popup.ShowMenu(owner); menu.DidOpen();
                if (keyboard) Navigate(popup, 1, true);
            }
        }
        public bool PreFilterMessage(ref Message m)
        {
            if (closing) return false;
            if (KeyMessage(ref m)) return true;
            if (m.Msg < 0x200 || m.Msg > 0x20e) return false;
            using (new DpiScope())
            {
                GetCursorPos(out Point point);
                var popup = popups.LastOrDefault(p => p.BodyBounds.Contains(point));
                bool down = m.Msg == 0x201 || m.Msg == 0x204 || m.Msg == 0x207;
                bool up = m.Msg == 0x202 || m.Msg == 0x205 || m.Msg == 0x208;
                if (down && popup == null)
                {
                    IntPtr target = WindowFromPoint(point);
                    bool captured = OwnsHandle(m.HWnd);
                    Dispose();
                    if (!captured) return false;
                    // Repost the initiating down to its actual control after releasing capture.
                    if (target != IntPtr.Zero)
                    {
                        PhysicalToLogicalPointForPerMonitorDPI(target, ref point); ScreenToClient(target, ref point);
                        PostMessage(target, m.Msg, m.WParam, new IntPtr((point.X & 0xffff) | (point.Y << 16)));
                    }
                    return true;
                }
                if (m.Msg == 0x200) { if (point != lastPoint) { Hover(popup, point); lastPoint = point; } return true; }
                if (m.Msg == 0x20a)
                {
                    if (popup != null) Scroll(popup, unchecked((short)((long)m.WParam >> 16)) > 0 ? -1 : 1);
                    return true;
                }
                if (down) { tooltip.Press(); dragging = m.Msg == 0x201; Hover(popup, point); return true; }
                if (up)
                {
                    double dx = point.X - openingPoint.X, dy = point.Y - openingPoint.Y;
                    if (popup != null && !((DateTime.UtcNow - opened).TotalMilliseconds < 200 && dx * dx + dy * dy < 16 * popup.Scale * popup.Scale))
                    {
                        var row = popup.Hit(point, out int part);
                        if (row != null && row.Item.Enabled)
                        {
                            if (!(row.Item is ChromiumZoomMenuItem) || m.Msg == 0x202 && dragging) Activate(popup, row, part, m.Msg == 0x208 ? MouseButtons.Middle : m.Msg == 0x205 ? MouseButtons.Right : MouseButtons.Left, point);
                        }
                    }
                    dragging = false; return true;
                }
                return popup != null;
            }
        }
        private bool KeyMessage(ref Message m)
        {
            if (m.Msg != 0x100 && m.Msg != 0x104 && m.Msg != 0x101 && m.Msg != 0x105 && m.Msg != 0x102 && m.Msg != 0x106) return false;
            if (closing || popups.Count == 0) return false;
            if (m.Msg == 0x101 || m.Msg == 0x105 || m.Msg == 0x102 || m.Msg == 0x106) return true;
            tooltip.Reset();
            var popup = popups[popups.Count - 1]; Keys key = (Keys)(int)m.WParam & Keys.KeyCode;
            if (key == Keys.Menu || key == Keys.F10 && Control.ModifierKeys == Keys.None) { Dispose(); return true; }
            if (key == Keys.Escape) { if (popups.Count == 1) Dispose(); else CloseAfter(popups.Count - 2); return true; }
            // Chromium's open-menu Windows route consumes browser accelerators (including Ctrl-0/F11).
            if ((Control.ModifierKeys & (Keys.Control | Keys.Alt)) != 0) return true;
            popup.Cues = true;
            if (key == Keys.Down || key == Keys.PageDown) Navigate(popup, 1);
            else if (key == Keys.Up || key == Keys.PageUp) Navigate(popup, -1);
            else if (key == Keys.Home) Navigate(popup, 1, true);
            else if (key == Keys.End) Navigate(popup, -1, true);
            else if (key == (appearance.RightToLeft ? Keys.Left : Keys.Right))
            { var row = popup.LayoutData.Rows.FirstOrDefault(r => r.Item == popup.Selected); if (row != null) OpenSubmenu(popup, row, true); }
            else if (key == (appearance.RightToLeft ? Keys.Right : Keys.Left)) { if (popups.Count > 1) CloseAfter(popups.Count - 2); }
            else if (key == Keys.Enter || key == Keys.Space)
            {
                var row = popup.LayoutData.Rows.FirstOrDefault(r => r.Item == popup.Selected);
                if (row != null && (key == Keys.Enter || row.Item is ChromiumZoomMenuItem))
                {
                    if (row.Item.HasSubmenu) OpenSubmenu(popup, row, true);
                    else if (key == Keys.Enter && row.Item is ChromiumZoomMenuItem && popup.Part < 0) Dispose();
                    else Activate(popup, row, popup.Part, MouseButtons.Left, null);
                }
            }
            else if (key >= Keys.A && key <= Keys.Z || key >= Keys.D0 && key <= Keys.D9)
            {
                char letter = char.ToUpperInvariant((char)key);
                var matches = popup.LayoutData.Rows.Where(r => r.Item.Enabled && HasMnemonic(r.Item.Text, letter)).ToArray();
                if (matches.Length > 0)
                {
                    var row = matches[(Array.FindIndex(matches, r => r.Item == popup.Selected) + 1) % matches.Length]; Select(popup, row, -1);
                    if (matches.Length == 1) { if (row.Item.HasSubmenu) OpenSubmenu(popup, row, true); else Activate(popup, row, -1, MouseButtons.Left, null); }
                }
            }
            if (!popup.IsDisposed) popup.Render(); return true;
        }
        private static bool HasMnemonic(string text, char letter)
        {
            for (int i = 0; i < text.Length - 1; ++i) if (text[i] == '&') { if (text[i + 1] == '&') ++i; else if (char.ToUpperInvariant(text[i + 1]) == letter) return true; }
            return false;
        }
        private void Navigate(MenuPopup popup, int direction, bool edge = false)
        {
            var targets = new List<Tuple<MenuRow, int>>();
            foreach (var row in popup.LayoutData.Rows.Where(r => r.Item.Enabled && !(r.Item is ChromiumMenuSeparator)))
            {
                if (row.Item is ChromiumZoomMenuItem zoom) { for (int i = 0; i < 3; ++i) if (zoom.ButtonEnabled(i)) targets.Add(Tuple.Create(row, i)); }
                else targets.Add(Tuple.Create(row, -1));
            }
            if (targets.Count == 0) return;
            int index = targets.FindIndex(t => t.Item1.Item == popup.Selected && t.Item2 == popup.Part);
            if (!edge && index < 0 && popup.Selected != null)
            {
                // The formerly hot control can become disabled at a zoom endpoint.
                // Resume at the next enabled control in the same group, not at the root.
                int ordinal = popup.LayoutData.Rows.FindIndex(r => r.Item == popup.Selected) * 4 + popup.Part + 1;
                Func<Tuple<MenuRow, int>, int> position = t => popup.LayoutData.Rows.IndexOf(t.Item1) * 4 + t.Item2 + 1;
                index = direction > 0 ? targets.FindIndex(t => position(t) > ordinal) : targets.FindLastIndex(t => position(t) < ordinal);
                if (index < 0) index = direction > 0 ? 0 : targets.Count - 1;
            }
            else if (edge || index < 0) index = direction > 0 ? 0 : targets.Count - 1;
            else index = (index + direction + targets.Count) % targets.Count;
            pending = null; CloseAfter(popups.IndexOf(popup)); Select(popup, targets[index].Item1, targets[index].Item2);
        }
        private void Select(MenuPopup p, MenuRow row, int part)
        {
            bool changed = p.Selected != row?.Item || p.Part != part;
            p.Selected = row?.Item; p.Part = part;
            if (row != null) p.EnsureVisible(row);
            if (changed) { p.Render(); p.AnnounceFocus(); }
        }
        private void Hover(MenuPopup popup, Point point)
        {
            if (popup == null)
            {
                tooltip.Reset();
                foreach (var p in popups.Where(p => p.Selected is ChromiumZoomMenuItem && p.Part >= 0)) { p.Part = -1; p.Render(); }
                return;
            }
            var row = popup.Hit(point, out int part);
            tooltip.Move(popup, row, part, point, dragging);
            if (row != null && !row.Item.Enabled) row = null;
            if (row?.Item is ChromiumZoomMenuItem zoom && part >= 0 && !zoom.ButtonEnabled(part)) part = -1;
            if (popup.Selected == row?.Item && popup.Part == part) return;
            Select(popup, row, part);
            pending = row?.Item; pendingPopup = popup; hoverAt = DateTime.UtcNow;
        }
        private void Activate(MenuPopup popup, MenuRow row, int part, MouseButtons button, Point? point)
        {
            if (!row.Item.Enabled || button == MouseButtons.Right) return;
            if (row.Item is ChromiumZoomMenuItem zoom)
            {
                if (part < 0 || !zoom.ButtonEnabled(part) || button != MouseButtons.Left) return;
                if (part == 2) { var action = zoom.Fullscreen; Dispose(); Dispatch(action); }
                else { zoom.Step?.Invoke(part == 0 ? -1 : 1); if (!popup.IsDisposed) { popup.Render(); } }
                return;
            }
            if (row.Item.HasSubmenu && (!row.Item.HasClick || point == null || (appearance.RightToLeft ? popup.BodyPoint(point.Value).X < 44 : popup.BodyPoint(point.Value).X >= popup.LayoutData.Width - 44)))
            { OpenSubmenu(popup, row, false); return; }
            var item = row.Item;
            bool close = popups[0].Menu.WillClose();
            if (close) Dispose();
            // Never enter ShowDialog from IMessageFilter. WinForms holds a snapshot
            // of the filters during dispatch; a nested modal loop can then miss the
            // session filter added by menus in Profiles, Settings or History.
            Dispatch(() =>
            {
                if (button == MouseButtons.Left) item.PerformClick();
                item.RaiseMouseUp(button);
                if (!close && !popup.IsDisposed) Refresh(popup.Menu);
            });
        }
        private void Dispatch(Action action)
        {
            if (action != null && owner != null && !owner.IsDisposed)
                owner.BeginInvoke(action);
        }
        internal void InvokeAccessible(MenuPopup popup, MenuRow row, int part)
        {
            if (!row.Item.Enabled) return;
            popup.BeginInvoke((Action)(() => { if (!popup.IsDisposed) Activate(popup, row, part, MouseButtons.Left, null); }));
        }
        private void Tick(object sender, EventArgs e)
        {
            if (closing || owner == null || owner.IsDisposed || source.IsDisposed || GetForegroundWindow() != initialForeground && GetForegroundWindow() != owner.Handle && !OwnsHandle(GetForegroundWindow())) { Dispose(); return; }
            using (new DpiScope())
            {
                tooltip.Tick();
                if (pending != null && (DateTime.UtcNow - hoverAt).TotalMilliseconds >= delay)
                {
                    var row = pendingPopup.LayoutData.Rows.FirstOrDefault(r => r.Item == pending);
                    pending = null;
                    if (row != null && row.Item.HasSubmenu) OpenSubmenu(pendingPopup, row, false);
                    else CloseAfter(popups.IndexOf(pendingPopup));
                }
                GetCursorPos(out Point point);
                var popup = popups.LastOrDefault(p => p.BodyBounds.Contains(point));
                if (popup != null && popup.ScrollArrowHeight > 0 && (DateTime.UtcNow - scrollAt).TotalMilliseconds > 80)
                {
                    float y = popup.BodyPoint(point).Y;
                    if (y < popup.ScrollArrowHeight) Scroll(popup, -1);
                    else if (y >= popup.BodyHeight - popup.ScrollArrowHeight) Scroll(popup, 1);
                    scrollAt = DateTime.UtcNow;
                }
            }
        }
        private void Scroll(MenuPopup p, int direction)
        {
            tooltip.Reset();
            if (p.ScrollArrowHeight == 0) return;
            int max = p.LayoutData.Height - p.BodyHeight + p.ScrollArrowHeight * 2;
            var ys = p.LayoutData.Rows.Select(r => r.Y).Concat(new[] { 0, max }).OrderBy(y => y).ToArray();
            p.Scroll = direction > 0 ? ys.Where(y => y > p.Scroll).DefaultIfEmpty(max).First() : ys.LastOrDefault(y => y < p.Scroll);
            p.Scroll = Math.Max(0, Math.Min(max, p.Scroll)); CloseAfter(popups.IndexOf(p)); p.Render();
        }
        public void Dispose()
        {
            if (closing) return; closing = true;
            if (Current == this) Current = null;
            Application.RemoveMessageFilter(this); timer.Stop(); timer.Dispose(); tooltip.Dispose(); ReleaseCapture(); CloseAfter(-1);
        }
        private static Rectangle WorkArea(Point point, out float scale)
        {
            IntPtr monitor = MonitorFromPoint(point, 2); var info = new MonitorInfo { Size = Marshal.SizeOf(typeof(MonitorInfo)) }; GetMonitorInfo(monitor, ref info);
            uint x, y; if (GetDpiForMonitor(monitor, 0, out x, out y) < 0 || x == 0) x = GetDpiForSystem();
            scale = x / 96f * (float)MenuPlatform.AccessibilityScale;
            Rectangle work = info.Work.ToRectangle(); return work.Contains(point) ? work : info.Monitor.ToRectangle();
        }
        internal sealed class DpiScope : IDisposable
        {
            private readonly IntPtr previous = SetThreadDpiAwarenessContext(new IntPtr(-4));
            public void Dispose() { SetThreadDpiAwarenessContext(previous); }
        }
        [StructLayout(LayoutKind.Sequential)] private struct Rect { internal int Left, Top, Right, Bottom; internal Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom); }
        [StructLayout(LayoutKind.Sequential)] private struct MonitorInfo { internal int Size; internal Rect Monitor, Work; internal uint Flags; }
        [DllImport("user32.dll")] private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
        [DllImport("user32.dll")] private static extern bool LogicalToPhysicalPointForPerMonitorDPI(IntPtr hwnd, ref Point point);
        [DllImport("user32.dll")] private static extern bool PhysicalToLogicalPointForPerMonitorDPI(IntPtr hwnd, ref Point point);
        [DllImport("user32.dll")] private static extern uint GetDpiForSystem();
        [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(Point point);
        [DllImport("user32.dll")] private static extern bool ScreenToClient(IntPtr hwnd, ref Point point);
        [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hwnd, int message, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll")] private static extern IntPtr MonitorFromPoint(Point point, uint flags);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);
        [DllImport("shcore.dll")] private static extern int GetDpiForMonitor(IntPtr monitor, int type, out uint x, out uint y);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern IntPtr SetCapture(IntPtr hwnd);
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern bool GetCursorPos(out Point point);
        [DllImport("user32.dll")] private static extern bool SystemParametersInfo(uint action, int param, ref int value, int flags);
    }
}
