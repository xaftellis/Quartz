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
    public class OmniBoxUI
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

        public OmniBoxUI(Form Form, RichTextBox Omnibox)
        {
            form = Form;
            omniBox = Omnibox;
            httpClient = new HttpClient();

            omniBox.KeyDown += OmniBox_KeyDown;
            omniBox.SizeChanged += OmniBox_SizeChanged;
            omniBox.GotFocus += OmniBox_GotFocus;
            omniBox.LostFocus += OmniBox_LostFocus;


            // Initialize ListView
            lvSuggestions = new ListView
            {
                View = View.Details,
                HeaderStyle = ColumnHeaderStyle.None,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false,
                HotTracking = true,
                OwnerDraw = false,
                Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular),
                Visible = false
            };

            // Add to form
            form.Controls.Add(lvSuggestions);

            // Add single column to simulate a ListBox
            lvSuggestions.Columns.Add(string.Empty, lvSuggestions.Width);

            // Apply custom theme
            NewControlThemeChanger.ChangeControlTheme(lvSuggestions);

            // Hook events
            lvSuggestions.DrawItem += LvSuggestions_DrawItem;
            lvSuggestions.SelectedIndexChanged += LvSuggestions_SelectedIndexChanged;

            // Create and assign image list
            iconList = new ImageList
            {
                ImageSize = new Size(16, 16),
                ColorDepth = ColorDepth.Depth32Bit
            };
            lvSuggestions.SmallImageList = iconList;

            // Enable double buffering (reduce flicker)
            typeof(ListView)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(lvSuggestions, true);

            ((Browser)form).txtWebAddress.TextChanged += TxtWebAddress_TextChanged;
        }

        private void TxtWebAddress_TextChanged(object sender, EventArgs e)
        {
            if (lvSuggestions.SelectedItems.Count > 0)
            {
                var item = lvSuggestions.SelectedItems[0];
                var imgList = lvSuggestions.SmallImageList ?? lvSuggestions.LargeImageList;

                if (imgList != null)
                {
                    Image img = null;

                    if (!string.IsNullOrEmpty(item.ImageKey) && imgList.Images.ContainsKey(item.ImageKey))
                        img = imgList.Images[item.ImageKey];
                    else if (item.ImageIndex >= 0 && item.ImageIndex < imgList.Images.Count)
                        img = imgList.Images[item.ImageIndex];

                    ((Browser)form).picFavicon.Image = img;
                }
            }
        }

        private void LvSuggestions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSuggestions.SelectedItems.Count > 0)
            {
                var item = lvSuggestions.SelectedItems[0];
                var imgList = lvSuggestions.SmallImageList ?? lvSuggestions.LargeImageList;

                if (imgList != null)
                {
                    Image img = null;

                    if (!string.IsNullOrEmpty(item.ImageKey) && imgList.Images.ContainsKey(item.ImageKey))
                        img = imgList.Images[item.ImageKey];
                    else if (item.ImageIndex >= 0 && item.ImageIndex < imgList.Images.Count)
                        img = imgList.Images[item.ImageIndex];

                    ((Browser)form).picFavicon.Image = img;
                }
            }
        }

        private void LvSuggestions_DrawItem(object sender, DrawListViewItemEventArgs e)
        {

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

            string key = query;
            bool isUrl = QueryAnalyzer.IsProbablyUrl(query);

            Image icon = isUrl
                ? FaviconHelper.GetDefaultFaviconAsImage16()
                : GetSearchIcon();

            if (icon != null && !iconList.Images.ContainsKey(key))
                iconList.Images.Add(key, icon);

            lvSuggestions.BeginUpdate();

            // If there's no top item yet, insert it.
            if (lvSuggestions.Items.Count == 0)
            {
                lvSuggestions.Items.Add(query.Trim() + (isUrl ? "" : " - Google Search"), key);
            }
            else
            {
                // Update existing top item text and icon.
                lvSuggestions.Items[0].Text = query.Trim() + (isUrl ? "" : " - Google Search");
                lvSuggestions.Items[0].ImageKey = key;
            }


            // Keep it selected and focused
            lvSuggestions.Items[0].Selected = true;
            lvSuggestions.Items[0].Focused = true;

            lvSuggestions.EndUpdate();

            lvSuggestions.Visible = true;
            lvSuggestions.BringToFront();

            PositionAndSizeListView();

            var suggestions = await SearchSuggestions.GetAsync(query, SearchSuggestions.SearchEngine.Google);
            if (suggestions == null)
                return;

            lvSuggestions.BeginUpdate();

            // Remove any items after the top item
            while (lvSuggestions.Items.Count > 1)
                lvSuggestions.Items.RemoveAt(1);

            // Add new ones
            foreach (string s in suggestions)
            {
                string sKey = s.GetHashCode().ToString();
                Image sIcon = GetSearchIcon();
                if (sIcon != null && !iconList.Images.ContainsKey(sKey))
                    iconList.Images.Add(sKey, sIcon);

                var item = new ListViewItem(s)
                {
                    ImageKey = iconList.Images.ContainsKey(sKey) ? sKey : null
                };
                lvSuggestions.Items.Add(item);
            }

            PositionAndSizeListView();
            lvSuggestions.EndUpdate();
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

        //private List<string> GetHistorySuggestions()
        //{
        //    try
        //    {
        //        HistoryService historyService = new HistoryService();
        //        var suggestions = historyService.All()
        //            .Where(h =>
        //            {
        //                var uri = new Uri(h.WebAddress);

        //                return uri.Host.Replace("www.", "").StartsWith(query)
        //                || uri.Host.StartsWith(query)
        //                || uri.AbsoluteUri.StartsWith(query);
        //            })
        //            .Take(5)
        //            .Select(h => h.WebAddress)
        //            .Distinct()
        //            .ToList();

        //        return suggestions;
        //    }
        //    catch
        //    {
        //        return new List<string>();
        //    }
        //}

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
