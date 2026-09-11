using Newtonsoft.Json;
using Quartz.Libs;
using Quartz.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            EnsureIds();
        }

        private void EnsureIds()
        {
            // Old files have no IDs. Derive repeatable IDs until the next normal
            // save persists them, so tabs opened before that save agree on identity.
            // Reading a favourites file must not require write access.
            var reserved = new HashSet<Guid>(_items.Where(f => f.Id != Guid.Empty).Select(f => f.Id));
            var seen = new HashSet<Guid>();
            using (var hash = SHA256.Create())
                for (int i = 0; i < _items.Count; i++)
                {
                    var favourite = _items[i];
                    if (favourite.Id != Guid.Empty && seen.Add(favourite.Id)) continue;
                    int attempt = 0;
                    Guid id;
                    do
                    {
                        string key = JsonConvert.SerializeObject(new object[] { "Quartz legacy favourite",
                            favourite.ProfileId, favourite.Name, favourite.WebAddress, favourite.Index, i, attempt++ });
                        id = new Guid(hash.ComputeHash(Encoding.UTF8.GetBytes(key)).Take(16).ToArray());
                    } while (id == Guid.Empty || reserved.Contains(id) || !seen.Add(id));
                    favourite.Id = id;
                }
        }

        public List<FavouriteModel> All()
        {
            return _items.Where(f => f.ProfileId == ProfileService.Current).ToList();
        }

        public FavouriteModel Get(Guid id)
        {
            return _items.FirstOrDefault(f => f.ProfileId == ProfileService.Current && f.Id == id);
        }

        public FavouriteModel Add(FavouriteModel favourite)
        {
            if (favourite == null)
                throw new ArgumentNullException("favourite");

            // Compact legacy gaps/duplicate indices without changing visible order,
            // then append. Count - 1 can collide with an index left by a deletion.
            var ordered = All().OrderBy(f => f.Index).ToList();
            for (int i = 0; i < ordered.Count; i++) ordered[i].Index = i;
            // Adding always creates a separate record, even when the caller
            // passes an existing favourite (for example a copied item).
            var added = new FavouriteModel
            {
                Id = Guid.NewGuid(), ProfileId = ProfileService.Current, Index = ordered.Count,
                Name = favourite.Name, WebAddress = favourite.WebAddress
            };
            _items.Add(added);
            return added;
        }

        public void Remove(Guid id)
        {
            var favourite = Get(id);
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
            if (order.Any(f => f.Id == Guid.Empty) || order.Select(f => f.Id).Distinct().Count() != order.Count) return false;
            var storedOrder = new List<FavouriteModel>();
            foreach (var item in order)
            {
                var stored = current.FirstOrDefault(f => f.Id == item.Id && f.ProfileId == item.ProfileId &&
                    f.Name == item.Name && f.WebAddress == item.WebAddress);
                if (stored == null) return false;
                storedOrder.Add(stored);
            }
            if (current.OrderBy(f => f.Index).SequenceEqual(storedOrder)) return true;
            for (int i = 0; i < storedOrder.Count; i++) storedOrder[i].Index = i;
            SaveChanges();
            return true;
        }

        public void Edit(Guid id, string name, string address)
        {
            var original = Get(id);
            if (original == null) throw new ArgumentException("This favourite no longer exists.", nameof(id));
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
                if (actual == null || actual.Id != wanted.Id || actual.ProfileId != wanted.ProfileId ||
                    actual.Name != wanted.Name || actual.WebAddress != wanted.WebAddress) return false;
                using (Image icon = showIcons ? getIcon(wanted.WebAddress) : null)
                    if (!SameIcon(buttons[i].Image, icon)) return false;
            }
            return true;
        }

        public void SortAlphabetically()
        {
            var ordered = All().OrderBy(f => f.Name).ThenBy(f => f.Index).ToList();
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
