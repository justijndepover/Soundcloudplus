using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string CreatedAt { get; set; }
        public int UserId { get; set; }
        public int Duration { get; set; }
        public bool Commentable { get; set; }
        public string State { get; set; }
        public string Sharing { get; set; }
        public string TagList { get; set; }
        public string Permalink { get; set; }
        public object Description { get; set; }
        public bool Streamable { get; set; }
        public bool Downloadable { get; set; }
        public object Genre { get; set; }
        public object Release { get; set; }
        public object PurchaseUrl { get; set; }
        public object LabelId { get; set; }
        public object LabelName { get; set; }
        public object Isrc { get; set; }
        public object VideoUrl { get; set; }
        public string TrackType { get; set; }
        public object KeySignature { get; set; }
        public object Bpm { get; set; }
        public string Title { get; set; }
        public object ReleaseYear { get; set; }
        public object ReleaseMonth { get; set; }
        public object ReleaseDay { get; set; }
        public string OriginalFormat { get; set; }
        public int OriginalContentSize { get; set; }
        public string License { get; set; }
        public string Uri { get; set; }
        public string PermalinkUrl { get; set; }
        public object ArtworkUrl { get; set; }
        public string WaveformUrl { get; set; }
        public User User { get; set; }
        public string StreamUrl { get; set; }
        public string DownloadUrl { get; set; }
        public int PlaybackCount { get; set; }
        public int DownloadCount { get; set; }
        public int FavoritingsCount { get; set; }
        public int CommentCount { get; set; }
        public User CreatedWith { get; set; }
        public string AttachmentsUri { get; set; }
    }
}