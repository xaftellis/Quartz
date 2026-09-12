using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz.Libs
{
    internal class Power
    {
        public static bool CloseAppContainer()
        {
            Program.Session?.PrepareForShutdown();
            var forms = Application.OpenForms.Cast<Form>().Where(f => f.Name == "AppContainer").ToList();
            foreach (var form in forms)
            {
                form.Close();
                if (!form.IsDisposed)
                {
                    Program.Session?.CancelShutdown();
                    return false;
                }
            }
            return true;
        }

        public static void Shutdown()
        {
            if (Application.OpenForms["AppContainer"] != null)
            {
                CloseAppContainer();
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
                if (CloseAppContainer()) Application.Restart();
            }
            else
            {
                Application.Restart();
            }
        }
        public static void RestartWithArguments(string address)
        {
            if (!CloseAppContainer()) return;
            Program.ReleaseInstanceForRestart();
            Process.Start(Application.ExecutablePath, "\"" + address.Replace("\"", "%22") + "\"");
            Application.Exit();
        }
    }
}
