using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClassLibrary.API;
using ClassLibrary.Common;
using ClassLibrary.Models;
using Enough.Storage;
using Newtonsoft.Json;
using System.Net;

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
        private AudioPlayer _audioPlayer;
        public AudioPlayer AudioPlayer
        {
            get { return _audioPlayer; }
            set { _audioPlayer = value; OnPropertyChanged(nameof(AudioPlayer)); }
        }

        public SoundCloud()
        {
            ApiProxy = new ApiProxy();
            AsyncHelper.RunSync(IsAuthenticated);
            AudioPlayer = new AudioPlayer();
        }

        public async Task<bool> IsAuthenticated()
        {
            //I am not letting this run aync because it causes issues when other code tries to use propery before async is completed
            if (CurrentUser == null || Code == null || Token == null)
            {
                CurrentUser = AsyncHelper.RunSync(() => StorageHelper.TryLoadObjectAsync<User>("currentUser"));
                Code = AsyncHelper.RunSync(() => StorageHelper.TryLoadObjectAsync<string>("code"));
                Token = AsyncHelper.RunSync(() => StorageHelper.TryLoadObjectAsync<string>("token"));
                if (CurrentUser == null && Code == null && Token == null)
                {

                }
                else
                {
                    if (CurrentUser == null)
                    {
                        new ErrorLogProxy("CurrentUser is null");
                    }
                    if (Code == null)
                    {
                        new ErrorLogProxy("Code == null");
                    }
                    if (Token == null)
                    {
                        new ErrorLogProxy("Token == null");
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
            ObservableCollection<StreamCollection> tracks = new ObservableCollection<StreamCollection>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    tracks.Add(JsonConvert.DeserializeObject<StreamCollection>(item.ToString()));
                }
                _streamNextHref = apiResponse.Data["next_href"];
            }
            return tracks;
        }

        public async Task<ObservableCollection<StreamCollection>> GetStream(string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<StreamCollection> tracks = new ObservableCollection<StreamCollection>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {

                    tracks.Add(JsonConvert.DeserializeObject<StreamCollection>(item.ToString()));

                }
                _streamNextHref = apiResponse.Data["next_href"];
            }
            return tracks;
        }

        public async Task<WaveForm> getWaveForm(string url)
        {
            WaveForm json;
            //ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, null, null, null, );
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if(response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    json = JsonConvert.DeserializeObject<WaveForm>(content);
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
        public async Task<ObservableCollection<Track>> GetExplore(int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/explore/Popular+Music", null, new { tag = "out-of-experiment", limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
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
                        new ErrorLogProxy(ex.ToString());
                    }
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
                foreach (var item in apiResponse.Data["tracks"])
                {
                    try
                    {
                        tracks.Add(JsonConvert.DeserializeObject<Track>(item.ToString()));
                    }
                    catch (Exception ex)
                    {
                        new ErrorLogProxy(ex.ToString());
                    }
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
        public async Task<ObservableCollection<PlaylistCollection>> GetOwnPlaylists(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/playlists", null, new { representation = "speedy", limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            PlaylistObject pO = new PlaylistObject();
            if (apiResponse.Succes)
            {
                try
                {
                    if (pO.Collection[0].Playlist.ArtworkUrl != null)
                    {
                        pO.Collection[0].Playlist.ArtworkUrl = pO.Collection[0].Playlist.User.AvatarUrl;
                    }
                    pO = JsonConvert.DeserializeObject<PlaylistObject>(apiResponse.Data.ToString());
                }
                catch (Exception ex)
                {
                    new ErrorLogProxy(ex.ToString());
                    Debug.WriteLine(ex);
                }
            }
            //return pO;
            int l = pO.Collection.Count();
            ObservableCollection<PlaylistCollection> c = new ObservableCollection<PlaylistCollection>();

            if (l == 0)
            {
                return c;
            }
            else
            {
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
                object nhref = pO.NextHref;
                if (nhref != null)
                {
                    _playlistNextHref = nhref.ToString();
                }
                else
                {
                    _playlistNextHref = "";
                }
                return c;
            }
        }

        public async Task<ObservableCollection<PlaylistCollection>> GetOwnPlaylists(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            PlaylistObject pO = new PlaylistObject();
            if (apiResponse.Succes)
            {
                pO = JsonConvert.DeserializeObject<PlaylistObject>(apiResponse.Data.ToString());
            }
            //return pO;
            int l = pO.Collection.Count();
            ObservableCollection<PlaylistCollection> c = new ObservableCollection<PlaylistCollection>();

            if (l == 0)
            {
                return c;
            }
            else
            {
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
                object nhref = pO.NextHref;
                if (nhref != null)
                {
                    _playlistNextHref = nhref.ToString();
                }
                else
                {
                    _playlistNextHref = "";
                }
                return c;
            }
        }

        public string GetProfilePlaylistNextHref()
        {
            return _profilePlaylistNextHref;
        }
        #endregion

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
                        new ErrorLogProxy(ex.ToString());
                    }
                }
            }
            return tracks;
        }

        #region Playlists_Page
        private string _playlistNextHref = "";
        public async Task<ObservableCollection<PlaylistCollection>> GetPlaylists(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/playlists/liked_and_owned", null, new { keepBlocked = true, limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
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
                try
                {
                    if (pO.Collection[i].ArtworkUrl == null)
                    {
                        int pId = pO.Collection[i].Id;
                        ObservableCollection<Track> trackList = await GetTracksFromPlaylist(pId);
                        pO.Collection[i].ArtworkUrl = trackList[0].ArtworkUrl.ToString();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                c.Add(pO.Collection[i]);
            }
            object nhref = pO.NextHref;
            if (nhref != null)
            {
                _playlistNextHref = nhref.ToString();
            }
            else
            {
                _playlistNextHref = "";
            }
            return c;
        }

        public async Task<ObservableCollection<PlaylistCollection>> GetPlaylists(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
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
            object nhref = pO.NextHref;
            if (nhref != null)
            {
                _playlistNextHref = nhref.ToString();
            }
            else
            {
                _playlistNextHref = "";
            }
            return c;
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
        public async Task<ObservableCollection<Track>> GetTracks(int userId, int limitValue)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/users/" + userId + "/tracks", null, new { keepBlocked = false, limit = limitValue, offset = 0, linked_partitioning = 1, client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            TrackObject tO = new TrackObject();
            if (apiResponse.Succes)
            {
                tO = JsonConvert.DeserializeObject<TrackObject>(apiResponse.Data.ToString());
                try
                {
                    if (tO.Collection[0].ArtworkUrl == null)
                    {
                        User u = await GetUser(tO.Collection[0].User.Id);
                        tO.Collection[0].ArtworkUrl = u.AvatarUrl;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            int l = tO.Collection.Count();
            ObservableCollection<Track> c = new ObservableCollection<Track>();
            if (l == 0)
            {
                return c;
            }
            else
            {
                for (int i = 0; i < l; i++)
                {
                    c.Add(tO.Collection[i]);
                }
                object nhref = tO.NextHref;
                if (nhref != null)
                {
                    _profileTracksNextHref = nhref.ToString();
                }
                else
                {
                    _profileTracksNextHref = "";
                }
                return c;
            }
        }

        public async Task<ObservableCollection<Track>> GetTracks(int userId, string nextHref)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, nextHref, null, null, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token }, false);
            TrackObject tO = new TrackObject();
            if (apiResponse.Succes)
            {
                tO = JsonConvert.DeserializeObject<TrackObject>(apiResponse.Data.ToString());

                if (tO.Collection[0].ArtworkUrl == null)
                {
                    User u = await GetUser(tO.Collection[0].User.Id);
                    tO.Collection[0].ArtworkUrl = u.AvatarUrl;
                }
            }

            int l = tO.Collection.Count();
            ObservableCollection<Track> c = new ObservableCollection<Track>();
            for (int i = 0; i < l; i++)
            {
                c.Add(tO.Collection[i]);
            }
            object nhref = tO.NextHref;
            if (nhref != null)
            {
                _profileTracksNextHref = nhref.ToString();
            }
            else
            {
                _profileTracksNextHref = "";
            }
            return c;
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
                        new ErrorLogProxy(ex.ToString());
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
                        new ErrorLogProxy(ex.ToString());
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
                        new ErrorLogProxy(ex.ToString());
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
                        new ErrorLogProxy(ex.ToString());
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
                        new ErrorLogProxy(ex.ToString());
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
                        new ErrorLogProxy(ex.ToString());
                    }
                }
                _likesNextHref = apiResponse.Data["next_href"];
            }
            return tracklikes;
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
                Token = await StorageHelper.TryLoadObjectAsync<string>("token");
                ApiResponse apiResponse =
                    await ApiProxy.RequestTask(HttpMethod.Get, "/me", null, new { oauth_token = Token });
                CurrentUser = JsonConvert.DeserializeObject<User>(apiResponse.Data.ToString());
                await StorageHelper.SaveObjectAsync(CurrentUser, "currentUser");
                return true;
            }
            return false;
        }



        ///profile/soundcloud:users:26691406?keepBlocked=true&limit=10&offset=0&linked_partitioning=1&client_id=02gUJC0hH2ct1EGOcYXQIzRFU91c72Ea&app_version=eef6f5d HTTP/1.1
        //https://api.soundcloud.com/users/178017941
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
        public async Task<ObservableCollection<Track>> Search(string query)
        {
            ApiResponse apiResponse = await ApiProxy.RequestTask(HttpMethod.Get, "/search", null, new { q = query, facet = "model", limit = "10", offset = "0", client_id = ClientId, app_version = "a089efd" }, new { Accept = "application/json, text/javascript, */*; q=0.01", Authorization = "OAuth " + Token });
            ObservableCollection<Track> tracks = new ObservableCollection<Track>();
            if (apiResponse.Succes)
            {
                foreach (var item in apiResponse.Data["collection"])
                {
                    if (item["kind"].ToString().Contains("track"))
                    {
                        tracks.Add(JsonConvert.DeserializeObject<Track>(item.ToString()));
                    }
                }
            }
            return tracks;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}