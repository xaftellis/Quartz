using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Services
{
    internal class GetRealTimeInZone
    {
        public static DateTime GetRealTimeInComputerTimeZone()
        {
            if (!string.IsNullOrWhiteSpace(SettingsService.Get("timeMachine")) && SettingsService.Get("simulateDate") == "true")
            {
                return DateTime.Parse(SettingsService.Get("timeMachine"));
            }
            else
            {
                return DateTime.Now;
            }
        }
    }
}
