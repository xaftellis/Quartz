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
        private string _jsonPath = Path.Combine(Application.StartupPath + @"\UserData\Jsons", "favourites.json");
        private List<FavouriteModel> _items = null;

        public FavouriteService()
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            _items = JsonConvert.DeserializeObject<List<FavouriteModel>>(jsonString);
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
            return _items.Any(f => f.ProfileId == ProfileService.Current && f.Name == name);
        }

        public bool ExistsAddress(string address)
        {
            return _items.Any(f => f.ProfileId == ProfileService.Current && f.WebAddress == address);
        }

        public bool ExistsModify(string name, string Original)
        {
            return _items.Any(f => f.ProfileId == ProfileService.Current && f.Name == name && f.Name != Original);
        }

        public bool ExistsAddressModify(string address, string Original)
        {
            return _items.Any(f => f.ProfileId == ProfileService.Current && f.WebAddress == address && f.WebAddress != Original);
        }

        public void Add(FavouriteModel favourite)
        {
            if (favourite == null)
                throw new ArgumentNullException("favourite");

            if (Exists(favourite.Name))
                throw new ApplicationException("Favourite already exists.");

            favourite.ProfileId = ProfileService.Current;
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
                original.Index = favourite.Index;
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
            File.WriteAllText(_jsonPath, jsonString);
        }

        public void Edit(string Original, string name, string address)
        {
            var original = Get(Original);
            original.Name = name;
            original.WebAddress = address;
        }

        public static async Task<bool> ValidatePanelAsync(FlowLayoutPanel panel)
        {
            return await Task.Run(() =>
            {
                var service = new FavouriteService();
                var expected = service.All().OrderBy(f => f.Index).ToList();

                var buttons = panel.Controls.OfType<Button>().ToList();

                bool CheckIcon(Image a, Image b)
                {
                    if (a == null && b == null) return true;
                    if (a == null || b == null) return false;

                    // Compare sizes first (cheap)
                    if (a.Width != b.Width || a.Height != b.Height)
                        return false;

                    // Compare pixel data
                    using (var bmpA = new Bitmap(a))
                    using (var bmpB = new Bitmap(b))
                    {
                        for (int y = 0; y < bmpA.Height; y++)
                        {
                            for (int x = 0; x < bmpA.Width; x++)
                            {
                                if (bmpA.GetPixel(x, y) != bmpB.GetPixel(x, y))
                                    return false;
                            }
                        }
                    }

                    return true;
                }

                bool allExist = expected.All(fav =>
                {
                    // expected icon
                    var expectedIcon = FaviconHelper.GetFaviconFileExternalAsImage(fav.WebAddress);

                    return buttons.Any(b =>
                        b.Text.Trim() == fav.Name &&
                        b.Tag?.ToString() == fav.WebAddress &&
                        CheckIcon(b.Image, expectedIcon)
                    );
                });

                bool noExtra = buttons.All(btn =>
                {
                    string name = btn.Text.Trim();
                    string url = btn.Tag?.ToString();

                    var match = expected.FirstOrDefault(f => f.Name == name && f.WebAddress == url);
                    if (match == null) return false;

                    // expected icon
                    var expectedIcon = FaviconHelper.GetFaviconFileExternalAsImage(match.WebAddress);

                    return CheckIcon(btn.Image, expectedIcon);
                });

                return allExist && noExtra;
            });
        }
    }
}
