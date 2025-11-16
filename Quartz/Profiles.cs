using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Services;
using Quartz.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Web.Profile;

namespace Quartz
{
    public partial class Profiles : Form
    {
        public HistoryService HistoryService = new HistoryService();
        public FavouriteService FavouriteService = new FavouriteService();
        public BirthdayService BirthdayService = new BirthdayService();

        int tabIndex = 0;

        string address;

        public Browser _browser = null;

        public Profiles(Browser browser, string url)
        {
            InitializeComponent();
            _browser = browser;
            address = url;
        }

        public string GetTimeAgo(DateTime? pastDate)
        {
            if (!pastDate.HasValue)
            {
                return "No date provided";  // Handle null DateTime
            }

            DateTime date = pastDate.Value;

            // Check if the date is ridiculously far in the future or past
            if (date == default)
            {
                return "Never";  // Handle extreme past or future dates
            }

            if (date > DateTime.Now)
            {
                return "Invalid date";  // Handle extreme past or future dates
            }


            TimeSpan timeSpan = DateTime.Now - date;

            if (timeSpan.TotalSeconds < 60)
            {
                // Seconds ago
                int seconds = (int)timeSpan.TotalSeconds;
                return seconds == 1 ? "1 second ago" : $"{seconds} seconds ago";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                // Minutes ago
                int minutes = (int)timeSpan.TotalMinutes;
                return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
            }
            else if (timeSpan.TotalHours < 24)
            {
                // Hours ago
                int hours = (int)timeSpan.TotalHours;
                return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }
            else if (timeSpan.TotalDays < 7)
            {
                // Days ago
                int days = (int)timeSpan.TotalDays;
                return days == 1 ? "1 day ago" : $"{days} days ago";
            }
            else if (timeSpan.TotalDays < 30)
            {
                // Weeks ago
                int weeks = (int)(timeSpan.TotalDays / 7);
                return weeks == 1 ? "1 week ago" : $"{weeks} weeks ago";
            }
            else if (timeSpan.TotalDays < 365)
            {
                // Months ago
                int months = (int)(timeSpan.TotalDays / 30); // Approximate months
                return months == 1 ? "1 month ago" : $"{months} months ago";
            }
            else
            {
                // Years ago
                int years = (int)(timeSpan.TotalDays / 365); // Approximate years
                return years == 1 ? "1 year ago" : $"{years} years ago";
            }
        }

        public void LoadProfiles()
        {
            pnlProfiles.Controls.Clear();

            List<ProfileModel> dataSource = null;

            Program.profileService.Reload();

            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                if (SettingsService.Get("sortProfilesBy") == "alphabetically")
                    dataSource = Program.profileService.All().OrderBy(i => i.Name).ToList();
                else if (SettingsService.Get("sortProfilesBy") == "lastactive")
                    dataSource = dataSource = Program.profileService.All()
                    .OrderByDescending(p => p.lastActive != default(DateTime))                // Active profiles first
                    .ThenByDescending(p => p.lastActive != default(DateTime) ? p.lastActive : DateTime.MinValue) // Most recent lastActive first
                    .ThenByDescending(p => p.dateCreated)
                    .ToList();
                else if (SettingsService.Get("sortProfilesBy") == "datecreated")
                    dataSource = dataSource = Program.profileService.All()
                    .OrderBy(p => p.dateCreated)
                    .ToList();
            }
            else
            {
                dataSource = Program.profileService.Find(txtSearch.Text).OrderBy(i => i.Name).ToList();
            }

            foreach (var profile in dataSource)
            {
                var button = new CircularImageButton
                {
                    ButtonText = profile.Name,
                    Tag = profile.Id,
                    AutoSize = false,
                    Height = 100,
                    Width = 100,
                    ContextMenuStrip = ContextMenuStripProfiles,
                    CircularImageSize = 64,
                    CircularImageToTextGapping = 5,
                };

                if (Program.profileService.GetProfilePicture(profile.Id) != null)
                {
                    button.CircularImage = Program.profileService.GetProfilePicture(profile.Id);
                }

                ToolTip toolTip = new ToolTip();

                string lastActive;
                if (profile.Active == true && Application.OpenForms["AppContainer"] != null)
                {
                    lastActive = "Last active: Now";
                }
                else
                {
                    lastActive = "Last active: " + GetTimeAgo(profile.lastActive);
                }

                string dateCreated = "Created: " + profile.dateCreated.ToString("D").Replace(profile.dateCreated.DayOfWeek + ", ", "");

                string tooltip = 
                    $@"{profile.Name}
{lastActive}
{dateCreated}";

                toolTip.SetToolTip(button, tooltip);

                pnlProfiles.Controls.Add(button);
                button.Click += button_click;

                button.Enabled = !profile.isDisposable;

                NewControlThemeChanger.ChangeTheme(this);
            }
        }

        private void button_click(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                var button = (Button)sender;

                if (ProfileService.Current == Guid.Parse(button.Tag.ToString()) && Application.OpenForms["AppContainer"] != null)
                {
                    MessageBox.Show("This is already the current profile, silly!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!string.IsNullOrEmpty(Program.profileService.GetPassword(Guid.Parse(button.Tag.ToString()))))
                {
                    Password form = new Password(Guid.Parse(button.Tag.ToString()), this);
                    form.ShowDialog();

                    if (!form.IsPasswordCorrect)
                    {
                        return;
                    }
                }

                if (string.IsNullOrEmpty(address))
                {
                    Program.profileService.SetActive(Guid.Parse(button.Tag.ToString()));
                    Program.profileService.SaveChanges();
                    MainSettingsService.Set("RunBrowser", "true");
                    Power.Restart();
                }
                else
                {
                    if (Application.OpenForms["AppContainer"] != null)
                    {
                        Program.profileService.SetActive(Guid.Parse(button.Tag.ToString()));
                        Program.profileService.SaveChanges();
                        MainSettingsService.Set("RunBrowser", "true");

                        string[] newArgs = { address };

                        RestartProgram(newArgs);
                    }
                    else
                    {
                        Program.profileService.SetActive(Guid.Parse(button.Tag.ToString()));
                        Program.profileService.SaveChanges();
                        MainSettingsService.Set("RunBrowser", "true");

                        string[] newArgs = { address };

                        RestartProgram(newArgs);
                    }
                }
                Program._bypassPassword = true;
            }
        }

        private void RestartProgram(string[] newArgs)
        {
            // Start a new instance of the program with the new arguments
            Process.Start(Application.ExecutablePath, string.Join(" ", newArgs));

            // Optionally, terminate the current process to avoid multiple instances
            Environment.Exit(0);
        }

        private void Profiles_Load(object sender, EventArgs e)
        {
            bool isappcontainerOpen = Application.OpenForms["AppContainer"] != null;

            if (isappcontainerOpen && SettingsService.Get("DraggableForms") == "true")
            {
                MouseDragger mouseDragger = new MouseDragger(this);
            }

            LoadProfiles();

            if (SettingsService.Get("sortProfilesBy") == "alphabetically")
            {
                cbSortBy.SelectedIndex = 0;
            }
            else if (SettingsService.Get("sortProfilesBy") == "lastactive")
            {
                cbSortBy.SelectedIndex = 1;
            }
            else if (SettingsService.Get("sortProfilesBy") == "datecreated")
            {
                cbSortBy.SelectedIndex = 2;
            }

            NewControlThemeChanger.ChangeTheme(this);
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            var create = new ModifyProfile(this, ModifyProfile.ProfileOptions.Create, Guid.Empty, null);
            create.Owner = this;
            create.ShowDialog();
        }

        private async void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Button button = (Button)owner.SourceControl;

                    if (!string.IsNullOrEmpty(Program.profileService.GetPassword(Guid.Parse(button.Tag.ToString()))))
                    {
                        Password passwordForm = new Password(Guid.Parse(button.Tag.ToString()), this);
                        passwordForm.ShowDialog();
                        if (!passwordForm.IsPasswordCorrect)
                        {
                            return;
                        }
                    }

                    if (MessageBox.Show($"You are about to remove {button.Text}. Are you sure you want to continue?", "Profiles", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        await DeleteWebViewProfile(button.Tag.ToString());

                        SettingsService.DeleteProfileSettings(Guid.Parse(button.Tag.ToString()));
                        HistoryService.DeleteProfileHistory(Guid.Parse(button.Tag.ToString()));
                        FavouriteService.DeleteProfileFav(Guid.Parse(button.Tag.ToString()));
                        BirthdayService.DeleteProfileBirthdays(Guid.Parse(button.Tag.ToString()));

                        Program.profileService.Remove(Guid.Parse(button.Tag.ToString()));
                        Program.profileService.SaveChanges();

                        if (Guid.Parse(button.Tag.ToString()) == ProfileService.Current)
                        {
                            Program.profileService.SetActive(Program.profileService.LastActiveProfile());
                        }
                        Program.profileService.SaveChanges();
                        ProfileService.LoadCurrentProfile();
                        LoadProfiles();
                    }
                }
            }
        }

        public async System.Threading.Tasks.Task DeleteWebViewProfile(string profile)
        {
            WebView2 webView2 = new WebView2();

            CoreWebView2EnvironmentOptions environmentOptions = new CoreWebView2EnvironmentOptions();
            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, $@"{Application.StartupPath}\UserData\WebView2\", environmentOptions);
            CoreWebView2ControllerOptions controllerOptions = environment.CreateCoreWebView2ControllerOptions();

            controllerOptions.ProfileName = profile;

            if (webView2.CoreWebView2 == null)
            {
                await webView2.EnsureCoreWebView2Async(environment, controllerOptions);
            }

            webView2.CoreWebView2.Profile.Delete();

            webView2.Dispose();
        }


        private void setDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Button button = (Button)owner.SourceControl;

                    if (!string.IsNullOrEmpty(Program.profileService.GetPassword(Guid.Parse(button.Tag.ToString()))))
                    {
                        Password passwordForm = new Password(Guid.Parse(button.Tag.ToString()), this);
                        passwordForm.ShowDialog();
                        if (!passwordForm.IsPasswordCorrect)
                        {
                            return;
                        }
                    }

                    if (setDefaultToolStripMenuItem.Text == "Unset Default")
                    {
                        Program.profileService.UnsetDefault(Guid.Parse(button.Tag.ToString()));
                        Program.profileService.SaveChanges();
                        MainSettingsService.Set("RunBrowser", "false");
                    }
                    else if (setDefaultToolStripMenuItem.Text == "Set Default")
                    {
                        Program.profileService.SetDefault(Guid.Parse(button.Tag.ToString()));
                        Program.profileService.SetActive(Guid.Parse(button.Tag.ToString()));
                        Program.profileService.SaveChanges();
                        MainSettingsService.Set("RunBrowser", "true");
                    }
                }
            }
        }

        private void ContextMenuStripProfiles_Opening(object sender, CancelEventArgs e)
        {
            if (SettingsService.Get("Animation") == "true")
            {
                Animation.AnimateWindow(ContextMenuStripProfiles.Handle, 100, Animation.AW_BLEND);
            }

            // Get the control that is displaying this context menu
            Button button = (Button)ContextMenuStripProfiles.SourceControl;
            button.Select();

            profilesToolStripMenuItem.Text = "Profiles: " + Program.profileService.All().Count.ToString();

            //if (Program.profileService.Current == Guid.Parse(button.Tag.ToString()) && Application.OpenForms["AppContainer"] != null)
            //{

            //    editToolStripMenuItem.Enabled = false;
            //}
            //else
            //{
            //    editToolStripMenuItem.Enabled = true;
            //}

            //if there is only one profile
            if ((Program.profileService.All().Count == 1)
            || (Application.OpenForms["AppContainer"] != null && ProfileService.Current == Guid.Parse(button.Tag.ToString())))
            {
                deleteToolStripMenuItem.Enabled = false;
            }
            else
            {
                deleteToolStripMenuItem.Enabled = true;
            }

            if (Program.profileService.GetDefault() != null)
            {
                if (Guid.Parse(button.Tag.ToString()) == Program.profileService.GetDefault().Id)
                {
                    setDefaultToolStripMenuItem.Text = "Unset Default";
                    //createPasswordToolStripMenuItem.Enabled = false;
                }
                else
                {
                    setDefaultToolStripMenuItem.Text = "Set Default";
                    //createPasswordToolStripMenuItem.Enabled = true;
                }
            }
            else
            {
                setDefaultToolStripMenuItem.Text = "Set Default";
            }

            //if (string.IsNullOrEmpty(Program.profileService.GetPassword(Guid.Parse(button.Tag.ToString()))))
            //{
            //    //createPasswordToolStripMenuItem.Text = "Create Password";
            //    //removePasswordToolStripMenuItem.Visible = false;
            //    //setDefaultToolStripMenuItem.Enabled = true;
            //}
            //else
            //{
            //    createPasswordToolStripMenuItem.Text = "Change Password";
            //    removePasswordToolStripMenuItem.Visible = true;
            //    //setDefaultToolStripMenuItem.Enabled = false;
            //}
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    CircularImageButton button = (CircularImageButton)owner.SourceControl;

                    if (!string.IsNullOrEmpty(Program.profileService.GetPassword(Guid.Parse(button.Tag.ToString()))))
                    {
                        Password passwordForm = new Password(Guid.Parse(button.Tag.ToString()), this);
                        passwordForm.ShowDialog();
                        if (!passwordForm.IsPasswordCorrect)
                        {
                            return;
                        }
                    }

                    var form = new ModifyProfile(this, ModifyProfile.ProfileOptions.Edit, Guid.Parse(button.Tag.ToString()), button);
                    form.Owner = this;
                    form.ShowDialog();
                }
            }
        }

        private void Profiles_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_browser != null)
            {
                _browser.Shortcuts(true);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadProfiles();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadProfiles();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var create = new Models.ProfileModel
            {
                isDisposable = true,
                endSession = true,
            };

            Program.profileService.Modify(create);

            Program.profileService.SetActive(create.Id);

            var current = Program.profileService.Get(ProfileService.Current);

            if (current != null)
            {
                current.Name = current.Id.ToString();
            }

            Program.profileService.SaveChanges();

            ProfileService.LoadCurrentProfile();

            // Add favourites
            AddFavourite("Apple", "https://www.apple.com/");
            AddFavourite("Google", "https://www.google.com/");
            AddFavourite("YouTube", "https://www.youtube.com/");

            BirthdayService birthdayService = new BirthdayService();
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

            // Apply settings
            MainSettingsService.Set("RunBrowser", "true");
            SettingsService.Set("Theme", "black");
            SettingsService.Set("simulateDate", "false");
            SettingsService.Set("IsSwipeNavigationEnabled", "true");
            SettingsService.Set("SearchEngine", "google");
            SettingsService.Set("IsZoomControlEnabled", "true");
            SettingsService.Set("IsPinchZoomEnabled", "true");
            SettingsService.Set("AreDevToolsEnabled", "true");
            SettingsService.Set("AreBrowserAcceleratorKeysEnabled", "true");
            SettingsService.Set("DefaultHomePage", "true");
            SettingsService.Set("IsPasswordAutosaveEnabled", "true");
            SettingsService.Set("IsGeneralAutofillEnabled", "true");
            SettingsService.Set("Zoom", "1.0");
            SettingsService.Set("DownloadAlignment", "TopLeft");
            SettingsService.Set("Private", "false");
            SettingsService.Set("TrackingPreventionLevel", "balanced");
            SettingsService.Set("Animation", "true");
            SettingsService.Set("MemoryUsage", "normal");
            SettingsService.Set("escClose", "true");
            SettingsService.Set("DraggableForms", "true");
            SettingsService.Set("IsScriptEnabled", "true");
            SettingsService.Set("IsStatusBarEnabled", "true");
            SettingsService.Set("HiddenPDFNone", "true");
            SettingsService.Set("SettingsTabAlignment", "top");
            SettingsService.Set("showFavouriteIcon", "true");
            SettingsService.Set("sortProfilesBy", "alphabetically");
            SettingsService.Set("defaultFavicon", "default");
            SettingsService.Set("sortFavouritesBy", "alphabetically");
            SettingsService.Set("displayFullURLs", "true");
            SettingsService.Set("showFavouritesBar", "false");

            Power.Restart();
        }

        // Helper for favourites with logging
        private void AddFavourite(string name, string url)
        {
            var favourite = new Models.FavouriteModel { Name = name, WebAddress = url };
            FavouriteService.Modify(favourite);
            FavouriteService.SaveChanges();
        }


        private void Profiles_KeyUp(object sender, KeyEventArgs e)
        {
            bool isappcontainerOpen = Application.OpenForms["AppContainer"] != null;


            if (isappcontainerOpen && SettingsService.Get("escClose") == "true")
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            }
        }

        private void cbSortBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            //alphabetically
            if (cbSortBy.SelectedIndex == 0)
            {
                SettingsService.Set("sortProfilesBy", "alphabetically");
                LoadProfiles();
            }
            else if (cbSortBy.SelectedIndex == 1)
            {
                SettingsService.Set("sortProfilesBy", "lastactive");
                LoadProfiles();
            }
            else if (cbSortBy.SelectedIndex == 2)
            {
                SettingsService.Set("sortProfilesBy", "datecreated");
                LoadProfiles();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            msg.Dialog();
        }
    }
}
