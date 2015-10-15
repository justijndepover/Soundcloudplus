using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class Track
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; }
        [JsonProperty("commentable")]
        public bool Commentable { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("sharing")]
        public string Sharing { get; set; }
        [JsonProperty("tag_list")]
        public string TagList { get; set; }
        [JsonProperty("permalink")]
        public string Permalink { get; set; }
        [JsonProperty("description")]
        public object Description { get; set; }
        [JsonProperty("streamable")]
        public bool Streamable { get; set; }
        [JsonProperty("downloadable")]
        public bool Downloadable { get; set; }
        [JsonProperty("genre")]
        public object Genre { get; set; }
        [JsonProperty("release")]
        public object Release { get; set; }
        [JsonProperty("purchase_url")]
        public object PurchaseUrl { get; set; }
        [JsonProperty("label_id")]
        public object LabelId { get; set; }
        [JsonProperty("label_name")]
        public object LabelName { get; set; }
        [JsonProperty("isrc")]
        public object Isrc { get; set; }
        [JsonProperty("video_url")]
        public object VideoUrl { get; set; }
        [JsonProperty("track_type")]
        public string TrackType { get; set; }
        [JsonProperty("key_signature")]
        public object KeySignature { get; set; }
        [JsonProperty("bpm")]
        public object Bpm { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("release_year")]
        public object ReleaseYear { get; set; }
        [JsonProperty("release_month")]
        public object ReleaseMonth { get; set; }
        [JsonProperty("release_day")]
        public object ReleaseDay { get; set; }
        [JsonProperty("original_format")]
        public string OriginalFormat { get; set; }
        [JsonProperty("original_content_size")]
        public int OriginalContentSize { get; set; }
        [JsonProperty("license")]
        public string License { get; set; }
        [JsonProperty("uri")]
        public string Uri { get; set; }
        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }
        [JsonProperty("artwork_url")]
        public object ArtworkUrl { get; set; }
        [JsonProperty("waveform_url")]
        public string WaveformUrl { get; set; }
        [JsonProperty("user")]
        public User User { get; set; }
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        [JsonProperty("stream_url")]
        public string StreamUrl { get; set; }
        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }
        [JsonProperty("playback_count")]
        public int PlaybackCount { get; set; }
        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }
        [JsonProperty("favoritings_count")]
        public int FavoritingsCount { get; set; }
        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }
        [JsonProperty("created_with")]
        public User CreatedWith { get; set; }
        [JsonProperty("attachments_uri")]
        public string AttachmentsUri { get; set; }
    }
}