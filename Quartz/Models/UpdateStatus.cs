using System;

namespace Quartz.Models
{
    public class UpdateStatusModel
    {
        public int SchemaVersion { get; set; }
        public string State { get; set; }
        public DateTime CheckedAtUtc { get; set; }
        public string InstalledVersion { get; set; }
        public string LatestVersion { get; set; }

        public bool IsUpdateAvailable
        {
            get
            {
                return State == "updateAvailable";
            }
        }
    }
}
