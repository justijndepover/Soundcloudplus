using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class PlaylistCollection
    {
        [JsonProperty("artwork_url")]
        public string ArtworkUrl { get; set; }
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        [JsonProperty("description")]
        public object Description { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; }
        [JsonProperty("embeddable_by")]
        public string EmbeddableBy { get; set; }
        [JsonProperty("genre")]
        public object Genre { get; set; }
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("label_name")]
        public object LabelName { get; set; }
        [JsonProperty("license")]
        public string License { get; set; }
        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }
        [JsonProperty("permalink")]
        public string Permalink { get; set; }
        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }
        [JsonProperty("public")]
        public bool Public { get; set; }
        [JsonProperty("purchase_title")]
        public object PurchaseTitle { get; set; }
        [JsonProperty("purchase_url")]
        public object PurchaseUrl { get; set; }
        [JsonProperty("release_date")]
        public object ReleaseDate { get; set; }
        [JsonProperty("reposts_count")]
        public int RepostsCount { get; set; }
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
        [JsonProperty("track_count")]
        public int TrackCount { get; set; }
        //
        /*[JsonProperty("artwork_url")]
        public string ArtworkUrl { get; set; }*/
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("user")]
        public User User { get; set; }
        [JsonProperty("playlist")]
        public Playlist Playlist { get; set; }
        [JsonProperty("uuid")]
        public string Uuid { get; set; }
        /*[JsonProperty("created_at")]
        public string CreatedAt { get; set; }*/

    }
}
