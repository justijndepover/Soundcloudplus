using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;

namespace SoundCloudPlus.Pages
{
    public sealed partial class HomePage : Page
    {
        private HomePageViewModel _homePageViewModel;
        private BackgroundWorker bwExplore = new BackgroundWorker();
        private BackgroundWorker bwStream = new BackgroundWorker();
        private ObservableCollection<Track> newStreamCollection = new ObservableCollection<Track>();
        private ObservableCollection<Track> newExploreCollection = new ObservableCollection<Track>();
        private double verticalOffsetStream;
        private double verticalOffsetExplore;
        private double maxVerticalOffsetStream;
        private double maxVerticalOffsetExplore;

        public HomePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            // backgroundworker init
            initBwStream();
            initBwExplore();
        }

        #region BackgroundWorkerStream
        private void initBwStream()
        {
            bwStream.DoWork += BwStream_DoWork;
            bwStream.WorkerSupportsCancellation = true;
            bwStream.RunWorkerCompleted += BwStream_RunWorkerCompleted;
        }

        private void BwStream_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (Track t in newStreamCollection)
                {
                    _homePageViewModel.StreamCollection.Add(t);
                }
            }
            catch (Exception) { }

            bwStream.CancelAsync();
        }

        private void BwStream_DoWork(object sender, DoWorkEventArgs e)
        {
            StreamScroller();
        }
        #endregion

        #region BackgroundWorkerExplore
        private void initBwExplore()
        {
            bwExplore.DoWork += BwExplore_DoWork;
            bwExplore.WorkerSupportsCancellation = true;
            bwExplore.RunWorkerCompleted += BwExplore_RunWorkerCompleted;
        }

        private void BwExplore_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (Track t in newExploreCollection)
                {
                    _homePageViewModel.ExploreCollection.Add(t);
                }
            }
            catch (Exception) { }

            bwStream.CancelAsync();
        }

        private void BwExplore_DoWork(object sender, DoWorkEventArgs e)
        {
            ExploreScroller();
        }
        #endregion

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            Frame.BackStack.Clear();
            if (e.NavigationMode != NavigationMode.Back)
            {
                _homePageViewModel =
                    (HomePageViewModel)Resources["HomePageViewModel"];
                while (!await App.SoundCloud.IsAuthenticated())
                {
                    new ManualResetEvent(false).WaitOne(1000);
                }

                UpdateStreamExploreCollection();
            }
            base.OnNavigatedTo(e);
        }

        private async void UpdateStreamExploreCollection()
        {
            var bounds = Window.Current.Bounds;
            double height = bounds.Height;
            double width = bounds.Width;
            int limit = Screen.getLimitItems(height, width, 400, 800, 200, 400);
            _homePageViewModel.StreamCollection = await App.SoundCloud.GetStream(limit);
            _homePageViewModel.ExploreCollection = await App.SoundCloud.GetExplore(limit);
        }

        private void StreamGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, StreamGridView);
            UpdateStreamExploreCollection();
        }

        private void ExploreGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, ExploreGridView);
            UpdateStreamExploreCollection();
        }
        private void TrackGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            App.SoundCloud.AudioPlayer.PlayTrack(e.ClickedItem as Track);
        }

        #region StreamScroller
        private void ScrollViewerStream_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            verticalOffsetStream = svSteam.VerticalOffset;
            maxVerticalOffsetStream = svSteam.ScrollableHeight;

            if (maxVerticalOffsetStream < 0 || verticalOffsetStream == maxVerticalOffsetStream)
            {
                if (bwStream.IsBusy == false)
                {
                    bwStream.RunWorkerAsync();
                }
            }
        }

        private async void StreamScroller()
        {
            var e = App.SoundCloud.GetStreamNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                ObservableCollection<Track> newCollection = await App.SoundCloud.GetStream(b);
                newStreamCollection = newCollection;
            }
        }
        #endregion

        #region ExploreScroller
        private void ScrollViewerExplore_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            verticalOffsetExplore = svExplore.VerticalOffset;
            maxVerticalOffsetExplore = svExplore.ScrollableHeight;

            if (maxVerticalOffsetExplore < 0 || verticalOffsetExplore == maxVerticalOffsetExplore)
            {
                if (bwExplore.IsBusy == false)
                {
                    bwExplore.RunWorkerAsync();
                }
            }
        }

        private async void ExploreScroller()
        {
            var e = App.SoundCloud.GetExploreNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                ObservableCollection<Track> newCollection = await App.SoundCloud.GetExplore(b);
                newExploreCollection = newCollection;
            }
        }
        #endregion
    }
}
