using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Quartz.Controls.ChromiumMenus
{
    // Independent command model. No native menu or ToolStrip is created, even off screen.
    public class ChromiumMenu : Component, EasyTabs.IContextMenuPresenter
    {
        public ChromiumMenu() { Items = new MenuItems(this); }
        public ChromiumMenu(IContainer container) : this() { container.Add(this); }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public MenuItems Items { get; }
        public string Name { get; set; }
        public object Tag { get; set; }
        public Size Size { get; set; } // Resource compatibility; preferred size is always computed.
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control SourceControl { get; internal set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChromiumMenuItem OwnerItem { get; internal set; }
        public bool Enabled { get; set; } = true;
        public bool IsDisposed { get; private set; }
        public bool Visible => MenuSession.Current?.Contains(this) == true;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MenuAppearance Appearance { get; set; }
        public event CancelEventHandler Opening;
        public event CancelEventHandler Closing;
        public event EventHandler Opened;
        public event EventHandler Closed;
        public void SuspendLayout() { }
        public void ResumeLayout(bool performLayout = true) { }
        public void PerformLayout() { Invalidate(); }
        public void Invalidate() { MenuSession.Current?.Refresh(this); }
        internal bool Prepare(Control source)
        {
            SourceControl = source;
            var e = new CancelEventArgs(); Opening?.Invoke(this, e);
            return Enabled && !e.Cancel && Items.Any(i => i.Visible);
        }
        internal void DidOpen() { Opened?.Invoke(this, EventArgs.Empty); }
        internal bool WillClose()
        {
            var e = new CancelEventArgs(); Closing?.Invoke(this, e); return !e.Cancel;
        }
        internal void DidClose() { Closed?.Invoke(this, EventArgs.Empty); }
        public void Show(Control source, Point location) { MenuSession.Open(this, source, source.PointToScreen(location)); }
        public void Show(Point screenPoint) { MenuSession.Open(this, SourceControl ?? Form.ActiveForm, screenPoint); }
        public void Show(int x, int y) { Show(new Point(x, y)); }
        public void Close() { MenuSession.Current?.Close(this); }
        public void Attach(Control control)
        {
            // Only Quartz-owned controls are attached. Never attach the WebView control.
            var hook = new ContextHook(this, control);
            control.Disposed += (s, e) => hook.ReleaseHandle();
            if (control is ChromiumButton button)
            {
                EventHandler changed = (s, e) => button.SetOwnedMenuActive(!IsDisposed && Visible && SourceControl == button);
                Opened += changed; Closed += changed; Disposed += changed;
                control.Disposed += (s, e) => { Opened -= changed; Closed -= changed; Disposed -= changed; };
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                MenuSession.Current?.Close(this, true);
                IsDisposed = true;
                foreach (var item in Items.ToArray()) item.Dispose();
            }
            base.Dispose(disposing);
        }
        private sealed class ContextHook : NativeWindow
        {
            private readonly ChromiumMenu menu;
            private readonly Control control;
            public ContextHook(ChromiumMenu menu, Control control)
            {
                this.menu = menu; this.control = control;
                if (control.IsHandleCreated) AssignHandle(control.Handle);
                control.HandleCreated += (s, e) => AssignHandle(control.Handle);
                control.HandleDestroyed += (s, e) => ReleaseHandle();
            }
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x007B && !menu.IsDisposed)
                {
                    var p = new Point(unchecked((short)(long)m.LParam), unchecked((short)((long)m.LParam >> 16)));
                    if (p.X == -1 && p.Y == -1) p = control.PointToScreen(new Point(control.Width / 2, control.Height / 2));
                    // Finish the source control's right-button processing before
                    // acquiring capture; native Button/MonthCalendar may release
                    // their capture after sending WM_CONTEXTMENU.
                    control.BeginInvoke((Action)(() => { if (!control.IsDisposed && !menu.IsDisposed) MenuSession.Open(menu, control, p); }));
                    return;
                }
                base.WndProc(ref m);
            }
        }
    }

    public sealed class MenuItems : Collection<ChromiumMenuItem>
    {
        private readonly ChromiumMenu owner;
        internal MenuItems(ChromiumMenu owner) { this.owner = owner; }
        protected override void InsertItem(int index, ChromiumMenuItem item) { item.Owner = owner; base.InsertItem(index, item); }
        protected override void SetItem(int index, ChromiumMenuItem item) { item.Owner = owner; base.SetItem(index, item); }
        public void AddRange(ChromiumMenuItem[] items) { foreach (var item in items) Add(item); }
        public ChromiumMenuItem Add(string text) { var item = new ChromiumMenuItem(text); Add(item); return item; }
    }

    public class ChromiumMenuItem : Component
    {
        private ChromiumMenu dropdown;
        private bool isChecked;
        public ChromiumMenuItem() { }
        public ChromiumMenuItem(string text) { Text = text; }
        public ChromiumMenuItem(string text, Image image, EventHandler click) : this(text) { Image = image; Click += click; }
        public string Text { get; set; } = "";
        public string SecondaryText { get; set; }
        public string Name { get; set; }
        public object Tag { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Visible { get; set; } = true;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Available { get => Visible; set => Visible = value; }
        [DefaultValue(false)]
        public bool CheckOnClick { get; set; }
        [DefaultValue(false)]
        public bool Radio { get; set; }
        [DefaultValue(false)]
        public bool IsCheck { get; set; }
        [DefaultValue(false)]
        public bool Checked { get => isChecked; set { IsCheck = true; if (isChecked == value) return; isChecked = value; CheckedChanged?.Invoke(this, EventArgs.Empty); } }
        public Image Image { get; set; }
        public Size ImageSize { get; set; } = new Size(16, 16);
        public string VectorIcon { get; set; }
        public Size Size { get; set; }
        public string ToolTipText { get; set; }
        public Keys ShortcutKeys { get; set; }
        public string ShortcutKeyDisplayString { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChromiumMenu Owner { get; internal set; }
        public ChromiumMenu DropDown
        {
            get => dropdown;
            set { dropdown = value; if (value != null) value.OwnerItem = this; }
        }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MenuItems DropDownItems
        {
            get { if (dropdown == null) DropDown = new ChromiumMenu(); return dropdown.Items; }
        }
        public bool HasDropDownItems => dropdown != null && dropdown.Items.Count > 0;
        internal bool HasSubmenu => dropdown != null && (HasDropDownItems || dropdown.Name != null);
        internal bool HasClick => Click != null;
        public event EventHandler Click;
        public event MouseEventHandler MouseUp;
        public event EventHandler CheckedChanged;
        public event EventHandler DropDownOpening;
        internal void PrepareDropDown() { DropDownOpening?.Invoke(this, EventArgs.Empty); }
        public void PerformClick()
        {
            if (!Enabled) return;
            if (CheckOnClick) Checked = !Checked;
            Click?.Invoke(this, EventArgs.Empty);
        }
        internal void RaiseMouseUp(MouseButtons button) { MouseUp?.Invoke(this, new MouseEventArgs(button, 1, 0, 0, 0)); }
        public void Invalidate() { Owner?.Invalidate(); }
        protected override void Dispose(bool disposing) { if (disposing) Image = null; base.Dispose(disposing); }
    }

    public enum MenuSeparatorKind { Normal, Upper, Lower, Double, Spacing, Padded }
    public sealed class ChromiumMenuSeparator : ChromiumMenuItem
    {
        public MenuSeparatorKind Kind { get; set; }
    }
    public sealed class ChromiumZoomMenuItem : ChromiumMenuItem
    {
        public ChromiumZoomMenuItem() : base("Zoom") { }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<double> GetZoom { get; set; } = () => 1;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<bool> HasContents { get; set; } = () => true;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<bool> CanZoom { get; set; } = () => true;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<bool> CanFullscreen { get; set; } = () => true;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<bool> IsFullscreen { get; set; } = () => false;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Action<int> Step { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Action Fullscreen { get; set; }
        public int Percent => (int)(GetZoom() * 100 + .5);
        internal bool ButtonEnabled(int part) => Enabled && (part == 2 ? CanFullscreen() || IsFullscreen() : CanZoom() && (part == 0 ? Percent > 25 : Percent < 500));
        public static readonly double[] Factors = { .25, 1.0 / 3, .5, 2.0 / 3, .75, .8, .9, 1, 1.1, 1.25, 1.5, 1.75, 2, 2.5, 3, 4, 5 };
        public static double NextFactor(double current, int direction, double defaultFactor = 1)
        {
            double level = Math.Log(current) / Math.Log(1.2);
            double defaultLevel = Math.Log(defaultFactor) / Math.Log(1.2);
            var presets = Factors.AsEnumerable();
            if (defaultFactor > .25 && defaultFactor < 5 && !Factors.Any(f => Math.Abs(Math.Log(f) / Math.Log(1.2) - defaultLevel) <= .001))
                presets = presets.Concat(new[] { defaultFactor }).OrderBy(f => f);
            var factors = direction < 0 ? presets.Reverse() : presets;
            return factors.FirstOrDefault(f => direction * (Math.Log(f) / Math.Log(1.2) - level) > .001) is double next && next > 0 ? next : current;
        }
    }
}
