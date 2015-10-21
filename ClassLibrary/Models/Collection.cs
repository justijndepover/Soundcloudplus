using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public class Collection
    {
        public string Type { get; set; }
        public User User { get; set; }
        public string Uuid { get; set; }
        public string CreatedAt { get; set; }
    }
}
