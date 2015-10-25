using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class ActivityCollection
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
