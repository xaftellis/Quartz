using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Models
{
    public class HistoryModel
    {
        public Guid ProfileId { get; set; }
        public Guid Id { get; set; }
        public DateTime When { get; set; }
        public string Title { get; set; }
        public string WebAddress { get; set; }
    }
}
