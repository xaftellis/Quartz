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
    public partial class AddBirthday : Form
    {
        DateTime selectedDate;
        string selectedDateString;
        Settings settings;

        bool isModifying;
        string name;
        DateTime _DOB;
        Guid id;

        public AddBirthday(Settings settings, bool IsModifying, string Name, DateTime DOB, Guid ID)
        {
            InitializeComponent();
            this.settings = settings;
            isModifying = IsModifying;
            name = Name;
            _DOB = DOB;
            id = ID;
        }


        private void btnDown_Click(object sender, EventArgs e)
        {
            if (!mcCalender.Visible)
            {
                button1.Location = new Point(201, 247);
                Size = new Size(304, 318);
                if (SettingsService.Get("Animation") == "true")
                {
                    Animation.AnimateWindow(mcCalender.Handle, 250, Animation.AW_SLIDE | Animation.AW_VER_POSITIVE);
                    mcCalender.Visible = true;
                    btnDown.Text = "▲";
                }
                else
                {
                    mcCalender.Visible = true;
                    btnDown.Text = "▲";
                }
            }
            else
            {
                if (SettingsService.Get("Animation") == "true")
                {
                    Animation.AnimateWindow(mcCalender.Handle, 250, Animation.AW_SLIDE | Animation.AW_VER_NEGATIVE | Animation.AW_HIDE);
                    mcCalender.Visible = false;
                    btnDown.Text = "▼";
                }
                else
                {
                    mcCalender.Visible = false;
                    btnDown.Text = "▼";
                }
                button1.Location = new Point(201, 70);
                Size = new Size(304, 144);
            }
        }

        private void AddBirthday_Load(object sender, EventArgs e)
        {
            NewControlThemeChanger.ChangeTheme(this);
            txtExists.ForeColor = Color.Red;

            if (isModifying)
            {
                selectedDate = _DOB;
                selectedDateString = _DOB.ToString("D").Replace(mcCalender.SelectionStart.DayOfWeek + ", ", "");
                txtDOB.Text = selectedDateString;

                mcCalender.SelectionStart = selectedDate;
                txtName.Text = name;
            }
            else
            {
                selectedDate = DateTime.Parse(mcCalender.SelectionStart.ToString("D").Replace(mcCalender.SelectionStart.DayOfWeek + ", ", ""));
                selectedDateString = mcCalender.SelectionStart.ToString("D").Replace(mcCalender.SelectionStart.DayOfWeek + ", ", "");
                txtDOB.Text = selectedDateString;
            }
        }

        private void mcCalender_DateChanged(object sender, DateRangeEventArgs e)
        {
            // If future date selected → rewind to last valid date
            if (e.Start > DateTime.Today)
            {
                txtDOB.Text = selectedDateString;
                mcCalender.SelectionStart = selectedDate;
                mcCalender.SelectionEnd = selectedDate;
                return;
            }

            mcCalender.AddBoldedDate(e.Start);
            mcCalender.SelectionStart = e.Start;
            mcCalender.SelectionEnd = e.Start;
            selectedDate = DateTime.Parse(mcCalender.SelectionStart.ToString("D").Replace(mcCalender.SelectionStart.DayOfWeek + ", ", ""));
            selectedDateString = mcCalender.SelectionStart.ToString("D").Replace(mcCalender.SelectionStart.DayOfWeek + ", ", "");

            txtDOB.Text = selectedDateString;
        }

        private void txtTimeMachine_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (txtDOB.Text != selectedDateString)
                    {
                        mcCalender.SelectionStart = DateTime.Parse(txtDOB.Text);
                    }
                }
                catch
                {
                    txtDOB.Text = selectedDateString;
                    mcCalender.SelectionStart = selectedDate;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)
                || string.IsNullOrEmpty(txtDOB.Text)
                || selectedDate == null)
            {
                txtExists.Visible = true;
                return;
            }

            txtExists.Visible = false;

            if (isModifying)
            {
                settings.birthdayService.Get(id).Name = txtName.Text;
                settings.birthdayService.Get(id).DOB = selectedDate;
            }
            else
            {
                var model = new Models.BirthdayModel
                {
                    Name = txtName.Text,
                    DOB = selectedDate
                };

                settings.birthdayService.Modify(model);

            }

            settings.birthdayService.SaveChanges();
            this.Close();
        }
    }
}
