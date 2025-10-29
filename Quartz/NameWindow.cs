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
using System.Windows.Media.Animation;

namespace Quartz
{
    public partial class NameWindow : Form
    {
        private AppContainer container;

        public NameWindow(AppContainer appContainer)
        {
            InitializeComponent();
            this.container = appContainer;
        }

        private void NameWindow_Load(object sender, EventArgs e)
        {
            NewControlThemeChanger.ChangeTheme(this);

            if (container._windowName != string.Empty)
            {
                textBox1.Text = container._windowName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                container._windowName = string.Empty;
                container.Text = container.SelectedTab.Content.Text;
                this.Close();
            }
            else
            {
                container._windowName = textBox1.Text;
                container.Text = textBox1.Text;
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (SettingsService.Get("escClose") == "true")
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }
    }
}
