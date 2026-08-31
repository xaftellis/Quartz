using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuartzUpdater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await UpdaterClass.Initalize();

            label2.Text = Program.CurrentVersion.ToString();
            label1.Text = Program.LatestVersion.ToString();
            label3.Text = Program.LatestDownloadUrl.ToString();


        }
    }
}
