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
        private readonly Guid _favouriteId;
        Button favButton;

        public 
            Favourite(Browser browser, string text, string webAddress, bool modify, Button button)
        {
            _browser = browser;
            _text = text;
            _webAddress = webAddress;
            _modify = modify;
            favButton = button;
            _favouriteId = (button?.Tag as Models.FavouriteModel)?.Id ?? Guid.Empty;
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

            txtURLBad.ForeColor = Color.Red;
            NameMessage.ForeColor = Color.Red;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Re-read storage so another tab's additions/reordering survive this save.
            _service = new FavouriteService();
            if (_modify)
            {
                var original = _service.Get(_favouriteId);
                if (original == null || original.Name != _text || original.WebAddress != _webAddress)
                {
                    MessageBox.Show(this, "This favourite has been removed or changed. Please reopen it to edit.",
                        "Favourite", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                    return;
                }
            }

            string name = NameTextBox.Text.Trim();
            string address = AddressTextBox.Text.Trim();
            bool missing = string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address);
            bool validAddress = Uri.IsWellFormedUriString(address, UriKind.Absolute);
            NameMessage.Visible = missing;
            txtURLBad.Visible = !missing && !validAddress;
            NewControlThemeChanger.ChangeControlTheme(NameTextBox);
            NewControlThemeChanger.ChangeControlTheme(AddressTextBox);
            if (!validAddress) AddressTextBox.ForeColor = Color.Red;
            if (missing || !validAddress) return;

            if (_modify) _service.Edit(_favouriteId, name, address);
            else _service.Add(new Models.FavouriteModel { Name = name, WebAddress = address });
            if (SettingsService.Get("sortFavouritesBy") == "alphabetically")
                _service.SortAlphabetically();
            _service.SaveChanges();
            Close();
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
