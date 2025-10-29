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
    public class BirthdayService
    {
        private string _jsonPath = Path.Combine(Application.StartupPath + @"\UserData\Jsons", "birthdays.json");
        private List<BirthdayModel> _items = null;

        public BirthdayService()
        {
            Reload();
        }

        public List<BirthdayModel> All()
        {
            return _items.Where(f => f.ProfileId == ProfileService.Current).ToList(); 
        }

        public BirthdayModel Get(Guid id)
        {
            return _items.FirstOrDefault(h => h.ProfileId == ProfileService.Current && h.Id == id);
        }

        public bool Exists(Guid id)
        {
            return _items.Any(h => h.ProfileId == ProfileService.Current && h.Id == id);
        }
        
        public void Add(BirthdayModel birthday)
        {
            if (birthday == null)
                throw new ArgumentNullException("birthday");

            birthday.ProfileId = ProfileService.Current;
            birthday.Id = Guid.NewGuid();

            _items.Add(birthday);
        }

        public void Modify(BirthdayModel birthday)
        {
            if (birthday == null)
                throw new ArgumentNullException("birthday");

            var original = Get(birthday.Id);
            if (original == null)
            {
                Add(birthday);
            }
            else
            {
                original.Name = birthday.Name;
                original.DOB = birthday.DOB;
            }
        }

        public void Remove(Guid id)
        {
            var birthday = Get(id); 
            if (birthday == null) 
                throw new ArgumentNullException("birthday");

            _items.Remove(birthday);
        }

        public void Reload()
        {
            var jsonString = "[]";

            if (File.Exists(_jsonPath))
            {
                jsonString = File.ReadAllText(_jsonPath);
            }
            _items = JsonConvert.DeserializeObject<List<BirthdayModel>>(jsonString);
        }

        public void DeleteProfileBirthdays(Guid profileId)
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
