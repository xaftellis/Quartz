using Quartz.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Win32Interop.Enums;

namespace Quartz
{
    public partial class Favourite : Form
    {
        private FavouriteService _service = new FavouriteService();
        private Browser _browser;
        private string _text;
        private string _webAddress;
        private bool _modify;
        Button favButton;

        public Favourite(Browser browser, string text, string webAddress, bool modify, Button button)
        {
            _browser = browser;
            _text = text;
            _webAddress = webAddress;
            _modify = modify;
            favButton = button;
            InitializeComponent();
        }

        private void Favourite_Load(object sender, EventArgs e)
        {
            if (SettingsService.Get("DraggableForms") == "true")
            {
                MouseDragger mouseDragger = new MouseDragger(this);
            }
            ProfileService.LoadCurrentProfile();

            NameTextBox.Text = _text;
            AddressTextBox.Text = _webAddress;

            NewControlThemeChanger.ChangeTheme(this);

            txtExist.ForeColor = Color.Red;
            txtURLBad.ForeColor = Color.Red;
            NameMessage.ForeColor = Color.Red;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_modify == false)
            {
                var rawUrl = AddressTextBox.Text;
                if (Uri.IsWellFormedUriString(rawUrl, UriKind.Absolute))
                {
                    txtURLBad.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(AddressTextBox);
                }
                else
                {
                    txtURLBad.Visible = true;
                    AddressTextBox.ForeColor = Color.Red;
                }

                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    NameMessage.Visible = true;
                }
                else
                {
                    NameMessage.Visible = false;
                }

                if (string.IsNullOrEmpty(AddressTextBox.Text))
                {
                    NameMessage.Visible = true;
                }
                else
                {
                    NameMessage.Visible = false;
                }

                if (_service.Exists(NameTextBox.Text))
                {
                    txtExist.Visible = true;
                    NameTextBox.ForeColor = Color.Red;
                }
                else
                {
                    txtExist.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(AddressTextBox);

                }

                if (_service.ExistsAddress(AddressTextBox.Text))
                {
                    txtExist.Visible = true;
                    AddressTextBox.ForeColor = Color.Red;
                }
                else
                {
                    txtExist.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(AddressTextBox);
                }

                if (NameTextBox.Text != string.Empty && AddressTextBox.Text != string.Empty && !_service.Exists(NameTextBox.Text) && !_service.ExistsAddress(AddressTextBox.Text) && Uri.IsWellFormedUriString(rawUrl, UriKind.Absolute))
                {
                    var favourite = new Models.FavouriteModel
                    {
                        Name = NameTextBox.Text,
                        WebAddress = AddressTextBox.Text,
                    };

                    _service.Modify(favourite);
                    _service.SaveChanges();

                    if (SettingsService.Get("sortFavouritesBy") == "alphabetically")
                    {
                        _browser.SortByAlphabetially();
                    }
                    else
                    {
                        _service.Get(NameTextBox.Text).Index = _service.All().Count - 1;
                        _service.SaveChanges();
                    }

                        this.Close();
                    _browser.LoadFavourites();
                }
            }
            else
            {
                var rawUrl = AddressTextBox.Text;
                if (Uri.IsWellFormedUriString(rawUrl, UriKind.Absolute))
                {
                    txtURLBad.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(AddressTextBox);
                }
                else
                {
                    txtURLBad.Visible = true;
                    AddressTextBox.ForeColor = Color.Red;
                }

                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    NameMessage.Visible = true;
                }
                else
                {
                    NameMessage.Visible = false;
                }

                if (string.IsNullOrEmpty(AddressTextBox.Text))
                {
                    NameMessage.Visible = true;
                }
                else
                {
                    NameMessage.Visible = false;
                }

                if (_service.ExistsModify(NameTextBox.Text, _text))
                {
                    txtExist.Visible = true;
                    NameTextBox.ForeColor = Color.Red;
                }
                else
                {
                    txtExist.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(AddressTextBox);

                }

                if (_service.ExistsAddressModify(AddressTextBox.Text, _service.Get(_text).WebAddress))
                {
                    txtExist.Visible = true;
                    AddressTextBox.ForeColor = Color.Red;
                }
                else
                {
                    txtExist.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(AddressTextBox);
                }

                if (NameTextBox.Text != string.Empty && AddressTextBox.Text != string.Empty && !_service.ExistsModify(NameTextBox.Text, _text) && !_service.ExistsAddressModify(AddressTextBox.Text, _service.Get(_text).WebAddress) && Uri.IsWellFormedUriString(rawUrl, UriKind.Absolute))
                {
                    _service.Edit(_text, NameTextBox.Text, AddressTextBox.Text);
                    _service.SaveChanges();
                    this.Close();
                    _browser.LoadFavourites();
                }
            }
        }

        private void Favourite_Leave(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Favourite_FormClosing(object sender, FormClosingEventArgs e)
        {
            _browser.Shortcuts(true);
            _browser.LoadFavourites();
        }

        private void Favourite_KeyUp(object sender, KeyEventArgs e)
        {
            if (SettingsService.Get("escClose") == "true")
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.Close();
                }
            }
        }

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if(favButton != null)
            {
                if (SettingsService.Get("showFavouriteIcon") == "true")
                {
                    favButton.Text = "      " + NameTextBox.Text;
                }
                else
                {
                    favButton.Text = NameTextBox.Text;
                }
            }
        }
    }
}
