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
        private string _jsonPath = Path.Combine(Application.StartupPath + @"\UserData\Jsons", "favicons.json");
        private List<FaviconModel> _items = null;

        public FaviconService()
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            _items = JsonConvert.DeserializeObject<List<FaviconModel>>(jsonString);
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

        public void SaveChanges()
        {
            var jsonString = JsonConvert.SerializeObject(_items);
            File.WriteAllText(_jsonPath, jsonString);
        }
    }
}
