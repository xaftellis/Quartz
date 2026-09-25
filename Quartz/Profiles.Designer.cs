using Quartz.Controls.ChromiumMenus;
namespace Quartz
{
    partial class Profiles
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
            this.pnlProfiles = new System.Windows.Forms.FlowLayoutPanel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.toolStripSeparator1 = new ChromiumMenuSeparator();
            this.toolStripSeparator5 = new ChromiumMenuSeparator();
            this.cbSortBy = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.button1 = new Quartz.Controls.ChromiumButton();
            this.btnCreate = new Quartz.Controls.ChromiumButton();
            this.ContextMenuStripProfiles = new ChromiumMenu(this.components);
            this.setDefaultToolStripMenuItem = new ChromiumMenuItem();
            this.editToolStripMenuItem = new ChromiumMenuItem();
            this.toolStripSeparator3 = new ChromiumMenuSeparator();
            this.deleteToolStripMenuItem = new ChromiumMenuItem();
            this.ContextMenuStripProfiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlProfiles
            // 
            this.pnlProfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlProfiles.AutoScroll = true;
            this.pnlProfiles.Location = new System.Drawing.Point(12, 65);
            this.pnlProfiles.Name = "pnlProfiles";
            this.pnlProfiles.Size = new System.Drawing.Size(776, 327);
            this.pnlProfiles.TabIndex = 0;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(12, 39);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(776, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(152, 6);
            // 
            // cbSortBy
            // 
            this.cbSortBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSortBy.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbSortBy.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbSortBy.FormattingEnabled = true;
            this.cbSortBy.Items.AddRange(new object[] {
            "Alphabetically (Default)",
            "Last Active",
            "Date Created"});
            this.cbSortBy.Location = new System.Drawing.Point(58, 398);
            this.cbSortBy.Name = "cbSortBy";
            this.cbSortBy.Size = new System.Drawing.Size(121, 21);
            this.cbSortBy.TabIndex = 4;
            this.cbSortBy.SelectedIndexChanged += new System.EventHandler(this.cbSortBy_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 401);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Sort by:";
            // 
            // LabelTitle
            // 
            this.LabelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.LabelTitle.Font = new System.Drawing.Font("Segoe UI", 20.25F);
            this.LabelTitle.Location = new System.Drawing.Point(0, 0);
            this.LabelTitle.Name = "LabelTitle";
            this.LabelTitle.Size = new System.Drawing.Size(800, 36);
            this.LabelTitle.TabIndex = 0;
            this.LabelTitle.Text = "Who\'s Browsing Today";
            this.LabelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.FocusRingColor = System.Drawing.Color.Empty;
            this.button1.InkColor = System.Drawing.Color.Empty;
            this.button1.Location = new System.Drawing.Point(563, 395);
            this.button1.Name = "button1";
            this.button1.Padding = new System.Windows.Forms.Padding(6);
            this.button1.Size = new System.Drawing.Size(137, 28);
            this.button1.TabIndex = 3;
            this.button1.Text = "Create Disposable Profile";
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnCreate
            // 
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreate.FocusRingColor = System.Drawing.Color.Empty;
            this.btnCreate.InkColor = System.Drawing.Color.Empty;
            this.btnCreate.Location = new System.Drawing.Point(706, 395);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Padding = new System.Windows.Forms.Padding(6);
            this.btnCreate.Size = new System.Drawing.Size(82, 28);
            this.btnCreate.TabIndex = 1;
            this.btnCreate.Text = "Create Profile";
            this.btnCreate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // ContextMenuStripProfiles
            // 
            this.ContextMenuStripProfiles.Items.AddRange(new ChromiumMenuItem[] {
            this.setDefaultToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolStripSeparator3,
            this.deleteToolStripMenuItem});
            this.ContextMenuStripProfiles.Name = "ChromiumMenu";
            this.ContextMenuStripProfiles.Size = new System.Drawing.Size(131, 76);
            this.ContextMenuStripProfiles.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStripProfiles_Opening);
            // 
            // setDefaultToolStripMenuItem
            // 
            this.setDefaultToolStripMenuItem.Name = "setDefaultToolStripMenuItem";
            this.setDefaultToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.setDefaultToolStripMenuItem.Text = "Set default";
            this.setDefaultToolStripMenuItem.Click += new System.EventHandler(this.setDefaultToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(127, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // Profiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(800, 433);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbSortBy);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.pnlProfiles);
            this.KeyPreview = true;
            this.Name = "Profiles";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Profiles";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Profiles_FormClosing);
            this.Load += new System.EventHandler(this.Profiles_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Profiles_KeyUp);
            this.ContextMenuStripProfiles.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Quartz.Controls.ChromiumButton btnCreate;
        private ChromiumMenu ContextMenuStripProfiles;
        private ChromiumMenuItem deleteToolStripMenuItem;
        private ChromiumMenuItem editToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator3;
        private System.Windows.Forms.FlowLayoutPanel pnlProfiles;
        private System.Windows.Forms.TextBox txtSearch;
        private Quartz.Controls.ChromiumButton button1;
        private ChromiumMenuItem setDefaultToolStripMenuItem;
        private ChromiumMenuSeparator toolStripSeparator1;
        private ChromiumMenuSeparator toolStripSeparator5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbSortBy;
        private System.Windows.Forms.Label LabelTitle;
    }
}