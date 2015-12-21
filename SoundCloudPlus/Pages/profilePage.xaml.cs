using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.API;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;
using ClassLibrary.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
//TODO: Fix UpdatePlaylistCollection & UpdateRepostCollection (bij een nieuw account)
namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        private int _userId;
        private BackgroundWorker _bwProfileTrack = new BackgroundWorker();
        private BackgroundWorker _bwProfilePlaylist = new BackgroundWorker();
        private BackgroundWorker _bwRepost = new BackgroundWorker();
        private ObservableCollection<Track> _newProfileTrackCollection = new ObservableCollection<Track>();

        private ObservableCollection<PlaylistCollection> _newProfilePlaylistCollection =
            new ObservableCollection<PlaylistCollection>();

        private ObservableCollection<RepostCollection> _newProfileRepostCollection = new ObservableCollection<RepostCollection>();
        private double _verticalOffsetProfileTrack;
        private double _verticalOffsetProfilePlaylist;
        private double _verticalOffsetProfileRepost;
        private double _maxVerticalOffsetProfileTrack;
        private double _maxVerticalOffsetProfilePlaylist;
        private double _maxVerticalOffsetProfileRepost;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public ProfilePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            InitBwProfileTrack();
            InitBwProfilePlaylist();
            InitBwProfileRepost();
        }
        #region BackgroundWorkerProfileTrack
        private void InitBwProfileTrack()
        {
            _bwProfileTrack.DoWork += BwProfileTrack_DoWork;
            _bwProfileTrack.WorkerSupportsCancellation = true;
            _bwProfileTrack.RunWorkerCompleted += BwProfileTrack_RunWorkerCompleted; ;
        }

        private void BwProfileTrack_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (Track t in _newProfileTrackCollection)
                {
                    _profilePageViewModel.TrackCollection.Add(t);
                }
                _newProfileTrackCollection.Clear();
            }
            catch (Exception) { }
            _bwProfileTrack.CancelAsync();
        }

        private void BwProfileTrack_DoWork(object sender, DoWorkEventArgs e)
        {
            ProfileTrackScroller();
        }
        #endregion
        #region ProfileTrackScroller
        private async void ProfileTrackScroller()
        {
            var e = App.SoundCloud.GetProfileTrackNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                ObservableCollection<Track> newCollection = await App.SoundCloud.GetTracks(App.SoundCloud.CurrentUser.Id, b);
                _newProfileTrackCollection = newCollection;
            }
        }
        #endregion

        #region BackgroundWorkerProfilePlaylist
        private void InitBwProfilePlaylist()
        {
            _bwProfilePlaylist.DoWork += BwProfilePlaylist_DoWork;
            _bwProfilePlaylist.WorkerSupportsCancellation = true;
            _bwProfilePlaylist.RunWorkerCompleted += BwProfilePlaylist_RunWorkerCompleted;
        }

        private void BwProfilePlaylist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (PlaylistCollection pc in _newProfilePlaylistCollection)
                {
                    _profilePageViewModel.PlaylistCollection.Add(pc);
                }
                _newProfileTrackCollection.Clear();
            }
            catch (Exception) { }
            _bwProfilePlaylist.CancelAsync();
        }

        private void BwProfilePlaylist_DoWork(object sender, DoWorkEventArgs e)
        {
            ProfilePlaylistScroller();
        }
        #endregion
        #region ProfilePlaylistScroller
        private async void ProfilePlaylistScroller()
        {
            var e = App.SoundCloud.GetProfilePlaylistNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                ObservableCollection<PlaylistCollection> newCollection = await App.SoundCloud.GetOwnPlaylists(App.SoundCloud.CurrentUser.Id, b);
                _newProfilePlaylistCollection = newCollection;
            }
        }
        #endregion

        #region BackgroundWorkerProfileTrack
        private void InitBwProfileRepost()
        {
            _bwRepost.DoWork += BwRepost_DoWork;
            _bwRepost.WorkerSupportsCancellation = true;
            _bwRepost.RunWorkerCompleted += BwRepost_RunWorkerCompleted;
        }

        private void BwRepost_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (RepostCollection rc in _newProfileRepostCollection)
                {
                    _profilePageViewModel.RepostCollection.Add(rc);
                }
                _newProfileRepostCollection.Clear();
            }
            catch (Exception) { }
            _bwRepost.CancelAsync();
        }

        private void BwRepost_DoWork(object sender, DoWorkEventArgs e)
        {
            ProfileRepostScroller();
        }
        #endregion
        #region ProfileRepostScroller
        private async void ProfileRepostScroller()
        {
            var e = App.SoundCloud.GetProfilePlaylistNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                ObservableCollection<RepostCollection> newCollection = await App.SoundCloud.GetReposts(App.SoundCloud.CurrentUser.Id, b);
                _newProfileRepostCollection = newCollection;
            }
        }
        #endregion

        private ProfilePageViewModel _profilePageViewModel;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _profilePageViewModel =
                    (ProfilePageViewModel)Resources["ProfilePageViewModel"];
                if (await App.SoundCloud.IsAuthenticated())
                {

                    int id = MainPage.Current.UserId;

                    _profilePageViewModel.UserObject = await App.SoundCloud.GetUser(id);

                    try { UpdatePlaylistCollection(id); }
                    catch (Exception) { _profilePageViewModel.PlaylistCollection = null; }

                    try { UpdateRepostCollection(id); }
                    catch (Exception) { _profilePageViewModel.RepostCollection = null; }

                    try { UpdateTrackCollection(id); }
                    catch (Exception) { _profilePageViewModel.TrackCollection = null; }
                }
            }
            base.OnNavigatedTo(e);
        }

        #region UpdateCollection
        private async void UpdatePlaylistCollection(int id)
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                int limit = Screen.GetLimitItems(height, width, 400, 800, 200, 400);
                _profilePageViewModel.PlaylistCollection = await App.SoundCloud.GetOwnPlaylists(id, limit);
            }
            catch (Exception)
            {
                Application.Current.Exit();
            }
        }

        private async void UpdateTrackCollection(int id)
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                int limit = Screen.GetLimitItems(height, width, 400, 800, 200, 400);
                _profilePageViewModel.TrackCollection = await App.SoundCloud.GetTracks(id, limit);
            }
            catch (Exception)
            {
                Application.Current.Exit();
            }
        }

        private async void UpdateRepostCollection(int id)
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                int limit = Screen.GetLimitItems(height, width, 400, 800, 200, 400);
                _profilePageViewModel.RepostCollection = await App.SoundCloud.GetReposts(id, limit);
            }
            catch (Exception)
            {
                Application.Current.Exit();
            }
        }
        #endregion

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            List<int> l = MainPage.Current.UserIdHistory;
            int prevId = l[l.Count - 1];
            MainPage.Current.UserId = prevId;
            if (Frame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            currentView.BackRequested -= CurrentView_BackRequested;
        }

        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        { }

        private void MakeVisibleInvisible(bool all, bool trackCollection, bool playlistCollection, bool repostCollection)
        {
            if (all)
            {
                TrackCollectionGridView.Visibility = Visibility.Visible;
                PlaylistCollectionGridView.Visibility = Visibility.Visible;
                RepostCollectionGridView.Visibility = Visibility.Visible;
            }
            else
            {
                TrackCollectionGridView.Visibility = Visibility.Collapsed;
                PlaylistCollectionGridView.Visibility = Visibility.Collapsed;
                RepostCollectionGridView.Visibility = Visibility.Collapsed;

                if (trackCollection)
                {
                    TrackCollectionGridView.Visibility = Visibility.Visible;
                }

                if (playlistCollection)
                {
                    PlaylistCollectionGridView.Visibility = Visibility.Visible;
                }

                if (repostCollection)
                {
                    RepostCollectionGridView.Visibility = Visibility.Visible;
                }
            }
        }

        private async void RepostPlaylist(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            int playlistId = Convert.ToInt32(b.Tag);
            //repost niet mogelijk bij eigen user!!
            try
            {
                ApiResponse aR = await App.SoundCloud.RepostPlaylist(playlistId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void SvProfileTracks_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void SvProfilePlaylist_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void SvProfileRepost_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void TrackCollectionGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, TrackCollectionGridView);
        }

        private void RepostCollectionGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, RepostCollectionGridView);
        }

        private void btnFollowers_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b.Tag != null)
            {
                UserId = (int) b.Tag;
            }
            int userId = UserId;
            if (userId == 0)
            {
                userId = App.SoundCloud.CurrentUser.Id;
            }
            b.Tag = userId;
            MainPage.Current.Navigate(sender, "followers");
        }

        private void btnFollowing_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b.Tag != null)
            {
                UserId = (int)b.Tag;
            }
            int userId = UserId;
            if (userId == 0)
            {
                userId = App.SoundCloud.CurrentUser.Id;
            }
            b.Tag = userId;
            MainPage.Current.Navigate(sender, "following");
        }
    }
}
