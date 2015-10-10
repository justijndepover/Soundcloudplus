namespace ClassLibrary.Models
{

    public class Playlist
    {
        public string Kind { get; set; }
        public int Id { get; set; }
        public string CreatedAt { get; set; }
        public int UserId { get; set; }
        public int Duration { get; set; }
        public string Sharing { get; set; }
        public string TagList { get; set; }
        public string Permalink { get; set; }
        public int TrackCount { get; set; }
        public bool Streamable { get; set; }
        public bool Downloadable { get; set; }
        public string EmbeddableBy { get; set; }
        public object PurchaseUrl { get; set; }
        public object LabelId { get; set; }
        public string Type { get; set; }
        public string PlaylistType { get; set; }
        public string Ean { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public string Release { get; set; }
        public object PurchaseTitle { get; set; }
        public string LabelName { get; set; }
        public string Title { get; set; }
        public object ReleaseYear { get; set; }
        public object ReleaseMonth { get; set; }
        public object ReleaseDay { get; set; }
        public string License { get; set; }
        public string Uri { get; set; }
        public string PermalinkUrl { get; set; }
        public string ArtworkUrl { get; set; }
        public User User { get; set; }
        public Track[] Tracks { get; set; }
    }
}