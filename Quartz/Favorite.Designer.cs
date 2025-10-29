namespace Quartz
{
    partial class Favourite
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
            this.NameLabel = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.NameMessage = new System.Windows.Forms.Label();
            this.AddressTextBox = new System.Windows.Forms.TextBox();
            this.AddressLabel = new System.Windows.Forms.Label();
            this.txtExist = new System.Windows.Forms.Label();
            this.txtURLBad = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.BackColor = System.Drawing.Color.Transparent;
            this.NameLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.NameLabel.Location = new System.Drawing.Point(18, 16);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(38, 13);
            this.NameLabel.TabIndex = 0;
            this.NameLabel.Text = "Name:";
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(55, 13);
            this.NameTextBox.MaxLength = 50;
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(326, 20);
            this.NameTextBox.TabIndex = 1;
            this.NameTextBox.TextChanged += new System.EventHandler(this.NameTextBox_TextChanged);
            // 
            // SaveButton
            // 
            this.SaveButton.BackColor = System.Drawing.Color.Black;
            this.SaveButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SaveButton.ForeColor = System.Drawing.Color.White;
            this.SaveButton.Location = new System.Drawing.Point(311, 78);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 3;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = false;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // NameMessage
            // 
            this.NameMessage.AutoSize = true;
            this.NameMessage.BackColor = System.Drawing.Color.Transparent;
            this.NameMessage.ForeColor = System.Drawing.Color.Red;
            this.NameMessage.Location = new System.Drawing.Point(52, 62);
            this.NameMessage.Name = "NameMessage";
            this.NameMessage.Size = new System.Drawing.Size(132, 13);
            this.NameMessage.TabIndex = 3;
            this.NameMessage.Text = "Name/Address is required.";
            this.NameMessage.Visible = false;
            // 
            // AddressTextBox
            // 
            this.AddressTextBox.Location = new System.Drawing.Point(55, 39);
            this.AddressTextBox.MaxLength = 10000;
            this.AddressTextBox.Name = "AddressTextBox";
            this.AddressTextBox.Size = new System.Drawing.Size(326, 20);
            this.AddressTextBox.TabIndex = 2;
            // 
            // AddressLabel
            // 
            this.AddressLabel.AutoSize = true;
            this.AddressLabel.BackColor = System.Drawing.Color.Transparent;
            this.AddressLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.AddressLabel.Location = new System.Drawing.Point(8, 42);
            this.AddressLabel.Name = "AddressLabel";
            this.AddressLabel.Size = new System.Drawing.Size(48, 13);
            this.AddressLabel.TabIndex = 4;
            this.AddressLabel.Text = "Address:";
            // 
            // txtExist
            // 
            this.txtExist.AutoSize = true;
            this.txtExist.BackColor = System.Drawing.Color.Transparent;
            this.txtExist.ForeColor = System.Drawing.Color.Red;
            this.txtExist.Location = new System.Drawing.Point(52, 62);
            this.txtExist.Name = "txtExist";
            this.txtExist.Size = new System.Drawing.Size(120, 13);
            this.txtExist.TabIndex = 6;
            this.txtExist.Text = "Favourite already exists.";
            this.txtExist.Visible = false;
            // 
            // txtURLBad
            // 
            this.txtURLBad.AutoSize = true;
            this.txtURLBad.BackColor = System.Drawing.Color.Transparent;
            this.txtURLBad.ForeColor = System.Drawing.Color.Red;
            this.txtURLBad.Location = new System.Drawing.Point(52, 62);
            this.txtURLBad.Name = "txtURLBad";
            this.txtURLBad.Size = new System.Drawing.Size(122, 13);
            this.txtURLBad.TabIndex = 7;
            this.txtURLBad.Text = "The address is not valid.";
            this.txtURLBad.Visible = false;
            // 
            // Favourite
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(398, 113);
            this.Controls.Add(this.AddressTextBox);
            this.Controls.Add(this.AddressLabel);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.NameMessage);
            this.Controls.Add(this.txtURLBad);
            this.Controls.Add(this.txtExist);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Favourite";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Favourite";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Favourite_FormClosing);
            this.Load += new System.EventHandler(this.Favourite_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Favourite_KeyUp);
            this.Leave += new System.EventHandler(this.Favourite_Leave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Label NameMessage;
        private System.Windows.Forms.TextBox AddressTextBox;
        private System.Windows.Forms.Label AddressLabel;
        private System.Windows.Forms.Label txtExist;
        private System.Windows.Forms.Label txtURLBad;
    }
}