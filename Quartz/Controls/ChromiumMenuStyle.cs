using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Quartz.Controls
{
    // Windows Views menu metrics, pinned in chromeium/context_menu/README.md.
    // Keep ToolStrip's command routing, accessibility, scrolling and menu loop.
    public static class ChromiumMenuStyle
    {
        private static readonly ConditionalWeakTable<ToolStripDropDown, MenuState> States =
            new ConditionalWeakTable<ToolStripDropDown, MenuState>();

        public static void Apply(ToolStripDropDown menu)
        {
            if (menu == null || menu.IsDisposed) return;
            States.GetValue(menu, key => new MenuState(key)).Prepare();
        }

        internal static MenuState GetState(ToolStripDropDown menu)
        {
            return States.GetValue(menu, key => new MenuState(key));
        }

        internal static int Scale(Control control, int dip)
        {
            return (int)Math.Round(dip * control.DeviceDpi / 96.0);
        }

        internal static string Shortcut(ToolStripMenuItem item)
        {
            if (!item.ShowShortcutKeys) return "";
            if (!string.IsNullOrEmpty(item.ShortcutKeyDisplayString)) return item.ShortcutKeyDisplayString;
            if (item.ShortcutKeys == Keys.None) return "";
            return new KeysConverter().ConvertToString(item.ShortcutKeys);
        }

        internal static string Label(ToolStripItem item)
        {
            return (item.Text ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        }

        internal sealed class MenuState
        {
            internal readonly ToolStripDropDown Menu;
            internal ChromiumMenuPalette Palette;
            internal int LabelStart;
            internal int IconColumns;
            private bool preparing;
            private ChromiumMenuWindow surface;

            internal MenuState(ToolStripDropDown menu)
            {
                Menu = menu;
                menu.Opening += Opening;
                menu.Opened += Opened;
                menu.Closed += Closed;
                menu.Layout += Layout;
                menu.LocationChanged += LocationChanged;
                menu.VisibleChanged += (sender, e) => { if (!menu.Visible) surface?.Hide(); };
                menu.Disposed += Disposed;
                menu.ItemAdded += ItemAdded;
                menu.ItemRemoved += (sender, e) => { if (menu.Visible) Prepare(); };
            }

            private void Opening(object sender, CancelEventArgs e) { Prepare(); }
            private void Opened(object sender, EventArgs e)
            {
                Prepare();
                if (surface == null) surface = new ChromiumMenuWindow(Menu);
                surface.Update(Palette.Background);
            }
            private void Closed(object sender, ToolStripDropDownClosedEventArgs e) { surface?.Hide(); }
            private void LocationChanged(object sender, EventArgs e)
            {
                if (Menu.Visible && !preparing) surface?.Update(Palette.Background);
            }
            private void Layout(object sender, LayoutEventArgs e)
            {
                // ToolStripDropDownMenu recalculates and overwrites Padding before
                // raising Layout. Restore it here, before its layout engine runs.
                // Do not start a second layout (which would reset scroll position).
                var padding = new Padding(0, Scale(Menu, 12), 0, Scale(Menu, 12));
                if (Menu.Padding != padding)
                {
                    Menu.SuspendLayout();
                    Menu.Padding = padding;
                    Menu.ResumeLayout(false);
                }
            }
            private void ItemAdded(object sender, ToolStripItemEventArgs e)
            {
                if (Menu.Visible) Prepare();
            }
            private void Disposed(object sender, EventArgs e) { surface?.Dispose(); }

            internal void Prepare()
            {
                if (preparing || Menu.IsDisposed) return;
                preparing = true;
                Menu.SuspendLayout();
                try
                {
                    Palette = ChromiumMenuPalette.Current;
                    Menu.Renderer = ChromiumMenuRenderer.Instance;
                    Menu.BackColor = Palette.Background;
                    Menu.ForeColor = Palette.Foreground;
                    Menu.Font = SystemFonts.MenuFont;
                    Menu.DropShadowEnabled = false; // The Chromium shadow is drawn by the popup surface.
                    Menu.AutoSize = false;
                    Menu.Padding = new Padding(0, Scale(Menu, 12), 0, Scale(Menu, 12));
                    Menu.ImageScalingSize = new Size(Scale(Menu, 16), Scale(Menu, 16));
                    if (Menu is ToolStripDropDownMenu dropDown)
                    {
                        dropDown.ShowImageMargin = false;
                        dropDown.ShowCheckMargin = false;
                    }

                    var items = Menu.Items.Cast<ToolStripItem>().Where(item => item.Available).ToArray();
                    var commands = items.OfType<ToolStripMenuItem>().ToArray();
                    bool check = commands.Any(item => item.CheckOnClick || item.Checked);
                    bool icon = commands.Any(item => item.Image != null);
                    bool combined = commands.Any(item => item.Image != null && (item.CheckOnClick || item.Checked));
                    IconColumns = combined ? 2 : check || icon ? 1 : 0;
                    LabelStart = Scale(Menu, 20 + 24 * IconColumns);

                    int labelWidth = 0, minorWidth = 0, height = Menu.Padding.Vertical;
                    using (var graphics = Menu.CreateGraphics())
                    {
                        foreach (var item in commands)
                        {
                            item.Font = Menu.Font;
                            labelWidth = Math.Max(labelWidth, Measure(graphics, Label(item), Menu.Font));
                            int minor = Measure(graphics, Shortcut(item), Menu.Font);
                            if (item.HasDropDownItems) minor += Scale(Menu, 24);
                            minorWidth = Math.Max(minorWidth, minor);
                            if (item.HasDropDownItems) Apply(item.DropDown);
                        }
                    }
                    int width = LabelStart + labelWidth + Scale(Menu, 20) +
                        (minorWidth > 0 ? minorWidth + Scale(Menu, 8) : 0);
                    foreach (var host in items.OfType<ToolStripControlHost>())
                    {
                        host.Margin = new Padding(Scale(Menu, 20), Scale(Menu, 6), Scale(Menu, 20), Scale(Menu, 6));
                        host.BackColor = Palette.Background;
                        host.ForeColor = Palette.Foreground;
                        width = Math.Max(width, host.Width + host.Margin.Horizontal);
                    }
                    var root = Menu;
                    while (root.OwnerItem?.Owner is ToolStripDropDown parent) root = parent;
                    var source = (root as ContextMenuStrip)?.SourceControl;
                    Rectangle workArea = source != null ? Screen.FromControl(source).WorkingArea :
                        Screen.FromPoint(Menu.Visible ? Menu.Location : Cursor.Position).WorkingArea;
                    width = Math.Max(Scale(Menu, 80), Math.Min(width, workArea.Width));
                    int rowHeight = Math.Max(Menu.Font.Height, Scale(Menu, IconColumns == 0 ? 0 : 16)) + Scale(Menu, 12);
                    foreach (var item in items)
                    {
                        if (item is ToolStripControlHost)
                        {
                            height += item.Height + item.Margin.Vertical;
                            continue;
                        }
                        item.AutoSize = false;
                        item.Margin = Padding.Empty;
                        item.Padding = Padding.Empty;
                        item.Size = new Size(width, item is ToolStripSeparator ? Scale(Menu, 17) : rowHeight);
                        height += item.Height;
                    }
                    Menu.Size = new Size(width, Math.Min(height, workArea.Height));
                }
                finally
                {
                    Menu.ResumeLayout(true);
                    preparing = false;
                }
                if (Menu.Visible) surface?.Update(Palette.Background);
            }

            private static int Measure(Graphics graphics, string text, Font font)
            {
                if (string.IsNullOrEmpty(text)) return 0;
                return TextRenderer.MeasureText(graphics, text, font, Size.Empty,
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
            }
        }
    }

    internal sealed class ChromiumMenuPalette
    {
        internal Color Background, Foreground, Secondary, Disabled, Hover, Separator;
        internal static ChromiumMenuPalette Current
        {
            get
            {
                if (SystemInformation.HighContrast)
                    return new ChromiumMenuPalette { Background = SystemColors.Menu, Foreground = SystemColors.MenuText,
                        Secondary = SystemColors.MenuText, Disabled = SystemColors.GrayText,
                        Hover = SystemColors.Highlight, Separator = SystemColors.GrayText };
                return new ChromiumMenuPalette {
                    Background = Color.White,
                    Foreground = Color.FromArgb(31, 31, 31),
                    Secondary = Color.FromArgb(71, 71, 71),
                    Disabled = Color.FromArgb(128, 128, 128),
                    Hover = Color.FromArgb(242, 242, 242),
                    Separator = Color.FromArgb(211, 227, 253)
                };
            }
        }
    }
}
