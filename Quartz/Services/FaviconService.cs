using Newtonsoft.Json;
using Quartz.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Quartz.Services
{
    public class FaviconService
    {
        private readonly string _jsonPath;
        private List<FaviconModel> _items = null;

        public FaviconService()
            : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Xaftellis\Quartz\UserData\jsons\favicons.json"))
        {
        }

        internal FaviconService(string jsonPath)
        {
            _jsonPath = jsonPath;
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            _items = JsonConvert.DeserializeObject<List<FaviconModel>>(jsonString) ?? new List<FaviconModel>();
        }

        public List<FaviconModel> All()
        {
            return _items;
        }

        public FaviconModel Get(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("address");

            return _items.FirstOrDefault(f => f.WebAddress == address);
        }

        public bool Exists(string address)
        {
            return _items.Any(f => f.WebAddress == address);
        }

        public void Add(FaviconModel favicon)
        {
            if (favicon == null)
                throw new ArgumentNullException("favicon");

            if (Exists(favicon.WebAddress))
                throw new ApplicationException("Favicon already exists.");

            _items.Add(favicon);
        }

        public void Modify(FaviconModel favicon)
        {
            if (favicon == null)
                throw new ArgumentNullException("favicon");

            var original = Get(favicon.WebAddress);
            if (original == null)
            {
                Add(favicon);
            }
            else
            {
                original.WebAddress = favicon.WebAddress;
                original.Id = favicon.Id;
            }
        }

        public void Remove(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address");

            var favourite = Get(address); 
            if (favourite == null) 
                throw new ArgumentNullException("favicon");

            _items.Remove(favourite);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void ClearCache(string cachePath)
        {
            // Icons and their index are shared by all profiles, with no visit timestamps.
            // Retain the index if a file cannot be deleted so clearing can be retried.
            if (Directory.Exists(cachePath))
            {
                foreach (string file in Directory.EnumerateFiles(cachePath))
                    File.Delete(file);
            }

            Clear();
            Directory.CreateDirectory(Path.GetDirectoryName(_jsonPath));
            SaveChanges();
        }

        internal void RemoveHistoryIcons(IEnumerable<string> deletedAddresses,
            IEnumerable<string> retainedAddresses, string cachePath)
        {
            var addressesToDelete = new HashSet<string>(deletedAddresses, StringComparer.Ordinal);
            var addressesToKeep = new HashSet<string>(StringComparer.Ordinal);
            foreach (string address in retainedAddresses)
            {
                addressesToKeep.Add(NormalizeAddress(address));
            }

            var iconsToRemove = new List<FaviconModel>();
            foreach (FaviconModel icon in _items)
            {
                bool historyWasDeleted = addressesToDelete.Contains(icon.WebAddress);
                bool stillInUse = addressesToKeep.Contains(NormalizeAddress(icon.WebAddress));
                if (historyWasDeleted && !stillInUse)
                {
                    iconsToRemove.Add(icon);
                }
            }

            if (iconsToRemove.Count == 0)
            {
                return;
            }

            var remainingIcons = _items.Except(iconsToRemove).ToList();
            var iconIdsToKeep = new HashSet<Guid>(remainingIcons.Select(icon => icon.Id));
            // Delete only known ICO files, and retain icons still shared by another mapping.
            // Keep the index intact on failure so the same operation can be retried.
            if (Directory.Exists(cachePath))
            {
                foreach (Guid id in iconsToRemove.Select(icon => icon.Id).Distinct())
                {
                    if (iconIdsToKeep.Contains(id))
                    {
                        continue;
                    }

                    File.Delete(Path.Combine(cachePath, id + ".ico"));
                }
            }

            _items = remainingIcons;
            SaveChanges();
        }

        private static string NormalizeAddress(string address)
        {
            Uri uri;
            if (Uri.TryCreate(address, UriKind.Absolute, out uri))
            {
                return uri.AbsoluteUri;
            }

            return address;
        }

        public void SaveChanges()
        {
            var jsonString = JsonConvert.SerializeObject(_items);
            string temporaryPath = _jsonPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, jsonString);
                if (File.Exists(_jsonPath))
                {
                    File.Replace(temporaryPath, _jsonPath, null);
                }
                else
                {
                    File.Move(temporaryPath, _jsonPath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
    }
}
