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
            // Another tab/window may have reordered favourites while this editor
            // was open. Save against current storage, not the opening snapshot.
            _service = new FavouriteService();
            if (_modify && _service.Get(_text) == null)
            {
                MessageBox.Show(this, "This favourite has been removed or renamed. Please reopen it to edit.",
                    "Favourite", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                return;
            }
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
                if (_service.Exists(name))
                {
                    txtExist.Visible = true;
                    NameTextBox.ForeColor = Color.Red;
                }
                else
                {
                    txtExist.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(NameTextBox);

                }

                if (_service.ExistsAddress(address))
                {
                    txtExist.Visible = true;
                    AddressTextBox.ForeColor = Color.Red;
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
                    if (SettingsService.Get("sortFavouritesBy") == "alphabetically")
                        _service.SortAlphabetically();
                    _service.SaveChanges();

                    this.Close();
                    _browser.LoadFavourites();
                }
            }
            else
            {
                if (_service.ExistsModify(name, _text))
                {
                    txtExist.Visible = true;
                    NameTextBox.ForeColor = Color.Red;
                }
                else
                {
                    txtExist.Visible = false;
                    NewControlThemeChanger.ChangeControlTheme(NameTextBox);

                }

                if (_service.ExistsAddressModify(address, _service.Get(_text).WebAddress))
                {
                    txtExist.Visible = true;
                    AddressTextBox.ForeColor = Color.Red;
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
                    if (SettingsService.Get("sortFavouritesBy") == "alphabetically")
                        _service.SortAlphabetically();
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
            favButton = _browser.UpdateFavouriteButtonPreview(favButton, NameTextBox.Text.Trim());
        }

        private void NameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                SaveButton_Click(sender, e);
            }
        }

        private void AddressTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveButton_Click(sender, e);
            }
        }
    }
}
