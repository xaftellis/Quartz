using Newtonsoft.Json;
using Quartz.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Quartz.Libs;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Quartz.Services
{
    public class SettingsService
    {
        private static string _jsonPath = Path.Combine(Application.StartupPath + @"\UserData\Jsons", "settings.json");
        private static string WindowsTheme = ThemeHelper.GetTheme();
        private static bool DisplayOutOfDateThemeMessage = true;

        public static string Get(string name)
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            var items = JsonConvert.DeserializeObject<List<SettingModel>>(jsonString);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            var _items = items.FirstOrDefault(s => s.ProfileId == ProfileService.Current && s.Name == name)?.Value;

            if(name == "Theme" && _items == "auto (light/dark)")
            {
                String theme;
                if(GetWindowsTheme() == "light")
                {
                    theme = "light";
                }
                else
                {
                    theme = "dark";
                }

                return theme;
            }
            else if (name == "Theme" && _items == "auto (light/black)")
            {
                String theme;
                if (GetWindowsTheme() == "light")
                {
                    theme = "light";
                }
                else
                {
                    theme = "black";
                }

                return theme;
            }
            else
            {
                return _items;
            }
        }

        public static SettingModel GetModel(string name)
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            var items = JsonConvert.DeserializeObject<List<SettingModel>>(jsonString);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            var _items = items.FirstOrDefault(s => s.ProfileId == ProfileService.Current && s.Name == name);
            return _items;
        }

        public static string GetAutoTheme()
        {
            var name = "Theme";

            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            var items = JsonConvert.DeserializeObject<List<SettingModel>>(jsonString);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            var _items = items.FirstOrDefault(s => s.ProfileId == ProfileService.Current && s.Name == name)?.Value;

            if (name == "Theme" && _items == "auto (light/dark)" || name == "Theme" && _items == "auto (light/black)")
            {
                return _items;
            }
            else
            {
                return null;
            }
        }

        public static void Set(string name, string value)
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            var items = JsonConvert.DeserializeObject<List<SettingModel>>(jsonString);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            var original = items.FirstOrDefault(s => s.ProfileId == ProfileService.Current && s.Name == name);
            if (original == null)
            {
                items.Add(new SettingModel() { ProfileId = ProfileService.Current, Name = name, Value = value });
            }
            else
            {
                original.Value = value;
            }

            var _jsonString = JsonConvert.SerializeObject(items);
            File.WriteAllText(_jsonPath, _jsonString);
        }

        public static string GetWindowsTheme()
        {
            if(IsWindowsThemeUpToDate() == false && DisplayOutOfDateThemeMessage == true)
            {
                var msg = MessageBox.Show("We've detected a change in the Windows theme. Would you like to restart the application to synchronize the themes?", "Out Of Sync", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (msg == DialogResult.Yes)
                {
                    if (Application.OpenForms["AppContainer"] != null)
                    {
                        Power.Restart();
                    }
                    else
                    {
                        MainSettingsService.Set("RunBrowser", "true");
                        Power.Restart();
                    }
                }
                else if (msg == DialogResult.No)
                {
                    DisplayOutOfDateThemeMessage = false;
                }
            }

            return WindowsTheme;
        }

        public static bool IsWindowsThemeUpToDate()
        {
            return WindowsTheme == ThemeHelper.GetTheme();                                                        
        }


        public static void DeleteProfileSettings(Guid profileId)
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            var items = JsonConvert.DeserializeObject<List<SettingModel>>(jsonString);

            var profilesettings = items.Where(s => s.ProfileId == profileId).ToList();
            foreach (var item in profilesettings)
            {
                items.Remove(item);
            }

            var _jsonString = JsonConvert.SerializeObject(items);
            File.WriteAllText(_jsonPath, _jsonString);
        }
    }
}
