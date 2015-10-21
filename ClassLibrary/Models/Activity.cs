using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class Activity
    {
        [JsonProperty("collection")]
        public Collection[] Collection { get; set; }
        [JsonProperty("next_href")]
        public string NextHref { get; set; }
    }
}
