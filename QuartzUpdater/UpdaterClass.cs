using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace QuartzUpdater
{
    public static class UpdaterClass
    {
        public async static Task Initalize()
        {
            Program.CurrentVersion = GetQuartzVersion();
            Program.LatestVersion = await GetLatestVersion();

        }
        public static Version GetQuartzVersion()
        {
            string quartzPath = Path.Combine(
                AppContext.BaseDirectory,
                "Quartz.exe"
            );

            var info = FileVersionInfo.GetVersionInfo(quartzPath);
            var version = new Version(info.ProductVersion);

            return new Version(version.Major, version.Minor, version.Build);
        }

        public static async Task<Version> GetLatestVersion()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("QuartzUpdater");

                string json = await client.GetStringAsync(
                    "https://api.github.com/repos/xaftellis/Quartz/releases/latest"
                );

                JObject release = JObject.Parse(json);

                string tag = release["tag_name"].ToString();
                Version version = new Version(tag.TrimStart('v'));

                string expectedFileName = "Quartz." + tag + ".zip";

                JToken zipAsset = release["assets"]
                    .First(asset =>
                        string.Equals(
                            asset["name"].ToString(),
                            expectedFileName,
                            StringComparison.OrdinalIgnoreCase
                        ));

                Program.LatestDownloadUrl =
                    zipAsset["browser_download_url"].ToString();

                return version;
            }
        }
    }
}