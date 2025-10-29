using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    public partial class Password : Form
    {
        Guid profileGuid;
        Form parent;
        public bool IsPasswordCorrect = false;


        public void CenterForm(Form childForm, Form parentForm)
        {
            int x, y;

            if (parentForm != null)
            {
                // Center relative to the parent form
                x = parentForm.Location.X + (parentForm.Width - childForm.Width) / 2;
                y = parentForm.Location.Y + (parentForm.Height - childForm.Height) / 2;
            }
            else
            {
                // No parent, center on the monitor where the childForm will open
                Screen screen = Screen.FromControl(childForm);
                Rectangle workingArea = screen.WorkingArea;

                x = workingArea.Left + (workingArea.Width - childForm.Width) / 2;
                y = workingArea.Top + (workingArea.Height - childForm.Height) / 2;
            }

            childForm.StartPosition = FormStartPosition.Manual;
            childForm.Location = new Point(x, y);
        }

        public Password(Guid id, Form parent)
        {
            InitializeComponent();
            this.profileGuid = id;
            this.parent = parent;
            
        }

        private void Password_Load(object sender, EventArgs e)
        {
            if (SettingsService.Get("DraggableForms") == "true")
            {
                MouseDragger mouseDragger = new MouseDragger(this);
            }

            NewControlThemeChanger.ChangeTheme(this);

            label2.ForeColor = Color.Red;

            CenterForm(this, parent);
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCurrent.Text))
            {
                label2.Text = "Oops! It looks like you forgot to enter your password. Please try again.";
            }
            else if (txtCurrent.Text != Program.profileService.GetPassword(profileGuid))
            {
                label2.Text = "Incorrect password. Please try again.";
                txtCurrent.Clear();
            }
            label2.Visible = string.IsNullOrWhiteSpace(txtCurrent.Text) || txtCurrent.Text != Program.profileService.GetPassword(profileGuid);

            if (!string.IsNullOrWhiteSpace(txtCurrent.Text)
                && txtCurrent.Text == Program.profileService.GetPassword(profileGuid))
            {
                IsPasswordCorrect = true;
                label2.Visible = false;
                Close();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            btnEnter.Enabled = !string.IsNullOrEmpty(txtCurrent.Text);
        }

        private void Password_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnEnter_Click(sender, e);
            }
            else if (SettingsService.Get("escClose") == "true")
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            }
        }

        private void txtCurrent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                txtCurrent.UseSystemPasswordChar = false; // Show password
                txtCurrent.Refresh();
            }
        }

        private void txtCurrent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                txtCurrent.UseSystemPasswordChar = true; // Hide password
                txtCurrent.Refresh();
            }
        }


        private void txtCurrent_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }
    }
}
