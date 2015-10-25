using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class Activity
    {
        [JsonProperty("collection")]
        public ObservableCollection<ActivityCollection> Collection { get; set; }
        [JsonProperty("next_href")]
        public string NextHref { get; set; }
    }
}
