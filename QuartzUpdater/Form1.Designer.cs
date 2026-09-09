namespace QuartzUpdater
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.titleLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.currentVersionCaption = new System.Windows.Forms.Label();
            this.currentVersionValue = new System.Windows.Forms.Label();
            this.latestVersionCaption = new System.Windows.Forms.Label();
            this.latestVersionValue = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.detailLabel = new System.Windows.Forms.Label();
            this.logLinkLabel = new System.Windows.Forms.LinkLabel();
            this.updateButton = new System.Windows.Forms.Button();
            this.retryButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.logToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(24, 19);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(188, 32);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Quartz Updater";
            // 
            // statusLabel
            // 
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.Location = new System.Drawing.Point(27, 66);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(502, 42);
            this.statusLabel.TabIndex = 1;
            this.statusLabel.Text = "Preparing...";
            // 
            // currentVersionCaption
            // 
            this.currentVersionCaption.AutoSize = true;
            this.currentVersionCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this.currentVersionCaption.Location = new System.Drawing.Point(27, 121);
            this.currentVersionCaption.Name = "currentVersionCaption";
            this.currentVersionCaption.Size = new System.Drawing.Size(92, 15);
            this.currentVersionCaption.TabIndex = 2;
            this.currentVersionCaption.Text = "Current version";
            // 
            // currentVersionValue
            // 
            this.currentVersionValue.AutoSize = true;
            this.currentVersionValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentVersionValue.Location = new System.Drawing.Point(125, 121);
            this.currentVersionValue.Name = "currentVersionValue";
            this.currentVersionValue.Size = new System.Drawing.Size(14, 15);
            this.currentVersionValue.TabIndex = 3;
            this.currentVersionValue.Text = "—";
            // 
            // latestVersionCaption
            // 
            this.latestVersionCaption.AutoSize = true;
            this.latestVersionCaption.ForeColor = System.Drawing.SystemColors.GrayText;
            this.latestVersionCaption.Location = new System.Drawing.Point(302, 121);
            this.latestVersionCaption.Name = "latestVersionCaption";
            this.latestVersionCaption.Size = new System.Drawing.Size(81, 15);
            this.latestVersionCaption.TabIndex = 4;
            this.latestVersionCaption.Text = "Latest version";
            // 
            // latestVersionValue
            // 
            this.latestVersionValue.AutoSize = true;
            this.latestVersionValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.latestVersionValue.Location = new System.Drawing.Point(389, 121);
            this.latestVersionValue.Name = "latestVersionValue";
            this.latestVersionValue.Size = new System.Drawing.Size(14, 15);
            this.latestVersionValue.TabIndex = 5;
            this.latestVersionValue.Text = "—";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(30, 159);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(499, 20);
            this.progressBar.TabIndex = 6;
            // 
            // detailLabel
            // 
            this.detailLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.detailLabel.Location = new System.Drawing.Point(27, 190);
            this.detailLabel.Name = "detailLabel";
            this.detailLabel.Size = new System.Drawing.Size(502, 54);
            this.detailLabel.TabIndex = 7;
            this.detailLabel.Text = "Starting QuartzUpdater...";
            // 
            // logLinkLabel
            // 
            this.logLinkLabel.AutoSize = true;
            this.logLinkLabel.Location = new System.Drawing.Point(27, 228);
            this.logLinkLabel.Name = "logLinkLabel";
            this.logLinkLabel.Size = new System.Drawing.Size(94, 15);
            this.logLinkLabel.TabIndex = 12;
            this.logLinkLabel.TabStop = true;
            this.logLinkLabel.Text = "Open update log";
            this.logLinkLabel.Visible = false;
            this.logLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.logLinkLabel_LinkClicked);
            // 
            // updateButton
            // 
            this.updateButton.Location = new System.Drawing.Point(30, 259);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(170, 32);
            this.updateButton.TabIndex = 8;
            this.updateButton.Text = "Download and install";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Visible = false;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // retryButton
            // 
            this.retryButton.Location = new System.Drawing.Point(206, 259);
            this.retryButton.Name = "retryButton";
            this.retryButton.Size = new System.Drawing.Size(100, 32);
            this.retryButton.TabIndex = 9;
            this.retryButton.Text = "Retry";
            this.retryButton.UseVisualStyleBackColor = true;
            this.retryButton.Visible = false;
            this.retryButton.Click += new System.EventHandler(this.retryButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(312, 259);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 32);
            this.cancelButton.TabIndex = 10;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Visible = false;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(418, 259);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(111, 32);
            this.closeButton.TabIndex = 11;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Visible = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // Form1
            // 
            this.AcceptButton = this.updateButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(559, 315);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.retryButton);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.logLinkLabel);
            this.Controls.Add(this.detailLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.latestVersionValue);
            this.Controls.Add(this.latestVersionCaption);
            this.Controls.Add(this.currentVersionValue);
            this.Controls.Add(this.currentVersionCaption);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.titleLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quartz Updater";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label currentVersionCaption;
        private System.Windows.Forms.Label currentVersionValue;
        private System.Windows.Forms.Label latestVersionCaption;
        private System.Windows.Forms.Label latestVersionValue;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label detailLabel;
        private System.Windows.Forms.LinkLabel logLinkLabel;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Button retryButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.ToolTip logToolTip;
    }
}
