using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;
using System.ComponentModel;
namespace SoundCloudPlus.Pages
{
    public sealed partial class HomePage : Page
    {
        private HomePageViewModel _homePageViewModel;
        private BackgroundWorker bwStream = new BackgroundWorker();
        private ObservableCollection<Track> newStreamCollection = new ObservableCollection<Track>();
        public HomePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            // backgroundworker init
            initBwStream();
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

                /*
                TitleSetter T = new TitleSetter(SetTitle);
                invoke(T, new object[] { "Whatever the title should be" }); //This can fail horribly, need the try/catch logic.
                */
            }
            catch (Exception) { }

            bwStream.CancelAsync();
        }

        private void BwStream_DoWork(object sender, DoWorkEventArgs e)
        {
            StreamScroller();
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
                if (App.SoundCloud.IsAuthenticated)
                {
                    _homePageViewModel.StreamCollection = await App.SoundCloud.GetStream();
                }_homePageViewModel.ExploreCollection = await App.SoundCloud.GetExplore();
            }
            base.OnNavigatedTo(e);
        }

        private void StreamGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, StreamGridView);
        }

        private void ExploreGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, ExploreGridView);
        }
        private void TrackGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            MainPage mainPage = MainPage.Current;
            mainPage.PlayTrack(e.ClickedItem as Track);
        }

        private void ScrollViewerExplore_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // TODO: Laad nieuwe items in wanneer scrollviewer op einde is...
            //_homePageViewModel.ExploreCollection
            //throw new System.NotImplementedException();
            var verticalOffset = svExplore.VerticalOffset;
            var maxVerticalOffset = svExplore.ScrollableHeight; //sv.ExtentHeight - sv.ViewportHeight;

            if (maxVerticalOffset < 0 ||
                verticalOffset == maxVerticalOffset)
            {
                // Scrolled to bottom
                //_homePageViewModel.ExploreCollection += await App.SoundCloud.GetExplore()
            }
            else
            {
                // Not scrolled to bottom
            }
        }

        private async void ScrollViewerStream_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // TODO: Laad nieuwe items in wanneer scrollviewer op einde is...
            //_homePageViewModel.StreamCollection
            //throw new System.NotImplementedException();
            verticalOffsetStream = svExplore.VerticalOffset;
            maxVerticalOffsetStream = svExplore.ScrollableHeight; //sv.ExtentHeight - sv.ViewportHeight;
            //Task t = Task.Run((Action) StreamScroller);
            if (maxVerticalOffsetStream < 0 || verticalOffsetStream == maxVerticalOffsetStream)
            {
                if (bwStream.IsBusy == false) {
                    bwStream.RunWorkerAsync();
                }  
            }
        }
        private double verticalOffsetStream;
        private double maxVerticalOffsetStream;
        private async void StreamScroller()
        {
            try
            {
                ObservableCollection<Track> newCollection = await App.SoundCloud.GetStream(App.SoundCloud.GetStreamNextHref().Replace("https://api-v2.soundcloud.com", ""));
                newStreamCollection = newCollection;
            }
            catch (Exception)
            {}
            
        }
    }
}
