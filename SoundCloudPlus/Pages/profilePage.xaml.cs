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

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        private int _userId;
        private BackgroundWorker bwProfileTrack = new BackgroundWorker();
        private BackgroundWorker bwProfilePlaylist = new BackgroundWorker();
        private BackgroundWorker bwRepost = new BackgroundWorker();
        private ObservableCollection<Track> newProfileTrackCollection = new ObservableCollection<Track>();

        private ObservableCollection<PlaylistCollection> newProfilePlaylistCollection =
            new ObservableCollection<PlaylistCollection>();

        private ObservableCollection<RepostCollection> newProfileRepostCollection = new ObservableCollection<RepostCollection>();   
        private double verticalOffsetProfileTrack;
        private double verticalOffsetProfilePlaylist;
        private double verticalOffsetProfileRepost;
        private double maxVerticalOffsetProfileTrack;
        private double maxVerticalOffsetProfilePlaylist;
        private double maxVerticalOffsetProfileRepost;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public ProfilePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            initBwProfileTrack();
            initBwProfilePlaylist();
            initBwProfileRepost();
        }
        #region BackgroundWorkerProfileTrack
        private void initBwProfileTrack()
        {
            bwProfileTrack.DoWork += BwProfileTrack_DoWork;
            bwProfileTrack.WorkerSupportsCancellation = true;
            bwProfileTrack.RunWorkerCompleted += BwProfileTrack_RunWorkerCompleted; ;
        }

        private void BwProfileTrack_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (Track t in newProfileTrackCollection)
                {
                    _profilePageViewModel.TrackCollection.Add(t);
                }
                newProfileTrackCollection.Clear();
            }
            catch (Exception){ }
            bwProfileTrack.CancelAsync();
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
                newProfileTrackCollection = newCollection;
            }
        }
        #endregion

        #region BackgroundWorkerProfilePlaylist
        private void initBwProfilePlaylist()
        {
            bwProfilePlaylist.DoWork += BwProfilePlaylist_DoWork;
            bwProfilePlaylist.WorkerSupportsCancellation = true;
            bwProfilePlaylist.RunWorkerCompleted += BwProfilePlaylist_RunWorkerCompleted;
        }

        private void BwProfilePlaylist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (PlaylistCollection pc in newProfilePlaylistCollection)
                {
                    _profilePageViewModel.PlaylistCollection.Add(pc);
                }
                newProfileTrackCollection.Clear();
            }
            catch (Exception) { }
            bwProfilePlaylist.CancelAsync();
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
                newProfilePlaylistCollection = newCollection;
            }
        }
        #endregion

        #region BackgroundWorkerProfileTrack
        private void initBwProfileRepost()
        {
            bwRepost.DoWork += BwRepost_DoWork;
            bwRepost.WorkerSupportsCancellation = true;
            bwRepost.RunWorkerCompleted += BwRepost_RunWorkerCompleted;
        }

        private void BwRepost_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (RepostCollection rc in newProfileRepostCollection)
                {
                    _profilePageViewModel.RepostCollection.Add(rc);
                }
                newProfileRepostCollection.Clear();
            }
            catch (Exception) { }
            bwRepost.CancelAsync();
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
                newProfileRepostCollection = newCollection;
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

                    try { _profilePageViewModel.PlaylistCollection = await App.SoundCloud.GetOwnPlaylists(id); }
                    catch (Exception) { _profilePageViewModel.PlaylistCollection = null; }

                    try { _profilePageViewModel.RepostCollection = await App.SoundCloud.GetReposts(id); }
                    catch (Exception) { _profilePageViewModel.RepostCollection = null; }

                    try { _profilePageViewModel.TrackCollection = await App.SoundCloud.GetTracks(id); }
                    catch (Exception) { _profilePageViewModel.TrackCollection = null; }
                }
            }
            base.OnNavigatedTo(e);
        }

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
        {}

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
            Button b = (Button) sender;
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
            throw new NotImplementedException();
        }

        private void SvProfileRepost_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void TrackCollectionGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, TrackCollectionGridView);
        }

        private void RepostCollectionGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, RepostCollectionGridView);
        }
    }
}
