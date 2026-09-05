using EasyTabs;
using Quartz.Controls;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Quartz
{
    public partial class History : Form
    {
        private Browser _browser = null;
        private HistoryService _service = new HistoryService();
        bool clickedCheckbox = false;
        int checkboxIndex = 0;
        List<DataGridViewRow> selectedRows;

        private bool updatingRows;
        private bool syncingSelection;
        private List<HistoryModel> matchingData = new List<HistoryModel>();
        private Dictionary<string, Guid> faviconIds;
        private readonly Dictionary<Guid, Image> faviconImages = new Dictionary<Guid, Image>();
        private Image defaultFavicon;
        private string historyTheme;
        private readonly System.Windows.Forms.Timer searchTimer;

        private int currentOffset = 0;
        private const int pageSize = 25;


        //List<DataGridViewRow> selectedCheckBoxes;
        Keys keyPressed;

        public void LoadSeparatorTheme(Panel seporater)
        {
            string theme = SettingsService.Get("Theme");

            Color dividerColor = Color.FromArgb(219, 220, 221);

            if (theme == "dark")
                dividerColor = Color.FromArgb(88, 88, 88);

            if (theme == "black")
                dividerColor = Color.FromArgb(128, 128, 128);

            if (theme == "aqua")
                dividerColor = Color.Blue;

            if (theme == "xmas")
                dividerColor = Color.Lime;

            seporater.BackColor = dividerColor;
        }

        public History(Browser browser)
        {
            InitializeComponent();
            _browser = browser;
            selectedRows = new List<DataGridViewRow>();
            searchTimer = new System.Windows.Forms.Timer(components) { Interval = 200 };
            searchTimer.Tick += (sender, e) => { searchTimer.Stop(); Rebind(); };
            Disposed += (sender, e) => ClearFaviconImages();
        }

        private async void History_Load(object sender, EventArgs e)
        {
            NewControlThemeChanger.ChangeTheme(this);
            NewControlThemeChanger.ChangeControlTheme(contextMenuStrip1);
            LoadSeparatorTheme(pnlDivider);

            Rebind();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are You Sure Want To Clear All History?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _service.Clear();
                _service.SaveChanges();

                Rebind();

                _browser.wvWebView1.CoreWebView2.Profile.ClearBrowsingDataAsync(Microsoft.Web.WebView2.Core.CoreWebView2BrowsingDataKinds.BrowsingHistory, Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().AddYears(-999), Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone());
            }
        }

        private void BtnCache_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are You Sure Want To Clear All Cache?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    var dir = new DirectoryInfo(_browser.GetCachePath());
                    foreach (var file in dir.EnumerateFiles("*.*"))
                        file.Delete();

                    FaviconService faviconService = new FaviconService();
                    faviconService.Clear();
                    faviconService.SaveChanges();

                    Rebind();
                }
                catch (Exception a)
                {
                    var msg = MessageBox.Show($"{a.Message}", "This Isn't Right", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }


        #region Helper Functions
        // Class-level persistent list that the grid binds to
        // Class-level persistent list
        private BindingList<HistoryModel> _allData = new BindingList<HistoryModel>();

        private void Rebind()
        {
            searchTimer.Stop();
            updatingRows = true;
            dataGridView1.Scroll -= dataGridView1_Scroll;
            try
            {
                _service.Reload();
                int takeAmount = currentOffset > 0 ? currentOffset : pageSize;

                // Sort once per refresh/search; scrolling only takes the next page.
                matchingData = (string.IsNullOrEmpty(txtSearch.Text)
                    ? _service.All()
                    : _service.Find(txtSearch.Text))
                    .OrderByDescending(i => i.When)
                    .ToList();

                _allData.RaiseListChangedEvents = false;
                _allData.Clear(); // wipe old results
                selectedRows.Clear();
                ClearFaviconImages();
                historyTheme = SettingsService.Get("Theme");

                foreach (var item in matchingData.Take(takeAmount))
                    _allData.Add(item);

                _allData.RaiseListChangedEvents = true;
                if (bindingSource1.DataSource == null)
                    bindingSource1.DataSource = _allData;
                else
                    bindingSource1.ResetBindings(false);

                currentOffset = _allData.Count;
                SetDataLook();
            }
            finally
            {
                _allData.RaiseListChangedEvents = true;
                updatingRows = false;
                dataGridView1.Scroll += dataGridView1_Scroll;
            }
            dataGridView1_SelectionChanged(this, EventArgs.Empty);
        }

        private void LoadMoreRows()
        {
            if (updatingRows || searchTimer.Enabled || currentOffset >= matchingData.Count) return;

            int firstNewRow = _allData.Count;
            var moreData = matchingData
                .Skip(currentOffset)
                .Take(pageSize)
                .ToList();

            updatingRows = true;
            try
            {
                foreach (var item in moreData)
                    _allData.Add(item);

                currentOffset = _allData.Count;
                SetDataLook(firstNewRow);
            }
            finally
            {
                updatingRows = false;
            }

            dataGridView1.Focus();
        }

        #endregion

        private void SetDataLook(int firstRow = 0)
        {
            // Rebind styles all rows; scrolling styles only the new page, including a partial final page.
            var rowsToProcess = dataGridView1.Rows.Cast<DataGridViewRow>().Skip(firstRow);

            foreach (DataGridViewRow row in rowsToProcess)
            {
                var webAddress = Convert.ToString(row.Cells["WebAddress"].Value);
                var title = Convert.ToString(row.Cells["Title"].Value);
                if (!string.IsNullOrEmpty(webAddress) && Uri.IsWellFormedUriString(webAddress, UriKind.Absolute))
                {
                    var favicon = GetHistoryFavicon(webAddress);
                    row.Cells["FaviconColumn"].Value = favicon;

                    string theme = historyTheme;
                    if (theme == "light")
                    {
                        row.Cells["Delete"].Value = Properties.Resources.Close;
                    }
                    else if (theme == "dark")
                    {
                        row.Cells["Delete"].Value = Properties.Resources.Tabs_Close;
                    }
                    else if (theme == "black")
                    {
                        row.Cells["Delete"].Value = Properties.Resources.B_Close;
                    }
                    else if (theme == "aqua")
                    {
                        row.Cells["Delete"].Value = Properties.Resources.Blue_Close;
                    }
                    else if (theme == "xmas")
                    {
                        row.Cells["Delete"].Value = Properties.Resources.Close_xmas;
                    }
                }

                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.OwningColumn.Name != "CheckBox"
                        && cell.OwningColumn.Name != "Delete")
                    {
                        cell.ToolTipText = title + Environment.NewLine + webAddress;
                    }
                }

            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            currentOffset = 0;
            searchTimer.Stop();
            searchTimer.Start();
        }

        private Image GetHistoryFavicon(string webAddress)
        {
            // Read the favicon index once, not once or twice for every history row.
            if (faviconIds == null)
                faviconIds = new FaviconService().All()
                    .Where(item => !string.IsNullOrEmpty(item.WebAddress))
                    .GroupBy(item => item.WebAddress)
                    .ToDictionary(group => group.Key, group => group.First().Id);

            if (defaultFavicon == null)
                defaultFavicon = FaviconHelper.GetDefaultFavicon16().ToBitmap();

            Guid id;
            if (!faviconIds.TryGetValue(webAddress, out id)) return defaultFavicon;

            Image image;
            if (!faviconImages.TryGetValue(id, out image))
            {
                image = defaultFavicon;
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Xaftellis\Quartz\UserData\cache", id + ".ico");
                try
                {
                    if (File.Exists(path))
                        using (var icon = new Icon(path)) image = icon.ToBitmap();
                }
                catch (IOException) { } // A cache file can disappear while the history window is open.
                catch (ArgumentException) { } // Use the default for an invalid cached icon.
                faviconImages[id] = image;
            }
            return image;
        }

        private void ClearFaviconImages()
        {
            foreach (var image in faviconImages.Values.Distinct())
                if (!ReferenceEquals(image, defaultFavicon)) image.Dispose();
            faviconImages.Clear();
            defaultFavicon?.Dispose();
            defaultFavicon = null;
            faviconIds = null;
        }

  
        private void History_FormClosing(object sender, FormClosingEventArgs e)
        {
            _browser.Shortcuts(true);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //if checkboxcell was clicked, disale selectionchange event before it runs.
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex].Name == "CheckBox")
            {
                dataGridView1.SelectionChanged -= dataGridView1_SelectionChanged;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (updatingRows || syncingSelection) return;
            syncingSelection = true;
            try
            {
                if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.OwningColumn.Name == "CheckBox")
                {
                    var checkedRows = new HashSet<DataGridViewRow>(selectedRows);
                    for (int i = 0; i < selectedRows.Count; i++)
                    {
                        if (!selectedRows[i].Selected) selectedRows[i].Selected = true;
                    }

                    foreach (DataGridViewRow row in dataGridView1.SelectedRows.Cast<DataGridViewRow>().ToList())
                    {
                        if(!checkedRows.Contains(row))
                        {
                            row.Selected = false;
                        }
                    }

                    return;
                }

                SelectRowsWithAnySelectedCell();

                selectedRows.Clear();
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    selectedRows.Add(row);
                }
                //MessageBox.Show(selectedRows.Count.ToString(), "SelectionChanged");
            }
            finally
            {
                syncingSelection = false;
            }
        }

        void SelectRowsWithAnySelectedCell ()
        {
            // Only visit the current and previous selection, not every cell in the history grid.
            var rowsWithSelectedCells = new HashSet<DataGridViewRow>(dataGridView1.SelectedCells
                .Cast<DataGridViewCell>().Select(cell => cell.OwningRow));

            foreach (DataGridViewRow row in selectedRows)
            {
                if (!rowsWithSelectedCells.Contains(row))
                {
                    if (row.Selected) row.Selected = false;
                    if (Equals(row.Cells["CheckBox"].Value, true)) row.Cells["CheckBox"].Value = false;
                }
            }

            foreach (DataGridViewRow row in rowsWithSelectedCells)
            {
                if (!row.Selected) row.Selected = true;
                if (!Equals(row.Cells["CheckBox"].Value, true)) row.Cells["CheckBox"].Value = true;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (updatingRows || syncingSelection || e.ColumnIndex < 0 || e.RowIndex < 0) return;
            if (dataGridView1.Columns[e.ColumnIndex].Name == "CheckBox")
            {
                syncingSelection = true;
                try
                {
                    clickedCheckbox = false;
                    //MessageBox.Show(keyPressed.ToString());
                    DataGridViewRow changedRow = dataGridView1.Rows[e.RowIndex];
                    bool isChecked = Equals(changedRow.Cells["CheckBox"].Value, true);

                    if (isChecked)
                    {
                        if (!selectedRows.Contains(changedRow))
                        {
                            selectedRows.Add(changedRow);
                            clickedCheckbox = true;
                        }
                        changedRow.Selected = true; // Immediately select the row when checkbox is checked
                    }
                    else
                    {
                        if (selectedRows.Contains(changedRow))
                        {
                            selectedRows.Remove(changedRow);
                            clickedCheckbox = true;
                        }
                        changedRow.Selected = false; // Unselect the row when checkbox is unchecked
                    }
                }
                finally
                {
                    syncingSelection = false;
                    // Keep exactly one handler, including keyboard checkbox changes without CellClick.
                    dataGridView1.SelectionChanged -= dataGridView1_SelectionChanged;
                    dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
                }

                if (clickedCheckbox)
                {
                    // Manually trigger the SelectionChanged event
                    dataGridView1_SelectionChanged(this, EventArgs.Empty);

                }
            }
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Commit the edit when the current cell is dirty (i.e., when a checkbox is clicked)
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        int rowIndex;
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dataGridView1.Columns[e.ColumnIndex].Name == "CheckBox")
            {

            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "Delete")
            {
                var id = Guid.Parse(dataGridView1.Rows[e.RowIndex].Cells["Id"].Value.ToString());

                _service.Remove(id);
                _service.SaveChanges();

                Rebind();
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (keyPressed != Keys.ShiftKey && keyPressed != Keys.ControlKey)
                    {
                        var url = dataGridView1.Rows[e.RowIndex].Cells["WebAddress"].Value.ToString();
                        _browser.SetSource(url);
                        Close();
                        return;
                    }
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells["Title"];
                    dataGridView1.Rows[e.RowIndex].Cells["Title"].Selected = true;
                    Application.DoEvents(); // lets the UI update

                    var ParentTabs = _browser.ParentTabs;
                    var url = dataGridView1.Rows[e.RowIndex].Cells["WebAddress"].Value.ToString();

                    Browser browser = new Browser(url, true);
                    browser.InitializeTab();
                    var newtab = new TitleBarTab(ParentTabs) { Content = browser };
                    if (ParentTabs.InvokeRequired)
                    {
                        ParentTabs.Invoke(new Action(() =>
                        {
                            ParentTabs.Tabs.Insert(ParentTabs.SelectedTabIndex + 1, newtab);
                            ParentTabs.SelectedTabIndex++;
                            ParentTabs.RedrawTabs();
                            ParentTabs.Refresh();
                        }));
                    }
                    else
                    {
                        ParentTabs.Tabs.Insert(ParentTabs.SelectedTabIndex + 1, newtab);
                        ParentTabs.SelectedTabIndex++;
                        ParentTabs.RedrawTabs();
                        ParentTabs.Refresh();
                    }

                    this.Close();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (!dataGridView1.Rows[e.RowIndex].Selected)
                    {
                        dataGridView1.ClearSelection();
                        dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells["Title"];
                        dataGridView1.Rows[e.RowIndex].Selected = true;
                    }

                    rowIndex = e.RowIndex;
                    contextMenuStrip1.Show(MousePosition);
                }
            }
        }

        bool IsAtBottom(DataGridView dgv)
        {
            if (dgv.RowCount == 0) return true;
            int first = dgv.FirstDisplayedScrollingRowIndex;
            int visible = dgv.DisplayedRowCount(false);
            return first + visible >= dgv.RowCount;
        }


        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            if(contextMenuStrip1.Visible)
            {
                contextMenuStrip1.Close();
            }
            

            if(!updatingRows && !searchTimer.Enabled && currentOffset < matchingData.Count && IsAtBottom(dataGridView1))
            {
                int currentScrollingRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
                LoadMoreRows();

                if (currentScrollingRowIndex >= 0 && currentScrollingRowIndex < dataGridView1.RowCount)
                    dataGridView1.FirstDisplayedScrollingRowIndex = currentScrollingRowIndex;
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(selectedRows.Count > 1)
            {
                openInNewTabToolStripMenuItem.Text = "Open all in new tabs";
                openInNewWindowToolStripMenuItem.Text = "Open all in new window";
                copyLinkToolStripMenuItem.Text = "Copy links";
            }
            else
            {
                openInNewTabToolStripMenuItem.Text = "Open in new tab";
                openInNewWindowToolStripMenuItem.Text = "Open in new window";
                copyLinkToolStripMenuItem.Text = "Copy link";
            }

            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(contextMenuStrip1.Handle, 100, Animation.AW_BLEND);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the collection of selected rows
            DataGridViewSelectedRowCollection selectedRows = dataGridView1.SelectedRows;

            // Process the selected rows in reverse order
            for (int i = selectedRows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = selectedRows[i];

                // Ensure the row is valid and perform the delete operation
                if (row != null)
                {
                    var id = Guid.Parse(row.Cells[0].Value.ToString());

                    _service.Remove(id);
                }
            }

            // Reset bindings and rebind after deletion
            _service.SaveChanges();
            Rebind();
        }

        //WHEN KEY IS PRESSED
        private void History_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            keyPressed = e.KeyCode;
        }
        
        //WHEN KEY IS RELEASED
        private void History_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Get the collection of selected rows
                DataGridViewSelectedRowCollection selectedRows = dataGridView1.SelectedRows;

                // Process the selected rows in reverse order
                for (int i = selectedRows.Count - 1; i >= 0; i--)
                {
                    DataGridViewRow row = selectedRows[i];

                    // Ensure the row is valid and perform the delete operation
                    if (row != null)
                    {
                        var id = Guid.Parse(row.Cells[0].Value.ToString());

                        _service.Remove(id);
                    }
                }

                // Reset bindings and rebind after deletion
                _service.SaveChanges();
                Rebind();
            }

            keyPressed = Keys.None;
        }

        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6 && e.RowIndex >= 0)
            {
                string theme = historyTheme;
                if (theme == "light")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.Close;
                }
                else if (theme == "dark")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.Tabs_Close;
                }
                else if (theme == "black")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.B_Close;
                }
                else if (theme == "aqua")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.Blue_Close;
                }
                else if (theme == "xmas")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.Close_xmas;
                }
            }
        }

        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6 && e.RowIndex >= 0)
            {
                string theme = historyTheme;
                if (theme == "light")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.CloseHover;
                }
                else if (theme == "dark")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.Tabs_CloseHover;
                }
                else if (theme == "black")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.B_CloseHover;
                }
                else if (theme == "aqua")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.Blue_CloseHover;
                }
                else if (theme == "xmas")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.CloseHover_xmas;
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView1.Rows[rowIndex].Cells["Title"].Selected = true;
            Application.DoEvents(); // lets the UI update

            var url = dataGridView1.Rows[rowIndex].Cells["WebAddress"].Value.ToString();
            _browser.SetSource(url);
            Close();
        }

        private void openInNewTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the collection of selected rows
            DataGridViewSelectedRowCollection selectedRows = dataGridView1.SelectedRows;

            // Process the selected rows in reverse order
            for (int i = selectedRows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = selectedRows[i];

                // Ensure the row is valid and perform the delete operation
                if (row != null)
                {
                    string url = row.Cells["WebAddress"].Value.ToString();

                    var browser = new Browser(url, true);
                    browser.InitializeTab();

                    var newTab = new TitleBarTab(_browser.ParentTabs) { Content = browser };

                    void AddTab()
                    {
                        int index = _browser.ParentTabs.SelectedTabIndex + 1;
                        _browser.ParentTabs.Tabs.Insert(index, newTab);
                        _browser.ParentTabs.SelectedTabIndex = index;
                        _browser.ParentTabs.RedrawTabs();
                    }

                    if (_browser.ParentTabs.InvokeRequired)
                        _browser.ParentTabs.Invoke(new Action(AddTab));
                    else
                        AddTab();

                    this.Close();
                }
            }
        }

        private void openInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the collection of selected rows
            DataGridViewSelectedRowCollection selectedRows = dataGridView1.SelectedRows;

            List<string> urls = new List<string>();

            // Process the selected rows in reverse order
            for (int i = selectedRows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = selectedRows[i];

                // Ensure the row is valid and perform the delete operation
                if (row != null)
                {
                    urls.Add(row.Cells["WebAddress"].Value.ToString());
                }
            }
            Program.OpenNewWindowWithTabsFast(urls);
            this.Close();
        }

        private void copyLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the collection of selected rows
            DataGridViewSelectedRowCollection selectedRows = dataGridView1.SelectedRows;

            string urls = string.Empty;

            // Process the selected rows in reverse order
            for (int i = 0; i <= selectedRows.Count - 1; i++)
            {
                DataGridViewRow row = selectedRows[i];

                // Ensure the row is valid and perform the delete operation
                if (row != null)
                {
                    string url = dataGridView1.Rows[rowIndex].Cells["WebAddress"].Value.ToString();

                    if (i == selectedRows.Count - 1)
                    {
                        urls += url;
                    }
                    else
                    {
                        urls += url + Environment.NewLine;
                    }
                }
            }
            Clipboard.SetText(urls);
        }
    }
}
