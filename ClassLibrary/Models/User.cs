using Newtonsoft.Json;

namespace ClassLibrary.Models
{
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("permalink")]
        public string Permalink { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("uri")]
        public string Uri { get; set; }
        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("full_name")]
        public string FullName { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("discogs_name")]
        public object DiscogsName { get; set; }
        [JsonProperty("myspace_name")]
        public object MyspaceName { get; set; }
        [JsonProperty("website")]
        public string Website { get; set; }
        [JsonProperty("website_title")]
        public string WebsiteTitle { get; set; }
        [JsonProperty("online")]
        public bool Online { get; set; }
        [JsonProperty("track_count")]
        public int? TrackCount { get; set; }
        [JsonProperty("playlist_count")]
        public int? PlaylistCount { get; set; }
        [JsonProperty("followers_count")]
        public int? FollowersCount { get; set; }
        [JsonProperty("followings_count")]
        public int? FollowingsCount { get; set; }
        [JsonProperty("public_favorites_count")]
        public int? PublicFavoritesCount { get; set; }
    }
}