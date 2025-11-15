using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace Quartz.Services
{
    internal class NewControlThemeChanger
    {
        private static string ToBgr(Color c) => $"{c.B:X2}{c.G:X2}{c.R:X2}";
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        const int DWWMA_CAPTION_COLOR = 35;
        const int DWWMA_BORDER_COLOR = 34;
        const int DWMWA_TEXT_COLOR = 36;
        private static void CustomWindow(Color captionColor, Color fontColor, Color borderColor, IntPtr handle)
        {
            IntPtr hWnd = handle;
            //Change caption color
            int[] caption = new int[] { int.Parse(ToBgr(captionColor), System.Globalization.NumberStyles.HexNumber) };
            DwmSetWindowAttribute(hWnd, DWWMA_CAPTION_COLOR, caption, 4);
            //Change font color
            int[] font = new int[] { int.Parse(ToBgr(fontColor), System.Globalization.NumberStyles.HexNumber) };
            DwmSetWindowAttribute(hWnd, DWMWA_TEXT_COLOR, font, 4);
            //Change border color
            int[] border = new int[] { int.Parse(ToBgr(borderColor), System.Globalization.NumberStyles.HexNumber) };
            DwmSetWindowAttribute(hWnd, DWWMA_BORDER_COLOR, border, 4);
        }

        public static void ChangeWindowTheme(IntPtr handle)
        {
            var theme = SettingsService.Get("Theme");

            if (theme == "light")
            {
                var backcolor = Color.White;
                var forecolor = Color.Black;
                CustomWindow(backcolor, forecolor, Color.FromArgb(219, 220, 221), handle);
            }
            else if (theme == "black")
            {
                var backcolor = Color.Black;
                var forecolor = Color.White;

                CustomWindow(backcolor, forecolor, backcolor, handle);
            }
            else if (theme == "aqua")
            {
                var backcolor = Color.Aqua;
                var forecolor = Color.Blue;

                CustomWindow(backcolor, forecolor, backcolor, handle);
            }
            else if (theme == "xmas")
            {
                var backcolor = Color.Red;
                var forecolor = Color.Lime;

                CustomWindow(backcolor, forecolor, backcolor, handle);
            }
            else if (theme == "dark")
            {
                var backcolor = Color.FromArgb(35, 35, 35);
                var forecolor = Color.FromArgb(195, 195, 195);
                var windowOutline = System.Drawing.Color.FromArgb(88, 88, 88);
                CustomWindow(backcolor, forecolor, windowOutline, handle);
            }
        }
        public static void ChangeControlTheme(object _object)
        {
            var control = _object as Control;
            Color backcolor = Color.White;
            Color forecolor = Color.Black;
            Color extrabackcolor = Color.Blue;
            Color extraforecolor = Color.Black;
            Color mouseOver = Color.FromArgb(242, 242, 242);
            Image buttonimage = null;
            FlatStyle flatStyle = FlatStyle.Standard;
            FlatStyle checkboxflatstyle = FlatStyle.Standard;
            BorderStyle borderStyle = BorderStyle.Fixed3D;
            int buttonbordersize = 1;
            Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme coreWebView2ColorScheme = Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light;
            ToolStripProfessionalRenderer renderer = new ToolStripProfessionalRenderer();
            bool mnuShadow = false;
            

            var theme = SettingsService.Get("Theme");
            if (theme == "light")
            {
                backcolor = Color.White;
                forecolor = Color.Black;
                extrabackcolor = Color.White;
                extraforecolor = Color.Black;
                mouseOver = Color.FromArgb(242, 242, 242);
                flatStyle = FlatStyle.Flat;
                checkboxflatstyle = FlatStyle.Standard;
                borderStyle = BorderStyle.FixedSingle;
                buttonbordersize = 0;
                coreWebView2ColorScheme = Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light;
                renderer = new WhiteContextMenuRenderer();

            }
            else if (theme == "black")
            {
                backcolor = Color.Black;
                forecolor = Color.White;
                extrabackcolor = Color.Black;
                extraforecolor = Color.White;
                mouseOver = Color.FromArgb(18, 18, 18);
                flatStyle = FlatStyle.Popup;
                checkboxflatstyle = FlatStyle.Flat;
                borderStyle = BorderStyle.FixedSingle;
                buttonbordersize = 1;
                renderer = new BlackContextMenuRenderer();
                coreWebView2ColorScheme = Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark;
            }
            else if (theme == "aqua")
            {
                backcolor = Color.Aqua;
                forecolor = Color.Blue;
                extrabackcolor = Color.Blue;
                extraforecolor = Color.Aqua;
                mouseOver = Color.FromArgb(0, 238, 238);
                flatStyle = FlatStyle.Popup;
                checkboxflatstyle = FlatStyle.Flat;
                borderStyle = BorderStyle.FixedSingle;
                buttonbordersize = 1;
                coreWebView2ColorScheme = Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Auto;
                renderer = new AquaContextMenuRenderer();
            }
            else if (theme == "xmas")
            {
                backcolor = Color.Red;
                forecolor = Color.Lime;
                extrabackcolor = Color.Red;
                extraforecolor = Color.Lime;
                mouseOver = Color.FromArgb(255, 50, 50);
                flatStyle = FlatStyle.Flat;
                checkboxflatstyle = FlatStyle.Flat;
                borderStyle = BorderStyle.FixedSingle;
                buttonbordersize = 0;
                coreWebView2ColorScheme = Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Auto;
                renderer = new XmasContextMenuRenderer();
            }
            else if (theme == "dark")
            {
                backcolor = Color.FromArgb(35, 35, 35);
                forecolor = Color.FromArgb(195, 195, 195);
                extrabackcolor = Color.FromArgb(88, 88, 88);
                extraforecolor = Color.FromArgb(195, 195, 195);
                mouseOver = Color.FromArgb(64, 64, 64);
                flatStyle = FlatStyle.Popup;
                checkboxflatstyle = FlatStyle.Flat;
                borderStyle = BorderStyle.FixedSingle;
                buttonbordersize = 1;
                coreWebView2ColorScheme = Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Dark;
                renderer = new DarkContextMenuRenderer();
            }

            control.BackColor = backcolor;
            control.ForeColor = forecolor;

            if (control is Button)
            {
                var button = (Button)control;
                button.FlatStyle = flatStyle;

                if (buttonimage != null)
                {
                    button.Image = buttonimage;
                }

                button.BackColor = extrabackcolor;
                button.ForeColor = extraforecolor;
                button.FlatAppearance.BorderSize = buttonbordersize;
            }
            else if (control is ContextMenuStrip)
            {
                var ContextMenuStrip = (ContextMenuStrip)control;
                if (theme == "dark")
                {
                    ContextMenuStrip.BackColor = backcolor;
                    ContextMenuStrip.ForeColor = forecolor;
                }
                else if (theme == "aqua")
                {
                    ContextMenuStrip.BackColor = extrabackcolor;
                    ContextMenuStrip.ForeColor = extraforecolor;
                }
                else if (theme == "xmas")
                {
                    ContextMenuStrip.BackColor = extrabackcolor;
                    ContextMenuStrip.ForeColor = extraforecolor;
                }
                ContextMenuStrip.Renderer = renderer;
                ContextMenuStrip.BackgroundImage = buttonimage;    
                ContextMenuStrip.DropShadowEnabled = true;
            }
            else if (control is TextBox)
            {
                var textbox = (TextBox)control;
                textbox.BackColor = extrabackcolor;
                textbox.ForeColor = extraforecolor;
                textbox.BorderStyle = borderStyle;
            }
            else if (control is CheckBox)
            {
                var checkbox = (CheckBox)control;
                checkbox.FlatStyle = checkboxflatstyle;
            }
            else if (control is RadioButton)
            {
                var radiobutton = (RadioButton)control;
                radiobutton.FlatStyle = checkboxflatstyle;
            }
            else if (control is ComboBox)
            {
                var combobox = (ComboBox)control;
                combobox.FlatStyle = checkboxflatstyle;
                combobox.BackColor = extrabackcolor;
                combobox.ForeColor = extraforecolor;
            }
            else if (control is WebView2)
            {
                var webview = (WebView2)control;
                webview.DefaultBackgroundColor = backcolor;
                webview.CoreWebView2.Profile.PreferredColorScheme = coreWebView2ColorScheme;
            }
            else if (control is NumericUpDown)
            {
                var numericupdown = (NumericUpDown)control;
                numericupdown.BorderStyle = borderStyle;
                numericupdown.BackColor = extrabackcolor;
                numericupdown.ForeColor = extraforecolor;
            }
            else if (control is DataGridView)
            {
                var dataGridView = (DataGridView)control;

                //values that dont change
                dataGridView.EnableHeadersVisualStyles = false;
                dataGridView.BorderStyle = BorderStyle.None;
                dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
                dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                dataGridView.RowHeadersVisible = false;
                dataGridView.ColumnHeadersVisible = false;

                //values that changes
                dataGridView.BackgroundColor = backcolor;
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = backcolor;
                dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = forecolor;


                dataGridView.RowsDefaultCellStyle.BackColor = backcolor;
                dataGridView.RowsDefaultCellStyle.ForeColor = forecolor;

                dataGridView.DefaultCellStyle.SelectionBackColor = mouseOver;
                dataGridView.DefaultCellStyle.SelectionForeColor = forecolor;
            }
            else if (control is Button)
            {
                var button = (Button)control;

                button.FlatAppearance.MouseOverBackColor = mouseOver;
                button.FlatAppearance.MouseDownBackColor = mouseOver;
                button.FlatAppearance.BorderColor = forecolor;
            }
            else if (control is ToolStrip toolStrip)
            {
                // Handle ToolStripComboBox inside ToolStrip
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    if (item is ToolStripComboBox toolStripComboBox)
                    {
                        // Apply theme to ToolStripComboBox
                        toolStripComboBox.FlatStyle = checkboxflatstyle;
                        toolStripComboBox.BackColor = extrabackcolor;
                        toolStripComboBox.ForeColor = extraforecolor;
                    }
                }
            }
            else if (control is ListView listBox)
            {
                listBox.BackColor= backcolor;
                listBox.ForeColor= forecolor;
                listBox.BorderStyle = BorderStyle.None;
            }
        }

        public static void ChangeTheme(Control form)
        {
            ChangeWindowTheme(form.Handle);

            // Call the method to get all controls
            List<object> allControls = GetAllControls(form);

            // Display the names of all controls
            foreach (var ctrl in allControls)
            {
                var control = ctrl as Control;

                ChangeControlTheme(control); ChangeControlTheme(control);

            }
        }

        // Recursive method to get all controls and context menus
        public static List<object> GetAllControls(Control parent)
        {
            List<object> allElements = new List<object>();

            // Add the parent control itself
            allElements.Add(parent);

            // Check for context menus (both ContextMenuStrip and legacy ContextMenu)
            if (parent is Control ctrl)
            {
                // Add the ContextMenuStrip if set
                if (ctrl.ContextMenuStrip != null)
                {
                    allElements.Add(ctrl.ContextMenuStrip);
                }
                // Add the legacy ContextMenu if set
                else if (ctrl.ContextMenu != null)
                {
                    allElements.Add(ctrl.ContextMenu);
                }
            }

            // Recursively add child controls
            foreach (Control childControl in parent.Controls)
            {
                allElements.AddRange(GetAllControls(childControl));
            }

            return allElements;
        }
    }
}
