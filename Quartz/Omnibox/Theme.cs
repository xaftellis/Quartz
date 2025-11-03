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
        string input;

        public Theme(RichTextBox Omnibox)
        {
            omniBox = Omnibox;
        }

        private Color MainColor()
        {
            string theme = SettingsService.Get("Theme");
            if (theme == "light") return Color.Black;
            else if (theme == "dark") return Color.FromArgb(195, 195, 195);
            else if (theme == "black") return Color.Black;
            else if (theme == "aqua") return Color.Aqua;
            else if (theme == "xmas") return Color.Red;
            else return Color.Black;
        }

        private Color SecondaryColor()
        {
            string theme = SettingsService.Get("Theme");
            if (theme == "light") return Color.Gray;
            else if (theme == "dark") return Color.FromArgb(148, 148, 148);
            else if (theme == "black") return Color.Gray;
            else if (theme == "aqua") return Color.DeepSkyBlue;
            else if (theme == "xmas") return Color.Red;
            else return Color.Black;
        }

public void RenderText()
{
    input = omniBox.Text;

    if (string.IsNullOrWhiteSpace(input))
        return;

    int caretPos = omniBox.SelectionStart; // store current caret position
    string text = input.Trim();

    omniBox.Clear();

    // If it's not a URL, treat everything as main color
    if (!OmniBoxHelper.IsProbablyUrl(text))
    {
        omniBox.SelectionColor = MainColor();
        omniBox.AppendText(text);

        // Restore caret
        omniBox.SelectionStart = caretPos;
        omniBox.SelectionLength = 0;
        return;
    }

    // It's a URL, try to detect host
    string host = GetHostPart(text);
    if (string.IsNullOrEmpty(host))
    {
        omniBox.SelectionColor = MainColor();
        omniBox.AppendText(text);

        omniBox.SelectionStart = caretPos;
        omniBox.SelectionLength = 0;
        return;
    }

    int hostIndex = text.IndexOf(host, StringComparison.OrdinalIgnoreCase);
    if (hostIndex < 0) hostIndex = 0;

    // 1. Render everything before host as secondary
    if (hostIndex > 0)
    {
        omniBox.SelectionColor = SecondaryColor();
        omniBox.AppendText(text.Substring(0, hostIndex));
    }

    // 2. Render host as main
    omniBox.SelectionColor = MainColor();
    omniBox.AppendText(host);

    // 3. Render everything after host as secondary
    int afterHostIndex = hostIndex + host.Length;
    if (afterHostIndex < text.Length)
    {
        omniBox.SelectionColor = SecondaryColor();
        omniBox.AppendText(text.Substring(afterHostIndex));
    }

    // Restore caret position safely
    omniBox.SelectionStart = Math.Min(caretPos, omniBox.Text.Length);
    omniBox.SelectionLength = 0;
}


        /// <summary>
        /// Extracts host from URL text, even if incomplete
        /// </summary>
        private string GetHostPart(string input)
        {
            try
            {
                Uri uri;
                if (Uri.TryCreate(input, UriKind.Absolute, out uri))
                {
                    return uri.Host; // full URL
                }

                // Prepend scheme if missing
                if (!input.Contains("://"))
                {
                    if (Uri.TryCreate("http://" + input, UriKind.Absolute, out uri))
                        return uri.Host;
                }

                // Fallback: take until first slash
                int slash = input.IndexOf('/');
                if (slash >= 0)
                    return input.Substring(0, slash);
                return input;
            }
            catch
            {
                return null;
            }
        }

    }
}
