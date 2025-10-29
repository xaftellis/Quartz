using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Quartz.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Forms;
using Quartz.Libs;

namespace Quartz.Services
{
    public class MainSettingsService
    {
        private static string _jsonPath = Path.Combine(Application.StartupPath + @"\UserData\Jsons", "startup.json");

        public static string Get(string name)
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            var items = JsonConvert.DeserializeObject<List<MainSettingModel>>(jsonString);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            return items.FirstOrDefault(s => s.Name == name)?.Value;
        }

        public static void Set(string name, string value)
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            var items = JsonConvert.DeserializeObject<List<MainSettingModel>>(jsonString);
         

            if (string.IsNullOrEmpty(name) == null)
                throw new ArgumentNullException("name");

            var original = items.FirstOrDefault(s => s.Name == name);
            if (original == null)
            {
                items.Add(new MainSettingModel() { Name = name, Value = value });
            }
            else
            {
                original.Name = name;
                original.Value = value;
            }

            var _jsonString = JsonConvert.SerializeObject(items);
            File.WriteAllText(_jsonPath, _jsonString);
        }
    }

}
