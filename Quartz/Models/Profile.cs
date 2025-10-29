using stdole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Models
{
    public class ProfileModel
    {
        public Guid Id { get; set; }

        public string profilePicture { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; }
        public bool Default { get; set; }
        public bool isDisposable { get; set; }
        public bool endSession { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime lastActive { get; set; }
    }
}
