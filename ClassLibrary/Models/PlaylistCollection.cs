using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class PlaylistCollection
    {
        [JsonProperty("type")]
        public string type { get; set; }
        [JsonProperty("user")]
        public User user { get; set; }
        [JsonProperty("playlist")]
        public Playlist playlist { get; set; }
        [JsonProperty("uuid")]
        public string uuid { get; set; }
        [JsonProperty("created_at")]
        public string created_at { get; set; }
    }
}
