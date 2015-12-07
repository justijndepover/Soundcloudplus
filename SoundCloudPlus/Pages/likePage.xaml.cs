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
    public sealed partial class LikePage : Page
    {
        private BackgroundWorker bwLike = new BackgroundWorker();
        private ObservableCollection<Track> newLikeCollection = new ObservableCollection<Track>();
        private double verticalOffsetLike;
        private double maxVerticalOffsetLike;
        private LikePageViewModel _likePageViewModel;
        public LikePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            initBwLike();
        }
        #region BackgroundWorkerLike
        private void initBwLike()
        {
            bwLike.DoWork += BwLike_DoWork;
            bwLike.WorkerSupportsCancellation = true;
            bwLike.RunWorkerCompleted += BwLike_RunWorkerCompleted;
        }

        private void BwLike_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (Track t in newLikeCollection)
                {
                    _likePageViewModel.TrackLikesCollection.Add(t);
                }
                newLikeCollection.Clear();
            }
            catch (Exception){ }

            bwLike.CancelAsync();
        }

        private void BwLike_DoWork(object sender, DoWorkEventArgs e)
        {
            LikeScroller();
        }

        #endregion

        #region LikeScroller

        private async void LikeScroller()
        {
            var e = App.SoundCloud.GetLikesNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                ObservableCollection<Track> newCollection =
                    await App.SoundCloud.GetLikes(App.SoundCloud.CurrentUser.Id, b);
                newLikeCollection = newCollection;
            }
        }
        private void SvLikes_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            verticalOffsetLike = svLikes.VerticalOffset;
            maxVerticalOffsetLike = svLikes.ScrollableHeight;

            if (maxVerticalOffsetLike < 0 || verticalOffsetLike == maxVerticalOffsetLike)
            {
                if (bwLike.IsBusy == false)
                {
                    bwLike.RunWorkerAsync();
                }
            }
        }
        #endregion

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _likePageViewModel =
                    (LikePageViewModel)Resources["LikeViewModel"];
                if (await App.SoundCloud.IsAuthenticated())
                {
                    _likePageViewModel.TrackLikesCollection = await App.SoundCloud.GetLikes(App.SoundCloud.CurrentUser.Id);
                }
            }
            base.OnNavigatedTo(e);
            
        }

        private void TrackLikesGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, LikesGridView);
        }

        private void TrackLikesGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            MainPageViewModel a = (MainPageViewModel)DataContext;
            Track t = e.ClickedItem as Track;
            //a.PlayingTrack = await App.SoundCloud.GetMusicFile(t.Id.Value);
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
    }
}
