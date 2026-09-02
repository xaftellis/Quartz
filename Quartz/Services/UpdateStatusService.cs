using Newtonsoft.Json;
using Quartz.Models;
using System;
using System.IO;

namespace Quartz.Services
{
    public class UpdateStatusService
    {
        private static string _jsonPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Xaftellis",
            "Quartz",
            "UpdateStatus.json");

        public static UpdateStatusModel Get()
        {
            if (!File.Exists(_jsonPath))
                return null;

            try
            {
                string jsonString = File.ReadAllText(_jsonPath);

                return JsonConvert.DeserializeObject<UpdateStatusModel>(
                    jsonString);
            }
            catch
            {
                return null;
            }
        }
    }
}
