using System;
using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class StreamCollection
    {
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; }
        public PublishUser User { get; set; }
        public string Uuid { get; set; }
        public Track Track { get; set; }
    }

    public class PublishUser
    {
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("full_name")]
        public string FullName { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("last_modified")]
        public DateTime LastModified { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("permalink")]
        public string Permalink { get; set; }
        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }
        [JsonProperty("uri")]
        public string Uri { get; set; }
        [JsonProperty("urn")]
        public string Urn { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }

    public class PublisherMetadata
    {
        [JsonProperty("urn")]
        public string Urn { get; set; }
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("contains_music")]
        public bool ContainsMusic { get; set; }
        [JsonProperty("publisher")]
        public string Publisher { get; set; }
        [JsonProperty("isrc")]
        public string Isrc { get; set; }
        [JsonProperty("writer_composer")]
        public string WriterComposer { get; set; }
    }

}