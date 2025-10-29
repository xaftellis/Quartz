using Quartz.Libs;
using Quartz.Omnibox;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;


namespace Quartz.Omnibox
{
    public class Theme
    {
        private RichTextBox omniBox;

        public Theme(RichTextBox Omnibox)
        {
            omniBox = Omnibox;
        }

        public void ThemeAddress()
        {
            string input = omniBox.Text;
            if (string.IsNullOrWhiteSpace(input))
                return;

            int cursorPos = omniBox.SelectionStart;
            omniBox.Clear();

            string scheme = "";
            string userInfo = "";
            string host = "";
            string port = "";
            string path = "";
            string query = "";
            string fragment = "";

            // Try to split scheme
            int schemeIndex = input.IndexOf("://");
            if (schemeIndex >= 0)
            {
                scheme = input.Substring(0, schemeIndex + 3);
                input = input.Substring(schemeIndex + 3);
            }

            // Try to split user info
            int atIndex = input.IndexOf("@");
            if (atIndex >= 0)
            {
                userInfo = input.Substring(0, atIndex + 1);
                input = input.Substring(atIndex + 1);
            }

            // Fragment
            int fragIndex = input.IndexOf("#");
            if (fragIndex >= 0)
            {
                fragment = input.Substring(fragIndex);
                input = input.Substring(0, fragIndex);
            }

            // Query
            int queryIndex = input.IndexOf("?");
            if (queryIndex >= 0)
            {
                query = input.Substring(queryIndex);
                input = input.Substring(0, queryIndex);
            }

            // Port
            int portIndex = input.IndexOf(":");
            if (portIndex >= 0)
            {
                port = input.Substring(portIndex);
                host = input.Substring(0, portIndex);
            }
            else
            {
                host = input;
            }

            // Append all parts
            AppendColored(scheme, GetSecondaryURLThemeColor());
            AppendColored(userInfo, GetSecondaryURLThemeColor());
            AppendColored(host, GetMainURLThemeColor());
            AppendColored(port, GetSecondaryURLThemeColor());
            AppendColored(path, GetSecondaryURLThemeColor());
            AppendColored(query, GetSecondaryURLThemeColor());
            AppendColored(fragment, GetSecondaryURLThemeColor());

            // Restore cursor
            omniBox.SelectionStart = cursorPos;
            omniBox.SelectionLength = 0;
        }

        private void AppendColored(string text, Color color)
        {
            if (string.IsNullOrEmpty(text)) return;

            omniBox.SelectionStart = omniBox.TextLength;
            omniBox.SelectionLength = 0;
            omniBox.SelectionColor = color;
            omniBox.AppendText(text);
            omniBox.SelectionColor = omniBox.ForeColor;
        }

        private Color GetMainURLThemeColor()
        {
            string theme = SettingsService.Get("Theme");
            if (theme == "light") return Color.Black;
            else if (theme == "dark") return Color.FromArgb(195, 195, 195);
            else if (theme == "black") return Color.Black;
            else if (theme == "aqua") return Color.Aqua;
            else if (theme == "xmas") return Color.Red;
            else return Color.Black;
        }

        private Color GetSecondaryURLThemeColor()
        {
            string theme = SettingsService.Get("Theme");
            if (theme == "light") return Color.Gray;
            else if (theme == "dark") return Color.FromArgb(148, 148, 148);
            else if (theme == "black") return Color.Gray;
            else if (theme == "aqua") return Color.DeepSkyBlue;
            else if (theme == "xmas") return Color.Red;
            else return Color.Black;
        }
    }
}
