using ImageMagick;
using Quartz.Controls;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Win32Interop.Structs;

namespace Quartz
{
    public partial class ModifyProfile : Form
    {
        private Profiles _profiles;
        private FavouriteService _fav = new FavouriteService();
        Image profilePicture;
        Image originalProfilePicture;
        string extension;
        ProfileOptions profileOptions;
        Guid _id;
        CircularImageButton button;

        public enum ProfileOptions
        {
            Create,
            Edit,
        }

        public ModifyProfile(Profiles profiles, ProfileOptions options, Guid id, CircularImageButton button)
        {
            InitializeComponent();
            _profiles = profiles;
            profileOptions = options;
            _id = id;
            this.button = button;
        }

        public void SetProfilePicture(Guid id, System.Drawing.Image image, string extention)
        {
            string directory = $@"{Application.StartupPath}\UserData\pictures\";
            string filename = $@"{id.ToString()}.{extention}";
            string path = directory + filename;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string current = $@"{Application.StartupPath}\UserData\pictures\{Program.profileService.Get(id).profilePicture}";
            if (File.Exists(current))
            {
                File.Delete(current);
            }

            if (image != null)
            {
                image.Save(path);
            }

            if (File.Exists(path))
            {
                Program.profileService.SetProfilePicture(id, filename);
                Program.profileService.SaveChanges();
            }

            _profiles.LoadProfiles();
        }

        bool AreImagesEqual(Image img1, Image img2)
        {
                using (var ms1 = new MemoryStream())
                using (var ms2 = new MemoryStream())
                {
                    img1.Save(ms1, System.Drawing.Imaging.ImageFormat.Png);
                    img2.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);

                    byte[] imgBytes1 = ms1.ToArray();
                    byte[] imgBytes2 = ms2.ToArray();

                    return imgBytes1.SequenceEqual(imgBytes2);
                }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (profileOptions == ProfileOptions.Create)
            {
                // Check if the name is not empty or whitespace
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    // Name is empty or whitespace, show error message and hides other
                    LabelNameNull.Visible = true;
                    txtExists.Visible = false;
                    return; // Exit early
                }

                //name is not null, proceed with the update
                LabelNameNull.Visible = false;
                txtExists.Visible = false;
                NewControlThemeChanger.ChangeControlTheme(txtName);


                // Check if the new name already exists
                if (Program.profileService.ExistsName(txtName.Text))
                {
                    // Name exists, show error message and hide others
                    LabelNameNull.Visible = false;
                    txtExists.Visible = true;
                    txtName.ForeColor = Color.Red;
                    return; // Exit early
                }

                // Name doesn't exist, proceed with the update
                LabelNameNull.Visible = false;
                txtExists.Visible = false;
                NewControlThemeChanger.ChangeControlTheme(txtName);

                var create = new Models.ProfileModel
                {
                    Name = txtName.Text,
                    Password = txtPassword.Text,
                };

                Program.profileService.Modify(create);
                Program.profileService.SetActive(create.Id);
                Program.profileService.SaveChanges();
                ProfileService.LoadCurrentProfile();

                if (profilePicture != null)
                {
                    SetProfilePicture(ProfileService.Current, profilePicture, extension);
                }

                if (MainSettingsService.Get("hasCreatedFirstProfile") == "false")
                {
                    if (Program.profileService.GetDefault() != null)
                    {
                        Program.profileService.UnsetDefault(Program.profileService.GetDefault().Id);
                        MainSettingsService.Set("RunBrowser", "false");
                    }
                    MainSettingsService.Set("hasCreatedFirstProfile", "true");
                }

                var favourite = new Models.FavouriteModel
                {
                    Name = "Apple",
                    WebAddress = "https://www.apple.com/",
                };
                _fav.Modify(favourite);
                _fav.SaveChanges();

                var favourite2 = new Models.FavouriteModel
                {
                    Name = "Google",
                    WebAddress = "https://www.google.com/",
                };
                _fav.Modify(favourite2);
                _fav.SaveChanges();

                var favourite3 = new Models.FavouriteModel
                {
                    Name = "YouTube",
                    WebAddress = "https://www.youtube.com/",
                };
                _fav.Modify(favourite3);
                _fav.SaveChanges();


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

                if (Program.profileService.Get(ProfileService.Current).isDisposable != true)
                {
                    SettingsService.Set("Theme", "auto (light/dark)");
                }
                else
                {
                    SettingsService.Set("Theme", "black");
                }

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

                Program.profileService.SetActive(Program.profileService.LastActiveProfile());
                Program.profileService.SaveChanges();
                _profiles.LoadProfiles();
                this.Close();

            }
            else if (profileOptions == ProfileOptions.Edit)
            {
                // Check if the name is not empty or whitespace
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    // Name is empty or whitespace, show error message and hides other
                    LabelNameNull.Visible = true;
                    txtExists.Visible = false;
                    return; // Exit early
                }

                //name is not null, proceed with the update
                LabelNameNull.Visible = false;
                txtExists.Visible = false;
                NewControlThemeChanger.ChangeControlTheme(txtName);


                // Check if the new name already exists
                if (Program.profileService.EditExistsName(txtName.Text, Program.profileService.Get(_id).Name))
                {
                    // Name exists, show error message and hide others
                    LabelNameNull.Visible = false;
                    txtExists.Visible = true;
                    txtName.ForeColor = Color.Red;
                    return; // Exit early
                }

                // Name doesn't exist, proceed with the update
                LabelNameNull.Visible = false;
                txtExists.Visible = false;
                NewControlThemeChanger.ChangeControlTheme(txtName);


                Program.profileService.Get(_id).Name = txtName.Text;
                Program.profileService.Get(_id).Password = txtPassword.Text;

                if (profilePicture != null)
                {
                    if (originalProfilePicture == null || !AreImagesEqual(profilePicture, originalProfilePicture))
                    {
                        SetProfilePicture(ProfileService.Current, profilePicture, extension);
                    }
                }
                else
                {
                    //string path = $@"{Application.StartupPath}\UserData\pictures\{Program.profileService.Get(_id).profilePicture}";

                    //if (File.Exists(path))
                    //{
                    //    File.Delete(path);
                    //}

                    Program.profileService.Get(_id).profilePicture = string.Empty;
                }

                Program.profileService.SaveChanges();
                _profiles.LoadProfiles();
                this.Close();
            }
        }

        private void IsActionButtonEnabled(bool isActionButtonEnabled)
        {
            if(isActionButtonEnabled)
            {
                string theme = SettingsService.Get("Theme");
                if (theme == "light")
                {
                    circularImageButton1.ActionButtonImage = Properties.Resources.Close;
                    circularImageButton1.ActionButtonHoverImage = Properties.Resources.CloseHover;
                }
                else if (theme == "dark")
                {
                    circularImageButton1.ActionButtonImage = Properties.Resources.Tabs_Close;
                    circularImageButton1.ActionButtonHoverImage = Properties.Resources.Tabs_CloseHoverInactive;
                }
                else if (theme == "black")
                {
                    circularImageButton1.ActionButtonImage = Properties.Resources.B_Close;
                    circularImageButton1.ActionButtonHoverImage = Properties.Resources.B_CloseHover;
                }
                else if (theme == "aqua")
                {
                    circularImageButton1.ActionButtonImage = Properties.Resources.Aqua_Close;
                    circularImageButton1.ActionButtonHoverImage = Properties.Resources.Aqua_CloseHover;
                }
                else if (theme == "xmas")
                {
                    circularImageButton1.ActionButtonImage = Properties.Resources.Close_xmas;
                    circularImageButton1.ActionButtonHoverImage = Properties.Resources.CloseHover_xmas;
                }
            }
            else
            {
                circularImageButton1.ActionButtonImage = null;
                circularImageButton1.ActionButtonHoverImage = null;
            }
        }

        private void CreateProfile_Load(object sender, EventArgs e)
        {
            if (SettingsService.Get("DraggableForms") == "true")
            {
                MouseDragger mouseDragger = new MouseDragger(this);
            }

            NewControlThemeChanger.ChangeTheme(this);
            txtExists.ForeColor = Color.Red;
            LabelNameNull.ForeColor = Color.Red;
            circularImageButton1.CircularImageBorderColor = circularImageButton1.ForeColor;

            if(profileOptions == ProfileOptions.Edit)
            {
                btnCreate.Text = "Save";
                profilePicture = Program.profileService.GetProfilePicture(Program.profileService.Get(_id).Id);
                originalProfilePicture = Program.profileService.GetProfilePicture(Program.profileService.Get(_id).Id);
                circularImageButton1.CircularImage = Program.profileService.GetProfilePicture(Program.profileService.Get(_id).Id);
                txtName.Text = Program.profileService.Get(_id).Name;
                txtPassword.Text = Program.profileService.Get(_id).Password;
            }
        }

        private void CreateProfile_KeyUp(object sender, KeyEventArgs e)
        {
            if (SettingsService.Get("escClose") == "true")
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            }
        }

        private void circularImageButton1_Click(object sender, EventArgs e)
        {    
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Profile Picture";
            openFileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg";
            openFileDialog.Multiselect = false;

            openFileDialog.ShowDialog();

            if (!string.IsNullOrEmpty(openFileDialog.FileName))
            {
                extension = Path.GetExtension(openFileDialog.FileName).Replace(".", "");
                Image image = System.Drawing.Image.FromFile(openFileDialog.FileName);
                profilePicture = image;
                circularImageButton1.CircularImage = image;                
            }
        }

        private void circularImageButton1_ActionButtonClick(object sender, EventArgs e)
        {
            profilePicture = null;
            circularImageButton1.CircularImage = Quartz.Properties.Resources.blank;
        }

        private void circularImageButton1_CircularImageChanged(object sender, EventArgs e)
        {
            IsActionButtonEnabled(profilePicture != null);

            if (profileOptions == ProfileOptions.Edit)
            {
                if (profilePicture != null)
                {
                    button.CircularImage = profilePicture;
                }
                else
                {
                    button.CircularImage = null;
                }
            }
        }

        private void ModifyProfile_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (profileOptions == ProfileOptions.Edit)
            {
                _profiles.LoadProfiles();
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            if (profileOptions == ProfileOptions.Edit)
            {
                button.ButtonText = txtName.Text;
                button.Invalidate();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                txtPassword.UseSystemPasswordChar = false; // Show password
                txtPassword.Refresh();
            }
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                txtPassword.UseSystemPasswordChar = true; // Hide password
                txtPassword.Refresh();
            }
        }
    }
}
