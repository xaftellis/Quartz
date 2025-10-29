using Newtonsoft.Json;
using Quartz.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Quartz.Libs;


namespace Quartz.Services
{
    public class HistoryService
    {
        private string _jsonPath = Path.Combine(Application.StartupPath + @"\UserData\Jsons", "history.json");
        private List<HistoryModel> _items = null;

        public HistoryService()
        {
            Reload();
        }

        public List<HistoryModel> All()
        {
            return _items.Where(f => f.ProfileId == ProfileService.Current).ToList(); 
        }

        public HistoryModel Get(Guid id)
        {
            return _items.FirstOrDefault(h => h.ProfileId == ProfileService.Current && h.Id == id);
        }

        public bool Exists(Guid id)
        {
            return _items.Any(h => h.ProfileId == ProfileService.Current && h.Id == id);
        }
        
        public void Add(HistoryModel history)
        {
            if (history == null)
                throw new ArgumentNullException("history");

            history.ProfileId = ProfileService.Current;
            history.Id = Guid.NewGuid();
            history.When = DateTime.Now;

            _items.Add(history);
        }

        public void Modify(HistoryModel history)
        {
            if (history == null)
                throw new ArgumentNullException("history");

            var original = Get(history.Id);
            if (original == null)
            {
                Add(history);
            }
            else
            {
                original.When = history.When;
                original.Title = history.Title;
                original.WebAddress = history.WebAddress;
            }
        }

        public void Clear()
        {
            _items.Clear();
        }

        public List<HistoryModel> Find(string searchText)
        {
            return _items.Where(i => (i.ProfileId == ProfileService.Current && i.WebAddress.ToLower().Contains(searchText.ToLower())) || (i.ProfileId == ProfileService.Current && i.Title.ToLower().Contains(searchText.ToLower()))).ToList();
        }

        public void Remove(Guid id)
        {
            var history = Get(id); 
            if (history == null) 
                throw new ArgumentNullException("history");

            _items.Remove(history);
        }

        public void Reload()
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            _items = JsonConvert.DeserializeObject<List<HistoryModel>>(jsonString);
        }

        public void DeleteProfileHistory(Guid profileId)
        {
            var profilehistory = _items.Where(s => s.ProfileId == profileId).ToList();
            foreach (var item in profilehistory)
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
    }
}
