using Quartz.Controls.ChromiumMenus;
namespace Quartz
{
    partial class Browser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) DisposeWebViewFocus();
            if (disposing) DisposeTabPreview();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Browser));
            this.mnuMenu = new Quartz.Controls.ChromiumMenus.ChromiumMenu(this.components);
            this.openToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.openInNewTabToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.openInNewWindowToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator3 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.modifyToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator18 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.cutToolStripMenuItem1 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.copyToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.pasteToolStripMenuItem1 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator4 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.removeToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.removeAllToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator14 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.toolStripMenuItem5 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.sortByAlphabeticallyToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.showFavouritesBarToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.pnlFavourites = new Quartz.Controls.FavouritesBar();
            this.btnDownload = new Quartz.Controls.ChromiumButton();
            this.mnuDownloadsDropDown = new Quartz.Controls.ChromiumMenus.ChromiumMenu(this.components);
            this.locationToolStripMenuItem1 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.changeLocationToolStripMenuItem1 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.downloadsToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.btnAddFavourite = new Quartz.Controls.ChromiumButton();
            this.btnForward = new Quartz.Controls.ChromiumButton();
            this.wvLoadingProgress = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.btnBack = new Quartz.Controls.ChromiumButton();
            this.mnuSearch = new Quartz.Controls.ChromiumMenus.ChromiumMenu(this.components);
            this.emojiToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator9 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.undoToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.redoToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator11 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.cutToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.copyToolStripMenuItem1 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.pasteToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.pasteAndGoToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.deleteToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator15 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.selectAllToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator21 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.alwaysShowFullURLsToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.txtWebAddress = new System.Windows.Forms.RichTextBox();
            this.btnRefresh = new Quartz.Controls.ChromiumButton();
            this.pnlTop = new Quartz.Controls.BrowserToolbarPanel();
            this.btnSiteInformation = new Quartz.Controls.SiteInfoButton();
            this.btnSettings = new Quartz.Controls.ChromiumButton();
            this.UrlBox = new System.Windows.Forms.PictureBox();
            this.UrlLeft = new System.Windows.Forms.PictureBox();
            this.UrlRight = new System.Windows.Forms.PictureBox();
            this.SettingsMenuStrip = new Quartz.Controls.ChromiumMenus.ChromiumMenu(this.components);
            this.newTabToolStripMenuItem1 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.newWindowToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.changeProfileToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator20 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.historyToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.mnuHistory = new Quartz.Controls.ChromiumMenus.ChromiumMenu(this.components);
            this.historyToolStripMenuItem1 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator10 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.favouritesToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.mnuFavourites = new Quartz.Controls.ChromiumMenus.ChromiumMenu(this.components);
            this.addFavouritesToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator8 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.toolStripSeparator2 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.zoomToolStrip = new Quartz.Controls.ChromiumMenus.ChromiumZoomMenuItem();
            this.toolStripSeparator1 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.findToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.printToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.openFileInBrowserToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.expertsToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.mnuExperts = new Quartz.Controls.ChromiumMenus.ChromiumMenu(this.components);
            this.nameWindowToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator12 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.webview2TaskManagerToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator13 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.inspectToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator7 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.userDataToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.mnuUserData = new Quartz.Controls.ChromiumMenus.ChromiumMenu(this.components);
            this.openFolderToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator6 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.resetToolStripMenuItem1 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.settingsToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripSeparator16 = new Quartz.Controls.ChromiumMenus.ChromiumMenuSeparator();
            this.restartToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.exitToolStripMenuItem = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.pnlDivider = new System.Windows.Forms.Panel();
            this.wvWebView1 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.toolStripMenuItem2 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripMenuItem3 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.toolStripMenuItem4 = new Quartz.Controls.ChromiumMenus.ChromiumMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.pnlBottom = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.wvLoadingProgress)).BeginInit();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UrlBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UrlLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UrlRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wvWebView1)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuMenu
            // 
            this.mnuMenu.Items.AddRange(new ChromiumMenuItem[] {
            this.openToolStripMenuItem,
            this.openInNewTabToolStripMenuItem,
            this.openInNewWindowToolStripMenuItem,
            this.toolStripSeparator3,
            this.modifyToolStripMenuItem,
            this.toolStripSeparator18,
            this.cutToolStripMenuItem1,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem1,
            this.toolStripSeparator4,
            this.removeToolStripMenuItem,
            this.removeAllToolStripMenuItem,
            this.toolStripSeparator14,
            this.toolStripMenuItem5,
            this.sortByAlphabeticallyToolStripMenuItem,
            this.showFavouritesBarToolStripMenuItem});
            this.mnuMenu.Appearance = null;
            this.mnuMenu.Enabled = true;
            this.mnuMenu.Name = "contextMenuStrip1";
            this.mnuMenu.Size = new System.Drawing.Size(188, 292);
            this.mnuMenu.Tag = null;
            this.mnuMenu.Opening += new System.ComponentModel.CancelEventHandler(this.mnuMenu_Opening);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Available = true;
            this.openToolStripMenuItem.Checked = false;
            this.openToolStripMenuItem.CheckOnClick = false;
            this.openToolStripMenuItem.DropDown = null;
            this.openToolStripMenuItem.Enabled = true;
            this.openToolStripMenuItem.Image = null;
            this.openToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.openToolStripMenuItem.IsCheck = false;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Radio = false;
            this.openToolStripMenuItem.SecondaryText = null;
            this.openToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.openToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.openToolStripMenuItem.Tag = null;
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.ToolTipText = null;
            this.openToolStripMenuItem.VectorIcon = null;
            this.openToolStripMenuItem.Visible = true;
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // openInNewTabToolStripMenuItem
            // 
            this.openInNewTabToolStripMenuItem.Available = true;
            this.openInNewTabToolStripMenuItem.Checked = false;
            this.openInNewTabToolStripMenuItem.CheckOnClick = false;
            this.openInNewTabToolStripMenuItem.DropDown = null;
            this.openInNewTabToolStripMenuItem.Enabled = true;
            this.openInNewTabToolStripMenuItem.Image = null;
            this.openInNewTabToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.openInNewTabToolStripMenuItem.IsCheck = false;
            this.openInNewTabToolStripMenuItem.Name = "openInNewTabToolStripMenuItem";
            this.openInNewTabToolStripMenuItem.Radio = false;
            this.openInNewTabToolStripMenuItem.SecondaryText = null;
            this.openInNewTabToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openInNewTabToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.openInNewTabToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.openInNewTabToolStripMenuItem.Tag = null;
            this.openInNewTabToolStripMenuItem.Text = "Open in new tab";
            this.openInNewTabToolStripMenuItem.ToolTipText = null;
            this.openInNewTabToolStripMenuItem.VectorIcon = null;
            this.openInNewTabToolStripMenuItem.Visible = true;
            this.openInNewTabToolStripMenuItem.Click += new System.EventHandler(this.openInNewTabToolStripMenuItem_Click);
            // 
            // openInNewWindowToolStripMenuItem
            // 
            this.openInNewWindowToolStripMenuItem.Available = true;
            this.openInNewWindowToolStripMenuItem.Checked = false;
            this.openInNewWindowToolStripMenuItem.CheckOnClick = false;
            this.openInNewWindowToolStripMenuItem.DropDown = null;
            this.openInNewWindowToolStripMenuItem.Enabled = true;
            this.openInNewWindowToolStripMenuItem.Image = null;
            this.openInNewWindowToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.openInNewWindowToolStripMenuItem.IsCheck = false;
            this.openInNewWindowToolStripMenuItem.Name = "openInNewWindowToolStripMenuItem";
            this.openInNewWindowToolStripMenuItem.Radio = false;
            this.openInNewWindowToolStripMenuItem.SecondaryText = null;
            this.openInNewWindowToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openInNewWindowToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.openInNewWindowToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.openInNewWindowToolStripMenuItem.Tag = null;
            this.openInNewWindowToolStripMenuItem.Text = "Open in new window";
            this.openInNewWindowToolStripMenuItem.ToolTipText = null;
            this.openInNewWindowToolStripMenuItem.VectorIcon = null;
            this.openInNewWindowToolStripMenuItem.Visible = true;
            this.openInNewWindowToolStripMenuItem.Click += new System.EventHandler(this.openInNewWindowToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Available = true;
            this.toolStripSeparator3.Checked = false;
            this.toolStripSeparator3.CheckOnClick = false;
            this.toolStripSeparator3.DropDown = null;
            this.toolStripSeparator3.Enabled = true;
            this.toolStripSeparator3.Image = null;
            this.toolStripSeparator3.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator3.IsCheck = false;
            this.toolStripSeparator3.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Radio = false;
            this.toolStripSeparator3.SecondaryText = null;
            this.toolStripSeparator3.ShortcutKeyDisplayString = null;
            this.toolStripSeparator3.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator3.Size = new System.Drawing.Size(184, 6);
            this.toolStripSeparator3.Tag = null;
            this.toolStripSeparator3.Text = "";
            this.toolStripSeparator3.ToolTipText = null;
            this.toolStripSeparator3.VectorIcon = null;
            this.toolStripSeparator3.Visible = true;
            // 
            // modifyToolStripMenuItem
            // 
            this.modifyToolStripMenuItem.Available = true;
            this.modifyToolStripMenuItem.Checked = false;
            this.modifyToolStripMenuItem.CheckOnClick = false;
            this.modifyToolStripMenuItem.DropDown = null;
            this.modifyToolStripMenuItem.Enabled = true;
            this.modifyToolStripMenuItem.Image = null;
            this.modifyToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.modifyToolStripMenuItem.IsCheck = false;
            this.modifyToolStripMenuItem.Name = "modifyToolStripMenuItem";
            this.modifyToolStripMenuItem.Radio = false;
            this.modifyToolStripMenuItem.SecondaryText = null;
            this.modifyToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.modifyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.modifyToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.modifyToolStripMenuItem.Tag = null;
            this.modifyToolStripMenuItem.Text = "Edit";
            this.modifyToolStripMenuItem.ToolTipText = null;
            this.modifyToolStripMenuItem.VectorIcon = null;
            this.modifyToolStripMenuItem.Visible = true;
            this.modifyToolStripMenuItem.Click += new System.EventHandler(this.modifyToolStripMenuItem_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Available = true;
            this.toolStripSeparator18.Checked = false;
            this.toolStripSeparator18.CheckOnClick = false;
            this.toolStripSeparator18.DropDown = null;
            this.toolStripSeparator18.Enabled = true;
            this.toolStripSeparator18.Image = null;
            this.toolStripSeparator18.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator18.IsCheck = false;
            this.toolStripSeparator18.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Radio = false;
            this.toolStripSeparator18.SecondaryText = null;
            this.toolStripSeparator18.ShortcutKeyDisplayString = null;
            this.toolStripSeparator18.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator18.Size = new System.Drawing.Size(184, 6);
            this.toolStripSeparator18.Tag = null;
            this.toolStripSeparator18.Text = "";
            this.toolStripSeparator18.ToolTipText = null;
            this.toolStripSeparator18.VectorIcon = null;
            this.toolStripSeparator18.Visible = true;
            // 
            // cutToolStripMenuItem1
            // 
            this.cutToolStripMenuItem1.Available = true;
            this.cutToolStripMenuItem1.Checked = false;
            this.cutToolStripMenuItem1.CheckOnClick = false;
            this.cutToolStripMenuItem1.DropDown = null;
            this.cutToolStripMenuItem1.Enabled = true;
            this.cutToolStripMenuItem1.Image = null;
            this.cutToolStripMenuItem1.ImageSize = new System.Drawing.Size(16, 16);
            this.cutToolStripMenuItem1.IsCheck = false;
            this.cutToolStripMenuItem1.Name = "cutToolStripMenuItem1";
            this.cutToolStripMenuItem1.Radio = false;
            this.cutToolStripMenuItem1.SecondaryText = null;
            this.cutToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.cutToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.cutToolStripMenuItem1.Size = new System.Drawing.Size(187, 22);
            this.cutToolStripMenuItem1.Tag = null;
            this.cutToolStripMenuItem1.Text = "Cut";
            this.cutToolStripMenuItem1.ToolTipText = null;
            this.cutToolStripMenuItem1.VectorIcon = null;
            this.cutToolStripMenuItem1.Visible = true;
            this.cutToolStripMenuItem1.Click += new System.EventHandler(this.cutToolStripMenuItem1_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Available = true;
            this.copyToolStripMenuItem.Checked = false;
            this.copyToolStripMenuItem.CheckOnClick = false;
            this.copyToolStripMenuItem.DropDown = null;
            this.copyToolStripMenuItem.Enabled = true;
            this.copyToolStripMenuItem.Image = null;
            this.copyToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.copyToolStripMenuItem.IsCheck = false;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Radio = false;
            this.copyToolStripMenuItem.SecondaryText = null;
            this.copyToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.copyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.copyToolStripMenuItem.Tag = null;
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.ToolTipText = null;
            this.copyToolStripMenuItem.VectorIcon = null;
            this.copyToolStripMenuItem.Visible = true;
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem1
            // 
            this.pasteToolStripMenuItem1.Available = true;
            this.pasteToolStripMenuItem1.Checked = false;
            this.pasteToolStripMenuItem1.CheckOnClick = false;
            this.pasteToolStripMenuItem1.DropDown = null;
            this.pasteToolStripMenuItem1.Enabled = true;
            this.pasteToolStripMenuItem1.Image = null;
            this.pasteToolStripMenuItem1.ImageSize = new System.Drawing.Size(16, 16);
            this.pasteToolStripMenuItem1.IsCheck = false;
            this.pasteToolStripMenuItem1.Name = "pasteToolStripMenuItem1";
            this.pasteToolStripMenuItem1.Radio = false;
            this.pasteToolStripMenuItem1.SecondaryText = null;
            this.pasteToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.pasteToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.pasteToolStripMenuItem1.Size = new System.Drawing.Size(187, 22);
            this.pasteToolStripMenuItem1.Tag = null;
            this.pasteToolStripMenuItem1.Text = "Paste";
            this.pasteToolStripMenuItem1.ToolTipText = null;
            this.pasteToolStripMenuItem1.VectorIcon = null;
            this.pasteToolStripMenuItem1.Visible = true;
            this.pasteToolStripMenuItem1.Click += new System.EventHandler(this.pasteToolStripMenuItem1_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Available = true;
            this.toolStripSeparator4.Checked = false;
            this.toolStripSeparator4.CheckOnClick = false;
            this.toolStripSeparator4.DropDown = null;
            this.toolStripSeparator4.Enabled = true;
            this.toolStripSeparator4.Image = null;
            this.toolStripSeparator4.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator4.IsCheck = false;
            this.toolStripSeparator4.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Radio = false;
            this.toolStripSeparator4.SecondaryText = null;
            this.toolStripSeparator4.ShortcutKeyDisplayString = null;
            this.toolStripSeparator4.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator4.Size = new System.Drawing.Size(184, 6);
            this.toolStripSeparator4.Tag = null;
            this.toolStripSeparator4.Text = "";
            this.toolStripSeparator4.ToolTipText = null;
            this.toolStripSeparator4.VectorIcon = null;
            this.toolStripSeparator4.Visible = true;
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Available = true;
            this.removeToolStripMenuItem.Checked = false;
            this.removeToolStripMenuItem.CheckOnClick = false;
            this.removeToolStripMenuItem.DropDown = null;
            this.removeToolStripMenuItem.Enabled = true;
            this.removeToolStripMenuItem.Image = null;
            this.removeToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.removeToolStripMenuItem.IsCheck = false;
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Radio = false;
            this.removeToolStripMenuItem.SecondaryText = null;
            this.removeToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.removeToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.removeToolStripMenuItem.Tag = null;
            this.removeToolStripMenuItem.Text = "Delete";
            this.removeToolStripMenuItem.ToolTipText = null;
            this.removeToolStripMenuItem.VectorIcon = null;
            this.removeToolStripMenuItem.Visible = true;
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // removeAllToolStripMenuItem
            // 
            this.removeAllToolStripMenuItem.Available = true;
            this.removeAllToolStripMenuItem.Checked = false;
            this.removeAllToolStripMenuItem.CheckOnClick = false;
            this.removeAllToolStripMenuItem.DropDown = null;
            this.removeAllToolStripMenuItem.Enabled = true;
            this.removeAllToolStripMenuItem.Image = null;
            this.removeAllToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.removeAllToolStripMenuItem.IsCheck = false;
            this.removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            this.removeAllToolStripMenuItem.Radio = false;
            this.removeAllToolStripMenuItem.SecondaryText = null;
            this.removeAllToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.removeAllToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.removeAllToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.removeAllToolStripMenuItem.Tag = null;
            this.removeAllToolStripMenuItem.Text = "Delete all";
            this.removeAllToolStripMenuItem.ToolTipText = null;
            this.removeAllToolStripMenuItem.VectorIcon = null;
            this.removeAllToolStripMenuItem.Visible = true;
            this.removeAllToolStripMenuItem.Click += new System.EventHandler(this.removeAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Available = true;
            this.toolStripSeparator14.Checked = false;
            this.toolStripSeparator14.CheckOnClick = false;
            this.toolStripSeparator14.DropDown = null;
            this.toolStripSeparator14.Enabled = true;
            this.toolStripSeparator14.Image = null;
            this.toolStripSeparator14.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator14.IsCheck = false;
            this.toolStripSeparator14.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Radio = false;
            this.toolStripSeparator14.SecondaryText = null;
            this.toolStripSeparator14.ShortcutKeyDisplayString = null;
            this.toolStripSeparator14.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator14.Size = new System.Drawing.Size(184, 6);
            this.toolStripSeparator14.Tag = null;
            this.toolStripSeparator14.Text = "";
            this.toolStripSeparator14.ToolTipText = null;
            this.toolStripSeparator14.VectorIcon = null;
            this.toolStripSeparator14.Visible = true;
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Available = true;
            this.toolStripMenuItem5.Checked = false;
            this.toolStripMenuItem5.CheckOnClick = true;
            this.toolStripMenuItem5.DropDown = null;
            this.toolStripMenuItem5.Enabled = true;
            this.toolStripMenuItem5.Image = null;
            this.toolStripMenuItem5.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripMenuItem5.IsCheck = false;
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Radio = false;
            this.toolStripMenuItem5.SecondaryText = null;
            this.toolStripMenuItem5.ShortcutKeyDisplayString = null;
            this.toolStripMenuItem5.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripMenuItem5.Size = new System.Drawing.Size(187, 22);
            this.toolStripMenuItem5.Tag = null;
            this.toolStripMenuItem5.Text = "Show icons";
            this.toolStripMenuItem5.ToolTipText = null;
            this.toolStripMenuItem5.VectorIcon = null;
            this.toolStripMenuItem5.Visible = true;
            this.toolStripMenuItem5.CheckedChanged += new System.EventHandler(this.toolStripMenuItem5_CheckedChanged);
            // 
            // sortByAlphabeticallyToolStripMenuItem
            // 
            this.sortByAlphabeticallyToolStripMenuItem.Available = true;
            this.sortByAlphabeticallyToolStripMenuItem.Checked = false;
            this.sortByAlphabeticallyToolStripMenuItem.CheckOnClick = true;
            this.sortByAlphabeticallyToolStripMenuItem.DropDown = null;
            this.sortByAlphabeticallyToolStripMenuItem.Enabled = true;
            this.sortByAlphabeticallyToolStripMenuItem.Image = null;
            this.sortByAlphabeticallyToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.sortByAlphabeticallyToolStripMenuItem.IsCheck = false;
            this.sortByAlphabeticallyToolStripMenuItem.Name = "sortByAlphabeticallyToolStripMenuItem";
            this.sortByAlphabeticallyToolStripMenuItem.Radio = false;
            this.sortByAlphabeticallyToolStripMenuItem.SecondaryText = null;
            this.sortByAlphabeticallyToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.sortByAlphabeticallyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.sortByAlphabeticallyToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.sortByAlphabeticallyToolStripMenuItem.Tag = null;
            this.sortByAlphabeticallyToolStripMenuItem.Text = "Sort by alphabetically";
            this.sortByAlphabeticallyToolStripMenuItem.ToolTipText = null;
            this.sortByAlphabeticallyToolStripMenuItem.VectorIcon = null;
            this.sortByAlphabeticallyToolStripMenuItem.Visible = true;
            this.sortByAlphabeticallyToolStripMenuItem.Click += new System.EventHandler(this.sortByAlphabeticallyToolStripMenuItem_Click);
            // 
            // showFavouritesBarToolStripMenuItem
            // 
            this.showFavouritesBarToolStripMenuItem.Available = true;
            this.showFavouritesBarToolStripMenuItem.Checked = false;
            this.showFavouritesBarToolStripMenuItem.CheckOnClick = true;
            this.showFavouritesBarToolStripMenuItem.DropDown = null;
            this.showFavouritesBarToolStripMenuItem.Enabled = true;
            this.showFavouritesBarToolStripMenuItem.Image = null;
            this.showFavouritesBarToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.showFavouritesBarToolStripMenuItem.IsCheck = false;
            this.showFavouritesBarToolStripMenuItem.Name = "showFavouritesBarToolStripMenuItem";
            this.showFavouritesBarToolStripMenuItem.Radio = false;
            this.showFavouritesBarToolStripMenuItem.SecondaryText = null;
            this.showFavouritesBarToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.showFavouritesBarToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.showFavouritesBarToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            this.showFavouritesBarToolStripMenuItem.Tag = null;
            this.showFavouritesBarToolStripMenuItem.Text = "Show favourites bar";
            this.showFavouritesBarToolStripMenuItem.ToolTipText = null;
            this.showFavouritesBarToolStripMenuItem.VectorIcon = null;
            this.showFavouritesBarToolStripMenuItem.Visible = true;
            this.showFavouritesBarToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showFavouritesBarToolStripMenuItem_CheckedChanged);
            // 
            // pnlFavourites
            // 
            resources.ApplyResources(this.pnlFavourites, "pnlFavourites");
            this.pnlFavourites.BackColor = System.Drawing.Color.Transparent;
            this.pnlFavourites.ForeColor = System.Drawing.Color.Black;
            this.pnlFavourites.Name = "pnlFavourites";
            this.pnlFavourites.Click += new System.EventHandler(this.btnGotoFavourite_Click);
            this.pnlFavourites.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.pnlFavourites_ControlAdded);
            this.pnlFavourites.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.pnlFavourites_ControlRemoved);
            // 
            // btnDownload
            // 
            resources.ApplyResources(this.btnDownload, "btnDownload");
            this.btnDownload.BackColor = System.Drawing.Color.Transparent;
            this.btnDownload.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnDownload.FlatAppearance.BorderSize = 0;
            this.btnDownload.FocusRingColor = System.Drawing.Color.Empty;
            this.btnDownload.ForeColor = System.Drawing.Color.White;
            this.btnDownload.IconSize = 20;
            this.btnDownload.InkColor = System.Drawing.Color.Empty;
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.UseToolbarGeometry = false;
            this.btnDownload.UseVisualStyleBackColor = false;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // mnuDownloadsDropDown
            // 
            this.mnuDownloadsDropDown.Items.AddRange(new ChromiumMenuItem[] {
            this.locationToolStripMenuItem1,
            this.changeLocationToolStripMenuItem1});
            this.mnuDownloadsDropDown.Appearance = null;
            this.mnuDownloadsDropDown.Enabled = true;
            this.mnuDownloadsDropDown.Name = "mnuDownloadsDropDown";
            this.mnuDownloadsDropDown.Size = new System.Drawing.Size(165, 48);
            this.mnuDownloadsDropDown.Tag = null;
            this.mnuDownloadsDropDown.Opening += new System.ComponentModel.CancelEventHandler(this.mnuDownloadsDropDown_Opening);
            // 
            // locationToolStripMenuItem1
            // 
            this.locationToolStripMenuItem1.Available = true;
            this.locationToolStripMenuItem1.Checked = false;
            this.locationToolStripMenuItem1.CheckOnClick = false;
            this.locationToolStripMenuItem1.Enabled = true;
            this.locationToolStripMenuItem1.Image = null;
            this.locationToolStripMenuItem1.ImageSize = new System.Drawing.Size(16, 16);
            this.locationToolStripMenuItem1.IsCheck = false;
            this.locationToolStripMenuItem1.Name = "locationToolStripMenuItem1";
            this.locationToolStripMenuItem1.Radio = false;
            this.locationToolStripMenuItem1.SecondaryText = null;
            this.locationToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.locationToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.locationToolStripMenuItem1.Size = new System.Drawing.Size(164, 22);
            this.locationToolStripMenuItem1.Tag = null;
            this.locationToolStripMenuItem1.Text = "Location:";
            this.locationToolStripMenuItem1.ToolTipText = null;
            this.locationToolStripMenuItem1.VectorIcon = null;
            this.locationToolStripMenuItem1.Visible = true;
            // 
            // changeLocationToolStripMenuItem1
            // 
            this.changeLocationToolStripMenuItem1.Available = true;
            this.changeLocationToolStripMenuItem1.Checked = false;
            this.changeLocationToolStripMenuItem1.CheckOnClick = false;
            this.changeLocationToolStripMenuItem1.DropDown = null;
            this.changeLocationToolStripMenuItem1.Enabled = true;
            this.changeLocationToolStripMenuItem1.Image = null;
            this.changeLocationToolStripMenuItem1.ImageSize = new System.Drawing.Size(16, 16);
            this.changeLocationToolStripMenuItem1.IsCheck = false;
            this.changeLocationToolStripMenuItem1.Name = "changeLocationToolStripMenuItem1";
            this.changeLocationToolStripMenuItem1.Radio = false;
            this.changeLocationToolStripMenuItem1.SecondaryText = null;
            this.changeLocationToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.changeLocationToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.changeLocationToolStripMenuItem1.Size = new System.Drawing.Size(164, 22);
            this.changeLocationToolStripMenuItem1.Tag = null;
            this.changeLocationToolStripMenuItem1.Text = "Change Location";
            this.changeLocationToolStripMenuItem1.ToolTipText = null;
            this.changeLocationToolStripMenuItem1.VectorIcon = null;
            this.changeLocationToolStripMenuItem1.Visible = true;
            this.changeLocationToolStripMenuItem1.Click += new System.EventHandler(this.changeLocationToolStripMenuItem1_Click);
            // 
            // downloadsToolStripMenuItem
            // 
            this.downloadsToolStripMenuItem.Available = true;
            this.downloadsToolStripMenuItem.Checked = false;
            this.downloadsToolStripMenuItem.CheckOnClick = false;
            this.downloadsToolStripMenuItem.DropDown = this.mnuDownloadsDropDown;
            this.downloadsToolStripMenuItem.Enabled = true;
            this.downloadsToolStripMenuItem.Image = null;
            this.downloadsToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.downloadsToolStripMenuItem.IsCheck = false;
            this.downloadsToolStripMenuItem.Name = "downloadsToolStripMenuItem";
            this.downloadsToolStripMenuItem.Radio = false;
            this.downloadsToolStripMenuItem.SecondaryText = null;
            this.downloadsToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.downloadsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.downloadsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.downloadsToolStripMenuItem.Tag = null;
            this.downloadsToolStripMenuItem.Text = "Downloads";
            this.downloadsToolStripMenuItem.ToolTipText = null;
            this.downloadsToolStripMenuItem.VectorIcon = null;
            this.downloadsToolStripMenuItem.Visible = true;
            this.downloadsToolStripMenuItem.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnAddFavourite
            // 
            resources.ApplyResources(this.btnAddFavourite, "btnAddFavourite");
            this.btnAddFavourite.BackColor = System.Drawing.Color.Transparent;
            this.btnAddFavourite.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnAddFavourite.FlatAppearance.BorderSize = 0;
            this.btnAddFavourite.FocusRingColor = System.Drawing.Color.Empty;
            this.btnAddFavourite.ForeColor = System.Drawing.Color.White;
            this.btnAddFavourite.IconSize = 20;
            this.btnAddFavourite.InkColor = System.Drawing.Color.Empty;
            this.btnAddFavourite.Name = "btnAddFavourite";
            this.btnAddFavourite.UseToolbarGeometry = false;
            this.btnAddFavourite.UseVisualStyleBackColor = false;
            this.btnAddFavourite.Click += new System.EventHandler(this.btnAddFavourite_Click);
            // 
            // btnForward
            // 
            this.btnForward.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnForward, "btnForward");
            this.btnForward.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnForward.FlatAppearance.BorderSize = 0;
            this.btnForward.FocusRingColor = System.Drawing.Color.Empty;
            this.btnForward.ForeColor = System.Drawing.Color.White;
            this.btnForward.IconSize = 20;
            this.btnForward.InkColor = System.Drawing.Color.Empty;
            this.btnForward.Name = "btnForward";
            this.btnForward.UseToolbarGeometry = false;
            this.btnForward.UseVisualStyleBackColor = false;
            this.btnForward.EnabledChanged += new System.EventHandler(this.btnForward_EnabledChanged);
            this.btnForward.Click += new System.EventHandler(this.btnForward_Click);
            // 
            // wvLoadingProgress
            // 
            this.wvLoadingProgress.AllowExternalDrop = true;
            this.wvLoadingProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.wvLoadingProgress.CreationProperties = null;
            this.wvLoadingProgress.Cursor = System.Windows.Forms.Cursors.Default;
            this.wvLoadingProgress.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.wvLoadingProgress, "wvLoadingProgress");
            this.wvLoadingProgress.Name = "wvLoadingProgress";
            this.wvLoadingProgress.ZoomFactor = 1.2D;
            // 
            // btnBack
            // 
            this.btnBack.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnBack, "btnBack");
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnBack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnBack.FocusRingColor = System.Drawing.Color.Empty;
            this.btnBack.ForeColor = System.Drawing.Color.Transparent;
            this.btnBack.IconSize = 20;
            this.btnBack.InkColor = System.Drawing.Color.Empty;
            this.btnBack.Name = "btnBack";
            this.btnBack.UseToolbarGeometry = false;
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.EnabledChanged += new System.EventHandler(this.btnBack_EnabledChanged);
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // mnuSearch
            // 
            this.mnuSearch.Items.AddRange(new ChromiumMenuItem[] {
            this.emojiToolStripMenuItem,
            this.toolStripSeparator9,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator11,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem1,
            this.pasteToolStripMenuItem,
            this.pasteAndGoToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator15,
            this.selectAllToolStripMenuItem,
            this.toolStripSeparator21,
            this.alwaysShowFullURLsToolStripMenuItem});
            this.mnuSearch.Appearance = null;
            this.mnuSearch.Enabled = true;
            this.mnuSearch.Name = "contextMenuStrip1";
            this.mnuSearch.Size = new System.Drawing.Size(192, 248);
            this.mnuSearch.Tag = null;
            this.mnuSearch.Opening += new System.ComponentModel.CancelEventHandler(this.mnuSearch_Opening);
            this.mnuSearch.Opened += new System.EventHandler(this.mnuSearch_Opened);
            // 
            // emojiToolStripMenuItem
            // 
            this.emojiToolStripMenuItem.Available = true;
            this.emojiToolStripMenuItem.Checked = false;
            this.emojiToolStripMenuItem.CheckOnClick = false;
            this.emojiToolStripMenuItem.DropDown = null;
            this.emojiToolStripMenuItem.Enabled = true;
            this.emojiToolStripMenuItem.Image = null;
            this.emojiToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.emojiToolStripMenuItem.IsCheck = false;
            this.emojiToolStripMenuItem.Name = "emojiToolStripMenuItem";
            this.emojiToolStripMenuItem.Radio = false;
            this.emojiToolStripMenuItem.SecondaryText = null;
            this.emojiToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.emojiToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.emojiToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.emojiToolStripMenuItem.Tag = null;
            this.emojiToolStripMenuItem.Text = "Emoji";
            this.emojiToolStripMenuItem.ToolTipText = null;
            this.emojiToolStripMenuItem.VectorIcon = null;
            this.emojiToolStripMenuItem.Visible = true;
            this.emojiToolStripMenuItem.Click += new System.EventHandler(this.emojiToolStripMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Available = true;
            this.toolStripSeparator9.Checked = false;
            this.toolStripSeparator9.CheckOnClick = false;
            this.toolStripSeparator9.DropDown = null;
            this.toolStripSeparator9.Enabled = true;
            this.toolStripSeparator9.Image = null;
            this.toolStripSeparator9.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator9.IsCheck = false;
            this.toolStripSeparator9.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Radio = false;
            this.toolStripSeparator9.SecondaryText = null;
            this.toolStripSeparator9.ShortcutKeyDisplayString = null;
            this.toolStripSeparator9.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator9.Size = new System.Drawing.Size(188, 6);
            this.toolStripSeparator9.Tag = null;
            this.toolStripSeparator9.Text = "";
            this.toolStripSeparator9.ToolTipText = null;
            this.toolStripSeparator9.VectorIcon = null;
            this.toolStripSeparator9.Visible = true;
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Available = true;
            this.undoToolStripMenuItem.Checked = false;
            this.undoToolStripMenuItem.CheckOnClick = false;
            this.undoToolStripMenuItem.DropDown = null;
            this.undoToolStripMenuItem.Enabled = true;
            this.undoToolStripMenuItem.Image = null;
            this.undoToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.undoToolStripMenuItem.IsCheck = false;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Radio = false;
            this.undoToolStripMenuItem.SecondaryText = null;
            this.undoToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.undoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.undoToolStripMenuItem.Tag = null;
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.ToolTipText = null;
            this.undoToolStripMenuItem.VectorIcon = null;
            this.undoToolStripMenuItem.Visible = true;
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Available = true;
            this.redoToolStripMenuItem.Checked = false;
            this.redoToolStripMenuItem.CheckOnClick = false;
            this.redoToolStripMenuItem.DropDown = null;
            this.redoToolStripMenuItem.Enabled = true;
            this.redoToolStripMenuItem.Image = null;
            this.redoToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.redoToolStripMenuItem.IsCheck = false;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Radio = false;
            this.redoToolStripMenuItem.SecondaryText = null;
            this.redoToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.redoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.redoToolStripMenuItem.Tag = null;
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.ToolTipText = null;
            this.redoToolStripMenuItem.VectorIcon = null;
            this.redoToolStripMenuItem.Visible = true;
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Available = true;
            this.toolStripSeparator11.Checked = false;
            this.toolStripSeparator11.CheckOnClick = false;
            this.toolStripSeparator11.DropDown = null;
            this.toolStripSeparator11.Enabled = true;
            this.toolStripSeparator11.Image = null;
            this.toolStripSeparator11.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator11.IsCheck = false;
            this.toolStripSeparator11.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Radio = false;
            this.toolStripSeparator11.SecondaryText = null;
            this.toolStripSeparator11.ShortcutKeyDisplayString = null;
            this.toolStripSeparator11.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator11.Size = new System.Drawing.Size(188, 6);
            this.toolStripSeparator11.Tag = null;
            this.toolStripSeparator11.Text = "";
            this.toolStripSeparator11.ToolTipText = null;
            this.toolStripSeparator11.VectorIcon = null;
            this.toolStripSeparator11.Visible = true;
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Available = true;
            this.cutToolStripMenuItem.Checked = false;
            this.cutToolStripMenuItem.CheckOnClick = false;
            this.cutToolStripMenuItem.DropDown = null;
            this.cutToolStripMenuItem.Enabled = true;
            this.cutToolStripMenuItem.Image = null;
            this.cutToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.cutToolStripMenuItem.IsCheck = false;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Radio = false;
            this.cutToolStripMenuItem.SecondaryText = null;
            this.cutToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.cutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.cutToolStripMenuItem.Tag = null;
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.ToolTipText = null;
            this.cutToolStripMenuItem.VectorIcon = null;
            this.cutToolStripMenuItem.Visible = true;
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem1
            // 
            this.copyToolStripMenuItem1.Available = true;
            this.copyToolStripMenuItem1.Checked = false;
            this.copyToolStripMenuItem1.CheckOnClick = false;
            this.copyToolStripMenuItem1.DropDown = null;
            this.copyToolStripMenuItem1.Enabled = true;
            this.copyToolStripMenuItem1.Image = null;
            this.copyToolStripMenuItem1.ImageSize = new System.Drawing.Size(16, 16);
            this.copyToolStripMenuItem1.IsCheck = false;
            this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            this.copyToolStripMenuItem1.Radio = false;
            this.copyToolStripMenuItem1.SecondaryText = null;
            this.copyToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.copyToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.copyToolStripMenuItem1.Size = new System.Drawing.Size(191, 22);
            this.copyToolStripMenuItem1.Tag = null;
            this.copyToolStripMenuItem1.Text = "Copy";
            this.copyToolStripMenuItem1.ToolTipText = null;
            this.copyToolStripMenuItem1.VectorIcon = null;
            this.copyToolStripMenuItem1.Visible = true;
            this.copyToolStripMenuItem1.Click += new System.EventHandler(this.copyToolStripMenuItem1_Click_1);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Available = true;
            this.pasteToolStripMenuItem.Checked = false;
            this.pasteToolStripMenuItem.CheckOnClick = false;
            this.pasteToolStripMenuItem.DropDown = null;
            this.pasteToolStripMenuItem.Enabled = true;
            this.pasteToolStripMenuItem.Image = null;
            this.pasteToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.pasteToolStripMenuItem.IsCheck = false;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Radio = false;
            this.pasteToolStripMenuItem.SecondaryText = null;
            this.pasteToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.pasteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.pasteToolStripMenuItem.Tag = null;
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.ToolTipText = null;
            this.pasteToolStripMenuItem.VectorIcon = null;
            this.pasteToolStripMenuItem.Visible = true;
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // pasteAndGoToolStripMenuItem
            // 
            this.pasteAndGoToolStripMenuItem.Available = true;
            this.pasteAndGoToolStripMenuItem.Checked = false;
            this.pasteAndGoToolStripMenuItem.CheckOnClick = false;
            this.pasteAndGoToolStripMenuItem.DropDown = null;
            this.pasteAndGoToolStripMenuItem.Enabled = true;
            this.pasteAndGoToolStripMenuItem.Image = null;
            this.pasteAndGoToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.pasteAndGoToolStripMenuItem.IsCheck = false;
            this.pasteAndGoToolStripMenuItem.Name = "pasteAndGoToolStripMenuItem";
            this.pasteAndGoToolStripMenuItem.Radio = false;
            this.pasteAndGoToolStripMenuItem.SecondaryText = null;
            this.pasteAndGoToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.pasteAndGoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.pasteAndGoToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.pasteAndGoToolStripMenuItem.Tag = null;
            this.pasteAndGoToolStripMenuItem.Text = "Paste and go";
            this.pasteAndGoToolStripMenuItem.ToolTipText = null;
            this.pasteAndGoToolStripMenuItem.VectorIcon = null;
            this.pasteAndGoToolStripMenuItem.Visible = true;
            this.pasteAndGoToolStripMenuItem.Click += new System.EventHandler(this.pasteAndGoToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Available = true;
            this.deleteToolStripMenuItem.Checked = false;
            this.deleteToolStripMenuItem.CheckOnClick = false;
            this.deleteToolStripMenuItem.DropDown = null;
            this.deleteToolStripMenuItem.Enabled = true;
            this.deleteToolStripMenuItem.Image = null;
            this.deleteToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.deleteToolStripMenuItem.IsCheck = false;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Radio = false;
            this.deleteToolStripMenuItem.SecondaryText = null;
            this.deleteToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.deleteToolStripMenuItem.Tag = null;
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.ToolTipText = null;
            this.deleteToolStripMenuItem.VectorIcon = null;
            this.deleteToolStripMenuItem.Visible = true;
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Available = true;
            this.toolStripSeparator15.Checked = false;
            this.toolStripSeparator15.CheckOnClick = false;
            this.toolStripSeparator15.DropDown = null;
            this.toolStripSeparator15.Enabled = true;
            this.toolStripSeparator15.Image = null;
            this.toolStripSeparator15.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator15.IsCheck = false;
            this.toolStripSeparator15.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Radio = false;
            this.toolStripSeparator15.SecondaryText = null;
            this.toolStripSeparator15.ShortcutKeyDisplayString = null;
            this.toolStripSeparator15.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator15.Size = new System.Drawing.Size(188, 6);
            this.toolStripSeparator15.Tag = null;
            this.toolStripSeparator15.Text = "";
            this.toolStripSeparator15.ToolTipText = null;
            this.toolStripSeparator15.VectorIcon = null;
            this.toolStripSeparator15.Visible = true;
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Available = true;
            this.selectAllToolStripMenuItem.Checked = false;
            this.selectAllToolStripMenuItem.CheckOnClick = false;
            this.selectAllToolStripMenuItem.DropDown = null;
            this.selectAllToolStripMenuItem.Enabled = true;
            this.selectAllToolStripMenuItem.Image = null;
            this.selectAllToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.selectAllToolStripMenuItem.IsCheck = false;
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Radio = false;
            this.selectAllToolStripMenuItem.SecondaryText = null;
            this.selectAllToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.selectAllToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.selectAllToolStripMenuItem.Tag = null;
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.ToolTipText = null;
            this.selectAllToolStripMenuItem.VectorIcon = null;
            this.selectAllToolStripMenuItem.Visible = true;
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Available = true;
            this.toolStripSeparator21.Checked = false;
            this.toolStripSeparator21.CheckOnClick = false;
            this.toolStripSeparator21.DropDown = null;
            this.toolStripSeparator21.Enabled = true;
            this.toolStripSeparator21.Image = null;
            this.toolStripSeparator21.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator21.IsCheck = false;
            this.toolStripSeparator21.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Radio = false;
            this.toolStripSeparator21.SecondaryText = null;
            this.toolStripSeparator21.ShortcutKeyDisplayString = null;
            this.toolStripSeparator21.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator21.Size = new System.Drawing.Size(188, 6);
            this.toolStripSeparator21.Tag = null;
            this.toolStripSeparator21.Text = "";
            this.toolStripSeparator21.ToolTipText = null;
            this.toolStripSeparator21.VectorIcon = null;
            this.toolStripSeparator21.Visible = true;
            // 
            // alwaysShowFullURLsToolStripMenuItem
            // 
            this.alwaysShowFullURLsToolStripMenuItem.Available = true;
            this.alwaysShowFullURLsToolStripMenuItem.Checked = false;
            this.alwaysShowFullURLsToolStripMenuItem.CheckOnClick = true;
            this.alwaysShowFullURLsToolStripMenuItem.DropDown = null;
            this.alwaysShowFullURLsToolStripMenuItem.Enabled = true;
            this.alwaysShowFullURLsToolStripMenuItem.Image = null;
            this.alwaysShowFullURLsToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.alwaysShowFullURLsToolStripMenuItem.IsCheck = false;
            this.alwaysShowFullURLsToolStripMenuItem.Name = "alwaysShowFullURLsToolStripMenuItem";
            this.alwaysShowFullURLsToolStripMenuItem.Radio = false;
            this.alwaysShowFullURLsToolStripMenuItem.SecondaryText = null;
            this.alwaysShowFullURLsToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.alwaysShowFullURLsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.alwaysShowFullURLsToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.alwaysShowFullURLsToolStripMenuItem.Tag = null;
            this.alwaysShowFullURLsToolStripMenuItem.Text = "Always show full URLs";
            this.alwaysShowFullURLsToolStripMenuItem.ToolTipText = null;
            this.alwaysShowFullURLsToolStripMenuItem.VectorIcon = null;
            this.alwaysShowFullURLsToolStripMenuItem.Visible = true;
            this.alwaysShowFullURLsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.alwaysShowFullURLsToolStripMenuItem_CheckedChanged);
            // 
            // txtWebAddress
            // 
            resources.ApplyResources(this.txtWebAddress, "txtWebAddress");
            this.txtWebAddress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.txtWebAddress.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtWebAddress.DetectUrls = false;
            this.txtWebAddress.Name = "txtWebAddress";
            this.txtWebAddress.BackColorChanged += new System.EventHandler(this.txtWebAddress_BackColorChanged);
            this.txtWebAddress.TextChanged += new System.EventHandler(this.txtWebAddress_TextChanged);
            this.txtWebAddress.Enter += new System.EventHandler(this.txtWebAddress_Enter);
            this.txtWebAddress.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtWebAddress_KeyUp);
            this.txtWebAddress.Leave += new System.EventHandler(this.txtWebAddress_Leave);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.btnRefresh, "btnRefresh");
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FocusRingColor = System.Drawing.Color.Empty;
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.IconSize = 20;
            this.btnRefresh.InkColor = System.Drawing.Color.Empty;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.UseToolbarGeometry = false;
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.Transparent;
            this.pnlTop.Controls.Add(this.btnSiteInformation);
            this.pnlTop.Controls.Add(this.wvLoadingProgress);
            this.pnlTop.Controls.Add(this.pnlFavourites);
            this.pnlTop.Controls.Add(this.txtWebAddress);
            this.pnlTop.Controls.Add(this.btnBack);
            this.pnlTop.Controls.Add(this.btnForward);
            this.pnlTop.Controls.Add(this.btnSettings);
            this.pnlTop.Controls.Add(this.btnDownload);
            this.pnlTop.Controls.Add(this.btnAddFavourite);
            this.pnlTop.Controls.Add(this.UrlBox);
            this.pnlTop.Controls.Add(this.btnRefresh);
            this.pnlTop.Controls.Add(this.UrlLeft);
            this.pnlTop.Controls.Add(this.UrlRight);
            resources.ApplyResources(this.pnlTop, "pnlTop");
            this.pnlTop.ForeColor = System.Drawing.Color.Transparent;
            this.pnlTop.Name = "pnlTop";
            // 
            // btnSiteInformation
            // 
            resources.ApplyResources(this.btnSiteInformation, "btnSiteInformation");
            this.btnSiteInformation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.btnSiteInformation.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSiteInformation.FocusRingColor = System.Drawing.Color.Empty;
            this.btnSiteInformation.ForeColor = System.Drawing.Color.Black;
            this.btnSiteInformation.HoldActiveOnClick = true;
            this.btnSiteInformation.InkColor = System.Drawing.Color.Empty;
            this.btnSiteInformation.Name = "btnSiteInformation";
            this.btnSiteInformation.UseToolbarGeometry = false;
            this.btnSiteInformation.UseVisualStyleBackColor = false;
            // 
            // btnSettings
            // 
            resources.ApplyResources(this.btnSettings, "btnSettings");
            this.btnSettings.BackColor = System.Drawing.Color.Transparent;
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FocusRingColor = System.Drawing.Color.Empty;
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.IconSize = 20;
            this.btnSettings.InkColor = System.Drawing.Color.Empty;
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.UseToolbarGeometry = false;
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // UrlBox
            // 
            resources.ApplyResources(this.UrlBox, "UrlBox");
            this.UrlBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.UrlBox.Name = "UrlBox";
            this.UrlBox.TabStop = false;
            // 
            // UrlLeft
            // 
            resources.ApplyResources(this.UrlLeft, "UrlLeft");
            this.UrlLeft.Name = "UrlLeft";
            this.UrlLeft.TabStop = false;
            // 
            // UrlRight
            // 
            resources.ApplyResources(this.UrlRight, "UrlRight");
            this.UrlRight.BackColor = System.Drawing.Color.Transparent;
            this.UrlRight.Name = "UrlRight";
            this.UrlRight.TabStop = false;
            // 
            // SettingsMenuStrip
            // 
            this.SettingsMenuStrip.Items.AddRange(new ChromiumMenuItem[] {
            this.newTabToolStripMenuItem1,
            this.newWindowToolStripMenuItem,
            this.changeProfileToolStripMenuItem,
            this.toolStripSeparator20,
            this.historyToolStripMenuItem,
            this.favouritesToolStripMenuItem,
            this.downloadsToolStripMenuItem,
            this.toolStripSeparator2,
            this.zoomToolStrip,
            this.toolStripSeparator1,
            this.findToolStripMenuItem,
            this.printToolStripMenuItem,
            this.openFileInBrowserToolStripMenuItem,
            this.expertsToolStripMenuItem,
            this.toolStripSeparator7,
            this.userDataToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.toolStripSeparator16,
            this.restartToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.SettingsMenuStrip.Appearance = null;
            this.SettingsMenuStrip.Enabled = true;
            this.SettingsMenuStrip.Name = "SettingsMenuStrip";
            this.SettingsMenuStrip.Size = new System.Drawing.Size(183, 369);
            this.SettingsMenuStrip.Tag = null;
            this.SettingsMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.SettingsMenuStrip_Opening);
            // 
            // newTabToolStripMenuItem1
            // 
            this.newTabToolStripMenuItem1.Available = true;
            this.newTabToolStripMenuItem1.Checked = false;
            this.newTabToolStripMenuItem1.CheckOnClick = false;
            this.newTabToolStripMenuItem1.DropDown = null;
            this.newTabToolStripMenuItem1.Enabled = true;
            this.newTabToolStripMenuItem1.Image = null;
            this.newTabToolStripMenuItem1.ImageSize = new System.Drawing.Size(16, 16);
            this.newTabToolStripMenuItem1.IsCheck = false;
            this.newTabToolStripMenuItem1.Name = "newTabToolStripMenuItem1";
            this.newTabToolStripMenuItem1.Radio = false;
            this.newTabToolStripMenuItem1.SecondaryText = null;
            this.newTabToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.newTabToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.newTabToolStripMenuItem1.Size = new System.Drawing.Size(182, 22);
            this.newTabToolStripMenuItem1.Tag = null;
            this.newTabToolStripMenuItem1.Text = "New tab";
            this.newTabToolStripMenuItem1.ToolTipText = null;
            this.newTabToolStripMenuItem1.VectorIcon = null;
            this.newTabToolStripMenuItem1.Visible = true;
            this.newTabToolStripMenuItem1.Click += new System.EventHandler(this.newTabToolStripMenuItem_Click);
            // 
            // newWindowToolStripMenuItem
            // 
            this.newWindowToolStripMenuItem.Available = true;
            this.newWindowToolStripMenuItem.Checked = false;
            this.newWindowToolStripMenuItem.CheckOnClick = false;
            this.newWindowToolStripMenuItem.DropDown = null;
            this.newWindowToolStripMenuItem.Enabled = true;
            this.newWindowToolStripMenuItem.Image = null;
            this.newWindowToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.newWindowToolStripMenuItem.IsCheck = false;
            this.newWindowToolStripMenuItem.Name = "newWindowToolStripMenuItem";
            this.newWindowToolStripMenuItem.Radio = false;
            this.newWindowToolStripMenuItem.SecondaryText = null;
            this.newWindowToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.newWindowToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.newWindowToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.newWindowToolStripMenuItem.Tag = null;
            this.newWindowToolStripMenuItem.Text = "New window";
            this.newWindowToolStripMenuItem.ToolTipText = null;
            this.newWindowToolStripMenuItem.VectorIcon = null;
            this.newWindowToolStripMenuItem.Visible = true;
            this.newWindowToolStripMenuItem.Click += new System.EventHandler(this.newWindowToolStripMenuItem_Click);
            // 
            // changeProfileToolStripMenuItem
            // 
            this.changeProfileToolStripMenuItem.Available = true;
            this.changeProfileToolStripMenuItem.Checked = false;
            this.changeProfileToolStripMenuItem.CheckOnClick = false;
            this.changeProfileToolStripMenuItem.DropDown = null;
            this.changeProfileToolStripMenuItem.Enabled = true;
            this.changeProfileToolStripMenuItem.Image = null;
            this.changeProfileToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.changeProfileToolStripMenuItem.IsCheck = false;
            this.changeProfileToolStripMenuItem.Name = "changeProfileToolStripMenuItem";
            this.changeProfileToolStripMenuItem.Radio = false;
            this.changeProfileToolStripMenuItem.SecondaryText = null;
            this.changeProfileToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.changeProfileToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.changeProfileToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.changeProfileToolStripMenuItem.Tag = null;
            this.changeProfileToolStripMenuItem.Text = "Profiles";
            this.changeProfileToolStripMenuItem.ToolTipText = null;
            this.changeProfileToolStripMenuItem.VectorIcon = null;
            this.changeProfileToolStripMenuItem.Visible = true;
            this.changeProfileToolStripMenuItem.Click += new System.EventHandler(this.changeProfileToolStripMenuItem_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Available = true;
            this.toolStripSeparator20.Checked = false;
            this.toolStripSeparator20.CheckOnClick = false;
            this.toolStripSeparator20.DropDown = null;
            this.toolStripSeparator20.Enabled = true;
            this.toolStripSeparator20.Image = null;
            this.toolStripSeparator20.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator20.IsCheck = false;
            this.toolStripSeparator20.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Radio = false;
            this.toolStripSeparator20.SecondaryText = null;
            this.toolStripSeparator20.ShortcutKeyDisplayString = null;
            this.toolStripSeparator20.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator20.Size = new System.Drawing.Size(179, 6);
            this.toolStripSeparator20.Tag = null;
            this.toolStripSeparator20.Text = "";
            this.toolStripSeparator20.ToolTipText = null;
            this.toolStripSeparator20.VectorIcon = null;
            this.toolStripSeparator20.Visible = true;
            // 
            // historyToolStripMenuItem
            // 
            this.historyToolStripMenuItem.Available = true;
            this.historyToolStripMenuItem.Checked = false;
            this.historyToolStripMenuItem.CheckOnClick = false;
            this.historyToolStripMenuItem.DropDown = this.mnuHistory;
            this.historyToolStripMenuItem.Enabled = true;
            this.historyToolStripMenuItem.Image = null;
            this.historyToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.historyToolStripMenuItem.IsCheck = false;
            this.historyToolStripMenuItem.Name = "historyToolStripMenuItem";
            this.historyToolStripMenuItem.Radio = false;
            this.historyToolStripMenuItem.SecondaryText = null;
            this.historyToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.historyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.historyToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.historyToolStripMenuItem.Tag = null;
            this.historyToolStripMenuItem.Text = "History";
            this.historyToolStripMenuItem.ToolTipText = null;
            this.historyToolStripMenuItem.VectorIcon = null;
            this.historyToolStripMenuItem.Visible = true;
            this.historyToolStripMenuItem.Click += new System.EventHandler(this.historyToolStripMenuItem_Click);
            // 
            // mnuHistory
            // 
            this.mnuHistory.Items.AddRange(new ChromiumMenuItem[] {
            this.historyToolStripMenuItem1,
            this.toolStripSeparator10});
            this.mnuHistory.Appearance = null;
            this.mnuHistory.Enabled = true;
            this.mnuHistory.Name = "mnuHistory";
            this.mnuHistory.Size = new System.Drawing.Size(113, 32);
            this.mnuHistory.Tag = null;
            this.mnuHistory.Opening += new System.ComponentModel.CancelEventHandler(this.mnuHistory_Opening);
            // 
            // historyToolStripMenuItem1
            // 
            this.historyToolStripMenuItem1.Available = true;
            this.historyToolStripMenuItem1.Checked = false;
            this.historyToolStripMenuItem1.CheckOnClick = false;
            this.historyToolStripMenuItem1.DropDown = null;
            this.historyToolStripMenuItem1.Enabled = true;
            this.historyToolStripMenuItem1.Image = null;
            this.historyToolStripMenuItem1.ImageSize = new System.Drawing.Size(16, 16);
            this.historyToolStripMenuItem1.IsCheck = false;
            this.historyToolStripMenuItem1.Name = "historyToolStripMenuItem1";
            this.historyToolStripMenuItem1.Radio = false;
            this.historyToolStripMenuItem1.SecondaryText = null;
            this.historyToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.historyToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.historyToolStripMenuItem1.Size = new System.Drawing.Size(112, 22);
            this.historyToolStripMenuItem1.Tag = null;
            this.historyToolStripMenuItem1.Text = "History";
            this.historyToolStripMenuItem1.ToolTipText = null;
            this.historyToolStripMenuItem1.VectorIcon = null;
            this.historyToolStripMenuItem1.Visible = true;
            this.historyToolStripMenuItem1.Click += new System.EventHandler(this.historyToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Available = true;
            this.toolStripSeparator10.Checked = false;
            this.toolStripSeparator10.CheckOnClick = false;
            this.toolStripSeparator10.DropDown = null;
            this.toolStripSeparator10.Enabled = true;
            this.toolStripSeparator10.Image = null;
            this.toolStripSeparator10.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator10.IsCheck = false;
            this.toolStripSeparator10.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Radio = false;
            this.toolStripSeparator10.SecondaryText = null;
            this.toolStripSeparator10.ShortcutKeyDisplayString = null;
            this.toolStripSeparator10.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator10.Size = new System.Drawing.Size(109, 6);
            this.toolStripSeparator10.Tag = null;
            this.toolStripSeparator10.Text = "";
            this.toolStripSeparator10.ToolTipText = null;
            this.toolStripSeparator10.VectorIcon = null;
            this.toolStripSeparator10.Visible = true;
            // 
            // favouritesToolStripMenuItem
            // 
            this.favouritesToolStripMenuItem.Available = true;
            this.favouritesToolStripMenuItem.Checked = false;
            this.favouritesToolStripMenuItem.CheckOnClick = false;
            this.favouritesToolStripMenuItem.DropDown = this.mnuFavourites;
            this.favouritesToolStripMenuItem.Enabled = true;
            this.favouritesToolStripMenuItem.Image = null;
            this.favouritesToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.favouritesToolStripMenuItem.IsCheck = false;
            this.favouritesToolStripMenuItem.Name = "favouritesToolStripMenuItem";
            this.favouritesToolStripMenuItem.Radio = false;
            this.favouritesToolStripMenuItem.SecondaryText = null;
            this.favouritesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.favouritesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.favouritesToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.favouritesToolStripMenuItem.Tag = null;
            this.favouritesToolStripMenuItem.Text = "Favourites";
            this.favouritesToolStripMenuItem.ToolTipText = null;
            this.favouritesToolStripMenuItem.VectorIcon = null;
            this.favouritesToolStripMenuItem.Visible = true;
            // 
            // mnuFavourites
            // 
            this.mnuFavourites.Items.AddRange(new ChromiumMenuItem[] {
            this.addFavouritesToolStripMenuItem,
            this.toolStripSeparator8});
            this.mnuFavourites.Appearance = null;
            this.mnuFavourites.Enabled = true;
            this.mnuFavourites.Name = "contextMenuStrip1";
            this.mnuFavourites.Size = new System.Drawing.Size(147, 32);
            this.mnuFavourites.Tag = null;
            this.mnuFavourites.Opening += new System.ComponentModel.CancelEventHandler(this.mnuFavourites_Opening);
            // 
            // addFavouritesToolStripMenuItem
            // 
            this.addFavouritesToolStripMenuItem.Available = true;
            this.addFavouritesToolStripMenuItem.Checked = false;
            this.addFavouritesToolStripMenuItem.CheckOnClick = false;
            this.addFavouritesToolStripMenuItem.DropDown = null;
            this.addFavouritesToolStripMenuItem.Enabled = true;
            this.addFavouritesToolStripMenuItem.Image = null;
            this.addFavouritesToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.addFavouritesToolStripMenuItem.IsCheck = false;
            this.addFavouritesToolStripMenuItem.Name = "addFavouritesToolStripMenuItem";
            this.addFavouritesToolStripMenuItem.Radio = false;
            this.addFavouritesToolStripMenuItem.SecondaryText = null;
            this.addFavouritesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.addFavouritesToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.addFavouritesToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.addFavouritesToolStripMenuItem.Tag = null;
            this.addFavouritesToolStripMenuItem.Text = "Add favourite";
            this.addFavouritesToolStripMenuItem.ToolTipText = null;
            this.addFavouritesToolStripMenuItem.VectorIcon = null;
            this.addFavouritesToolStripMenuItem.Visible = true;
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Available = true;
            this.toolStripSeparator8.Checked = false;
            this.toolStripSeparator8.CheckOnClick = false;
            this.toolStripSeparator8.DropDown = null;
            this.toolStripSeparator8.Enabled = true;
            this.toolStripSeparator8.Image = null;
            this.toolStripSeparator8.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator8.IsCheck = false;
            this.toolStripSeparator8.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Radio = false;
            this.toolStripSeparator8.SecondaryText = null;
            this.toolStripSeparator8.ShortcutKeyDisplayString = null;
            this.toolStripSeparator8.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator8.Size = new System.Drawing.Size(143, 6);
            this.toolStripSeparator8.Tag = null;
            this.toolStripSeparator8.Text = "";
            this.toolStripSeparator8.ToolTipText = null;
            this.toolStripSeparator8.VectorIcon = null;
            this.toolStripSeparator8.Visible = true;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Available = true;
            this.toolStripSeparator2.Checked = false;
            this.toolStripSeparator2.CheckOnClick = false;
            this.toolStripSeparator2.DropDown = null;
            this.toolStripSeparator2.Enabled = true;
            this.toolStripSeparator2.Image = null;
            this.toolStripSeparator2.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator2.IsCheck = false;
            this.toolStripSeparator2.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Radio = false;
            this.toolStripSeparator2.SecondaryText = null;
            this.toolStripSeparator2.ShortcutKeyDisplayString = null;
            this.toolStripSeparator2.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator2.Size = new System.Drawing.Size(179, 6);
            this.toolStripSeparator2.Tag = null;
            this.toolStripSeparator2.Text = "";
            this.toolStripSeparator2.ToolTipText = null;
            this.toolStripSeparator2.VectorIcon = null;
            this.toolStripSeparator2.Visible = true;
            // 
            // zoomToolStrip
            // 
            this.zoomToolStrip.Available = true;
            this.zoomToolStrip.Checked = false;
            this.zoomToolStrip.CheckOnClick = false;
            this.zoomToolStrip.DropDown = null;
            this.zoomToolStrip.Enabled = true;
            this.zoomToolStrip.Image = null;
            this.zoomToolStrip.ImageSize = new System.Drawing.Size(16, 16);
            this.zoomToolStrip.IsCheck = false;
            this.zoomToolStrip.Name = "zoomToolStrip";
            this.zoomToolStrip.Radio = false;
            this.zoomToolStrip.SecondaryText = null;
            this.zoomToolStrip.ShortcutKeyDisplayString = null;
            this.zoomToolStrip.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.zoomToolStrip.Size = new System.Drawing.Size(121, 23);
            this.zoomToolStrip.Tag = null;
            this.zoomToolStrip.Text = "Zoom";
            this.zoomToolStrip.ToolTipText = null;
            this.zoomToolStrip.VectorIcon = null;
            this.zoomToolStrip.Visible = true;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Available = true;
            this.toolStripSeparator1.Checked = false;
            this.toolStripSeparator1.CheckOnClick = false;
            this.toolStripSeparator1.DropDown = null;
            this.toolStripSeparator1.Enabled = true;
            this.toolStripSeparator1.Image = null;
            this.toolStripSeparator1.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator1.IsCheck = false;
            this.toolStripSeparator1.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Radio = false;
            this.toolStripSeparator1.SecondaryText = null;
            this.toolStripSeparator1.ShortcutKeyDisplayString = null;
            this.toolStripSeparator1.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator1.Size = new System.Drawing.Size(179, 6);
            this.toolStripSeparator1.Tag = null;
            this.toolStripSeparator1.Text = "";
            this.toolStripSeparator1.ToolTipText = null;
            this.toolStripSeparator1.VectorIcon = null;
            this.toolStripSeparator1.Visible = true;
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Available = true;
            this.findToolStripMenuItem.Checked = false;
            this.findToolStripMenuItem.CheckOnClick = false;
            this.findToolStripMenuItem.DropDown = null;
            this.findToolStripMenuItem.Enabled = true;
            this.findToolStripMenuItem.Image = null;
            this.findToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.findToolStripMenuItem.IsCheck = false;
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.Radio = false;
            this.findToolStripMenuItem.SecondaryText = null;
            this.findToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.findToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.findToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.findToolStripMenuItem.Tag = null;
            this.findToolStripMenuItem.Text = "Find";
            this.findToolStripMenuItem.ToolTipText = null;
            this.findToolStripMenuItem.VectorIcon = null;
            this.findToolStripMenuItem.Visible = true;
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Available = true;
            this.printToolStripMenuItem.Checked = false;
            this.printToolStripMenuItem.CheckOnClick = false;
            this.printToolStripMenuItem.DropDown = null;
            this.printToolStripMenuItem.Enabled = true;
            this.printToolStripMenuItem.Image = null;
            this.printToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.printToolStripMenuItem.IsCheck = false;
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Radio = false;
            this.printToolStripMenuItem.SecondaryText = null;
            this.printToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.printToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.printToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.printToolStripMenuItem.Tag = null;
            this.printToolStripMenuItem.Text = "Print";
            this.printToolStripMenuItem.ToolTipText = null;
            this.printToolStripMenuItem.VectorIcon = null;
            this.printToolStripMenuItem.Visible = true;
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // openFileInBrowserToolStripMenuItem
            // 
            this.openFileInBrowserToolStripMenuItem.Available = true;
            this.openFileInBrowserToolStripMenuItem.Checked = false;
            this.openFileInBrowserToolStripMenuItem.CheckOnClick = false;
            this.openFileInBrowserToolStripMenuItem.DropDown = null;
            this.openFileInBrowserToolStripMenuItem.Enabled = true;
            this.openFileInBrowserToolStripMenuItem.Image = null;
            this.openFileInBrowserToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.openFileInBrowserToolStripMenuItem.IsCheck = false;
            this.openFileInBrowserToolStripMenuItem.Name = "openFileInBrowserToolStripMenuItem";
            this.openFileInBrowserToolStripMenuItem.Radio = false;
            this.openFileInBrowserToolStripMenuItem.SecondaryText = null;
            this.openFileInBrowserToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openFileInBrowserToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.openFileInBrowserToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.openFileInBrowserToolStripMenuItem.Tag = null;
            this.openFileInBrowserToolStripMenuItem.Text = "Open File In Browser";
            this.openFileInBrowserToolStripMenuItem.ToolTipText = null;
            this.openFileInBrowserToolStripMenuItem.VectorIcon = null;
            this.openFileInBrowserToolStripMenuItem.Visible = true;
            this.openFileInBrowserToolStripMenuItem.Click += new System.EventHandler(this.openFileInBrowserToolStripMenuItem_Click);
            // 
            // expertsToolStripMenuItem
            // 
            this.expertsToolStripMenuItem.Available = true;
            this.expertsToolStripMenuItem.Checked = false;
            this.expertsToolStripMenuItem.CheckOnClick = false;
            this.expertsToolStripMenuItem.DropDown = this.mnuExperts;
            this.expertsToolStripMenuItem.Enabled = true;
            this.expertsToolStripMenuItem.Image = null;
            this.expertsToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.expertsToolStripMenuItem.IsCheck = false;
            this.expertsToolStripMenuItem.Name = "expertsToolStripMenuItem";
            this.expertsToolStripMenuItem.Radio = false;
            this.expertsToolStripMenuItem.SecondaryText = null;
            this.expertsToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.expertsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.expertsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.expertsToolStripMenuItem.Tag = null;
            this.expertsToolStripMenuItem.Text = "More tools";
            this.expertsToolStripMenuItem.ToolTipText = null;
            this.expertsToolStripMenuItem.VectorIcon = null;
            this.expertsToolStripMenuItem.Visible = true;
            // 
            // mnuExperts
            // 
            this.mnuExperts.Items.AddRange(new ChromiumMenuItem[] {
            this.nameWindowToolStripMenuItem,
            this.toolStripSeparator12,
            this.webview2TaskManagerToolStripMenuItem,
            this.toolStripSeparator13,
            this.inspectToolStripMenuItem});
            this.mnuExperts.Appearance = null;
            this.mnuExperts.Enabled = true;
            this.mnuExperts.Name = "mnuExperts";
            this.mnuExperts.Size = new System.Drawing.Size(161, 82);
            this.mnuExperts.Tag = null;
            // 
            // nameWindowToolStripMenuItem
            // 
            this.nameWindowToolStripMenuItem.Available = true;
            this.nameWindowToolStripMenuItem.Checked = false;
            this.nameWindowToolStripMenuItem.CheckOnClick = false;
            this.nameWindowToolStripMenuItem.DropDown = null;
            this.nameWindowToolStripMenuItem.Enabled = true;
            this.nameWindowToolStripMenuItem.Image = null;
            this.nameWindowToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.nameWindowToolStripMenuItem.IsCheck = false;
            this.nameWindowToolStripMenuItem.Name = "nameWindowToolStripMenuItem";
            this.nameWindowToolStripMenuItem.Radio = false;
            this.nameWindowToolStripMenuItem.SecondaryText = null;
            this.nameWindowToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.nameWindowToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.nameWindowToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.nameWindowToolStripMenuItem.Tag = null;
            this.nameWindowToolStripMenuItem.Text = "Name window...";
            this.nameWindowToolStripMenuItem.ToolTipText = null;
            this.nameWindowToolStripMenuItem.VectorIcon = null;
            this.nameWindowToolStripMenuItem.Visible = true;
            this.nameWindowToolStripMenuItem.Click += new System.EventHandler(this.nameWindowToolStripMenuItem_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Available = true;
            this.toolStripSeparator12.Checked = false;
            this.toolStripSeparator12.CheckOnClick = false;
            this.toolStripSeparator12.DropDown = null;
            this.toolStripSeparator12.Enabled = true;
            this.toolStripSeparator12.Image = null;
            this.toolStripSeparator12.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator12.IsCheck = false;
            this.toolStripSeparator12.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Radio = false;
            this.toolStripSeparator12.SecondaryText = null;
            this.toolStripSeparator12.ShortcutKeyDisplayString = null;
            this.toolStripSeparator12.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator12.Size = new System.Drawing.Size(157, 6);
            this.toolStripSeparator12.Tag = null;
            this.toolStripSeparator12.Text = "";
            this.toolStripSeparator12.ToolTipText = null;
            this.toolStripSeparator12.VectorIcon = null;
            this.toolStripSeparator12.Visible = true;
            // 
            // webview2TaskManagerToolStripMenuItem
            // 
            this.webview2TaskManagerToolStripMenuItem.Available = true;
            this.webview2TaskManagerToolStripMenuItem.Checked = false;
            this.webview2TaskManagerToolStripMenuItem.CheckOnClick = false;
            this.webview2TaskManagerToolStripMenuItem.DropDown = null;
            this.webview2TaskManagerToolStripMenuItem.Enabled = true;
            this.webview2TaskManagerToolStripMenuItem.Image = null;
            this.webview2TaskManagerToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.webview2TaskManagerToolStripMenuItem.IsCheck = false;
            this.webview2TaskManagerToolStripMenuItem.Name = "webview2TaskManagerToolStripMenuItem";
            this.webview2TaskManagerToolStripMenuItem.Radio = false;
            this.webview2TaskManagerToolStripMenuItem.SecondaryText = null;
            this.webview2TaskManagerToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.webview2TaskManagerToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.webview2TaskManagerToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.webview2TaskManagerToolStripMenuItem.Tag = null;
            this.webview2TaskManagerToolStripMenuItem.Text = "Task manager";
            this.webview2TaskManagerToolStripMenuItem.ToolTipText = null;
            this.webview2TaskManagerToolStripMenuItem.VectorIcon = null;
            this.webview2TaskManagerToolStripMenuItem.Visible = true;
            this.webview2TaskManagerToolStripMenuItem.Click += new System.EventHandler(this.taskManagerToolStripMenuItem_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Available = true;
            this.toolStripSeparator13.Checked = false;
            this.toolStripSeparator13.CheckOnClick = false;
            this.toolStripSeparator13.DropDown = null;
            this.toolStripSeparator13.Enabled = true;
            this.toolStripSeparator13.Image = null;
            this.toolStripSeparator13.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator13.IsCheck = false;
            this.toolStripSeparator13.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Radio = false;
            this.toolStripSeparator13.SecondaryText = null;
            this.toolStripSeparator13.ShortcutKeyDisplayString = null;
            this.toolStripSeparator13.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator13.Size = new System.Drawing.Size(157, 6);
            this.toolStripSeparator13.Tag = null;
            this.toolStripSeparator13.Text = "";
            this.toolStripSeparator13.ToolTipText = null;
            this.toolStripSeparator13.VectorIcon = null;
            this.toolStripSeparator13.Visible = true;
            // 
            // inspectToolStripMenuItem
            // 
            this.inspectToolStripMenuItem.Available = true;
            this.inspectToolStripMenuItem.Checked = false;
            this.inspectToolStripMenuItem.CheckOnClick = false;
            this.inspectToolStripMenuItem.DropDown = null;
            this.inspectToolStripMenuItem.Enabled = true;
            this.inspectToolStripMenuItem.Image = null;
            this.inspectToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.inspectToolStripMenuItem.IsCheck = false;
            this.inspectToolStripMenuItem.Name = "inspectToolStripMenuItem";
            this.inspectToolStripMenuItem.Radio = false;
            this.inspectToolStripMenuItem.SecondaryText = null;
            this.inspectToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.inspectToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.inspectToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.inspectToolStripMenuItem.Tag = null;
            this.inspectToolStripMenuItem.Text = "Developer Tools";
            this.inspectToolStripMenuItem.ToolTipText = null;
            this.inspectToolStripMenuItem.VectorIcon = null;
            this.inspectToolStripMenuItem.Visible = true;
            this.inspectToolStripMenuItem.Click += new System.EventHandler(this.inspectToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Available = true;
            this.toolStripSeparator7.Checked = false;
            this.toolStripSeparator7.CheckOnClick = false;
            this.toolStripSeparator7.DropDown = null;
            this.toolStripSeparator7.Enabled = true;
            this.toolStripSeparator7.Image = null;
            this.toolStripSeparator7.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator7.IsCheck = false;
            this.toolStripSeparator7.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Radio = false;
            this.toolStripSeparator7.SecondaryText = null;
            this.toolStripSeparator7.ShortcutKeyDisplayString = null;
            this.toolStripSeparator7.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator7.Size = new System.Drawing.Size(179, 6);
            this.toolStripSeparator7.Tag = null;
            this.toolStripSeparator7.Text = "";
            this.toolStripSeparator7.ToolTipText = null;
            this.toolStripSeparator7.VectorIcon = null;
            this.toolStripSeparator7.Visible = true;
            // 
            // userDataToolStripMenuItem
            // 
            this.userDataToolStripMenuItem.Available = true;
            this.userDataToolStripMenuItem.Checked = false;
            this.userDataToolStripMenuItem.CheckOnClick = false;
            this.userDataToolStripMenuItem.DropDown = this.mnuUserData;
            this.userDataToolStripMenuItem.Enabled = true;
            this.userDataToolStripMenuItem.Image = null;
            this.userDataToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.userDataToolStripMenuItem.IsCheck = false;
            this.userDataToolStripMenuItem.Name = "userDataToolStripMenuItem";
            this.userDataToolStripMenuItem.Radio = false;
            this.userDataToolStripMenuItem.SecondaryText = null;
            this.userDataToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.userDataToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.userDataToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.userDataToolStripMenuItem.Tag = null;
            this.userDataToolStripMenuItem.Text = "UserData";
            this.userDataToolStripMenuItem.ToolTipText = null;
            this.userDataToolStripMenuItem.VectorIcon = null;
            this.userDataToolStripMenuItem.Visible = true;
            // 
            // mnuUserData
            // 
            this.mnuUserData.Items.AddRange(new ChromiumMenuItem[] {
            this.openFolderToolStripMenuItem,
            this.toolStripSeparator6,
            this.resetToolStripMenuItem1});
            this.mnuUserData.Appearance = null;
            this.mnuUserData.Enabled = true;
            this.mnuUserData.Name = "mnuUserData";
            this.mnuUserData.Size = new System.Drawing.Size(219, 54);
            this.mnuUserData.Tag = null;
            // 
            // openFolderToolStripMenuItem
            // 
            this.openFolderToolStripMenuItem.Available = true;
            this.openFolderToolStripMenuItem.Checked = false;
            this.openFolderToolStripMenuItem.CheckOnClick = false;
            this.openFolderToolStripMenuItem.DropDown = null;
            this.openFolderToolStripMenuItem.Enabled = true;
            this.openFolderToolStripMenuItem.Image = null;
            this.openFolderToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.openFolderToolStripMenuItem.IsCheck = false;
            this.openFolderToolStripMenuItem.Name = "openFolderToolStripMenuItem";
            this.openFolderToolStripMenuItem.Radio = false;
            this.openFolderToolStripMenuItem.SecondaryText = null;
            this.openFolderToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.openFolderToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.openFolderToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this.openFolderToolStripMenuItem.Tag = null;
            this.openFolderToolStripMenuItem.Text = "Open Folder in File Explorer";
            this.openFolderToolStripMenuItem.ToolTipText = null;
            this.openFolderToolStripMenuItem.VectorIcon = null;
            this.openFolderToolStripMenuItem.Visible = true;
            this.openFolderToolStripMenuItem.Click += new System.EventHandler(this.openFolderToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Available = true;
            this.toolStripSeparator6.Checked = false;
            this.toolStripSeparator6.CheckOnClick = false;
            this.toolStripSeparator6.DropDown = null;
            this.toolStripSeparator6.Enabled = true;
            this.toolStripSeparator6.Image = null;
            this.toolStripSeparator6.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator6.IsCheck = false;
            this.toolStripSeparator6.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Radio = false;
            this.toolStripSeparator6.SecondaryText = null;
            this.toolStripSeparator6.ShortcutKeyDisplayString = null;
            this.toolStripSeparator6.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator6.Size = new System.Drawing.Size(215, 6);
            this.toolStripSeparator6.Tag = null;
            this.toolStripSeparator6.Text = "";
            this.toolStripSeparator6.ToolTipText = null;
            this.toolStripSeparator6.VectorIcon = null;
            this.toolStripSeparator6.Visible = true;
            // 
            // resetToolStripMenuItem1
            // 
            this.resetToolStripMenuItem1.Available = true;
            this.resetToolStripMenuItem1.Checked = false;
            this.resetToolStripMenuItem1.CheckOnClick = false;
            this.resetToolStripMenuItem1.DropDown = null;
            this.resetToolStripMenuItem1.Enabled = true;
            this.resetToolStripMenuItem1.Image = null;
            this.resetToolStripMenuItem1.ImageSize = new System.Drawing.Size(16, 16);
            this.resetToolStripMenuItem1.IsCheck = false;
            this.resetToolStripMenuItem1.Name = "resetToolStripMenuItem1";
            this.resetToolStripMenuItem1.Radio = false;
            this.resetToolStripMenuItem1.SecondaryText = null;
            this.resetToolStripMenuItem1.ShortcutKeyDisplayString = null;
            this.resetToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.resetToolStripMenuItem1.Size = new System.Drawing.Size(218, 22);
            this.resetToolStripMenuItem1.Tag = null;
            this.resetToolStripMenuItem1.Text = "Reset";
            this.resetToolStripMenuItem1.ToolTipText = null;
            this.resetToolStripMenuItem1.VectorIcon = null;
            this.resetToolStripMenuItem1.Visible = true;
            this.resetToolStripMenuItem1.Click += new System.EventHandler(this.resetToolStripMenuItem1_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Available = true;
            this.settingsToolStripMenuItem.Checked = false;
            this.settingsToolStripMenuItem.CheckOnClick = false;
            this.settingsToolStripMenuItem.DropDown = null;
            this.settingsToolStripMenuItem.Enabled = true;
            this.settingsToolStripMenuItem.Image = null;
            this.settingsToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.settingsToolStripMenuItem.IsCheck = false;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Radio = false;
            this.settingsToolStripMenuItem.SecondaryText = null;
            this.settingsToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.settingsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.settingsToolStripMenuItem.Tag = null;
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.ToolTipText = null;
            this.settingsToolStripMenuItem.VectorIcon = null;
            this.settingsToolStripMenuItem.Visible = true;
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.testToolStripMenuItem_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Available = true;
            this.toolStripSeparator16.Checked = false;
            this.toolStripSeparator16.CheckOnClick = false;
            this.toolStripSeparator16.DropDown = null;
            this.toolStripSeparator16.Enabled = true;
            this.toolStripSeparator16.Image = null;
            this.toolStripSeparator16.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripSeparator16.IsCheck = false;
            this.toolStripSeparator16.Kind = Quartz.Controls.ChromiumMenus.MenuSeparatorKind.Normal;
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Radio = false;
            this.toolStripSeparator16.SecondaryText = null;
            this.toolStripSeparator16.ShortcutKeyDisplayString = null;
            this.toolStripSeparator16.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripSeparator16.Size = new System.Drawing.Size(179, 6);
            this.toolStripSeparator16.Tag = null;
            this.toolStripSeparator16.Text = "";
            this.toolStripSeparator16.ToolTipText = null;
            this.toolStripSeparator16.VectorIcon = null;
            this.toolStripSeparator16.Visible = true;
            // 
            // restartToolStripMenuItem
            // 
            this.restartToolStripMenuItem.Available = true;
            this.restartToolStripMenuItem.Checked = false;
            this.restartToolStripMenuItem.CheckOnClick = false;
            this.restartToolStripMenuItem.DropDown = null;
            this.restartToolStripMenuItem.Enabled = true;
            this.restartToolStripMenuItem.Image = null;
            this.restartToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.restartToolStripMenuItem.IsCheck = false;
            this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            this.restartToolStripMenuItem.Radio = false;
            this.restartToolStripMenuItem.SecondaryText = null;
            this.restartToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.restartToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.restartToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.restartToolStripMenuItem.Tag = null;
            this.restartToolStripMenuItem.Text = "Restart";
            this.restartToolStripMenuItem.ToolTipText = null;
            this.restartToolStripMenuItem.VectorIcon = null;
            this.restartToolStripMenuItem.Visible = true;
            this.restartToolStripMenuItem.Click += new System.EventHandler(this.restartToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Available = true;
            this.exitToolStripMenuItem.Checked = false;
            this.exitToolStripMenuItem.CheckOnClick = false;
            this.exitToolStripMenuItem.DropDown = null;
            this.exitToolStripMenuItem.Enabled = true;
            this.exitToolStripMenuItem.Image = null;
            this.exitToolStripMenuItem.ImageSize = new System.Drawing.Size(16, 16);
            this.exitToolStripMenuItem.IsCheck = false;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Radio = false;
            this.exitToolStripMenuItem.SecondaryText = null;
            this.exitToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.exitToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.exitToolStripMenuItem.Tag = null;
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.ToolTipText = null;
            this.exitToolStripMenuItem.VectorIcon = null;
            this.exitToolStripMenuItem.Visible = true;
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // pnlDivider
            // 
            this.pnlDivider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(88)))), ((int)(((byte)(88)))));
            resources.ApplyResources(this.pnlDivider, "pnlDivider");
            this.pnlDivider.Name = "pnlDivider";
            // 
            // wvWebView1
            // 
            this.wvWebView1.AllowExternalDrop = true;
            this.wvWebView1.BackColor = System.Drawing.Color.White;
            this.wvWebView1.CreationProperties = null;
            this.wvWebView1.DefaultBackgroundColor = System.Drawing.Color.White;
            resources.ApplyResources(this.wvWebView1, "wvWebView1");
            this.wvWebView1.Name = "wvWebView1";
            this.wvWebView1.ZoomFactor = 1D;
            this.wvWebView1.CoreWebView2InitializationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs>(this.wvWebView1_CoreWebView2InitializationCompleted);
            this.wvWebView1.NavigationStarting += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs>(this.wvWebView1_NavigationStarting);
            this.wvWebView1.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this.wvWebView1_NavigationCompleted);
            this.wvWebView1.ZoomFactorChanged += new System.EventHandler<System.EventArgs>(this.wvWebView1_ZoomFactorChanged);
            // 
            // openFileDialog1
            // 
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            this.openFileDialog1.ValidateNames = false;
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Available = true;
            this.toolStripMenuItem2.Checked = false;
            this.toolStripMenuItem2.CheckOnClick = false;
            this.toolStripMenuItem2.DropDown = null;
            this.toolStripMenuItem2.Enabled = true;
            this.toolStripMenuItem2.Image = null;
            this.toolStripMenuItem2.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripMenuItem2.IsCheck = false;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Radio = false;
            this.toolStripMenuItem2.SecondaryText = null;
            this.toolStripMenuItem2.ShortcutKeyDisplayString = null;
            this.toolStripMenuItem2.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripMenuItem2.Size = new System.Drawing.Size(25, 20);
            this.toolStripMenuItem2.Tag = null;
            this.toolStripMenuItem2.Text = "2";
            this.toolStripMenuItem2.ToolTipText = null;
            this.toolStripMenuItem2.VectorIcon = null;
            this.toolStripMenuItem2.Visible = true;
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Available = true;
            this.toolStripMenuItem3.Checked = false;
            this.toolStripMenuItem3.CheckOnClick = false;
            this.toolStripMenuItem3.DropDown = null;
            this.toolStripMenuItem3.Enabled = true;
            this.toolStripMenuItem3.Image = null;
            this.toolStripMenuItem3.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripMenuItem3.IsCheck = false;
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Radio = false;
            this.toolStripMenuItem3.SecondaryText = null;
            this.toolStripMenuItem3.ShortcutKeyDisplayString = null;
            this.toolStripMenuItem3.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripMenuItem3.Size = new System.Drawing.Size(25, 20);
            this.toolStripMenuItem3.Tag = null;
            this.toolStripMenuItem3.Text = "3";
            this.toolStripMenuItem3.ToolTipText = null;
            this.toolStripMenuItem3.VectorIcon = null;
            this.toolStripMenuItem3.Visible = true;
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Available = true;
            this.toolStripMenuItem4.Checked = false;
            this.toolStripMenuItem4.CheckOnClick = false;
            this.toolStripMenuItem4.DropDown = null;
            this.toolStripMenuItem4.Enabled = true;
            this.toolStripMenuItem4.Image = null;
            this.toolStripMenuItem4.ImageSize = new System.Drawing.Size(16, 16);
            this.toolStripMenuItem4.IsCheck = false;
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Radio = false;
            this.toolStripMenuItem4.SecondaryText = null;
            this.toolStripMenuItem4.ShortcutKeyDisplayString = null;
            this.toolStripMenuItem4.ShortcutKeys = System.Windows.Forms.Keys.None;
            this.toolStripMenuItem4.Size = new System.Drawing.Size(25, 20);
            this.toolStripMenuItem4.Tag = null;
            this.toolStripMenuItem4.Text = "4";
            this.toolStripMenuItem4.ToolTipText = null;
            this.toolStripMenuItem4.VectorIcon = null;
            this.toolStripMenuItem4.Visible = true;
            // 
            // notifyIcon1
            // 
            resources.ApplyResources(this.notifyIcon1, "notifyIcon1");
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.Transparent;
            this.pnlBottom.Controls.Add(this.wvWebView1);
            this.pnlBottom.Controls.Add(this.pnlDivider);
            resources.ApplyResources(this.pnlBottom, "pnlBottom");
            this.pnlBottom.ForeColor = System.Drawing.Color.Transparent;
            this.pnlBottom.Name = "pnlBottom";
            // 
            // Browser
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.ForeColor = System.Drawing.Color.Black;
            this.Name = "Browser";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Browser_FormClosing);
            this.Load += new System.EventHandler(this.Browser_Load);
            this.LocationChanged += new System.EventHandler(this.Browser_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.Browser_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.wvLoadingProgress)).EndInit();
            this.pnlTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.UrlBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UrlLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UrlRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wvWebView1)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private ChromiumMenu mnuMenu;
        private ChromiumMenuItem removeToolStripMenuItem;
        private Quartz.Controls.BrowserToolbarPanel pnlTop;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.PictureBox UrlLeft;
        private System.Windows.Forms.PictureBox UrlRight;
        private System.Windows.Forms.PictureBox UrlBox;
        private ChromiumMenuItem removeAllToolStripMenuItem;
        private System.Windows.Forms.Panel pnlDivider;
        private ChromiumMenuItem modifyToolStripMenuItem;
        private ChromiumMenuItem historyToolStripMenuItem;
        private ChromiumMenuItem changeProfileToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator2;
        private ChromiumMenuSeparator toolStripSeparator1;
        private ChromiumMenuItem openFileInBrowserToolStripMenuItem;
        private ChromiumMenuItem exitToolStripMenuItem;
        private ChromiumMenuItem copyToolStripMenuItem;
        private ChromiumMenu mnuDownloadsDropDown;
        private ChromiumMenuItem locationToolStripMenuItem1;
        private ChromiumMenuItem changeLocationToolStripMenuItem1;
        public Microsoft.Web.WebView2.WinForms.WebView2 wvLoadingProgress;
        private ChromiumMenuItem toolStripMenuItem2;
        private ChromiumMenuItem toolStripMenuItem3;
        private ChromiumMenuItem toolStripMenuItem4;
        private ChromiumMenuItem settingsToolStripMenuItem;
        private ChromiumMenuItem downloadsToolStripMenuItem;
        private ChromiumMenuItem expertsToolStripMenuItem;
        private ChromiumMenu mnuExperts;
        private ChromiumMenuItem inspectToolStripMenuItem;
        private ChromiumMenuItem webview2TaskManagerToolStripMenuItem;
        private ChromiumMenuItem restartToolStripMenuItem;
        public System.Windows.Forms.NotifyIcon notifyIcon1;
        public ChromiumMenu SettingsMenuStrip;
        public Microsoft.Web.WebView2.WinForms.WebView2 wvWebView1;
        private ChromiumMenu mnuHistory;
        private ChromiumMenuItem historyToolStripMenuItem1;
        private ChromiumMenuSeparator toolStripSeparator10;
        private ChromiumMenuItem userDataToolStripMenuItem;
        private ChromiumMenu mnuUserData;
        private ChromiumMenuItem resetToolStripMenuItem1;
        private System.Windows.Forms.Panel pnlBottom;
        private ChromiumMenuSeparator toolStripSeparator14;
        public Quartz.Controls.ChromiumButton btnDownload;
        public Quartz.Controls.ChromiumButton btnAddFavourite;
        public Quartz.Controls.ChromiumButton btnForward;
        public Quartz.Controls.ChromiumButton btnSettings;
        public Quartz.Controls.ChromiumButton btnBack;
        public Quartz.Controls.ChromiumButton btnRefresh;
        private ChromiumZoomMenuItem zoomToolStrip;
        private ChromiumMenuItem toolStripMenuItem5;
        private ChromiumMenuSeparator toolStripSeparator4;
        private ChromiumMenu mnuSearch;
        private ChromiumMenuItem sortByAlphabeticallyToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator18;
        private ChromiumMenuItem openInNewTabToolStripMenuItem;
        private ChromiumMenuItem openInNewWindowToolStripMenuItem;
        private ChromiumMenuItem openToolStripMenuItem;
        private ChromiumMenuItem newTabToolStripMenuItem1;
        private ChromiumMenuItem newWindowToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator20;
        private ChromiumMenuItem emojiToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator9;
        private ChromiumMenuItem undoToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator11;
        private ChromiumMenuItem cutToolStripMenuItem;
        private ChromiumMenuItem copyToolStripMenuItem1;
        private ChromiumMenuItem pasteToolStripMenuItem;
        private ChromiumMenuItem pasteAndGoToolStripMenuItem;
        private ChromiumMenuItem deleteToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator15;
        private ChromiumMenuItem selectAllToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator21;
        private ChromiumMenuItem alwaysShowFullURLsToolStripMenuItem;
        private ChromiumMenuItem openFolderToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator6;
        private ChromiumMenuItem redoToolStripMenuItem;
        public System.Windows.Forms.RichTextBox txtWebAddress;
        private ChromiumMenuSeparator toolStripSeparator3;
        private ChromiumMenuItem cutToolStripMenuItem1;
        private ChromiumMenuItem pasteToolStripMenuItem1;
        private ChromiumMenuItem showFavouritesBarToolStripMenuItem;
        public Quartz.Controls.FavouritesBar pnlFavourites;
        private ChromiumMenuItem printToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator7;
        private ChromiumMenuItem findToolStripMenuItem;
        private ChromiumMenuItem nameWindowToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator12;
        private ChromiumMenuSeparator toolStripSeparator13;
        private ChromiumMenuItem favouritesToolStripMenuItem;
        private ChromiumMenu mnuFavourites;
        private ChromiumMenuItem addFavouritesToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator8;
        private ChromiumMenuSeparator toolStripSeparator16;
        private Quartz.Controls.SiteInfoButton btnSiteInformation;
    }
}

