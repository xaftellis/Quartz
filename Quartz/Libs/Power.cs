using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz.Libs
{
    internal class Power
    {
        public static void CloseAppContainer()
        {
            if (Application.OpenForms["AppContainer"] != null)
            {
                var forms = Application.OpenForms.Cast<Form>().Where(f => f.Name == "AppContainer").ToList();
                foreach (var form in forms)
                {
                    form.Close();
                }
            }
        }

        public static void Shutdown()
        {
            if (Application.OpenForms["AppContainer"] != null)
            {
                var forms = Application.OpenForms.Cast<Form>().Where(f => f.Name == "AppContainer").ToList();
                foreach (var form in forms)
                {
                    form.Close();
                }
            }
            else
            {
                Application.Exit();
            }
        }

        public static void Restart()
        {
            if (Application.OpenForms["AppContainer"] != null)
            {
              CloseAppContainer();

                Application.Restart();
            }
            else
            {
                Application.Restart();
            }
        }
    }
}
