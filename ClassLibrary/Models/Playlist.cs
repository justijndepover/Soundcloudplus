using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class Playlist
    {
        
        public int id { get; set; }
        [JsonProperty("artwork_url")]
        public string artwork_url { get; set; }
        [JsonProperty("created_at")]
        public string created_at { get; set; }
        [JsonProperty("description")]
        public object description { get; set; }
        [JsonProperty("duration")]
        public int duration { get; set; }
        [JsonProperty("embeddable_by")]
        public string embeddable_by { get; set; }
        [JsonProperty("genre")]
        public object genre { get; set; }
        [JsonProperty("kind")]
        public string kind { get; set; }
        [JsonProperty("label_id")]
        public object label_id { get; set; }
        [JsonProperty("label_name")]
        public object label_name { get; set; }
        [JsonProperty("license")]
        public string license { get; set; }
        [JsonProperty("likes_count")]
        public int likes_count { get; set; }
        [JsonProperty("permalink")]
        public string permalink { get; set; }
        [JsonProperty("public")]
        public bool _public { get; set; }
        [JsonProperty("purchase_title")]
        public object purchase_title { get; set; }
        [JsonProperty("purchase_url")]
        public object purchase_url { get; set; }
        [JsonProperty("release")]
        public object release { get; set; }
        [JsonProperty("release_date")]
        public object release_date { get; set; }
        [JsonProperty("reposts_count")]
        public int reposts_count { get; set; }
        [JsonProperty("sharing")]
        public string sharing { get; set; }
        [JsonProperty("streamable")]
        public bool streamable { get; set; }
        [JsonProperty("tag_list")]
        public string tag_list { get; set; }
        [JsonProperty("title")]
        public string title { get; set; }
        [JsonProperty("track_count")]
        public int track_count { get; set; }
        [JsonProperty("user")]
        public User user { get; set; }
        [JsonProperty("user_id")]
        public int user_id { get; set; }
        [JsonProperty("last_modified")]
        public string last_modified { get; set; }
        [JsonProperty("permalink_url")]
        public string permalink_url { get; set; }
        [JsonProperty("secret_token")]
        public object secret_token { get; set; }
        [JsonProperty("uri")]
        public string uri { get; set; }
    }
}
