using System.Collections.Generic;
using System.Runtime.Serialization;
using ClassLibrary.Models;

namespace ClassLibrary.Messages
{
    [DataContract]
    public class UpdatePlaylistMessage
    {
        public UpdatePlaylistMessage(IEnumerable<Track> playList)
        {
            PlayList = playList;
        }

        [DataMember]
        public IEnumerable<Track> PlayList;
    }
}
