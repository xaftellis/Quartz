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

        public 
            Favourite(Browser browser, string text, string webAddress, bool modify, Button button)
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
            string name = NameTextBox.Text.Trim();
            string address = AddressTextBox.Text.Trim();

            if (Uri.IsWellFormedUriString(address, UriKind.Absolute))
            {
                txtURLBad.Visible = false;
                NewControlThemeChanger.ChangeControlTheme(address);
            }
            else
            {
                txtURLBad.Visible = true;
                AddressTextBox.ForeColor = Color.Red;
            }

            if (string.IsNullOrWhiteSpace(name)
                   || string.IsNullOrWhiteSpace(address))
            {
                NameMessage.Visible = true;
            }
            else
            {
                NameMessage.Visible = false;
            }

            if (_modify == false)
            {
                if (_service.Exists(name)
                    || _service.ExistsAddress(address))
                {
                    txtExist.Visible = true;
                    NameTextBox.ForeColor = Color.Red;
                }
                else
                {
                    txtExist.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(AddressTextBox);

                }

                if (!string.IsNullOrWhiteSpace(name)
                    && !string.IsNullOrWhiteSpace(address)
                    && !_service.Exists(name)
                    && !_service.ExistsAddress(address)
                    && Uri.IsWellFormedUriString(address, UriKind.Absolute))
                {
                    var favourite = new Models.FavouriteModel
                    {
                        Name = name,
                        WebAddress = address,
                    };

                    _service.Modify(favourite);
                    _service.SaveChanges();

                    if (SettingsService.Get("sortFavouritesBy") == "alphabetically")
                    {
                        _browser.SortByAlphabetially();
                    }
                    else
                    {
                        _service.Get(name).Index = _service.All().Count - 1;
                        _service.SaveChanges();
                    }

                    this.Close();
                    _browser.LoadFavourites();
                }
            }
            else
            {
                if (_service.ExistsModify(name, _text)
                    || _service.ExistsAddressModify(address, _service.Get(_text).WebAddress))
                {
                    txtExist.Visible = true;
                    NameTextBox.ForeColor = Color.Red;
                }
                else
                {
                    txtExist.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(AddressTextBox);

                }
          
                if (!string.IsNullOrWhiteSpace(name)
                    && !string.IsNullOrWhiteSpace(address)
                    && !_service.ExistsModify(name, _text) 
                    && !_service.ExistsAddressModify(address, _service.Get(_text).WebAddress) 
                    && Uri.IsWellFormedUriString(address, UriKind.Absolute))
                {
                    _service.Edit(_text, name, address);
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
                    favButton.Text = "      " + NameTextBox.Text.Trim();
                }
                else
                {
                    favButton.Text = NameTextBox.Text.Trim();
                }
                _browser.UpdateFavBar();
            }
        }
    }
}
