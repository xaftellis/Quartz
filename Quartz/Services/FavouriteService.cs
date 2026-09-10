using Newtonsoft.Json;
using Quartz.Libs;
using Quartz.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quartz.Services
{
    public class FavouriteService
    {
        private readonly string _jsonPath;
        private List<FavouriteModel> _items = null;

        public FavouriteService()
            : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Xaftellis\Quartz\UserData\jsons\favourites.json"))
        {
        }

        internal FavouriteService(string jsonPath)
        {
            _jsonPath = jsonPath;
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            _items = JsonConvert.DeserializeObject<List<FavouriteModel>>(jsonString) ?? new List<FavouriteModel>();
        }

        public List<FavouriteModel> All()
        {
            return _items.Where(f => f.ProfileId == ProfileService.Current).ToList();
        }

        public FavouriteModel Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            return _items.FirstOrDefault(f => f.ProfileId == ProfileService.Current && f.Name == name);
        }

        public bool Exists(string name)
        {
            return _items.Any(f => f.ProfileId == ProfileService.Current && f.Name.ToLower() == name.ToLower());
        }

        public bool ExistsAddress(string address)
        {
            return _items.Any(f => f.ProfileId == ProfileService.Current && f.WebAddress.ToLower() == address.ToLower());
        }

        public bool ExistsModify(string name, string Original)
        {
            return _items.Any(f => f.ProfileId == ProfileService.Current && f.Name.ToLower() == name.ToLower() && f.Name.ToLower() != Original.ToLower());
        }

        public bool ExistsAddressModify(string address, string Original)
        {
            return _items.Any(f => f.ProfileId == ProfileService.Current && f.WebAddress.ToLower() == address.ToLower() && f.WebAddress.ToLower() != Original.ToLower());
        }

        public void Add(FavouriteModel favourite)
        {
            if (favourite == null)
                throw new ArgumentNullException("favourite");

            if (Exists(favourite.Name))
                throw new ApplicationException("Favourite already exists.");

            favourite.ProfileId = ProfileService.Current;
            // Compact legacy gaps/duplicate indices without changing visible order,
            // then append. Count - 1 can collide with an index left by a deletion.
            var ordered = All().OrderBy(f => f.Index).ToList();
            for (int i = 0; i < ordered.Count; i++) ordered[i].Index = i;
            favourite.Index = ordered.Count;
            _items.Add(favourite);
        }

        public void Modify(FavouriteModel favourite)
        {
            if (favourite == null)
                throw new ArgumentNullException("favourite");

            var original = Get(favourite.Name);
            if (original == null)
            {
                Add(favourite);
            }
            else
            {
                // Updating an existing favourite must retain its chosen position.
                original.Name = favourite.Name;
                original.WebAddress = favourite.WebAddress;
            }
        }

        public void Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var favourite = Get(name);
            if (favourite == null)
                throw new ArgumentNullException("favourite");

            _items.Remove(favourite);
        }

        public void DeleteProfileFav(Guid profileId)
        {
            var profilefav = _items.Where(s => s.ProfileId == profileId).ToList();
            foreach (var item in profilefav)
            {
                _items.Remove(item);
            }

            SaveChanges();
        }

        public void SaveChanges()
        {
            var jsonString = JsonConvert.SerializeObject(_items);
            // Replace only after the complete JSON has been written successfully.
            // A failed write must not truncate the user's favourites file.
            string temporaryPath = _jsonPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, jsonString);
                if (File.Exists(_jsonPath)) File.Replace(temporaryPath, _jsonPath, null);
                else File.Move(temporaryPath, _jsonPath);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        public bool TryReorder(IList<FavouriteModel> order)
        {
            var current = All();
            if (order == null || order.Count != current.Count || order.Any(f => f == null)) return false;
            if (order.Select(f => f.Name).Distinct(StringComparer.Ordinal).Count() != order.Count) return false;
            var storedOrder = new List<FavouriteModel>();
            foreach (var item in order)
            {
                var stored = current.FirstOrDefault(f => f.ProfileId == item.ProfileId &&
                    f.Name == item.Name && f.WebAddress == item.WebAddress);
                if (stored == null) return false;
                storedOrder.Add(stored);
            }
            if (current.OrderBy(f => f.Index).SequenceEqual(storedOrder)) return true;
            for (int i = 0; i < storedOrder.Count; i++) storedOrder[i].Index = i;
            SaveChanges();
            return true;
        }

        public void Edit(string Original, string name, string address)
        {
            var original = Get(Original);
            original.Name = name;
            original.WebAddress = address;
        }

        public static Task<bool> ValidatePanelAsync(FlowLayoutPanel panel)
        {
            // All callers are UI events. Do not send controls or their live images
            // to Task.Run: a refresh can dispose them while that worker is reading.
            if (panel.IsDisposed || panel.Disposing) return Task.FromResult(true);
            if (panel.InvokeRequired) throw new InvalidOperationException("Validate favourites on the UI thread.");
            if (panel is Controls.FavouritesBar bar && bar.IsInteracting)
                return Task.FromResult(false); // Browser defers the refresh until release.
            return Task.FromResult(ValidateButtons(panel, new FavouriteService().All().OrderBy(f => f.Index).ToList(),
                SettingsService.Get("showFavouriteIcon") == "true", FaviconHelper.GetFaviconFileExternalAsImage));
        }

        internal static bool ValidateButtons(FlowLayoutPanel panel, IList<FavouriteModel> expected,
            bool showIcons, Func<string, Image> getIcon)
        {
            var buttons = panel.Controls.OfType<Button>().ToList();
            if (buttons.Count != expected.Count) return false;
            for (int i = 0; i < buttons.Count; i++)
            {
                var actual = buttons[i].Tag as FavouriteModel;
                var wanted = expected[i];
                if (actual == null || actual.ProfileId != wanted.ProfileId ||
                    actual.Name != wanted.Name || actual.WebAddress != wanted.WebAddress) return false;
                using (Image icon = showIcons ? getIcon(wanted.WebAddress) : null)
                    if (!SameIcon(buttons[i].Image, icon)) return false;
            }
            return true;
        }

        public void SortAlphabetically()
        {
            var ordered = All().OrderBy(f => f.Name).ToList();
            for (int i = 0; i < ordered.Count; i++) ordered[i].Index = i;
        }

        private static bool SameIcon(Image a, Image b)
        {
            if (a == null || b == null) return a == b;
            if (a.Size != b.Size) return false;
            using (var left = new Bitmap(a))
            using (var right = new Bitmap(b))
            {
                for (int y = 0; y < left.Height; y++)
                    for (int x = 0; x < left.Width; x++)
                        if (left.GetPixel(x, y) != right.GetPixel(x, y)) return false;
            }
            return true;
        }
    }
}
