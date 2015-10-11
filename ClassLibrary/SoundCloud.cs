using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using ClassLibrary.API;
using ClassLibrary.Common;
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

        private void LoadSettings()
        {
            //I am not letting this run aync because it causes issues when other code tries to use propery before async is completed
            CurrentUser = AsyncHelper.RunSync(() => StorageHelper.TryLoadObjectAsync<User>("currentUser"));
            Code = AsyncHelper.RunSync(() => StorageHelper.TryLoadObjectAsync<string>("code"));
            Token = AsyncHelper.RunSync(() => StorageHelper.TryLoadObjectAsync<string>("token"));
            if (CurrentUser != null && Code != null && Token != null)
            {
                IsAuthenticated = true;
            }
            else
            {
                IsAuthenticated = false;
            }
        }

        public async Task<ObservableCollection<Track>> GetStream()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/stream", null, new { limit = 10, offset = 0, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "Oauth " + Token});
            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            foreach (var item in apiResponse.Data["collection"])
            {
                if (item["type"].ToString().Equals("track"))
                {
                    tracks.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<Track>(item["track"].ToString()));
                }
            }
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