using EasyTabs;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz
{
    internal static class Program
    {
        // Keep this alive for the app lifetime
        public static TitleBarTabsApplicationContext EasyTabsContext;
        public static bool _bypassPassword = false;
        public static ProfileService profileService = new ProfileService();

        private static FavouriteService favouriteService = new FavouriteService();
        private static BirthdayService birthdayService = new BirthdayService();

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            EnsureUserDataFolders();
            InitializeDefaultProfile();
            HandleResetIfRequested();

            bool runBrowser = MainSettingsService.Get("RunBrowser") == "true";

            if (runBrowser)
                LaunchBrowser(args);
            else
                LaunchProfilesWindow(args);
        }

        #region Application Exit
        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (!_bypassPassword)
            {
                var defaultProfile = profileService.GetDefault();
                if (defaultProfile != null)
                {
                    MainSettingsService.Set("bypassPassword",
                        !string.IsNullOrEmpty(profileService.GetPassword(defaultProfile.Id)) ? "false" : "true");
                }
            }
            else
            {
                MainSettingsService.Set("bypassPassword", "true");
            }
        }
        #endregion

        #region Launch Methods
        private static void LaunchBrowser(string[] args)
        {
            OverrideActiveWithDisposableIfNecessary();

            var activeProfile = profileService.GetActive();
            if (!string.IsNullOrWhiteSpace(profileService.GetPassword(activeProfile.Id)) &&
                MainSettingsService.Get("bypassPassword") == "false")
            {
                var passwordDialog = new Password(activeProfile.Id, null);
                passwordDialog.ShowDialog();
                if (!passwordDialog.IsPasswordCorrect) return;

                MainSettingsService.Set("bypassPassword", "false");
            }

            EasyTabsContext = new TitleBarTabsApplicationContext();

            var firstContainer = new AppContainer();
            var browser = args.Length == 0 ? new Browser(null, false) : new Browser(args[0], true);
            browser.InitializeTab();

            firstContainer.Tabs.Add(new TitleBarTab(firstContainer) { Content = browser });
            firstContainer.SelectedTabIndex = 0;

            EasyTabsContext.Start(firstContainer);

            OpenBirthdayDialogIfNecessary();

            Application.ApplicationExit += Application_ApplicationExit;
            Application.Run(EasyTabsContext);
        }

        private static void LaunchProfilesWindow(string[] args)
        {
            Application.ApplicationExit += Application_ApplicationExit;

            if (args.Length == 0)
                Application.Run(new Profiles(null, null));
            else
                Application.Run(new Profiles(null, args[0]));
        }
        #endregion

        #region Birthday
        public static void OpenBirthdayDialogIfNecessary()
        {
            var today = GetRealTimeInZone.GetRealTimeInComputerTimeZone().Date;

            foreach (var birthday in birthdayService.All())
            {
                if (birthday.DOB.Date.Day == today.Day && birthday.DOB.Date.Month == today.Month)
                {
                    new HappyBirthdayMessage(birthday.Name, birthday.DOB.Year).ShowDialog();
                }
            }
        }
        #endregion

        #region Disposable
        private static void OverrideActiveWithDisposableIfNecessary()
        {
            if (profileService.GetDisposable() != null)
            {
                profileService.SetActive(profileService.GetDisposable().Id);
            }
        }
        #endregion

        #region Open New Windows/Tabs
        public static void OpenNewAppContainer(string address)
        {
            if (EasyTabsContext == null) return;

            var container = new AppContainer();
            var browser = string.IsNullOrEmpty(address) ? new Browser(null, false) : new Browser(address, true);
            browser.InitializeTab();

            container.Tabs.Add(new TitleBarTab(container) { Content = browser });
            container.SelectedTabIndex = 0;

            EasyTabsContext.Start(container);
        }

        public static async Task OpenNewWindowWithTabsFast(List<string> urls)
        {
            if (urls == null || urls.Count == 0 || EasyTabsContext == null)
                return;

            var container = new AppContainer();

            EasyTabsContext.Start(container);

            foreach (var url in urls)
            {
                var browser = string.IsNullOrEmpty(url)
                    ? new Browser(null, false)
                    : new Browser(url, true);

                browser.InitializeTab();

                var tab = new TitleBarTab(container) { Content = browser };
                container.Tabs.Add(tab);

                // Select immediately
                container.SelectedTab = tab;

                // Allow UI to process ONE frame → almost instant
                await Task.Yield();
            }
        }

        #endregion

        #region UserData & Reset
        private static void EnsureUserDataFolders()
        {
            string basePath = Path.Combine(Application.StartupPath, "UserData");
            Directory.CreateDirectory(basePath);

            foreach (var subDir in new[] { "webview2", "jsons" })
            {
                Directory.CreateDirectory(Path.Combine(basePath, subDir));
            }
        }

        private static void HandleResetIfRequested()
        {
            if (MainSettingsService.Get("Reset") != "true") return;

            string userdataPath = Path.Combine(Application.StartupPath, "UserData");

            int attempts = string.IsNullOrEmpty(MainSettingsService.Get("Attempts")) ? 1 : int.Parse(MainSettingsService.Get("Attempts")) + 1;
            MainSettingsService.Set("Attempts", attempts.ToString());

            if (attempts <= 10 && !Quartz.Libs.FileLockChecker.IsLocked(userdataPath))
            {
                try { if (Directory.Exists(userdataPath)) Directory.Delete(userdataPath, true); } catch { }
                Power.Restart();
            }
            else
            {
                var msg = MessageBox.Show(
                    "An error occurred while trying to delete your user data. Please ensure no files are locked and you have permission to delete them.",
                    "Something Went Wrong",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error);

                if (msg == DialogResult.Retry)
                {
                    MainSettingsService.Set("Attempts", "0");
                    Power.Restart();
                }
                else
                {
                    Power.Shutdown();
                }
            }

            Environment.Exit(0);
        }
        #endregion

        #region Initialize Default Profile
        private static async Task InitializeDefaultProfile()
        {
            if (profileService.All().Count == 0)
            {
                CreateDefaultProfile();
                await CreateDefaultFavourites();
                CreateDefaultBirthdays();
                SetDefaultSettings();
            }

            ProfileService.LoadCurrentProfile();
        }

        private static void CreateDefaultProfile()
        {
            var defaultProfile = new ProfileModel { Name = "Default" };
            profileService.Modify(defaultProfile);
            profileService.SetActive(defaultProfile.Id);
            profileService.SetDefault(defaultProfile.Id);
            profileService.SaveChanges();

            MainSettingsService.Set("hasCreatedFirstProfile", "false");
        }

        private static async Task CreateDefaultFavourites()
        {
            var favourites = new[]
            {
                new FavouriteModel { Name = "Apple", WebAddress = "https://www.apple.com/", Index = 0 },
                new FavouriteModel { Name = "Google", WebAddress = "https://www.google.com/", Index = 1 },
                new FavouriteModel { Name = "YouTube", WebAddress = "https://www.youtube.com/", Index = 2 }
            };

            foreach (var fav in favourites)
            {
                favouriteService.Modify(fav);
                favouriteService.SaveChanges();

                //await FaviconHelper.DownloadAndSaveFaviconAsync(fav.WebAddress);
            }
        }

        private static void CreateDefaultBirthdays()
        {
            var birthdays = new[]
            {
                new BirthdayModel { Name = "Quartz", DOB = DateTime.Parse("20 November 2022") },
                new BirthdayModel { Name = "Daniel Xaftellis", DOB = DateTime.Parse("2 May 2008") },
                new BirthdayModel { Name = "Kaitlyn Xaftellis", DOB = DateTime.Parse("24 February 2005") },
                new BirthdayModel { Name = "Taki Xaftellis", DOB = DateTime.Parse("17 July 1973") },
                new BirthdayModel { Name = "Carolyn Xaftellis", DOB = DateTime.Parse("2 October 1972") },
                new BirthdayModel { Name = "Julie Collen", DOB = DateTime.Parse("16 February 1942") },
                new BirthdayModel { Name = "Ebony Xaftellis", DOB = DateTime.Parse("13 October 2014") },
                new BirthdayModel { Name = "Argie Xaftellis", DOB = DateTime.Parse("17 March 1972") }
            };

            foreach (var bday in birthdays)
                birthdayService.Add(bday);

            birthdayService.SaveChanges();
        }

        private static void SetDefaultSettings()
        {
            MainSettingsService.Set("RunBrowser", "true");

            var settings = new Dictionary<string, string>
            {
                { "Theme", "auto (light/dark)" },
                { "simulateDate", "false" },
                { "IsSwipeNavigationEnabled", "true" },
                { "SearchEngine", "google" },
                { "IsZoomControlEnabled", "true" },
                { "IsPinchZoomEnabled", "true" },
                { "AreDevToolsEnabled", "true" },
                { "AreBrowserAcceleratorKeysEnabled", "true" },
                { "DefaultHomePage", "true" },
                { "IsPasswordAutosaveEnabled", "true" },
                { "IsGeneralAutofillEnabled", "true" },
                { "Zoom", "1.0" },
                { "DownloadAlignment", "TopLeft" },
                { "Private", "false" },
                { "TrackingPreventionLevel", "balanced" },
                { "Animation", "true" },
                { "MemoryUsage", "normal" },
                { "escClose", "true" },
                { "DraggableForms", "true" },
                { "IsScriptEnabled", "true" },
                { "IsStatusBarEnabled", "true" },
                { "HiddenPDFNone", "true" },
                { "SettingsTabAlignment", "top" },
                { "showFavouriteIcon", "true" },
                { "sortProfilesBy", "alphabetically" },
                { "defaultFavicon", "default" },
                { "sortFavouritesBy", "alphabetically" },
                { "displayFullURLs", "true" },
                { "showFavouritesBar", "false" }
            };

            foreach (var kv in settings)
                SettingsService.Set(kv.Key, kv.Value);
        }
        #endregion
    }
}
