using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ClassLibrary.API;
using ClassLibrary.Models;

namespace ClassLibrary
{
    public class SoundCloud
    {
        private static string ClientId { get; } = "bb45a30915dd5b2e04cf203b0f257c09";
        private static string ClientSecret { get; } = "85a0ba75066c4bd5fb14142b16c21d8a";
        private ApiProxy ApiProxy { get; }

        public SoundCloud()
        {
            ApiProxy = new ApiProxy();
        }

        public async Task<List<Track>> GetExplore()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/explore/sounds", null, new {limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId}, new {client_id = ClientId});
            List<Track> tracks = apiResponse.Data.Cast<Playlist>();
            return tracks;
        }
    }
}
