using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary.API;
using ClassLibrary.Models;

namespace ClassLibrary
{
    public class SoundCloud
    {
        public static string ClientId { get; } = "bb45a30915dd5b2e04cf203b0f257c09";
        private ApiProxy ApiProxy { get; }

        public SoundCloud()
        {
            ApiProxy = new ApiProxy();
        }

        public async Task<List<Track>> GetExplore()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/explore/sounds", null, new {limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId});
            List<Track> tracks = apiResponse.Data.Cast<Playlist>();
            return tracks;
        }
    }
}
