using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Quartz.Services;

namespace Quartz
{
    public partial class CustomMessageBox : Form
    {
        string _message;
        string _title;

        string _button1;
        string _button2;
        string _button3;

        public enum CustomMessageBoxResult
        {
            None,
            button1,
            button2,
            button3,
        }
        public CustomMessageBoxResult Result { get; private set; }
        public enum SystemIconType
        {
            None,
            Information,
            Warning,
            Error,
            Question,
        }
        public SystemIconType iconType { get; private set; }

        public Icon GetIcon(SystemIconType iconType)
        {
            // Return null when no icon rather than throwing
            switch (iconType)
            {
                case SystemIconType.Information:
                    return Quartz.Properties.Resources.InformationIcon;
                case SystemIconType.Warning:
                    return Quartz.Properties.Resources.WarningIcon;
                case SystemIconType.Error:
                    return Quartz.Properties.Resources.ErrorIcon;
                case SystemIconType.Question:
                    return Quartz.Properties.Resources.QuestionIcon;
                case SystemIconType.None:
                default:
                    return null;
            }
        }

        public string AddNewLinesEveryNChars(string input, int n)
        {
            // Ensure that n is a positive value and not zero
            if (n <= 0) throw new ArgumentException("The number of characters must be greater than zero.");

            StringBuilder result = new StringBuilder();

            // Iterate through the input string in steps of 'n' characters
            for (int i = 0; i < input.Length; i += n)
            {
                // Get the substring of the next 'n' characters
                string substring = input.Substring(i, Math.Min(n, input.Length - i));

                // Append the substring to the result, followed by a newline
                result.AppendLine(substring);
            }

            return result.ToString();
        }

        private void AdjustFormSize(bool icon)
        {
            // Base margins (you had these originally)
            int leftMargin = 8;
            if (icon)
            {
                leftMargin = 60; // room for icon
            }

            // Increase top/bottom margins when icon present to make the white/message area taller
            int topMargin = icon ? 20 : 22;    // slightly taller when icon shown
            int rightMargin = 26;
            int bottomMargin = icon ? 64 : 66; // give more space at bottom when icon present

            // Ensure label autosizes so widths/heights reflect the real rendered size
            label1.AutoSize = true;

            // Calculate form width/height from label and margins
            int formWidth = label1.Width + leftMargin + rightMargin;
            int formHeight = label1.Height + topMargin + bottomMargin;

            // Minimum width guard (so buttons don't overflow)
            int minWidth = 300;
            if (formWidth < minWidth) formWidth = minWidth;

            // Set client size
            this.ClientSize = new System.Drawing.Size(formWidth, formHeight);

            // If icon present, we want to vertically center the icon and the label inside the main area.
            // Define the content area top and height (the "white" area where icon + text sit)
            int contentTop = topMargin;
            int contentHeight = label1.Height;
            int iconHeight = 0;

            if (icon)
            {
                var ic = GetIcon(iconType);
                if (ic != null)
                {
                    iconHeight = ic.Height;
                    // ensure picturebox size matches icon (or at least able to display it nicely)
                    pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox1.Image = ic.ToBitmap();
                    pictureBox1.Visible = true;
                }
                else
                {
                    pictureBox1.Visible = false;
                }

                // contentHeight should be the max of icon and label heights so both fit and can be centered
                contentHeight = Math.Max(label1.Height, iconHeight);
                // Give a little vertical breathing room
                contentHeight += 6; // padding
            }
            else
            {
                pictureBox1.Visible = false;
            }

            // Recalculate overall form height to ensure content area fits (label + icon) plus margins
            formHeight = contentTop + contentHeight + bottomMargin;
            this.ClientSize = new Size(formWidth, formHeight);

            // Now compute vertical center offset inside the content area for label and icon
            int labelY = contentTop + (contentHeight - label1.Height) / 2;
            int iconY = contentTop + (contentHeight - iconHeight) / 2;

            // Position label at leftMargin (after icon space) and vertically centered
            label1.Location = new System.Drawing.Point(leftMargin, labelY);

            // Position pictureBox if icon present
            if (icon && pictureBox1.Visible)
            {
                pictureBox1.Location = new Point(20, iconY);
                // optional: size box to icon size (keeps perfect centering)
                pictureBox1.Size = new Size(iconHeight, iconHeight);
            }

            // Place buttons — I kept your original coordinates logic but they are now relative to the new form size
            if (_button1 != null)
            {
                btnButton1.Location = new Point(formWidth - 12 - btnButton1.Width, 9);
            }

            if (_button2 != null)
            {
                btnButton2.Location = new Point(formWidth - 93 - btnButton2.Width, 9);
            }

            if (_button3 != null)
            {
                btnButton3.Location = new Point(formWidth - 174 - btnButton3.Width, 9);
            }
        }

        public void CenterForm(Form childForm, Form parentForm)
        {
            if (parentForm == null)
            {
                // Get the screen's working area (excluding taskbar)
                Rectangle screenBounds = Screen.PrimaryScreen.WorkingArea;

                // Calculate the position so that the child form is centered on the screen
                int x = screenBounds.X + (screenBounds.Width - childForm.Width) / 2;
                int y = screenBounds.Y + (screenBounds.Height - childForm.Height) / 2;

                // Set the start position to manual and specify the location
                childForm.StartPosition = FormStartPosition.Manual;
                childForm.Location = new Point(x, y);

            }
            else
            {
                int x = parentForm.Location.X + (parentForm.Width - childForm.Width) / 2;
                int y = parentForm.Location.Y + (parentForm.Height - childForm.Height) / 2;

                childForm.StartPosition = FormStartPosition.Manual;
                childForm.Location = new Point(x, y);
            }
        }


        public CustomMessageBox(string Message, string Title, SystemIconType icon, string Button1, string Button2, string Button3)
        {
            InitializeComponent();
            _message = Message;
            _title = Title;
            _button1 = Button1;
            _button2 = Button2;
            _button3 = Button3;
            iconType = icon;

            //NewControlThemeChanger.ChangeTheme(this);
        }

        private void CustomMessageBox_Load(object sender, EventArgs e)
        {
            Text = _title;
            label1.Text = AddNewLinesEveryNChars(_message, 256);
            label1.AutoSize = true; // ensure measured height is accurate

            if (_button1 != null)
            {
                btnButton1.Visible = true;
                btnButton1.Text = _button1;
            }
            else
            {
                btnButton1.Visible = false;
            }

            // FIXED: set correct texts for button2 and button3 (previously using _button1 by mistake)
            if (_button2 != null)
            {
                btnButton2.Visible = true;
                btnButton2.Text = _button2;
            }
            else
            {
                btnButton2.Visible = false;
            }

            if (_button3 != null)
            {
                btnButton3.Visible = true;
                btnButton3.Text = _button3;
            }
            else
            {
                btnButton3.Visible = false;
            }

            AdjustFormSize(iconType != SystemIconType.None);

            //NewControlThemeChanger.ChangeTheme(this);
            CenterForm(this, null);

            //NewControlThemeChanger.ChangeWindowTheme(this.Handle);
            //NewControlThemeChanger.ChangeControlTheme(btnButton1);
            //NewControlThemeChanger.ChangeControlTheme(btnButton2);
            //NewControlThemeChanger.ChangeControlTheme(btnButton3);
            //NewControlThemeChanger.ChangeControlTheme(this);
            //pnlBottom.BackColor = 
        }

        private void btnButton1_Click(object sender, EventArgs e)
        {
            Result = CustomMessageBoxResult.button1;
            this.Close();
        }

        private void btnButton2_Click(object sender, EventArgs e)
        {
            Result = CustomMessageBoxResult.button2;
            this.Close();
        }

        private void btnButton3_Click(object sender, EventArgs e)
        {
            Result = CustomMessageBoxResult.button3;
            this.Close();
        }
    }
}
