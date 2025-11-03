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
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Controls;
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

        private bool rebindingAll;

        private int currentOffset = 0;
        private const int pageSize = 25;


        //List<DataGridViewRow> selectedCheckBoxes;
        Keys keyPressed;

        public History(Browser browser)
        {
            InitializeComponent();
            _browser = browser;
            selectedRows = new List<DataGridViewRow>();
            //selectedCheckBoxes = new List<DataGridViewRow>();
        }

        private async void History_Load(object sender, EventArgs e)
        {
            NewControlThemeChanger.ChangeTheme(this);
            NewControlThemeChanger.ChangeControlTheme(contextMenuStrip1);

            Rebind();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int currentScrollingRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
            Rebind();
            dataGridView1.FirstDisplayedScrollingRowIndex = currentScrollingRowIndex;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are You Sure Want To Clear All History?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _service.Clear();
                _service.SaveChanges();

                bindingSource1.ResetBindings(false);

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
            rebindingAll = true;
            dataGridView1.Scroll -= dataGridView1_Scroll;

            _service.Reload();

            _allData.Clear(); // wipe old results

            int takeAmount = currentOffset > 0 ? currentOffset : pageSize;

            // Load the first "page"
            var initialData = (string.IsNullOrEmpty(txtSearch.Text)
                ? _service.All()
                : _service.Find(txtSearch.Text))
                .OrderByDescending(i => i.When)
                .Take(takeAmount)
                .ToList();

            foreach (var item in initialData)
                _allData.Add(item);

            if (bindingSource1.DataSource == null)
                bindingSource1.DataSource = _allData;


            currentOffset = currentOffset > 0 ? currentOffset + 0 : currentOffset += pageSize;

            dataGridView1.Scroll += dataGridView1_Scroll;

            SetDataLook();
        }

        private void LoadMoreRows()
        {
            rebindingAll = false;

            var moreData = (string.IsNullOrEmpty(txtSearch.Text)
                ? _service.All()
                : _service.Find(txtSearch.Text))
                .OrderByDescending(i => i.When)
                .Skip(currentOffset)
                .Take(pageSize)
                .ToList();

            foreach (var item in moreData)
                _allData.Add(item);

            if (moreData.Count > 0)
            {
                currentOffset += pageSize;

                SetDataLook();
            }

            dataGridView1.Focus();
        }

        #endregion

        private void SetDataLook()
        {
            // Determine which rows to process
            IEnumerable<DataGridViewRow> rowsToProcess;
            if (rebindingAll)
            {
                // Process all rows
                rowsToProcess = dataGridView1.Rows.Cast<DataGridViewRow>();
            }
            else
            {
                // Only process newly added rows
                rowsToProcess = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .Skip(currentOffset - pageSize); // Skip existing rows
            }

            foreach (DataGridViewRow row in rowsToProcess)
            {
                var webAddress = row.Cells["WebAddress"].Value.ToString();
                var title = row.Cells["Title"].Value.ToString();
                if (!string.IsNullOrEmpty(webAddress) && Uri.IsWellFormedUriString(webAddress, UriKind.Absolute))
                {
                    var favicon = FaviconHelper.GetFaviconFileExternalAsImage(webAddress);
                    row.Cells["FaviconColumn"].Value = favicon;

                    string theme = SettingsService.Get("Theme");
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
                        row.Cells["Delete"].Value = Properties.Resources.Aqua_Close;
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
            Rebind();
        }

  
        private void History_FormClosing(object sender, FormClosingEventArgs e)
        {
            _browser.Shortcuts(true);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //if checkboxcell was clicked, disale selectionchange event before it runs.
            if (dataGridView1.Columns[e.ColumnIndex].Name == "CheckBox")
            {
                dataGridView1.SelectionChanged -= dataGridView1_SelectionChanged;
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell != null && dataGridView1.CurrentCell.OwningColumn.Name == "CheckBox")
            {
                for (int i = 0; i < selectedRows.Count; i++)
                {
                    selectedRows[i].Selected = true;  // Safe to modify because you're not using foreach
                }

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if(!selectedRows.Contains(row))
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

        void SelectRowsWithAnySelectedCell ()
        {
            // List of column names to be ignored during selection counting (like "CheckBox")
            var columnsToIgnore = new List<string> { "" };

            // Loop through each row in DataGridView
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                bool rowSelected = false;

                // Check each cell in the current row, ignoring the checkbox column
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (columnsToIgnore.Contains(cell.OwningColumn.Name))
                        continue; // Skip the checkbox column

                    if (cell.Selected)  // If any other cell in the row is selected
                    {
                        rowSelected = true;
                        break; // No need to check further once we find a selected cell
                    }
                }

                // If any cell (excluding the checkbox column) is selected, mark the row as selected
                row.Selected = rowSelected;
                row.Cells["CheckBox"].Value = rowSelected;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "CheckBox")
            {
                //MessageBox.Show(keyPressed.ToString());
                DataGridViewRow changedRow = dataGridView1.Rows[e.RowIndex];
                bool isChecked = (bool)changedRow.Cells["CheckBox"].Value;

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

                if (clickedCheckbox)
                {
                    // Manually trigger the SelectionChanged event
                    dataGridView1_SelectionChanged(this, EventArgs.Empty);

                    // Re-enable the SelectionChanged event
                    dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
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


        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "CheckBox")
            {

            }
            else if (dataGridView1.Columns[e.ColumnIndex].Name == "Delete")
            {
                var id = Guid.Parse(dataGridView1.Rows[e.RowIndex].Cells["Id"].Value.ToString());

                _service.Remove(id);
                _service.SaveChanges();

                bindingSource1.ResetBindings(false);

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
                    //if (keyPressed != Keys.Shift || keyPressed != Keys.Control)
                    //{
                        dataGridView1.ClearSelection();
                        dataGridView1.Rows[e.RowIndex].Cells["WebAddress"].Selected = true;
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
                    //}
                }
                else if (e.Button == MouseButtons.Right)
                {
                    //if (keyPressed != Keys.ShiftKey || keyPressed != Keys.ControlKey) 
                    //{
                    if (!dataGridView1.Rows[e.RowIndex].Selected)
                        {
                            dataGridView1.ClearSelection();
                            dataGridView1.Rows[e.RowIndex].Cells["Title"].Selected = true;
                        }

                    contextMenuStrip1.Show(MousePosition);
                    //}
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
            

            if(IsAtBottom(dataGridView1))
            {
                int currentScrollingRowIndex = dataGridView1.FirstDisplayedScrollingRowIndex;
                LoadMoreRows();

                dataGridView1.FirstDisplayedScrollingRowIndex = currentScrollingRowIndex;
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(contextMenuStrip1.Handle, 100, Animation.AW_BLEND);
            }


            if(dataGridView1.SelectedRows.Count > 1)
            {
                historyToolStripMenuItem.Text = "Selected: " + dataGridView1.SelectedRows.Count;
            }
            else
            {
                historyToolStripMenuItem.Text = "History: " + currentOffset;
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
                    _service.SaveChanges();
                }
            }

            // Reset bindings and rebind after deletion
            bindingSource1.ResetBindings(false);
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
                        _service.SaveChanges();
                    }
                }

                // Reset bindings and rebind after deletion
                bindingSource1.ResetBindings(false);
                Rebind();

            }

            keyPressed = Keys.None;
        }

        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6)
            {
                string theme = SettingsService.Get("Theme");
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
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.Aqua_Close;
                }
                else if (theme == "xmas")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.Close_xmas;
                }
            }
        }

        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6)
            {
                string theme = SettingsService.Get("Theme");
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
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.Aqua_CloseHover;
                }
                else if (theme == "xmas")
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Delete"].Value = Properties.Resources.CloseHover_xmas;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Delete form = new Delete(_browser.wvWebView1);
            form.ShowDialog();
        }
    }
}
