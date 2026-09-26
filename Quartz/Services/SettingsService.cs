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
        private static string _jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Xaftellis\Quartz\UserData\jsons", "settings.json");
        private static string WindowsTheme = ThemeHelper.GetTheme();

        [ThreadStatic] private static int _readSnapshotDepth;
        [ThreadStatic] private static List<SettingModel> _readSnapshot;

        // Use only around synchronous UI work, never across an await. Nested
        // setup helpers share one read; later events always see fresh settings.
        internal static IDisposable BeginReadSnapshot()
        {
            _readSnapshotDepth++;
            return new ReadSnapshotScope();
        }

        private sealed class ReadSnapshotScope : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (--_readSnapshotDepth == 0) _readSnapshot = null;
            }
        }

        private static List<SettingModel> ReadSettings()
        {
            if (_readSnapshotDepth > 0 && _readSnapshot != null) return _readSnapshot;
            string json = File.Exists(_jsonPath) ? File.ReadAllText(_jsonPath) : "[]";
            var items = JsonConvert.DeserializeObject<List<SettingModel>>(json) ?? new List<SettingModel>();
            if (_readSnapshotDepth > 0) _readSnapshot = items;
            return items;
        }

        public static string Get(string name)
        {
            return Get(name, ProfileService.Current);
        }

        public static string Get(string name, Guid profileId)
        {
            var items = ReadSettings();

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            var _items = items.FirstOrDefault(s => s.ProfileId == profileId && s.Name == name)?.Value;

            // Keep preferences saved by older Quartz versions usable.
            if (name == "Theme")
            {
                if (_items == "black") _items = "dark";
                else if (_items == "aqua" || _items == "xmas") _items = "light";
                else if (_items == "auto (light/black)") _items = "auto (light/dark)";
            }

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
            else
            {
                return _items;
            }
        }

        public static SettingModel GetModel(string name)
        {
            var items = ReadSettings();

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            var _items = items.FirstOrDefault(s => s.ProfileId == ProfileService.Current && s.Name == name);
            return _items;
        }

        public static string GetAutoTheme()
        {
            var name = "Theme";
            var items = ReadSettings();

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            var _items = items.FirstOrDefault(s => s.ProfileId == ProfileService.Current && s.Name == name)?.Value;

            if (name == "Theme" && _items == "auto (light/dark)" || name == "Theme" && _items == "auto (light/black)")
            {
                return "auto (light/dark)";
            }
            else
            {
                return null;
            }
        }

        public static void Set(string name, string value)
        {
            Set(name, value, ProfileService.Current);
        }

        public static void Set(string name, string value, Guid profileId)
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            var items = JsonConvert.DeserializeObject<List<SettingModel>>(jsonString);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            var original = items.FirstOrDefault(s => s.ProfileId == profileId && s.Name == name);
            if (original == null)
            {
                items.Add(new SettingModel() { ProfileId = profileId, Name = name, Value = value });
            }
            else
            {
                original.Value = value;
            }

            var _jsonString = JsonConvert.SerializeObject(items);
            File.WriteAllText(_jsonPath, _jsonString);
            _readSnapshot = null;
        }

        public static string GetWindowsTheme() => WindowsTheme;

        // Called on the UI thread by ThemeService, outside a settings read.
        internal static bool RefreshWindowsTheme()
        {
            string current = ThemeHelper.GetTheme();
            if (current == WindowsTheme) return false;
            WindowsTheme = current;
            return true;
        }

        public static bool IsWindowsThemeUpToDate() => WindowsTheme == ThemeHelper.GetTheme();

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
            _readSnapshot = null;
        }
    }
}
