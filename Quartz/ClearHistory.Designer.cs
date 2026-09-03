namespace Quartz
{
    partial class ClearHistory
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
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlBottomDivider = new System.Windows.Forms.Panel();
            this.pnlNotice = new System.Windows.Forms.Panel();
            this.lblNotice = new System.Windows.Forms.Label();
            this.pnlItems = new System.Windows.Forms.Panel();
            this.lblAutofillInfo = new System.Windows.Forms.Label();
            this.chkAutofill = new System.Windows.Forms.CheckBox();
            this.lblPasswordsInfo = new System.Windows.Forms.Label();
            this.chkPasswords = new System.Windows.Forms.CheckBox();
            this.lblCacheInfo = new System.Windows.Forms.Label();
            this.chkCache = new System.Windows.Forms.CheckBox();
            this.lblCookiesInfo = new System.Windows.Forms.Label();
            this.chkCookies = new System.Windows.Forms.CheckBox();
            this.lblDownloadHistoryInfo = new System.Windows.Forms.Label();
            this.chkDownloadHistory = new System.Windows.Forms.CheckBox();
            this.lblBrowsingHistoryInfo = new System.Windows.Forms.Label();
            this.chkBrowsingHistory = new System.Windows.Forms.CheckBox();
            this.pnlTopDivider = new System.Windows.Forms.Panel();
            this.cboTimeRange = new System.Windows.Forms.ComboBox();
            this.lblTimeRange = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlNotice.SuspendLayout();
            this.pnlItems.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(428, 599);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(87, 34);
            this.btnDelete.TabIndex = 22;
            this.btnDelete.Text = "Delete data";
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(338, 599);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 34);
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Location = new System.Drawing.Point(25, 601);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(290, 34);
            this.lblStatus.TabIndex = 20;
            this.lblStatus.Text = "Loading browsing data information...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlBottomDivider
            // 
            this.pnlBottomDivider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBottomDivider.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pnlBottomDivider.Location = new System.Drawing.Point(0, 584);
            this.pnlBottomDivider.Name = "pnlBottomDivider";
            this.pnlBottomDivider.Size = new System.Drawing.Size(540, 1);
            this.pnlBottomDivider.TabIndex = 19;
            // 
            // pnlNotice
            // 
            this.pnlNotice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlNotice.Controls.Add(this.lblNotice);
            this.pnlNotice.Location = new System.Drawing.Point(25, 515);
            this.pnlNotice.Name = "pnlNotice";
            this.pnlNotice.Size = new System.Drawing.Size(490, 54);
            this.pnlNotice.TabIndex = 18;
            // 
            // lblNotice
            // 
            this.lblNotice.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNotice.Location = new System.Drawing.Point(13, 9);
            this.lblNotice.Name = "lblNotice";
            this.lblNotice.Size = new System.Drawing.Size(464, 36);
            this.lblNotice.TabIndex = 0;
            this.lblNotice.Text = "Cookies and site data can sign you out. Saved passwords are only removed when tha" +
    "t option is selected.";
            // 
            // pnlItems
            // 
            this.pnlItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlItems.AutoScroll = true;
            this.pnlItems.Controls.Add(this.lblAutofillInfo);
            this.pnlItems.Controls.Add(this.chkAutofill);
            this.pnlItems.Controls.Add(this.lblPasswordsInfo);
            this.pnlItems.Controls.Add(this.chkPasswords);
            this.pnlItems.Controls.Add(this.lblCacheInfo);
            this.pnlItems.Controls.Add(this.chkCache);
            this.pnlItems.Controls.Add(this.lblCookiesInfo);
            this.pnlItems.Controls.Add(this.chkCookies);
            this.pnlItems.Controls.Add(this.lblDownloadHistoryInfo);
            this.pnlItems.Controls.Add(this.chkDownloadHistory);
            this.pnlItems.Controls.Add(this.lblBrowsingHistoryInfo);
            this.pnlItems.Controls.Add(this.chkBrowsingHistory);
            this.pnlItems.Location = new System.Drawing.Point(25, 152);
            this.pnlItems.Name = "pnlItems";
            this.pnlItems.Size = new System.Drawing.Size(490, 351);
            this.pnlItems.TabIndex = 17;
            // 
            // lblAutofillInfo
            // 
            this.lblAutofillInfo.Location = new System.Drawing.Point(29, 318);
            this.lblAutofillInfo.Name = "lblAutofillInfo";
            this.lblAutofillInfo.Size = new System.Drawing.Size(435, 31);
            this.lblAutofillInfo.TabIndex = 11;
            this.lblAutofillInfo.Text = "Addresses, payment details, and other saved form entries";
            // 
            // chkAutofill
            // 
            this.chkAutofill.AutoSize = true;
            this.chkAutofill.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkAutofill.Location = new System.Drawing.Point(3, 290);
            this.chkAutofill.Name = "chkAutofill";
            this.chkAutofill.Size = new System.Drawing.Size(135, 23);
            this.chkAutofill.TabIndex = 10;
            this.chkAutofill.Text = "Autofill form data";
            this.chkAutofill.UseVisualStyleBackColor = true;
            // 
            // lblPasswordsInfo
            // 
            this.lblPasswordsInfo.Location = new System.Drawing.Point(29, 260);
            this.lblPasswordsInfo.Name = "lblPasswordsInfo";
            this.lblPasswordsInfo.Size = new System.Drawing.Size(435, 31);
            this.lblPasswordsInfo.TabIndex = 9;
            this.lblPasswordsInfo.Text = "Saved sign-in details stored by this browser profile";
            // 
            // chkPasswords
            // 
            this.chkPasswords.AutoSize = true;
            this.chkPasswords.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkPasswords.Location = new System.Drawing.Point(3, 232);
            this.chkPasswords.Name = "chkPasswords";
            this.chkPasswords.Size = new System.Drawing.Size(132, 23);
            this.chkPasswords.TabIndex = 8;
            this.chkPasswords.Text = "Saved passwords";
            this.chkPasswords.UseVisualStyleBackColor = true;
            // 
            // lblCacheInfo
            // 
            this.lblCacheInfo.Location = new System.Drawing.Point(29, 202);
            this.lblCacheInfo.Name = "lblCacheInfo";
            this.lblCacheInfo.Size = new System.Drawing.Size(435, 31);
            this.lblCacheInfo.TabIndex = 7;
            this.lblCacheInfo.Text = "Calculating cached data...";
            // 
            // chkCache
            // 
            this.chkCache.AutoSize = true;
            this.chkCache.Checked = true;
            this.chkCache.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCache.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkCache.Location = new System.Drawing.Point(3, 174);
            this.chkCache.Name = "chkCache";
            this.chkCache.Size = new System.Drawing.Size(174, 23);
            this.chkCache.TabIndex = 6;
            this.chkCache.Text = "Cached images and files";
            this.chkCache.UseVisualStyleBackColor = true;
            // 
            // lblCookiesInfo
            // 
            this.lblCookiesInfo.Location = new System.Drawing.Point(29, 144);
            this.lblCookiesInfo.Name = "lblCookiesInfo";
            this.lblCookiesInfo.Size = new System.Drawing.Size(435, 31);
            this.lblCookiesInfo.TabIndex = 5;
            this.lblCookiesInfo.Text = "Calculating cookie and site data...";
            // 
            // chkCookies
            // 
            this.chkCookies.AutoSize = true;
            this.chkCookies.Checked = true;
            this.chkCookies.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCookies.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkCookies.Location = new System.Drawing.Point(3, 116);
            this.chkCookies.Name = "chkCookies";
            this.chkCookies.Size = new System.Drawing.Size(159, 23);
            this.chkCookies.TabIndex = 4;
            this.chkCookies.Text = "Cookies and site data";
            this.chkCookies.UseVisualStyleBackColor = true;
            // 
            // lblDownloadHistoryInfo
            // 
            this.lblDownloadHistoryInfo.Location = new System.Drawing.Point(29, 86);
            this.lblDownloadHistoryInfo.Name = "lblDownloadHistoryInfo";
            this.lblDownloadHistoryInfo.Size = new System.Drawing.Size(435, 31);
            this.lblDownloadHistoryInfo.TabIndex = 3;
            this.lblDownloadHistoryInfo.Text = "Clears the download list; downloaded files stay on this device";
            // 
            // chkDownloadHistory
            // 
            this.chkDownloadHistory.AutoSize = true;
            this.chkDownloadHistory.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkDownloadHistory.Location = new System.Drawing.Point(3, 58);
            this.chkDownloadHistory.Name = "chkDownloadHistory";
            this.chkDownloadHistory.Size = new System.Drawing.Size(136, 23);
            this.chkDownloadHistory.TabIndex = 2;
            this.chkDownloadHistory.Text = "Download history";
            this.chkDownloadHistory.UseVisualStyleBackColor = true;
            // 
            // lblBrowsingHistoryInfo
            // 
            this.lblBrowsingHistoryInfo.Location = new System.Drawing.Point(29, 28);
            this.lblBrowsingHistoryInfo.Name = "lblBrowsingHistoryInfo";
            this.lblBrowsingHistoryInfo.Size = new System.Drawing.Size(435, 31);
            this.lblBrowsingHistoryInfo.TabIndex = 1;
            this.lblBrowsingHistoryInfo.Text = "Calculating Quartz history entries...";
            // 
            // chkBrowsingHistory
            // 
            this.chkBrowsingHistory.AutoSize = true;
            this.chkBrowsingHistory.Checked = true;
            this.chkBrowsingHistory.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBrowsingHistory.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkBrowsingHistory.Location = new System.Drawing.Point(3, 0);
            this.chkBrowsingHistory.Name = "chkBrowsingHistory";
            this.chkBrowsingHistory.Size = new System.Drawing.Size(130, 23);
            this.chkBrowsingHistory.TabIndex = 0;
            this.chkBrowsingHistory.Text = "Browsing history";
            this.chkBrowsingHistory.UseVisualStyleBackColor = true;
            // 
            // pnlTopDivider
            // 
            this.pnlTopDivider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlTopDivider.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pnlTopDivider.Location = new System.Drawing.Point(0, 139);
            this.pnlTopDivider.Name = "pnlTopDivider";
            this.pnlTopDivider.Size = new System.Drawing.Size(540, 1);
            this.pnlTopDivider.TabIndex = 16;
            // 
            // cboTimeRange
            // 
            this.cboTimeRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTimeRange.FormattingEnabled = true;
            this.cboTimeRange.Items.AddRange(new object[] {
            "Last hour",
            "Last 24 hours",
            "Last 7 days",
            "Last 4 weeks",
            "All time"});
            this.cboTimeRange.Location = new System.Drawing.Point(124, 101);
            this.cboTimeRange.Name = "cboTimeRange";
            this.cboTimeRange.Size = new System.Drawing.Size(225, 21);
            this.cboTimeRange.TabIndex = 15;
            // 
            // lblTimeRange
            // 
            this.lblTimeRange.AutoSize = true;
            this.lblTimeRange.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTimeRange.Location = new System.Drawing.Point(25, 105);
            this.lblTimeRange.Name = "lblTimeRange";
            this.lblTimeRange.Size = new System.Drawing.Size(70, 15);
            this.lblTimeRange.TabIndex = 14;
            this.lblTimeRange.Text = "Time range";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Location = new System.Drawing.Point(25, 53);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(490, 36);
            this.lblSubtitle.TabIndex = 13;
            this.lblSubtitle.Text = "Choose what Quartz removes from the current profile. Downloaded files and favouri" +
    "tes are kept.";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(22, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(225, 32);
            this.lblTitle.TabIndex = 12;
            this.lblTitle.Text = "Clear browsing data";
            // 
            // ClearHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(540, 657);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pnlBottomDivider);
            this.Controls.Add(this.pnlNotice);
            this.Controls.Add(this.pnlItems);
            this.Controls.Add(this.pnlTopDivider);
            this.Controls.Add(this.cboTimeRange);
            this.Controls.Add(this.lblTimeRange);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ClearHistory";
            this.ShowIcon = false;
            this.Text = "ClearHistory";
            this.Load += new System.EventHandler(this.ClearHistory_Load);
            this.pnlNotice.ResumeLayout(false);
            this.pnlItems.ResumeLayout(false);
            this.pnlItems.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel pnlBottomDivider;
        private System.Windows.Forms.Panel pnlNotice;
        private System.Windows.Forms.Label lblNotice;
        private System.Windows.Forms.Panel pnlItems;
        private System.Windows.Forms.Label lblAutofillInfo;
        private System.Windows.Forms.CheckBox chkAutofill;
        private System.Windows.Forms.Label lblPasswordsInfo;
        private System.Windows.Forms.CheckBox chkPasswords;
        private System.Windows.Forms.Label lblCacheInfo;
        private System.Windows.Forms.CheckBox chkCache;
        private System.Windows.Forms.Label lblCookiesInfo;
        private System.Windows.Forms.CheckBox chkCookies;
        private System.Windows.Forms.Label lblDownloadHistoryInfo;
        private System.Windows.Forms.CheckBox chkDownloadHistory;
        private System.Windows.Forms.Label lblBrowsingHistoryInfo;
        private System.Windows.Forms.CheckBox chkBrowsingHistory;
        private System.Windows.Forms.Panel pnlTopDivider;
        private System.Windows.Forms.ComboBox cboTimeRange;
        private System.Windows.Forms.Label lblTimeRange;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblTitle;
    }
}