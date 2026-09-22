using System;
using System.Drawing;
using System.Windows.Forms;
using Quartz.Services;

namespace Quartz
{
    public partial class Browser
    {
        private FlowLayoutPanel _zoomMenuRow;
        private Button _zoomOutButton;
        private Button _zoomResetButton;
        private Button _zoomInButton;

        private static readonly double[] ChromiumZoomFactors =
        {
            0.25,
            1.0 / 3.0,
            0.5,
            2.0 / 3.0,
            0.75,
            0.8,
            0.9,
            1.0,
            1.1,
            1.25,
            1.5,
            1.75,
            2.0,
            2.5,
            3.0,
            4.0,
            5.0
        };

        private void InitializeZoomMenuRow()
        {
            int rowHeight = zoomToolStrip.GetPreferredSize(Size.Empty).Height;

            _zoomMenuRow = new FlowLayoutPanel
            {
                Size = new Size(224, rowHeight),
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                WrapContents = false,
                Font = SettingsMenuStrip.Font
            };

            _zoomMenuRow.Controls.Add(new Label
            {
                Text = "Zoom",
                Size = new Size(57, rowHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            });

            _zoomOutButton = CreateZoomMenuButton(
                "−",
                "Zoom out (" + ShortcutManager.GetDisplayShortcut(BrowserCommand.ZoomOut) + ")",
                30);

            _zoomResetButton = CreateZoomMenuButton(
                "100%",
                "Reset zoom (" + ShortcutManager.GetDisplayShortcut(BrowserCommand.ResetZoom) + ")",
                55);

            _zoomInButton = CreateZoomMenuButton(
                "+",
                "Zoom in (" + ShortcutManager.GetDisplayShortcut(BrowserCommand.ZoomIn) + ")",
                30);

            var fullscreenButton = CreateZoomMenuButton(
                "",
                "Full screen (" + ShortcutManager.GetDisplayShortcut(BrowserCommand.Fullscreen) + ")",
                52);

            fullscreenButton.Paint += (sender, e) =>
            {
                int unit = Math.Max(
                    1,
                    fullscreenButton.DeviceDpi / 96);

                int x =
                    fullscreenButton.ClientSize.Width / 2 -
                    6 * unit;

                int y =
                    fullscreenButton.ClientSize.Height / 2 -
                    5 * unit;

                using (var pen = new Pen(
                    fullscreenButton.ForeColor,
                    unit))
                {
                    for (int corner = 0; corner < 4; corner++)
                    {
                        int dx =
                            corner % 2 == 0
                                ? 1
                                : -1;

                        int dy =
                            corner < 2
                                ? 1
                                : -1;

                        int cx =
                            x +
                            (dx == 1
                                ? 0
                                : 12 * unit);

                        int cy =
                            y +
                            (dy == 1
                                ? 0
                                : 10 * unit);

                        e.Graphics.DrawLine(
                            pen,
                            cx,
                            cy,
                            cx + dx * 4 * unit,
                            cy);

                        e.Graphics.DrawLine(
                            pen,
                            cx,
                            cy,
                            cx,
                            cy + dy * 4 * unit);
                    }
                }
            };

            _zoomOutButton.Click += (sender, e) =>
                ShortcutManager.ExecuteCommand(this, BrowserCommand.ZoomOut);

            _zoomInButton.Click += (sender, e) =>
                ShortcutManager.ExecuteCommand(this, BrowserCommand.ZoomIn);

            _zoomResetButton.Click += (sender, e) =>
                ShortcutManager.ExecuteCommand(this, BrowserCommand.ResetZoom);

            fullscreenButton.Click += (sender, e) =>
            {
                SettingsMenuStrip.Close();

                fullscreenToolStripMenuItem_Click(
                    sender,
                    e);
            };

            int index =
                SettingsMenuStrip.Items.IndexOf(
                    zoomToolStrip);

            SettingsMenuStrip.Items.Remove(
                zoomToolStrip);

            var zoomHost =
                new ZoomMenuControlHost(_zoomMenuRow)
                {
                    AutoSize = false,
                    Size = _zoomMenuRow.Size,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                };

            bool layingOutZoomRow = false;

            SettingsMenuStrip.LayoutCompleted +=
                (sender, e) =>
                {
                    if (layingOutZoomRow ||
                        _zoomMenuRow.IsDisposed)
                    {
                        return;
                    }

                    layingOutZoomRow = true;

                    SettingsMenuStrip.SuspendLayout();
                    _zoomMenuRow.SuspendLayout();

                    try
                    {
                        int rowWidth = Math.Max(
                            zoomHost.Width,
                            SettingsMenuStrip
                                .DisplayRectangle
                                .Right -
                            _zoomMenuRow.Left);

                        int buttonsWidth =
                            _zoomOutButton.Width +
                            _zoomResetButton.Width +
                            _zoomInButton.Width +
                            fullscreenButton.Width;

                        _zoomMenuRow.Width =
                            rowWidth;

                        _zoomMenuRow.Controls[0].Width =
                            Math.Max(
                                0,
                                rowWidth - buttonsWidth);
                    }
                    finally
                    {
                        _zoomMenuRow.ResumeLayout(true);

                        SettingsMenuStrip
                            .ResumeLayout(false);

                        layingOutZoomRow = false;
                    }
                };

            SettingsMenuStrip.Items.Insert(
                index,
                zoomHost);

            UpdateZoomMenuRow();
        }

        private Button CreateZoomMenuButton(
            string text,
            string description,
            int width)
        {
            var button = new ZoomMenuButton
            {
                Text = text,
                AccessibleName = description,

                Size = new Size(
                    width,
                    _zoomMenuRow.Height),

                Margin = Padding.Empty,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;

            if (text == "−" ||
                text == "+")
            {
                button.Text = "";

                button.Paint += (sender, e) =>
                {
                    int unit = Math.Max(
                        1,
                        button.DeviceDpi / 96);

                    int x =
                        button.ClientSize.Width / 2;

                    int y =
                        button.ClientSize.Height / 2;

                    using (var pen = new Pen(
                        button.Enabled
                            ? button.ForeColor
                            : SystemColors.GrayText,
                        unit))
                    {
                        e.Graphics.DrawLine(
                            pen,
                            x - 4 * unit,
                            y,
                            x + 4 * unit,
                            y);

                        if (text == "+")
                        {
                            e.Graphics.DrawLine(
                                pen,
                                x,
                                y - 4 * unit,
                                x,
                                y + 4 * unit);
                        }
                    }
                };
            }

            var tooltip =
                new ToolTip(components);

            tooltip.SetToolTip(
                button,
                description);

            _zoomMenuRow.Controls.Add(
                button);

            return button;
        }

        private void StepMenuZoom(
            int direction)
        {
            double currentZoom =
                wvWebView1.ZoomFactor;

            const double epsilon = 0.001;

            if (direction < 0)
            {
                for (
                    int i =
                        ChromiumZoomFactors.Length - 1;
                    i >= 0;
                    i--)
                {
                    double zoom =
                        ChromiumZoomFactors[i];

                    if (Math.Abs(
                        zoom - currentZoom) <= epsilon)
                    {
                        continue;
                    }

                    if (zoom < currentZoom)
                    {
                        SetMenuZoom(zoom);
                        return;
                    }
                }
            }
            else
            {
                for (
                    int i = 0;
                    i < ChromiumZoomFactors.Length;
                    i++)
                {
                    double zoom =
                        ChromiumZoomFactors[i];

                    if (Math.Abs(
                        zoom - currentZoom) <= epsilon)
                    {
                        continue;
                    }

                    if (zoom > currentZoom)
                    {
                        SetMenuZoom(zoom);
                        return;
                    }
                }
            }
        }

        private void SetMenuZoom(
            double zoom)
        {
            wvWebView1.ZoomFactor =
                Math.Max(
                    0.25,
                    Math.Min(
                        5,
                        zoom));

            SettingsService.Set(
                "Zoom",
                wvWebView1.ZoomFactor.ToString());

            UpdateZoomMenuRow();
        }

        private void UpdateZoomMenuRow()
        {
            if (_zoomMenuRow == null)
                return;

            _zoomMenuRow.BackColor =
                SettingsMenuStrip.BackColor;

            _zoomMenuRow.ForeColor =
                SettingsMenuStrip.ForeColor;

            foreach (
                Control control
                in _zoomMenuRow.Controls)
            {
                control.BackColor =
                    _zoomMenuRow.BackColor;

                control.ForeColor =
                    _zoomMenuRow.ForeColor;

                if (
                    control is Button button &&
                    SettingsMenuStrip.Renderer
                        is ToolStripProfessionalRenderer renderer)
                {
                    button.FlatAppearance.MouseOverBackColor =
                        renderer.ColorTable.MenuItemSelected;

                    button.FlatAppearance.MouseDownBackColor =
                        renderer.ColorTable.MenuItemSelected;
                }
            }

            double zoom =
                wvWebView1.ZoomFactor;

            _zoomResetButton.Text =
                Math.Round(
                    zoom * 100) +
                "%";

            _zoomOutButton.Enabled =
                zoom > ChromiumZoomFactors[0];

            _zoomInButton.Enabled =
                zoom <
                ChromiumZoomFactors[
                    ChromiumZoomFactors.Length - 1];
        }

        private sealed class ZoomMenuButton : Button
        {
            public ZoomMenuButton()
            {
                SetStyle(
                    ControlStyles.Selectable,
                    false);

                TabStop = false;
            }
        }

        private sealed class ZoomMenuControlHost
            : ToolStripControlHost
        {
            public ZoomMenuControlHost(
                Control control)
                : base(control)
            {
            }

            protected override void OnHostedControlResize(
                EventArgs e)
            {
                // Keep the host's layout size unchanged when
                // the visible row is widened to the right.
            }
        }
    }
}
