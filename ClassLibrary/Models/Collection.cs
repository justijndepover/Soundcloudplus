using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public class Collection
    {
        public string type { get; set; }
        public User user { get; set; }
        public string uuid { get; set; }
        public string created_at { get; set; }
    }
}
