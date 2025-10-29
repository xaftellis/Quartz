using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Models
{
    public class BirthdayModel
    {
        public Guid ProfileId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DOB { get; set; }
    }
}
