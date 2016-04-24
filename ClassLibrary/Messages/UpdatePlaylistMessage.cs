using System.Collections.Generic;
using System.Runtime.Serialization;
using ClassLibrary.Models;

namespace ClassLibrary.Messages
{
    [DataContract]
    public class UpdatePlaylistMessage
    {
        public UpdatePlaylistMessage(List<Track> playList)
        {
            PlayList = playList;
        }

        [DataMember]
        public List<Track> PlayList;
    }
}
