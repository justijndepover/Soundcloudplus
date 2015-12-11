using System;
using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class StreamCollection
    {
        public DateTime Created_at { get; set; }
        public string Type { get; set; }
        public PublishUser User { get; set; }
        public string Uuid { get; set; }
        public Track Track { get; set; }
    }

    public class PublishUser
    {
        [JsonProperty("avatar_url")]
        public string Avatar_url { get; set; }
        [JsonProperty("first_name")]
        public string First_name { get; set; }
        [JsonProperty("full_name")]
        public string Full_name { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("last_modified")]
        public DateTime Last_modified { get; set; }
        [JsonProperty("last_name")]
        public string Last_name { get; set; }
        [JsonProperty("permalink")]
        public string Permalink { get; set; }
        [JsonProperty("permalink_url")]
        public string Permalink_url { get; set; }
        [JsonProperty("uri")]
        public string Uri { get; set; }
        [JsonProperty("urn")]
        public string Urn { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }

    public class Publisher_Metadata
    {
        [JsonProperty("urn")]
        public string Urn { get; set; }
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("contains_music")]
        public bool Contains_music { get; set; }
        [JsonProperty("publisher")]
        public string Publisher { get; set; }
        [JsonProperty("isrc")]
        public string Isrc { get; set; }
        [JsonProperty("writer_composer")]
        public string Writer_composer { get; set; }
    }

}