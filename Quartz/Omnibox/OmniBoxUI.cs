using Newtonsoft.Json;
using Quartz.Libs;
using Quartz.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Management;
using System.Windows.Forms;

namespace Quartz.Omnibox
{
    internal class OmniBoxThemeUI
    {
        private Form form;
        private RichTextBox omniBox;
        private ListView lvSuggestions;
        private string query;

        private ImageList iconList;
        private List<string> searchSuggestions;
        private List<string> historySuggestions;
        private HttpClient httpClient;
        private bool isCommitting = true;
        private int selectedIndex;
        private string originalText;

        public OmniBoxThemeUI(Form Form, RichTextBox Omnibox, ListView ListView)
        {
            form = Form;
            omniBox = Omnibox;
            lvSuggestions = ListView;

            omniBox.KeyDown += OmniBox_KeyDown;
            omniBox.SizeChanged += OmniBox_SizeChanged;
            omniBox.GotFocus += OmniBox_GotFocus;
            omniBox.LostFocus += OmniBox_LostFocus;

            lvSuggestions.OwnerDraw = true;
            lvSuggestions.DrawItem += LvSuggestions_DrawItem;

            httpClient = new HttpClient();

            iconList = new ImageList
            {
                ImageSize = new Size(16, 16),
                ColorDepth = ColorDepth.Depth32Bit
            };
            lvSuggestions.SmallImageList = iconList;

        }

        private void LvSuggestions_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var item = e.Item;
            Image icon = item.ImageList?.Images[item.ImageIndex];
            int iconSize = item.ImageList?.ImageSize.Width ?? 16;
            int iconY = e.Bounds.Top + (e.Bounds.Height - iconSize) / 2;

            // Draw full row background if selected
            if (item.Selected)
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            else
                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);

            // Draw icon without changing its color
            if (icon != null)
                e.Graphics.DrawImage(icon, e.Bounds.Left, iconY, iconSize, iconSize);

            // Draw text using ListView's font (or a new font if you want)
            Font textFont = lvSuggestions.Font; // or new Font("Segoe UI", 9F) for default
            Color textColor = item.Selected ? SystemColors.HighlightText : item.ForeColor;

            TextRenderer.DrawText(
                e.Graphics,
                item.Text,
                textFont,
                new Rectangle(e.Bounds.Left + iconSize + 2, e.Bounds.Top, e.Bounds.Width - iconSize, e.Bounds.Height),
                textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );

            // Draw focus rectangle if needed
            if (item.Selected && lvSuggestions.Focused)
                e.DrawFocusRectangle();
        }

        private void OmniBox_LostFocus(object sender, EventArgs e)
        {
            lvSuggestions.Visible = false;
        }

        private void OmniBox_GotFocus(object sender, EventArgs e)
        {
            ShowSuggestionsAsync();
        }

        public async Task ShowSuggestionsAsync()
        {
            query = omniBox.Text;

            if (!omniBox.Focused || string.IsNullOrWhiteSpace(query))
            {
                lvSuggestions.Visible = false;
                return;
            }

            if (!isCommitting)
            {
                isCommitting = true;
                return;
            }

            lvSuggestions.BeginUpdate();

            lvSuggestions.Items.Clear();

            string _key = query;
            Image _icon;

            // Decide if this is a URL or not
            bool isUrl = QueryAnalyzer.IsProbablyUrl(query);

            // Get icon
            if (!isUrl)
            {
                _icon = GetSearchIcon();
                if (_icon != null && !iconList.Images.ContainsKey(_key))
                    iconList.Images.Add(_key, _icon);

                lvSuggestions.Items.Add(query.Trim() + " - Google Search", _key);
            }
            else
            {
                _icon = FaviconHelper.GetDefaultFaviconAsImage16();
                if (_icon != null && !iconList.Images.ContainsKey(_key))
                    iconList.Images.Add(_key, _icon);

                lvSuggestions.Items.Add(query.Trim() + " - Google Search", _key);
            }

            // Preserve selection
            lvSuggestions.Items[0].Selected = true;
            lvSuggestions.Items[0].Focused = true;

            lvSuggestions.EndUpdate();


            searchSuggestions = await GetSearchSuggestionsAsync();
            if (searchSuggestions == null)
                return;

            if (searchSuggestions.Any())
            {
                foreach (string s in searchSuggestions)
                {
                    string key = s.GetHashCode().ToString();

                    Image icon = GetSearchIcon();
                    if (icon != null && !iconList.Images.ContainsKey(key))
                        iconList.Images.Add(key, icon);

                    var item = new ListViewItem(s)
                    {
                        ImageKey = iconList.Images.ContainsKey(key) ? key : null
                    };
                    lvSuggestions.Items.Add(item);
                }
            }

            lvSuggestions.Visible = true;
            lvSuggestions.BringToFront();

            PositionAndSizeListView();
        }

        //if (historySuggestions.Any())
        //{
        //    foreach (string s in historySuggestions)
        //    {
        //        string key = s.GetHashCode().ToString();

        //        Image icon = FaviconHelper.GetFaviconFileExternalAsImage(s);
        //        if (icon != null && !iconList.Images.ContainsKey(key))
        //            iconList.Images.Add(key, icon);

        //        var item = new ListViewItem(s)
        //        {
        //            ImageKey = iconList.Images.ContainsKey(key) ? key : null
        //        };
        //        lvSuggestions.Items.Add(item);
        //    }
        //}

        private void PositionAndSizeListView()
        {
            // Position ListView below omniBox
            var point = omniBox.PointToScreen(new System.Drawing.Point(0, omniBox.Height));
            point = form.PointToClient(point);
            lvSuggestions.Left = point.X;
            lvSuggestions.Top = point.Y;

            // Match width
            lvSuggestions.Width = omniBox.Width;

            // Height based on items
            int itemHeight = lvSuggestions.Items.Count > 0 ? lvSuggestions.GetItemRect(0).Height : 16; // fallback
            int totalHeight = itemHeight * lvSuggestions.Items.Count;

            // Optional: add some padding for borders
            totalHeight += 2 * SystemInformation.BorderSize.Height;

            lvSuggestions.Height = totalHeight;

            lvSuggestions.Columns[0].Width = lvSuggestions.Width;
        }

        private List<string> GetHistorySuggestions()
        {
            try
            {
                HistoryService historyService = new HistoryService();
                var suggestions = historyService.All()
                    .Where(h =>
                    {
                        var uri = new Uri(h.WebAddress);

                        return uri.Host.Replace("www.", "").StartsWith(query)
                        || uri.Host.StartsWith(query)
                        || uri.AbsoluteUri.StartsWith(query);
                    })
                    .Take(5)
                    .Select(h => h.WebAddress)
                    .Distinct()
                    .ToList();

                return suggestions;
            }
            catch
            {
                return new List<string>();
            }
        }

        private async Task<List<string>> GetSearchSuggestionsAsync()
        {
            try
            {
                string url = GetSearchSuggestionsAPI();
                string response = await httpClient.GetStringAsync(url);

                var data = JsonConvert.DeserializeObject<object[]>(response);
                var suggestions = JsonConvert.DeserializeObject<List<string>>(data[1].ToString());
                
                return suggestions;
            }
            catch
            {
                return new List<string>();
            }
        }


        private string GetSearchSuggestionsAPI()
        {
            string url;
            switch (SettingsService.Get("SearchEngine"))
            {
                case "bing":
                    url = $"https://api.bing.com/osjson.aspx?query={Uri.EscapeDataString(query)}";
                    break;

                case "duckduckgo":
                    url = $"https://duckduckgo.com/ac/?q={Uri.EscapeDataString(query)}&type=list";
                    break;

                case "youtube":
                    url = $"https://suggestqueries.google.com/complete/search?client=youtube&ds=yt&q=QUERY{Uri.EscapeDataString(query)}";
                    break;

                default:
                    url = $"https://suggestqueries.google.com/complete/search?client=firefox&q={Uri.EscapeDataString(query)}";
                    break;

            }

            return url;
        }

        public Image GetSearchIcon()
        {
            Image searchIcon = null;

            var theme = SettingsService.Get("Theme");
            if (theme == "light")
                searchIcon = Quartz.Properties.Resources.search_16dp_555753_FILL0_wght400_GRAD0_opsz20;
            else if (theme == "dark")
                searchIcon = Quartz.Properties.Resources.search_16dp_C3C3C3_FILL0_wght400_GRAD0_opsz20;
            else if (theme == "black")
                searchIcon = Quartz.Properties.Resources.search_16dp_FFFFFF_FILL0_wght400_GRAD0_opsz20;
            else if (theme == "aqua")
                searchIcon = Quartz.Properties.Resources.search_16dp_0000FF_FILL0_wght400_GRAD0_opsz20;
            else if (theme == "xmas")
                searchIcon = Quartz.Properties.Resources.search_16dp_00FF00_FILL0_wght400_GRAD0_opsz20;

            return searchIcon;
        }


        #region OmniBox Funtionality

        private void OmniBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (lvSuggestions.Visible && lvSuggestions.Items.Count > 0)
            {
                int lastIndex = lvSuggestions.Items.Count - 1;
                int currentIndex = lvSuggestions.SelectedItems.Count > 0
                    ? lvSuggestions.SelectedItems[0].Index
                    : -1;

                if (e.KeyCode == Keys.Down)
                {
                    // Move down or wrap around
                    int nextIndex = (currentIndex < lastIndex) ? currentIndex + 1 : 0;

                    lvSuggestions.Items[nextIndex].Selected = true;
                    lvSuggestions.Items[nextIndex].Focused = true;
                    lvSuggestions.EnsureVisible(nextIndex);

                    isCommitting = false;
                    originalText = omniBox.Text;
                    selectedIndex = nextIndex;
                    omniBox.Text = nextIndex != 0 ? lvSuggestions.Items[nextIndex].Text : lvSuggestions.Items[nextIndex].Text.Replace(" - Google Search", "");
                    omniBox.SelectionStart = omniBox.Text.Length;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    // Move up or wrap to last
                    int prevIndex = (currentIndex > 0) ? currentIndex - 1 : lastIndex;

                    lvSuggestions.Items[prevIndex].Selected = true;
                    lvSuggestions.Items[prevIndex].Focused = true;
                    lvSuggestions.EnsureVisible(prevIndex);

                    isCommitting = false;
                    originalText = omniBox.Text;
                    selectedIndex = prevIndex;
                    omniBox.Text = prevIndex != 0 ? lvSuggestions.Items[prevIndex].Text : lvSuggestions.Items[prevIndex].Text.Replace(" - Google Search", "");
                    omniBox.SelectionStart = omniBox.Text.Length;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    lvSuggestions.Visible = false;

                    if (lvSuggestions.SelectedItems.Count > 0)
                    {
                        //omniBox_Click(null, null);
                    }
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    isCommitting = false;
                    omniBox.Text = originalText;
                    omniBox.SelectionStart = omniBox.Text.Length;
                    lvSuggestions.Visible = false;
                    e.Handled = true;
                }
                else
                {
                    if (lvSuggestions.Items.Count > 0)
                        return;

                    if (historySuggestions.Contains(lvSuggestions.Items[0].Text))
                    {
                        string history = lvSuggestions.Items[0].Text; 
                    }
                }
            }
        }

        private void OmniBox_SizeChanged(object sender, EventArgs e)
        {
            PositionAndSizeListView();
        }
        #endregion
    }
}
