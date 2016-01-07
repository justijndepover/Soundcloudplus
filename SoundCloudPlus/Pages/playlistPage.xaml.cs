using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistPage
    {
        private BackgroundWorker _bwPlaylist = new BackgroundWorker();
        private ObservableCollection<PlaylistCollection> _newPlaylistCollection = new ObservableCollection<PlaylistCollection>();
        private double _verticalOffsetPlaylist;
        private double _maxVerticalOffsetPlaylist;

        public PlaylistPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            InitBwPlaylist();
        }

        #region BackgroundWorkerPlaylist

        private void InitBwPlaylist()
        {
            _bwPlaylist.DoWork += BwPlaylist_DoWork;
            _bwPlaylist.WorkerSupportsCancellation = true;
            _bwPlaylist.RunWorkerCompleted += BwPlaylist_RunWorkerCompleted; ;
        }

        private void BwPlaylist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PlaylistScroller();
        }

        private void BwPlaylist_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (PlaylistCollection pC in _newPlaylistCollection)
                {
                    _playlistViewModel.PlaylistCollection.Add(pC);
                }
                _newPlaylistCollection.Clear();
            }
            catch (Exception){}
            _bwPlaylist.CancelAsync();
        }

        #endregion

        #region PlaylistScroller

        private async void PlaylistScroller()
        {
            var e = App.SoundCloud.GetPlaylistNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                if (b != "")
                {
                    ObservableCollection<PlaylistCollection> newCollection =
                    await App.SoundCloud.GetPlaylists(App.SoundCloud.CurrentUser.Id, b);
                    _newPlaylistCollection = newCollection;
                }
            }
        }
        private void SvPlaylist_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is ...
            _verticalOffsetPlaylist = SvPlaylist.VerticalOffset;
            _maxVerticalOffsetPlaylist = SvPlaylist.ScrollableHeight;

            if (_maxVerticalOffsetPlaylist < 0 || _verticalOffsetPlaylist == _maxVerticalOffsetPlaylist)
            {
                if (_bwPlaylist.IsBusy == false)
                {
                    _bwPlaylist.RunWorkerAsync();
                }
            }
        }
        #endregion

        private PlaylistPageViewModel _playlistViewModel;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _playlistViewModel =
                    (PlaylistPageViewModel)Resources["PlaylistViewModel"];
                if (await App.SoundCloud.IsAuthenticated())
                {
                    UpdatePlaylistCollection();
                }
            }
            base.OnNavigatedTo(e);
        }

        private async void UpdatePlaylistCollection()
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                int limit = Screen.GetLimitItems(height, width, 400, 800, 200, 400);
                _playlistViewModel.PlaylistCollection = await App.SoundCloud.GetPlaylists(App.SoundCloud.CurrentUser.Id, limit);
            }
            catch (Exception)
            {
                Application.Current.Exit();
            }
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            currentView.BackRequested -= CurrentView_BackRequested;
        }

        private void PlaylistGridView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, PlaylistGridView);
        }

        private void PlaylistGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                PlaylistCollection pc = e.ClickedItem as PlaylistCollection;
                Playlist p = pc.Playlist;
                App.SoundCloud.AudioPlayer.PlayTrack(p.Tracks, p.Tracks[0]);
                MainPage.Current.CurrentPlaylist = p;
                MainPage.Current.Navigate(sender, "playlistview");
            }
            catch
            {

            }
        }
    }
}
