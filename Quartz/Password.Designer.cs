namespace Quartz
{
    partial class Password
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
            this.label2 = new System.Windows.Forms.Label();
            this.txtCurrent = new System.Windows.Forms.TextBox();
            this.btnEnter = new System.Windows.Forms.Button();
            this.labelCurrent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(71, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(181, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Incorrect password. Please try again.";
            this.label2.Visible = false;
            // 
            // txtCurrent
            // 
            this.txtCurrent.Location = new System.Drawing.Point(74, 12);
            this.txtCurrent.MaxLength = 256;
            this.txtCurrent.Name = "txtCurrent";
            this.txtCurrent.Size = new System.Drawing.Size(500, 20);
            this.txtCurrent.TabIndex = 1;
            this.txtCurrent.UseSystemPasswordChar = true;
            this.txtCurrent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCurrent_KeyDown);
            this.txtCurrent.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtCurrent_KeyUp);
            this.txtCurrent.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.txtCurrent_PreviewKeyDown);
            // 
            // btnEnter
            // 
            this.btnEnter.Location = new System.Drawing.Point(499, 55);
            this.btnEnter.Name = "btnEnter";
            this.btnEnter.Size = new System.Drawing.Size(75, 23);
            this.btnEnter.TabIndex = 3;
            this.btnEnter.Text = "Enter";
            this.btnEnter.UseVisualStyleBackColor = true;
            this.btnEnter.Click += new System.EventHandler(this.btnEnter_Click);
            // 
            // labelCurrent
            // 
            this.labelCurrent.AutoSize = true;
            this.labelCurrent.Location = new System.Drawing.Point(12, 15);
            this.labelCurrent.Name = "labelCurrent";
            this.labelCurrent.Size = new System.Drawing.Size(56, 13);
            this.labelCurrent.TabIndex = 4;
            this.labelCurrent.Text = "Password:";
            // 
            // Password
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 90);
            this.Controls.Add(this.txtCurrent);
            this.Controls.Add(this.labelCurrent);
            this.Controls.Add(this.btnEnter);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Password";
            this.ShowIcon = false;
            this.Text = "Password";
            this.Load += new System.EventHandler(this.Password_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Password_KeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCurrent;
        private System.Windows.Forms.Button btnEnter;
        private System.Windows.Forms.Label labelCurrent;
    }
}