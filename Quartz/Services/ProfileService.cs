using Newtonsoft.Json;
using Quartz.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Win32Interop.Structs;
using Quartz.Libs;
using System.Windows.Shapes;
using System.Drawing;

namespace Quartz.Services
{
    public class ProfileService
    {
        public static Guid Current = Guid.Empty;
        private static string _jsonPath = System.IO.Path.Combine(Application.StartupPath + @"\UserData\Jsons", "profiles.json");
        private List<ProfileModel> _items = null;

        public ProfileService()
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            _items = JsonConvert.DeserializeObject<List<ProfileModel>>(jsonString);
        }

        public void Reload()
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            _items = JsonConvert.DeserializeObject<List<ProfileModel>>(jsonString);
        }

        public List<ProfileModel> Find(string searchText)
        {
            return _items.Where(i => i.Name.ToLower().Contains(searchText.ToLower())).ToList();
        }

        public static void LoadCurrentProfile()
        {
            var jsonString = File.ReadAllText(_jsonPath);
            var items = JsonConvert.DeserializeObject<List<ProfileModel>>(jsonString);

            Current = items.FirstOrDefault(f => f.Active).Id;
        }

        public Guid LastActiveProfile()
        {
            var profile = _items
               .OrderByDescending(p => p.lastActive != default(DateTime)) // Active profiles first
               .ThenByDescending(p => p.lastActive != default(DateTime) ? p.lastActive : DateTime.MinValue) // Most recent lastActive first
               .ThenByDescending(p => p.dateCreated) // For unused profiles, oldest created first
               .FirstOrDefault();

            return profile?.Id ?? Guid.Empty;
        }

        //public Guid LastActiveProfile()
        //{
        //    var profile = _items
        //       .OrderByDescending(p => p.lastActive != default(DateTime)) // Active profiles first
        //       .ThenByDescending(p => p.lastActive != default(DateTime) ? p.lastActive : DateTime.MinValue) // Most recent lastActive first
        //       .ThenByDescending(p => p.dateCreated) // For unused profiles, oldest created first
        //       .FirstOrDefault();

        //    return profile?.Id ?? Guid.Empty;
        //}


        public ProfileModel GetDefault()
        {
            return _items.FirstOrDefault(f => f.Default);
        }

        public void SetDefault(Guid id)
        {
            _items.Where(f => f.Default).ToList().ForEach(f => f.Default = false);

            var profile = Get(id);
            if (profile == null)
                throw new ArgumentNullException("profile");

            profile.Default = true;
        }

        public void SetProfilePicture(Guid id, string filename)
        {
            var profile = Get(id);
            if (profile == null)
                throw new ArgumentNullException("profile");

            profile.profilePicture = filename;
        }

        public System.Drawing.Image GetProfilePicture(Guid id)
        {
            var profile = Get(id);
            if (profile == null)
                throw new ArgumentNullException("profile");

            string path = $@"{Application.StartupPath}\UserData\pictures\{profile.profilePicture}";

            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return Image.FromStream(fs);  // This returns an Image, not a Bitmap
                }       
            }
            catch
            {
                return null;
            }
        }

        public void UnsetDefault(Guid id)
        {
            var profile = Get(id);
            if (profile == null)
                throw new ArgumentNullException("profile");

            profile.Default = false;
        }

        public List<ProfileModel> All()
        {
            return _items;
        }

        public ProfileModel Get(Guid id)
        {
            return _items.FirstOrDefault(f => f.Id == id);
        }

        public ProfileModel GetActive()
        {
            return _items.FirstOrDefault(f => f.Active);
        }

        public ProfileModel GetDisposable()
        {
            return _items.FirstOrDefault(f => f.isDisposable);
        }

        public void SetActive(Guid id)
        {
            _items.Where(f => f.Active).ToList().ForEach(f => f.Active = false);

            var profile = Get(id);
            if (profile == null)
                throw new ArgumentNullException("profile");

            profile.Active = true;
            ProfileService.Current = id;
        }

        public void SetPassword(Guid id, string password)
        {
            var profile = Get(id);
            if (profile == null)
                throw new ArgumentNullException("profile");

            profile.Password = password;
        }
        public void RemovePassword(Guid id)
        {
            var profile = Get(id);
            if (profile == null)
                throw new ArgumentNullException("profile");

            profile.Password = string.Empty;
        }


        public string GetPassword(Guid id)
        {
            var profile = Get(id);
            if (profile == null)
                throw new ArgumentNullException("profile");

            return profile.Password;
        }

        public bool Exists(Guid id)
        {
            return _items.Any(f => f.Id == id);
        }

        public bool ExistsName(string name)
        {
            return _items.Any(f => f.Name.ToLower() == name.ToLower());
        }

        public bool EditExistsName(string name, string original)
        {
            return _items.Any(f => f.Name.ToLower() == name.ToLower() && f.Name.ToLower() != original.ToLower());
        }
        public void Add(ProfileModel profile)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");

            if (Exists(profile.Id))
                throw new ApplicationException("Profile already exists.");

            profile.Id = Guid.NewGuid();
            profile.dateCreated = DateTime.Now;
            _items.Add(profile);
        }

        public void Modify(ProfileModel profile)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");

            var original = Get(profile.Id);
            if (original == null)
            {
                Add(profile);
            }
            else
            {
                original.Name = profile.Name;
                original.Active = profile.Active;
                original.isDisposable = profile.isDisposable;
                original.Default = profile.Default;
                original.Password = profile.Password;
                original.lastActive = profile.lastActive;
                original.endSession = profile.endSession;
                original.profilePicture = profile.profilePicture;
            }
        }

        public void Edit(Guid id, string name)
        {
            var original = Get(id);
            original.Name = name;
        }

        public void Remove(Guid id)
        {
            var profile = Get(id); 
            if (profile == null) 
                throw new ArgumentNullException("profile");

            _items.Remove(profile);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void SaveChanges()
        {
            var jsonString = JsonConvert.SerializeObject(_items);
            File.WriteAllText(_jsonPath, jsonString);
        }
    }
}
 