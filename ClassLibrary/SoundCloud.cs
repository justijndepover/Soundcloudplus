using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ClassLibrary.API;
using ClassLibrary.Models;
using Enough.Storage;

namespace ClassLibrary
{
    public class SoundCloud
    {
        private static string ClientId { get; } = "776ca412db7b101b1602c6a67b1a0579";
        private static string ClientSecret { get; } = "2a1fb6127a52a2ef55dcfa5474baa9d5";
        private string Code { get; set; }
        private string Token { get; set; }
        private ApiProxy ApiProxy { get; }
        public User CurrentUser { get; set; }
        public bool IsAuthenticated { get; set; }

        public SoundCloud()
        {
            ApiProxy = new ApiProxy();
            LoadSettings();
        }

        private async void LoadSettings()
        {
            CurrentUser = await StorageHelper.TryLoadObjectAsync<User>("currentUser");
            Code = await StorageHelper.TryLoadObjectAsync<string>("code");
            Token = await StorageHelper.TryLoadObjectAsync<string>("token");
            if (CurrentUser != null && Code != null && Token != null)
            {
                IsAuthenticated = true;
            }
            else
            {
                IsAuthenticated = false;
            }
        }

        public async Task<List<Track>> GetExplore()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/explore/sounds", null, new {limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId}, new {client_id = ClientId});
            List<Track> tracks = apiResponse.Data.Cast<Playlist>();
            return tracks;
        }

        public async void SignIn()
        {
            if (await ApiProxy.Authenticate())
            {
                Token = await StorageHelper.TryLoadObjectAsync<string>("token");
                ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/me", null, new { oauth_token = Token});
                CurrentUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(apiResponse.Data.ToString());
                await StorageHelper.SaveObjectAsync(CurrentUser, "currentUser");
            }
        }
    }
}