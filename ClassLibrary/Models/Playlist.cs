using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class Playlist
    {
        [JsonProperty("artwork_url")]
        public string ArtworkUrl { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; }
        [JsonProperty("embeddable_by")]
        public string EmbeddableBy { get; set; }
        [JsonProperty("genre")]
        public string Genre { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("label_name")]
        public string LabelName { get; set; }
        [JsonProperty("last_modified")]
        public DateTime LastModified { get; set; }
        [JsonProperty("license")]
        public string License { get; set; }
        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }
        [JsonProperty("permalink")]
        public string Permalink { get; set; }
        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }
        [JsonProperty("_public")]
        public bool Public { get; set; }
        [JsonProperty("purchase_title")]
        public object PurchaseTitle { get; set; }
        [JsonProperty("purchase_url")]
        public object PurchaseUrl { get; set; }
        [JsonProperty("release_date")]
        public object ReleaseDate { get; set; }
        [JsonProperty("reposts_count")]
        public int RepostsCount { get; set; }
        [JsonProperty("secret_token")]
        public object SecretToken { get; set; }
        [JsonProperty("sharing")]
        public string Sharing { get; set; }
        [JsonProperty("tag_list")]
        public string TagList { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("uri")]
        public string Uri { get; set; }
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        [JsonProperty("user")]
        public User User { get; set; }
        [JsonProperty("tracks")]
        public List<Track> Tracks { get; set; }
        [JsonProperty("track_count")]
        public int TrackCount { get; set; }
    }
}