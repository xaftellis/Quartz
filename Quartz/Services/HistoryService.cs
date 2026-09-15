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
        private readonly string _jsonPath;
        private List<HistoryModel> _items = null;

        public HistoryService()
            : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Xaftellis\Quartz\UserData\jsons\history.json"))
        {
        }

        internal HistoryService(string jsonPath)
        {
            _jsonPath = jsonPath;
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
            _items = JsonConvert.DeserializeObject<List<HistoryModel>>(jsonString) ?? new List<HistoryModel>();
        }

        public List<HistoryModel> GetProfileHistoryFromRange(
            Guid profileId,
            DateTime? startTime,
            DateTime endTime)
        {
            IEnumerable<HistoryModel> profileHistory =
                _items.Where(item =>
                    item.ProfileId == profileId &&
                    item.When <= endTime);

            if (startTime.HasValue)
            {
                profileHistory = profileHistory.Where(
                    item => item.When >= startTime.Value);
            }

            return profileHistory
                .OrderByDescending(item => item.When)
                .ToList();
        }

        public void DeleteProfileHistory(Guid profileId, DateTime? startTime = null, DateTime? endTime = null)
        {
            Reload();
            IEnumerable<HistoryModel> profileHistory =
                _items.Where(item => item.ProfileId == profileId);

            if (startTime.HasValue)
                profileHistory = profileHistory.Where(item => item.When >= startTime.Value);

            if (endTime.HasValue)
                profileHistory = profileHistory.Where(item => item.When <= endTime.Value);

            var historyToDelete = profileHistory.ToList();
            if (historyToDelete.Count == 0)
            {
                return;
            }

            string jsonDirectory = Path.GetDirectoryName(_jsonPath);
            string cacheDirectory = Path.Combine(Path.GetDirectoryName(jsonDirectory), "cache");
            var favourites = new FavouriteService(Path.Combine(jsonDirectory, "favourites.json"));
            var faviconService = new FaviconService(Path.Combine(jsonDirectory, "favicons.json"));
            var remainingHistory = _items.Except(historyToDelete).ToList();
            var addressesToKeep = remainingHistory.Select(item => item.WebAddress).ToList();
            addressesToKeep.AddRange(favourites.GetAllWebAddresses());

            // Finish icon cleanup before saving history, so a failure retains the
            // affected URLs for a retry. Preserve references from every profile.
            faviconService.RemoveHistoryIcons(
                historyToDelete.Select(item => item.WebAddress), addressesToKeep, cacheDirectory);

            foreach (HistoryModel item in historyToDelete)
            {
                _items.Remove(item);
            }

            SaveChanges();
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
