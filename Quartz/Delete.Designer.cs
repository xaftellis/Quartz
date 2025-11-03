namespace Quartz
{
    partial class Delete
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
            this.chkBrowsingHistory = new System.Windows.Forms.CheckBox();
            this.chkDownloadHistory = new System.Windows.Forms.CheckBox();
            this.chkCookies = new System.Windows.Forms.CheckBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkBrowsingHistory
            // 
            this.chkBrowsingHistory.AutoSize = true;
            this.chkBrowsingHistory.Location = new System.Drawing.Point(12, 12);
            this.chkBrowsingHistory.Name = "chkBrowsingHistory";
            this.chkBrowsingHistory.Size = new System.Drawing.Size(102, 17);
            this.chkBrowsingHistory.TabIndex = 0;
            this.chkBrowsingHistory.Text = "Browsing history";
            this.chkBrowsingHistory.UseVisualStyleBackColor = true;
            // 
            // chkDownloadHistory
            // 
            this.chkDownloadHistory.AutoSize = true;
            this.chkDownloadHistory.Location = new System.Drawing.Point(12, 35);
            this.chkDownloadHistory.Name = "chkDownloadHistory";
            this.chkDownloadHistory.Size = new System.Drawing.Size(107, 17);
            this.chkDownloadHistory.TabIndex = 1;
            this.chkDownloadHistory.Text = "Download history";
            this.chkDownloadHistory.UseVisualStyleBackColor = true;
            // 
            // chkCookies
            // 
            this.chkCookies.AutoSize = true;
            this.chkCookies.Location = new System.Drawing.Point(12, 58);
            this.chkCookies.Name = "chkCookies";
            this.chkCookies.Size = new System.Drawing.Size(64, 17);
            this.chkCookies.TabIndex = 2;
            this.chkCookies.Text = "Cookies";
            this.chkCookies.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(87, 77);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete Data";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // Delete
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(174, 112);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.chkCookies);
            this.Controls.Add(this.chkDownloadHistory);
            this.Controls.Add(this.chkBrowsingHistory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Delete";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Delete browsing data";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkBrowsingHistory;
        private System.Windows.Forms.CheckBox chkDownloadHistory;
        private System.Windows.Forms.CheckBox chkCookies;
        private System.Windows.Forms.Button btnDelete;
    }
}