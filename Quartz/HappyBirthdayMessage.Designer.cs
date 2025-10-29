namespace Quartz
{
    partial class HappyBirthdayMessage
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtHappy = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.BackgroundImage = global::Quartz.Properties.Resources.pngimg_com___confetti_PNG87006;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = global::Quartz.Properties.Resources.Birthday_Quartz1;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(287, 311);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // txtHappy
            // 
            this.txtHappy.Font = new System.Drawing.Font("Microsoft Sans Serif", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHappy.Image = global::Quartz.Properties.Resources.pngimg_com___confetti_PNG87006;
            this.txtHappy.Location = new System.Drawing.Point(288, 0);
            this.txtHappy.Name = "txtHappy";
            this.txtHappy.Size = new System.Drawing.Size(496, 54);
            this.txtHappy.TabIndex = 1;
            this.txtHappy.Text = "Happy 0th Birthday";
            this.txtHappy.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtName
            // 
            this.txtName.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtName.Image = global::Quartz.Properties.Resources.pngimg_com___confetti_PNG87006;
            this.txtName.Location = new System.Drawing.Point(288, 117);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(496, 74);
            this.txtName.TabIndex = 2;
            this.txtName.Text = "John Smith";
            this.txtName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Image = global::Quartz.Properties.Resources.pngimg_com___confetti_PNG87006;
            this.label2.Location = new System.Drawing.Point(288, 270);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(495, 41);
            this.label2.TabIndex = 3;
            this.label2.Text = "-Quartz";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // HappyBirthdayMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Quartz.Properties.Resources.pngimg_com___confetti_PNG87006;
            this.ClientSize = new System.Drawing.Size(784, 311);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtHappy);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HappyBirthdayMessage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Happy Birthday";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HappyBirthdayMessage_FormClosing);
            this.Load += new System.EventHandler(this.HappyBirthdayMessage_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.HappyBirthdayMessage_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label txtHappy;
        private System.Windows.Forms.Label txtName;
        private System.Windows.Forms.Label label2;
    }
}