using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class PlaylistObject
    {
        [JsonProperty("collection")]
        public PlaylistCollection[] Collection { get; set; }
        [JsonProperty("next_href")]
        public object NextHref { get; set; }
    }
}
