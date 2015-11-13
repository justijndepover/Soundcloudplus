using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class TrackObject
    {
        [JsonProperty("collection")]
        public Track[] Collection { get; set; }
        [JsonProperty("next_href")]
        public object NextHref { get; set; }
    }
}
