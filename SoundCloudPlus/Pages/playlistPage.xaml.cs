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
    public sealed partial class PlaylistPage : Page
    {
        private BackgroundWorker bwPlaylist = new BackgroundWorker();
        private ObservableCollection<PlaylistCollection> newPlaylistCollection = new ObservableCollection<PlaylistCollection>();
        private double verticalOffsetPlaylist;
        private double maxVerticalOffsetPlaylist;
        private FollowerPageViewModel _followerViewModel;
        public PlaylistPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            initBwPlaylist();
        }

        #region BackgroundWorkerPlaylist

        private void initBwPlaylist()
        {
            bwPlaylist.DoWork += BwPlaylist_DoWork;
            bwPlaylist.WorkerSupportsCancellation = true;
            bwPlaylist.RunWorkerCompleted += BwPlaylist_RunWorkerCompleted; ;
        }

        private void BwPlaylist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            PlaylistScroller();
        }

        private void BwPlaylist_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (PlaylistCollection pC in newPlaylistCollection)
                {
                    _playlistViewModel.PlaylistCollection.Add(pC);
                }
                newPlaylistCollection.Clear();
            }
            catch (Exception){}
            bwPlaylist.CancelAsync();
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
                    newPlaylistCollection = newCollection;
                }
                
            }
        }
        private void SvPlaylist_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is ...
            verticalOffsetPlaylist = svPlaylist.VerticalOffset;
            maxVerticalOffsetPlaylist = svPlaylist.ScrollableHeight;

            if (maxVerticalOffsetPlaylist < 0 || verticalOffsetPlaylist == maxVerticalOffsetPlaylist)
            {
                if (bwPlaylist.IsBusy == false)
                {
                    bwPlaylist.RunWorkerAsync();
                }
            }
        }
        #endregion

        private PlaylistPageViewModel _playlistViewModel;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
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
                    _playlistViewModel.PlaylistCollection = await App.SoundCloud.GetPlaylists(App.SoundCloud.CurrentUser.Id);
                }
            }
            base.OnNavigatedTo(e);
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

        
    }
}
