using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Models
{
    public class FavouriteModel
    {
        public Guid ProfileId { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string WebAddress { get; set; }
    }
}
