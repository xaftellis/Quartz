using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Models
{
    public class SettingModel
    {
        public Guid ProfileId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
