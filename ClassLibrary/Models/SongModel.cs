using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
