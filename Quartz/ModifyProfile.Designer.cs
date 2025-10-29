namespace Quartz
{
    partial class ModifyProfile
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
            this.LabelNameNull = new System.Windows.Forms.Label();
            this.NameLabel = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtExists = new System.Windows.Forms.Label();
            this.btnCreate = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.circularImageButton1 = new Quartz.Controls.CircularImageButton();
            this.SuspendLayout();
            // 
            // LabelNameNull
            // 
            this.LabelNameNull.AutoSize = true;
            this.LabelNameNull.ForeColor = System.Drawing.Color.Red;
            this.LabelNameNull.Location = new System.Drawing.Point(71, 187);
            this.LabelNameNull.Name = "LabelNameNull";
            this.LabelNameNull.Size = new System.Drawing.Size(89, 13);
            this.LabelNameNull.TabIndex = 6;
            this.LabelNameNull.Text = "Name is required.";
            this.LabelNameNull.Visible = false;
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(30, 141);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(38, 13);
            this.NameLabel.TabIndex = 1;
            this.NameLabel.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(74, 138);
            this.txtName.MaxLength = 64;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(326, 20);
            this.txtName.TabIndex = 1;
            this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
            // 
            // txtExists
            // 
            this.txtExists.AutoSize = true;
            this.txtExists.ForeColor = System.Drawing.Color.Red;
            this.txtExists.Location = new System.Drawing.Point(71, 187);
            this.txtExists.Name = "txtExists";
            this.txtExists.Size = new System.Drawing.Size(105, 13);
            this.txtExists.TabIndex = 19;
            this.txtExists.Text = "Profile already exists.";
            this.txtExists.Visible = false;
            // 
            // btnCreate
            // 
            this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCreate.Location = new System.Drawing.Point(325, 208);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 23);
            this.btnCreate.TabIndex = 3;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(74, 164);
            this.txtPassword.MaxLength = 64;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(326, 20);
            this.txtPassword.TabIndex = 2;
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            this.txtPassword.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 167);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Password:";
            // 
            // circularImageButton1
            // 
            this.circularImageButton1.ActionButtonGapping = 3;
            this.circularImageButton1.ActionButtonHoverImage = null;
            this.circularImageButton1.ActionButtonImage = null;
            this.circularImageButton1.ButtonText = "";
            this.circularImageButton1.CircularImage = global::Quartz.Properties.Resources.blank;
            this.circularImageButton1.CircularImageBorderColor = System.Drawing.Color.Black;
            this.circularImageButton1.CircularImageBorderSize = 1.6F;
            this.circularImageButton1.CircularImageSize = 96;
            this.circularImageButton1.CircularImageToTextGapping = 1;
            this.circularImageButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.circularImageButton1.Location = new System.Drawing.Point(144, 12);
            this.circularImageButton1.Name = "circularImageButton1";
            this.circularImageButton1.Size = new System.Drawing.Size(111, 111);
            this.circularImageButton1.TabIndex = 4;
            this.circularImageButton1.Text = "Profile Picture";
            this.circularImageButton1.UseVisualStyleBackColor = true;
            this.circularImageButton1.ActionButtonClick += new System.EventHandler(this.circularImageButton1_ActionButtonClick);
            this.circularImageButton1.CircularImageChanged += new System.EventHandler(this.circularImageButton1_CircularImageChanged);
            this.circularImageButton1.Click += new System.EventHandler(this.circularImageButton1_Click);
            // 
            // ModifyProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(412, 243);
            this.Controls.Add(this.txtExists);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.circularImageButton1);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.LabelNameNull);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModifyProfile";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create Profile";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModifyProfile_FormClosing);
            this.Load += new System.EventHandler(this.CreateProfile_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CreateProfile_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label LabelNameNull;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label txtExists;
        private System.Windows.Forms.Button btnCreate;
        private Quartz.Controls.CircularImageButton circularImageButton1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label1;
    }
}