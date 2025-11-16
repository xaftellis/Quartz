using EasyTabs;
using ImageMagick;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Quartz.Libs
{
    internal class FaviconHelper
    {
        private static bool IsInitialized = false;
        private static string defaultHash;

        public static async Task EnsureDefaultFaviconInitializedAsync()
        {
            if (IsInitialized) return;

            var webView = new WebView2();

            //CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, null, null);
            //CoreWebView2ControllerOptions controllerOptions = environment.CreateCoreWebView2ControllerOptions();

            await webView.EnsureCoreWebView2Async();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            webView.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                tcs.SetResult(true);
            };

            webView.CoreWebView2.Navigate("https://example.com/");

            // Wait until navigation finishes
            await tcs.Task;

            Stream stream = await webView.CoreWebView2.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png);
            if (stream != null && stream.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    defaultHash = ComputeSHA256(ms.ToArray());
                }
            }
            
            IsInitialized = true;
        }

        public static Icon ConvertToIcon(Stream stream)
        {
            using (MagickImage magickImage = new MagickImage(stream))
            {
                magickImage.Format = MagickFormat.Icon;

                using (var memoryStream = new MemoryStream())
                {
                    magickImage.Write(memoryStream);
                    memoryStream.Position = 0;

                    return new Icon(memoryStream);
                }
            }
        }

        public static bool DoesFaviconFileExist(string address)
        {
            FaviconService faviconService = new FaviconService();
            if(faviconService.Get(address) == null)
            {
                return false;
            }

            string directory = $@"{Application.StartupPath}\UserData\cache\";
            string filename = $@"{faviconService.Get(address).Id}.ico";
            string path = directory + filename;
            
            if(File.Exists(path))
            {
                return true;
            }

            return false;
        }

        private static WebView2 webView2 = new WebView2();
        public static async Task DownloadAndSaveFaviconAsync(string address)
        {
            if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
                return;

            // Skip if already saved
            if (FaviconHelper.DoesFaviconFileExist(address))
                return;

            if(webView2.CoreWebView2 == null)
            {
                //CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, null, null);
                //CoreWebView2ControllerOptions controllerOptions = environment.CreateCoreWebView2ControllerOptions();

                await webView2.EnsureCoreWebView2Async();
            }

            var tcs = new TaskCompletionSource<bool>();

            // Temporary event handler
            void Handler(object s, CoreWebView2NavigationCompletedEventArgs e)
            {
                tcs.TrySetResult(true);
            }

            webView2.CoreWebView2.NavigationCompleted += Handler;
            webView2.Source = new Uri(address);

            // Wait for navigation to complete
            await tcs.Task;
            webView2.CoreWebView2.NavigationCompleted -= Handler; // remove handler after use

            Stream faviconStream = await webView2.CoreWebView2.GetFaviconAsync(
                           Microsoft.Web.WebView2.Core.CoreWebView2FaviconImageFormat.Png);

            if (faviconStream == null || faviconStream.Length == 0)
                return;

            var memoryStream = new MemoryStream();
            await faviconStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            Icon icon = FaviconHelper.ConvertToIcon(memoryStream);
            FaviconHelper.SaveToFile(icon, address);
        }


        public static void UpdateCurrentTab(TitleBarTabs ParentTabs, Form _this)
        {
            ParentTabs.UpdateThumbnailPreviewIcon(ParentTabs.Tabs.Single(t => t.Content == _this));
            ParentTabs.RedrawTabs();
        }

        public static Icon GetFaviconFileExternal(string address)
        {
            if(DoesFaviconFileExist(address))
            {
                return GetFaviconFile(address);
            }
            else
            {
                return GetDefaultFavicon16();
            }
        }
        public static Image GetFaviconFileExternalAsImage(string address)
        {
            if (DoesFaviconFileExist(address))
            {
                return IconToImage(GetFaviconFile(address));
            }
            else
            {
                return IconToImage(GetDefaultFavicon16());
            }
        }

        public static Icon GetFaviconFile(string address)
        {
            FaviconService faviconService = new FaviconService();
            string directory = $@"{Application.StartupPath}\UserData\cache\";
            string filename = $@"{faviconService.Get(address).Id}.ico";
            string path = directory + filename;

            Icon icon = new Icon(path);
            return icon;
        }

        public static void SaveToFile(Icon icon, string address)
        {
            Guid guid = Guid.NewGuid();
            string directory = $@"{Application.StartupPath}\UserData\cache\";
            string filename = $@"{guid}.ico";
            string path = directory + filename;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if(icon != null)
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    icon.Save(fs);
                }
            }

            FaviconService faviconService = new FaviconService();
            FaviconModel faviconModel = new FaviconModel
            {
                Id = guid,
                WebAddress = address,
            };

            faviconService.Modify(faviconModel);
            faviconService.SaveChanges();
        }

        public async static Task<bool> IsDefaultFaviconAsync(Stream stream)
        {
            if (!IsInitialized) await EnsureDefaultFaviconInitializedAsync();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                byte[] faviconBytes = memoryStream.ToArray();

                // Compute the hash of the favicon

                string currentHash = ComputeSHA256(faviconBytes);


                if (currentHash == defaultHash)
                {
                    //MessageBox.Show("true");
                    return true;
                }
                else
                {
                    //MessageBox.Show("false");
                    return false;
                }
            }
        }

        public static string ComputeSHA256(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(data);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        //Default Favicon
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

        private static bool IsTodaySomeonesBirthday()
        {
            bool isTodaySomeonesBirthday = false;
            BirthdayService birthdayService = new BirthdayService();
            DateTime today = GetRealTimeInZone.GetRealTimeInComputerTimeZone();

            foreach(BirthdayModel birthday in birthdayService.All())
            {
                if(birthday.DOB.Day == today.Day 
                    && birthday.DOB.Month == today.Month)
                {
                    isTodaySomeonesBirthday = true;
                    break;
                }
            }

            return isTodaySomeonesBirthday;
        }

        public static Icon GetFullResDefaultFavicon()
        {
            Icon icon;

            if (SettingsService.Get("defaultFavicon").StartsWith("custom - "))
            {
                icon = new Icon(SettingsService.Get("defaultFavicon").Replace("custom - ", ""));
            }
            else
            {
                DateTime currentDateTime = GetRealTimeInZone.GetRealTimeInComputerTimeZone();
                DateTime easterDateTime = CalculateEaster(currentDateTime.Year);
                DateTime goodfridayDateTime = CalculateGoodFriday(easterDateTime);
                string theme = SettingsService.Get("Theme");

                //xmas icon (theme is xmas or its december)
                if (theme == "xmas" 
                    || currentDateTime.Month == 12)
                {
                    icon = Quartz.Properties.Resources.favicon_xmas;
                }
                //birthday icon (if its someones birthday)
                else if (IsTodaySomeonesBirthday())
                {
                    icon = Quartz.Properties.Resources.Birthday_Quartz;
                }
                //good friday icon (when its good friday) (needs updating)
                else if (currentDateTime.Date == goodfridayDateTime.Date)
                {
                    icon = Quartz.Properties.Resources.Cross;
                }
                //easter icon (when its easter) (needs updating)
                else if (currentDateTime.Date == easterDateTime.Date)
                {
                    icon = Quartz.Properties.Resources.Easter;
                }
                else
                {
                    icon = Quartz.Properties.Resources.favicon;
                }
            }
            return icon;
        }

        public static Icon GetFullResDefaultFaviconWithoutCustomFavicon()
        {
            Icon icon;

            DateTime currentDateTime = GetRealTimeInZone.GetRealTimeInComputerTimeZone();
            DateTime easterDateTime = CalculateEaster(currentDateTime.Year);
            DateTime goodfridayDateTime = CalculateGoodFriday(easterDateTime);
            string theme = SettingsService.Get("Theme");

            //xmas icon (theme is xmas or its december)
            if (theme == "xmas"
                || currentDateTime.Month == 12)
            {
                icon = Quartz.Properties.Resources.favicon_xmas;
            }
            //birthday icon (if its someones birthday)
            else if (IsTodaySomeonesBirthday())
            {
                icon = Quartz.Properties.Resources.Birthday_Quartz;
            }
            //good friday icon (when its good friday) (needs updating)
            else if (currentDateTime.Date == goodfridayDateTime.Date)
            {
                icon = Quartz.Properties.Resources.Cross;
            }
            //easter icon (when its easter) (needs updating)
            else if (currentDateTime.Date == easterDateTime.Date)
            {
                icon = Quartz.Properties.Resources.Easter;
            }
            else
            {
                icon = Quartz.Properties.Resources.favicon;
            }
            return icon;
        }

        public static Icon GetDefaultFavicon16()
        {
            Icon icon;

            if (SettingsService.Get("defaultFavicon").StartsWith("custom - "))
            {
                icon = new Icon(SettingsService.Get("defaultFavicon").Replace("custom - ", ""));
            }
            else
            {
                DateTime currentDateTime = GetRealTimeInZone.GetRealTimeInComputerTimeZone();
                DateTime easterDateTime = CalculateEaster(currentDateTime.Year);
                DateTime goodfridayDateTime = CalculateGoodFriday(easterDateTime);
                string theme = SettingsService.Get("Theme");

                //xmas icon (theme is xmas or its december)
                if (theme == "xmas"
                    || currentDateTime.Month == 12)
                {
                    icon = Quartz.Properties.Resources.xmas16;
                }
                //birthday icon (if its someones birthday)
                else if (IsTodaySomeonesBirthday())
                {
                    icon = Quartz.Properties.Resources.birthday16;
                }
                //good friday icon (when its good friday) (needs updating)
                else if (currentDateTime.Date == goodfridayDateTime.Date)
                {
                    icon = Quartz.Properties.Resources.goodFriday16;
                }
                //easter icon (when its easter) (needs updating)
                else if (currentDateTime.Date == easterDateTime.Date)
                {
                    icon = Quartz.Properties.Resources.easter16;
                }
                else
                {
                    icon = Quartz.Properties.Resources.favicon16;
                }
            }
            return icon;
        }

        public static Image GetFullResDefaultFaviconAsImage()
        {
            Image image;

            DateTime currentDateTime = GetRealTimeInZone.GetRealTimeInComputerTimeZone();
            DateTime easterDateTime = CalculateEaster(currentDateTime.Year);
            DateTime goodfridayDateTime = CalculateGoodFriday(easterDateTime);
            string theme = SettingsService.Get("Theme");

            //xmas icon (theme is xmas or its december)
            if (theme == "xmas"
                || currentDateTime.Month == 12)
            {
                image = Quartz.Properties.Resources.Quartz_Xmas;
            }
            //birthday icon (if its someones birthday)
            else if (IsTodaySomeonesBirthday())
            {
                image = Quartz.Properties.Resources.Birthday_Quartz1;
            }
            //good friday icon (when its good friday) (needs updating)
            else if (currentDateTime.Date == goodfridayDateTime.Date)
            {
                image = Quartz.Properties.Resources.Cross1;
            }
            //easter icon (when its easter) (needs updating)
            else if (currentDateTime.Date == easterDateTime.Date)
            {
                image = Quartz.Properties.Resources.Easter1;
            }
            else
            {
                image = Quartz.Properties.Resources.Quartz;
            }
            return image;
        }

        public static Image GetDefaultFaviconAsImage16()
        {
            Icon icon = GetDefaultFavicon16();
            var stream = new MemoryStream();
            icon.Save(stream);
            Image image = Image.FromStream(stream);

            return image;
        }

        public static Image IconToImage(Icon icon)
        {
            var stream = new MemoryStream();
            icon.Save(stream);
            Image image = Image.FromStream(stream);

            return image;
        }
    }
}
