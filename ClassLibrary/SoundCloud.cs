using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClassLibrary.API;
using ClassLibrary.Common;
using ClassLibrary.Models;
using Newtonsoft.Json;

namespace ClassLibrary
{
    public class SoundCloud : INotifyPropertyChanged
    {
        private static string ClientId { get; } = "776ca412db7b101b1602c6a67b1a0579";
        private static string ClientSecret { get; } = "2a1fb6127a52a2ef55dcfa5474baa9d5";
        private string Code { get; set; }
        private string Token { get; set; }
        private ApiProxy ApiProxy { get; }
        public User CurrentUser { get; set; }

        public SoundCloud()
        {
            ApiProxy = new ApiProxy();
            AsyncHelper.RunSync(IsAuthenticated);
        }

        public async Task<bool> IsAuthenticated()
        {
            //I am not letting this run aync because it causes issues when other code tries to use propery before async is completed
            if (CurrentUser == null || Code == null || Token == null)
            {
                CurrentUser = (User)ApplicationSettingsHelper.ReadRoamingSettingsValue<User>("currentUser");
                Code = (string)ApplicationSettingsHelper.ReadRoamingSettingsValue<string>("code");
                Token = (string)ApplicationSettingsHelper.ReadRoamingSettingsValue<string>("token");
                if (CurrentUser == null && Code == null && Token == null)
                {

                }
                else
                {
                    if (CurrentUser == null)
                    {
                        CurrentUser = (User)ApplicationSettingsHelper.ReadLocalSettingsValue<User>("currentUser");
                        ErrorLogProxy.LogError("CurrentUser is null");
                        ErrorLogProxy.NotifyErrorInDebug("CurrentUser is null");
                    }
                    if (Code == null)
                    {
                        Code = (string)ApplicationSettingsHelper.ReadLocalSettingsValue<string>("code");
                    }
                    if (Token == null)
                    {
                        Token = (string)ApplicationSettingsHelper.ReadLocalSettingsValue<string>("token");
                    }
                }
            }
            if (CurrentUser != null && Code != null && Token != null)
            {
                return true;
            }
            return await SignIn();
        }

        #region Stream
        private string _streamNextHref = "";
        public async Task<ObservableCollection<StreamCollection>> GetStream(int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/stream", null, new { limit = limitValue, offset = 0, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<StreamCollection> streamCollection = new ObservableCollection<StreamCollection>();
            if (apiResponse.Succes)
            {
                var trackLikes = await GetCurrentUserTrackLikes();
                var playlistLikes = await GetCurrentUserPlaylistLikes();
                foreach (var item in apiResponse.Data["collection"])
                {

                    StreamCollection stream = JsonConvert.DeserializeObject<StreamCollection>(item.ToString());
                    if (stream.Track != null)
                    {
                        foreach (var like in trackLikes)
                        {
                            if (stream.Track.Id == like)
                            {
                                stream.Track.IsLiked = true;
                            }
                        }
                    }
                    else if(stream.Playlist != null)
                    {
                        stream.Playlist = await GetPlaylist(stream.Playlist.Id);

                        foreach (var like in playlistLikes)
                        {
                            if (stream.Playlist.Id.ToString() == like)
                            {
                                stream.Playlist.IsLiked = true;
                            }
                        }
                    }
                    streamCollection.Add(stream);
                }
                _streamNextHref = apiResponse.Data["next_href"];
            }
            return streamCollection;
        }

        public async Task<ObservableCollection<StreamCollection>> GetStream(string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<StreamCollection> streamCollection = new ObservableCollection<StreamCollection>();
            if (apiResponse.Succes)
            {
                var trackLikes = await GetCurrentUserTrackLikes();
                var playlistLikes = await GetCurrentUserPlaylistLikes();
                foreach (var item in apiResponse.Data["collection"])
                {
                    StreamCollection stream = JsonConvert.DeserializeObject<StreamCollection>(item.ToString());
                    if (stream.Track != null)
                    {
                        foreach (var like in trackLikes)
                        {
                            if (stream.Track.Id == like)
                            {
                                stream.Track.IsLiked = true;
                            }
                        }
                    }
                    else if (stream.Playlist != null)
                    {
                        stream.Playlist = await GetPlaylist(stream.Playlist.Id);
                        foreach (var like in playlistLikes)
                        {
                            if (stream.Playlist.Id.ToString() == like)
                            {
                                stream.Playlist.IsLiked = true;
                            }
                        }
                    }
                    streamCollection.Add(stream);
                }
                _streamNextHref = apiResponse.Data["next_href"];
            }
            return streamCollection;
        }

        public async Task<WaveForm> GetWaveForm(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if(response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    var json = JsonConvert.DeserializeObject<WaveForm>(content);
                    return json;
                }
            }
            return null;
        }

        public string GetStreamNextHref()
        {
            return _streamNextHref;
        }
        #endregion

        #region Explore
        private string _exploreNextHref = "";
        public async Task<ObservableCollection<Track>> GetExplore(int limitValue, string genre)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/charts", null, new { kind = "top", genre = "soundcloud:genres:all-" + genre, limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                var likes = await GetCurrentUserTrackLikes();
                foreach (var item in apiResponse.Data["collection"])
                {
                    Track t = JsonConvert.DeserializeObject<Track>(item["track"].ToString());
                    foreach (var like in likes)
                    {
                        if (t.Id == like)
                        {
                            t.IsLiked = true;
                        }
                    }
                    tracks.Add(t);

                }
                _exploreNextHref = apiResponse.Data["next_href"];
            }
            return tracks;
        }

        public async Task<ObservableCollection<Track>> GetExplore(string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                var likes = await GetCurrentUserTrackLikes();
                foreach (var item in apiResponse.Data["collection"])
                {
                    Track t = JsonConvert.DeserializeObject<Track>(item["track"].ToString());
                    foreach (var like in likes)
                    {
                        if (t.Id == like)
                        {
                            t.IsLiked = true;
                        }
                    }
                    tracks.Add(t);

                }
                _exploreNextHref = apiResponse.Data["next_href"];
            }
            return tracks;
        }

        public string GetExploreNextHref()
        {
            return _exploreNextHref;
        }
        #endregion

        #region Categories
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
        #endregion

        #region Activities
        public async Task<Activity> GetActivities(int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/activities", null, new { limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            Activity activity = new Activity();
            if (apiResponse.Succes)
            {
                activity = JsonConvert.DeserializeObject<Activity>(apiResponse.Data.ToString());
            }
            return activity;
        }
        #endregion

        #region Playlist

        #region ProfilePlaylist
        private string _profilePlaylistNextHref = "";
        public async Task<ObservableCollection<Playlist>> GetOwnPlaylists(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/playlists", null, new { representation = "speedy", limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Playlist> playlist = new ObservableCollection<Playlist>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    Playlist p = await GetPlaylist(item["playlist"]["id"].ToString());
                    foreach (var track in p.Tracks.Where(track => p.ArtworkUrl == null))
                    {
                        p.ArtworkUrl = track.ArtworkUrl;
                    }
                    playlist.Add(p);
                }
            }
            object nhref = apiResponse.Data["next_href"].ToString();
            _playlistNextHref = nhref != null ? nhref.ToString() : "";
            return playlist;
        }

        public async Task<ObservableCollection<Playlist>> GetOwnPlaylists(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Playlist> playlist = new ObservableCollection<Playlist>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    Playlist p = await GetPlaylist(item["playlist"]["id"].ToString());
                    foreach (var track in p.Tracks.Where(track => p.ArtworkUrl == null))
                    {
                        p.ArtworkUrl = track.ArtworkUrl;
                    }
                    playlist.Add(p);
                }
            }
            object nhref = apiResponse.Data["next_href"].ToString();
            _playlistNextHref = nhref != null ? nhref.ToString() : "";
            return playlist;
        }

        public string GetProfilePlaylistNextHref()
        {
            return _profilePlaylistNextHref;
        }
        #endregion

        public async Task<ObservableCollection<string>> GetCurrentUserPlaylistLikes()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/e1/me/playlist_likes/ids", null, new { limit = 5000, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            ObservableCollection<string> userLikes = new ObservableCollection<string>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    try
                    {
                        userLikes.Add(JsonConvert.DeserializeObject<string>(item.ToString()));
                    }
                    catch (Exception ex)
                    {
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
            }
            return userLikes;
        }
        public async Task<Playlist> GetPlaylist(int playlistId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/playlists/" + playlistId, null, new { limit = 50, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            Playlist playlist = new Playlist();
            if (apiResponse.Succes)
            {
                var trackLikes = await GetCurrentUserTrackLikes();
                var playlistLikes = await GetCurrentUserTrackLikes();
                try
                {
                    playlist = JsonConvert.DeserializeObject<Playlist>(apiResponse.Data.ToString());
                    for (int i = 0; i < playlist.Tracks.Count; i++)
                    {
                        foreach (var like in trackLikes)
                        {
                            if (playlist.Tracks[i].Id == like)
                            {
                                playlist.Tracks[i].IsLiked = true;
                            }
                        }
                        if (playlist.ArtworkUrl == null)
                        {
                            playlist.ArtworkUrl = playlist.Tracks[i].ArtworkUrl;
                        }
                        if (playlist.Tracks[i].Title == null)
                        {
                            playlist.Tracks[i] = await GetTrackById(playlist.Tracks[i].Id);
                        }
                    }
                    foreach (var like in playlistLikes)
                    {
                        if (playlist.Id.ToString() == like)
                        {
                            playlist.IsLiked = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogProxy.LogError(ex.ToString());
                    ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                }
            }
            return playlist;
        }

        #region Playlists_Page
        private string _playlistNextHref = "";
        public async Task<ObservableCollection<Playlist>> GetPlaylists(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/playlists/liked_and_owned", null, new { keepBlocked = true, limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Playlist> playlist = new ObservableCollection<Playlist>();
            if (apiResponse.Succes)
            {
                try
                {
                    foreach (var item in apiResponse.Data["collection"])
                    {
                        Playlist p = await GetPlaylist(Convert.ToInt32(item["playlist"]["id"].Value));
                        foreach (var track in p.Tracks.Where(track => p.ArtworkUrl == null))
                        {
                            p.ArtworkUrl = track.ArtworkUrl;
                        }
                        playlist.Add(p);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                }
            }
            object nhref = apiResponse.Data["next_href"].ToString();
            _playlistNextHref = nhref != null ? nhref.ToString() : "";
            return playlist;
        }

        public async Task<ObservableCollection<Playlist>> GetPlaylists(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Playlist> playlist = new ObservableCollection<Playlist>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    Playlist p = await GetPlaylist(item["playlist"]["id"].ToString());
                    foreach (var track in p.Tracks.Where(track => p.ArtworkUrl == null))
                    {
                        p.ArtworkUrl = track.ArtworkUrl;
                    }
                    playlist.Add(p);
                }
            }
            object nhref = apiResponse.Data["next_href"].ToString();
            _playlistNextHref = nhref != null ? nhref.ToString() : "";
            return playlist;
        }

        public string GetPlaylistNextHref()
        {
            return _playlistNextHref;
        }
        #endregion

        //PUT /e1/me/playlist_reposts/164963817?client_id=02gUJC0hH2ct1EGOcYXQIzRFU91c72Ea&app_version=ed72175 HTTP/1.1
        public async Task<ApiResponse> RepostPlaylist(int playlistId)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Put, "/e1/me/playlist_reposts/" + playlistId, null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });

            if (apiResponse.Succes)
            {
                return apiResponse;
            }
            return null;
        }
        #endregion

        #region Tracks

        private string _profileTracksNextHref = "";

        public async Task<Track> GetTrackById(string id)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/tracks", null, new { ids = id, }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            Track tracks = new Track();
            if (apiResponse.Succes)
            {
                var likes = await GetCurrentUserTrackLikes();
                foreach (var item in apiResponse.Data["collection"])
                {
                    Track t = JsonConvert.DeserializeObject<Track>(item["track"].ToString());
                    foreach (var like in likes)
                    {
                        if (t.Id == like)
                        {
                            t.IsLiked = true;
                        }
                    }
                }
            }

            object nhref = apiResponse.Data["next_href"].ToString();
            _profileTracksNextHref = nhref != null ? nhref.ToString() : "";
            return tracks;
        }
        public async Task<ObservableCollection<Track>> GetTracks(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/tracks", null, new { keepBlocked = false, limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                var likes = await GetCurrentUserTrackLikes();
                foreach (var item in apiResponse.Data["collection"])
                {
                    Track t = JsonConvert.DeserializeObject<Track>(item["track"].ToString());
                    foreach (var like in likes)
                    {
                        if (t.Id == like)
                        {
                            t.IsLiked = true;
                        }
                    }
                }
            }
            
            object nhref = apiResponse.Data["next_href"].ToString();
            _profileTracksNextHref = nhref != null ? nhref.ToString() : "";
            return tracks;
        }

        public async Task<ObservableCollection<Track>> GetTracks(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                var likes = await GetCurrentUserTrackLikes();
                foreach (var item in apiResponse.Data["collection"])
                {
                    Track t = JsonConvert.DeserializeObject<Track>(item["track"].ToString());
                    foreach (var like in likes)
                    {
                        if (t.Id == like)
                        {
                            t.IsLiked = true;
                        }
                    }
                }
            }

            object nhref = apiResponse.Data["next_href"].ToString();
            _profileTracksNextHref = nhref != null ? nhref.ToString() : "";
            return tracks;
        }

        public string GetProfileTrackNextHref()
        {
            return _profileTracksNextHref;
        }

        public async Task<Uri> GetMusicFile(int id)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/tracks/" + id + "/streams", null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            string mp3 = "";
            if (apiResponse.Succes)
            {
                mp3 = apiResponse.Data["http_mp3_128_url"].Value;
            }
            return new Uri(mp3);
        }
        #endregion

        #region Reposts
        private string _repostNextHref = "";
        public async Task<ObservableCollection<RepostCollection>> GetReposts(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/profile/soundcloud:users:" + userId + "/reposts", null, new { keepBlocked = false, limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<RepostCollection> rC = new ObservableCollection<RepostCollection>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    rC.Add(JsonConvert.DeserializeObject<RepostCollection>(item.ToString()));
                }
                _repostNextHref = apiResponse.Data["next_href"];
            }
            return rC;
        }

        public async Task<ObservableCollection<RepostCollection>> GetReposts(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<RepostCollection> rC = new ObservableCollection<RepostCollection>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    rC.Add(JsonConvert.DeserializeObject<RepostCollection>(item.ToString()));
                }
                _repostNextHref = apiResponse.Data["next_href"];
            }
            return rC;
        }
        public string GetProfileRepostNextHref()
        {
            return _repostNextHref;
        }
        #endregion

        #region Followers
        private string _followerNextHref = "";
        public async Task<ObservableCollection<User>> GetFollowers(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/followers", null, new { keepBlocked = true, limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
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
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
                _followerNextHref = apiResponse.Data["next_href"];
            }
            return followers;
        }

        public async Task<ObservableCollection<User>> GetFollowers(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
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
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
                _followerNextHref = apiResponse.Data["next_href"];
            }
            return followers;
        }
        public string GetFollowerNextHref()
        {
            return _followerNextHref;
        }
        #endregion

        #region Followings
        private string _followingNextHref = "";
        public async Task<ObservableCollection<User>> GetFollowings(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/followings", null, new { limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
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
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
                _followingNextHref = apiResponse.Data["next_href"];
            }
            return followings;
        }

        public async Task<ObservableCollection<User>> GetFollowings(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
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
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
                _followingNextHref = apiResponse.Data["next_href"];
            }
            return followings;
        }

        public string GetFollowingNextHref()
        {
            return _followingNextHref;
        }
        #endregion

        #region Likes
        private string _likesNextHref = "";
        public async Task<ObservableCollection<string>> GetCurrentUserTrackLikes()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/e1/me/track_likes/ids", null, new { limit = 5000, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            ObservableCollection<string> userLikes = new ObservableCollection<string>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    try
                    {
                        userLikes.Add(JsonConvert.DeserializeObject<string>(item.ToString()));
                    }
                    catch (Exception ex)
                    {
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
            }
            return userLikes;
        }
        public async Task<ObservableCollection<Track>> GetLikes(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/track_likes", null, new { tag = "out-of-experiment", limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Track> tracklikes = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    try
                    {
                        Track t = JsonConvert.DeserializeObject<Track>(item["track"].ToString());
                        if (t.ArtworkUrl == null)
                        {
                            User u = await GetUser(t.User.Id);
                            t.ArtworkUrl = u.AvatarUrl;
                        }
                        tracklikes.Add(t);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
                _likesNextHref = apiResponse.Data["next_href"];
            }
            return tracklikes;
        }

        public async Task<ObservableCollection<Track>> GetLikes(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Track> tracklikes = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    try
                    {
                        Track t = JsonConvert.DeserializeObject<Track>(item["track"].ToString());
                        if (t.ArtworkUrl == null)
                        {
                            User u = await GetUser(t.User.Id);
                            t.ArtworkUrl = u.AvatarUrl;
                        }
                        tracklikes.Add(t);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
                _likesNextHref = apiResponse.Data["next_href"];
            }
            return tracklikes;
        }

        public async Task<bool> LikeTrack(string id)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Put, "/e1/me/track_likes/" + id, null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            return apiResponse.Succes;
        }
        public async Task<bool> UnlikeTrack(string id)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Delete, "/e1/me/track_likes/" + id, null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            return apiResponse.Succes;
        }
        public async Task<bool> LikePlaylist(int id)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Put, "/e1/me/playlist_likes/" + id, null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            return apiResponse.Succes;
        }

        public async Task<bool> UnlikePlaylist(int id)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Delete, "/e1/me/playlist_likes/" + id, null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            return apiResponse.Succes;
        }

        public string GetLikesNextHref()
        {
            return _likesNextHref;
        }
        #endregion

        #region SignIn + User
        public async Task<bool> SignIn()
        {
            if (await ApiProxy.Authenticate())
            {
                Token = ApplicationSettingsHelper.ReadRoamingSettingsValue<string>("token") as string;
                CurrentUser = await GetCurrentUser();
                return true;
            }
            return false;
        }

        public async Task<User> GetCurrentUser()
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/me", null, new { oauth_token = Token });
            User user = new User();
            if (apiResponse.Succes)
            {
                CurrentUser = JsonConvert.DeserializeObject<User>(apiResponse.Data.ToString());
            }
            return user;
        } 
        public async Task<User> GetUser(int id)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + id, null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            User u = new User();
            if (apiResponse.Succes)
            {
                u = JsonConvert.DeserializeObject<User>(apiResponse.Data.ToString());
            }
            return u;
        }
        #endregion

        #region Search + AutoSuggest
        public async Task<ObservableCollection<SearchCollection>> Search(string query)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/search", null, new { q = query, facet = "model", limit = "50", offset = "0", client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<SearchCollection> results = new ObservableCollection<SearchCollection>();
            if (apiResponse.Succes)
            {
                var trackLikes = await GetCurrentUserTrackLikes();
                var playlistLikes = await GetCurrentUserPlaylistLikes();
                foreach (var item in apiResponse.Data["collection"])
                {
                    SearchCollection result = new SearchCollection();
                    if (item["kind"].ToString().Contains("track"))
                    {
                        Track t = JsonConvert.DeserializeObject<Track>(item.ToString());
                        foreach (var like in trackLikes)
                        {
                            if (t.Id == like)
                            {
                                t.IsLiked = true;
                            }
                        }
                        result.Track = t;
                    } else if (item["kind"].ToString().Contains("user"))
                    {
                        User u = JsonConvert.DeserializeObject<User>(item.ToString());
                        result.User = u;
                    } else if (item["kind"].ToString().Contains("playlist"))
                    {
                        Playlist p = JsonConvert.DeserializeObject<Playlist>(item.ToString());

                        p = await GetPlaylist(p.Id);
                        foreach (var like in playlistLikes)
                        {
                            if (p.Id.ToString() == like)
                            {
                                p.IsLiked = true;
                            }
                        }
                        result.Playlist = p;
                    }
                    results.Add(result);
                }
            }
            return results;
        }
        public async Task<ObservableCollection<String>> AutoSuggest(string query)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/search/autocomplete", null, new { q = query, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<String> results = new ObservableCollection<String>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["results"])
                {
                    results.Add(item["output"].ToString());
                }
            }
            return results;
        }
        #endregion

        #region Genre
            public async Task<List<String>> GetGenres()
        {
            //https://api-v2.soundcloud.com/explore/categories?limit=10&offset=0&linked_partitioning=1&client_id=02gUJC0hH2ct1EGOcYXQIzRFU91c72Ea&app_version=8f103e4
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/charts/categories", null, new { client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            List<String> results = new List<String>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["music"])
                {
                    results.Add(item.ToString());
                }
                foreach (var item in apiResponse.Data["audio"])
                {
                    results.Add(item.ToString());
                }
            }
            return results;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}