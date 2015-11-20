﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ClassLibrary.API;
using ClassLibrary.Common;
using ClassLibrary.Models;
using Enough.Storage;
using Newtonsoft.Json;

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

        #region Stream / Explore
        public async Task<ObservableCollection<Track>> GetStream()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/stream", null, new { limit = 10, offset = 0, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    if (item["type"].ToString().Contains("track"))
                    {
                        tracks.Add(JsonConvert.DeserializeObject<Track>(item["track"].ToString()));
                    }
                }
            }
            return tracks;
        }

        public async Task<ObservableCollection<Track>> GetExplore()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/explore/Popular+Music", null, new { tag = "out-of-experiment", limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["tracks"])
                {
                    try
                    {
                        tracks.Add(JsonConvert.DeserializeObject<Track>(item.ToString()));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            return tracks;
        }
        #endregion

        #region Categories/Activities/Playlists
        public async Task<ObservableCollection<string>> GetCategories()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/explore/categories", null, new { tag = "out-of-experiment", limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<string> categories = new ObservableCollection<string>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["music"])
                {
                    categories.Add(JsonConvert.DeserializeObject<string>(item.ToString()));
                }
            }
            return categories;
        }

        public async Task<Activity> GetActivities()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/activities", null, new { limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            Activity activity = new Activity();
            if (apiResponse.Succes)
            {
                activity = JsonConvert.DeserializeObject<Activity>(apiResponse.Data.ToString());
            }
            return activity;
        }

        public async Task<ObservableCollection<PlaylistCollection>> GetOwnPlaylists(int userId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/playlists", null, new { representation = "speedy", limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            PlaylistObject pO = new PlaylistObject();
            if (apiResponse.Succes)
            {
                pO = JsonConvert.DeserializeObject<PlaylistObject>(apiResponse.Data.ToString());
            }
            //return pO;
            int l = pO.Collection.Count();
            ObservableCollection<PlaylistCollection> c = new ObservableCollection<PlaylistCollection>();
            for (int i = 0; i < l; i++)
            {
                if (pO.Collection[i].ArtworkUrl == null)
                {
                    int pId = pO.Collection[i].Id;
                    ObservableCollection<Track> trackList = await GetTracksFromPlaylist(pId);
                    pO.Collection[i].ArtworkUrl = trackList[0].ArtworkUrl.ToString();
                }
                c.Add(pO.Collection[i]);
            }
            return c;
        }

        public async Task<ObservableCollection<Track>> GetTracksFromPlaylist(int playlistId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/playlists/" + playlistId, null, new { limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["tracks"])
                {
                    try
                    {
                        tracks.Add(JsonConvert.DeserializeObject<Track>(item.ToString()));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            return tracks;
        }

        public async Task<ObservableCollection<PlaylistCollection>> GetPlaylists(int userId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/playlists/liked_and_owned", null, new { keepBlocked = true, limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            PlaylistObject pO = new PlaylistObject();
            if (apiResponse.Succes)
            {
                pO = JsonConvert.DeserializeObject<PlaylistObject>(apiResponse.Data.ToString());
            }
            //return pO;
            int l = pO.Collection.Count();
            ObservableCollection<PlaylistCollection> c = new ObservableCollection<PlaylistCollection>();
            for (int i = 0; i < l; i++)
            {
                c.Add(pO.Collection[i]);
            }
            return c;
        }

        public async Task<ObservableCollection<Track>> GetTracks(int userId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/tracks", null, new { keepBlocked = false, limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            TrackObject tO = new TrackObject();
            if (apiResponse.Succes)
            {
                tO = JsonConvert.DeserializeObject<TrackObject>(apiResponse.Data.ToString());
            }

            int l = tO.Collection.Count();
            ObservableCollection<Track> c = new ObservableCollection<Track>();
            for (int i = 0; i < l; i++)
            {
                c.Add(tO.Collection[i]);
            }
            return c;
        }

        public async Task<ObservableCollection<RepostCollection>> GetReposts(int userId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/profile/soundcloud:users:" + userId + "/reposts", null, new { keepBlocked = false, limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<RepostCollection> rC = new ObservableCollection<RepostCollection>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    rC.Add(JsonConvert.DeserializeObject<RepostCollection>(item.ToString()));
                }
            }
            return rC;
        }
        #endregion

        #region Followers/Followings
        public async Task<ObservableCollection<User>> GetFollowers(int userId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/followers", null, new { keepBlocked = true, limit = 25, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            ObservableCollection<User> followers = new ObservableCollection<User>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    try
                    {
                        followers.Add(JsonConvert.DeserializeObject<User>(item.ToString()));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            return followers;
        }

        public async Task<ObservableCollection<User>> GetFollowings(int userId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/followings", null, new { limit = 25, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            ObservableCollection<User> followings = new ObservableCollection<User>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    try
                    {
                        followings.Add(JsonConvert.DeserializeObject<User>(item.ToString()));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            return followings;
        }
        #endregion

        //PUT /e1/me/playlist_reposts/164963817?client_id=02gUJC0hH2ct1EGOcYXQIzRFU91c72Ea&app_version=ed72175 HTTP/1.1
        public async Task<ApiResponse> RepostPlaylist(int playlistId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Put, "/e1/me/playlist_reposts/"+playlistId, null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            
            if (apiResponse.Succes)
            {
                return apiResponse;
            }
            return null;
        }

        public async Task<ObservableCollection<Track>> GetLikes(int userId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/explore/Popular+Music", null, new { tag = "out-of-experiment", limit = 10, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Track> tracklikes = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["tracks"])
                {
                    try
                    {
                        tracklikes.Add(JsonConvert.DeserializeObject<Track>(item.ToString()));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            return tracklikes;
        }

        public async Task<bool> SignIn()
        {
            if (await ApiProxy.Authenticate())
            {
                Token = await StorageHelper.TryLoadObjectAsync<string>("token");
                ApiResponse apiResponse =
                    await ApiProxy.RequestTask(HttpMethod.Get, "/me", null, new {oauth_token = Token});
                CurrentUser = JsonConvert.DeserializeObject<User>(apiResponse.Data.ToString());
                await StorageHelper.SaveObjectAsync(CurrentUser, "currentUser");
                return true;
            }
            return false;
        }

        public async Task<Uri> GetMusicFile(int id)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/tracks/"+id+"/streams", null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            string mp3 = "";
            if (apiResponse.Succes)
            {
                mp3 = apiResponse.Data["http_mp3_128_url"].Value;
            }
            return new Uri(mp3);
        }

        ///profile/soundcloud:users:26691406?keepBlocked=true&limit=10&offset=0&linked_partitioning=1&client_id=02gUJC0hH2ct1EGOcYXQIzRFU91c72Ea&app_version=eef6f5d HTTP/1.1
        //https://api.soundcloud.com/users/178017941
        public async Task<User> GetUser(int id)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/"+id, null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            User u = new User();
            if (apiResponse.Succes)
            {
                u = JsonConvert.DeserializeObject<User>(apiResponse.Data.ToString());
            }
            return u;
        }
    }
}