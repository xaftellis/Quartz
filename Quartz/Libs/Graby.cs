using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Win32Interop.Structs;
using System.Web;

namespace Quartz.Libs
{
    internal class Graby
    {
        #region Methods
        private static DateTime CalculateEaster(int year)
        {
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (19 * a + b - d - f + 15) % 30;
            int h = c / 4;
            int i = c % 4;
            int k = (32 + 2 * e + 2 * h - g - i) % 7;
            int l = (a + 11 * g + 22 * k) / 451;
            int m = g + k - 7 * l + 114;
            int month = m / 31;
            int day = (m % 31) + 1;

            return new DateTime(year, month, day);
        }

        private static DateTime CalculateGoodFriday(DateTime easterDate)
        {
            return easterDate.AddDays(-2); // Good Friday is 2 days before Easter Sunday
        }

        private static Icon GetDefaultFavicon()
        {
            var theme = SettingsService.Get("Theme");
            Icon icon = Quartz.Properties.Resources.favicon;

            //Checks to see if theme needs to override default selection.
            if (theme == "light")
            {
            }
            else if (theme == "dark")
            {
            }
            else if (theme == "black")
            {
                //icon = Properties.Resources._2D_Quartz__White_1;
            }
            else if (theme == "aqua")
            {
                //icon = Quartz.Properties.Resources.Aqua_Quartz1;
            }
            else if (theme == "xmas")
            {
                //override the default icon selection since its a theme based on a special holiday.
                icon = Quartz.Properties.Resources.favicon_xmas;
            }


            //Default Icon Selection

            //calkulates easter and good friday dates for the current year.
            var EasterDate = CalculateEaster(Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Year);
            var GoodFridayDate = CalculateGoodFriday(EasterDate);

            //Set Icon To Birthday Version.
            if (Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Day == 24 && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 2 ||
                         Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Day == 17 && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 7 ||
                         Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Day == 2 && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 10 ||
                         Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Day == 16 && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 2 ||
                         Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Day == 13 && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 10 ||
                         Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Day == 17 && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 3)
            {
                icon = Quartz.Properties.Resources.Birthday_Quartz1;
            }
            else if (Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 12)
            {
                icon = Properties.Resources.favicon_xmas;
            }
            //Sets Icon To Good Friday Version
            else if (Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Day == GoodFridayDate.Day && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == GoodFridayDate.Month && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Year == GoodFridayDate.Year)
            {
                icon = Quartz.Properties.Resources.Cross1;
            }

            else if (Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Day == EasterDate.Day && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == EasterDate.Month && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Year == EasterDate.Year)
            {
                icon = Quartz.Properties.Resources.Easter;
            }
            //carine logo
            //else if (Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Day == 31 && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Month == 1 && Quartz.Services.GetRealTimeInZone.GetRealTimeInComputerTimeZone().Year == 2025)
            //{
            //    icon = Quartz.Properties.Resources.CSHS;
            //}

            return icon;
        }
        #endregion

        public static async Task<Icon> ConvertAsync(Bitmap bmp)
        {
            MultiIcon multiIcon = new MultiIcon();
            SingleIcon singleIcon = multiIcon.Add("Icon");
            singleIcon.CreateFrom(bmp, IconOutputFormat.Vista);

            using (MemoryStream ms = new MemoryStream())
            {
                singleIcon.Save(ms);
                ms.Position = 0;
                return new Icon(ms);
            }
        }

        public static async Task<Icon> GetFaviconAsync(string address, int size)
        {
            if (Uri.IsWellFormedUriString(address, UriKind.Absolute))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    byte[] bytes = await client.GetByteArrayAsync($"https://www.google.com/s2/favicons?domain={address}&sz={size}");
                    MemoryStream stream = new MemoryStream(bytes);
                    Bitmap bmp = new Bitmap(stream);
                    Icon icon = await ConvertAsync(bmp);
                    return icon;
                }
                catch
                {
                    return GetDefaultFavicon();
                }
            }
            else
            {
                return GetDefaultFavicon();
            }
        }
    }
}
