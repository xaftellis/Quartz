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
                default:
                    throw new ArgumentOutOfRangeException();
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
            // Left Margin (11), Right Margin (28), Top Margin (25), Bottom Margin (67)
            int leftMargin = 8;
            if (icon)
            {
                leftMargin = 64;
            }

            int topMargin = 22;
            int rightMargin = 26;
            int bottomMargin = 66;

            // Calculate the size of the form based on label1 size and margins
            int formWidth = label1.Width + leftMargin + rightMargin;
            int formHeight = label1.Height + topMargin + bottomMargin;

            // Set the size of the form
            this.ClientSize = new System.Drawing.Size(formWidth, formHeight);

            // Recalculate the position of label1 within the form
            label1.Location = new System.Drawing.Point(leftMargin, topMargin);

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


            if (icon)
            {
                pictureBox1.Location = new Point(20, 15);
                pictureBox1.Image = GetIcon(iconType).ToBitmap();
            }
        }
        public void CenterForm(Form childForm, Form parentForm)
        {
            if(parentForm == null)
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

        public static int CountLines(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;

            // Split by both Unix and Windows newlines
            string[] lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            return lines.Length;
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
        }

        private void CustomMessageBox_Load(object sender, EventArgs e)
        {
            Text = _title;
            label1.Text = AddNewLinesEveryNChars(_message, 256);

            if(_button1 != null)
            {
                btnButton1.Visible = true;
                btnButton1.Text = _button1;
            }
            else
            {
                btnButton1.Visible = false;
            }

            if (_button2 != null)
            {
                btnButton2.Visible = true;
                btnButton2.Text = _button1;
            }
            else
            {
                btnButton2.Visible = false;
            }

            if (_button3 != null)
            {
                btnButton3.Visible = true;
                btnButton3.Text = _button1;
            }
            else
            {
                btnButton3.Visible = false;
            }

            //if()
            AdjustFormSize(iconType != SystemIconType.None);

            //NewControlThemeChanger.ChangeTheme(this);
            CenterForm(this, null);
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
