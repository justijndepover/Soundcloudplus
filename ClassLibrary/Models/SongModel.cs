using System;
using System.Runtime.Serialization;

namespace ClassLibrary.Models
{
    [DataContract]
    public class SongModel
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public Uri MediaUri { get; set; }

        [DataMember]
        public Uri AlbumArtUri { get; set; }
    }
}
