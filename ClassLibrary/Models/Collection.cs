using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class Collection
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("user")]
        public User User { get; set; }
        [JsonProperty("uuid")]
        public string Uuid { get; set; }
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }
}
