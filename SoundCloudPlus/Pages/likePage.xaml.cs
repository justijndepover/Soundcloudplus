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
        private BackgroundWorker _bwLike = new BackgroundWorker();
        private ObservableCollection<Track> _newLikeCollection = new ObservableCollection<Track>();
        private double _verticalOffsetLike;
        private double _maxVerticalOffsetLike;
        private LikePageViewModel _likePageViewModel;
        public LikePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            InitBwLike();
        }
        #region BackgroundWorkerLike
        private void InitBwLike()
        {
            _bwLike.DoWork += BwLike_DoWork;
            _bwLike.WorkerSupportsCancellation = true;
            _bwLike.RunWorkerCompleted += BwLike_RunWorkerCompleted;
        }

        private void BwLike_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (Track t in _newLikeCollection)
                {
                    _likePageViewModel.TrackLikesCollection.Add(t);
                }
                _newLikeCollection.Clear();
            }
            catch (Exception){ }

            _bwLike.CancelAsync();
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
                _newLikeCollection = newCollection;
            }
        }
        private void SvLikes_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            _verticalOffsetLike = SvLikes.VerticalOffset;
            _maxVerticalOffsetLike = SvLikes.ScrollableHeight;

            if (_maxVerticalOffsetLike < 0 || _verticalOffsetLike == _maxVerticalOffsetLike)
            {
                if (_bwLike.IsBusy == false)
                {
                    _bwLike.RunWorkerAsync();
                }
            }
        }
        #endregion

        protected override async void OnNavigatedTo(NavigationEventArgs e)
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
                    UpdateLikeCollection();
                }
            }
            base.OnNavigatedTo(e);
            
        }

        private async void UpdateLikeCollection()
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                int limit = Screen.GetLimitItems(height, width, 400, 800, 200, 400);
                _likePageViewModel.TrackLikesCollection = await App.SoundCloud.GetLikes(App.SoundCloud.CurrentUser.Id, limit);
            }
            catch (Exception)
            {
                Application.Current.Exit();
            }
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
