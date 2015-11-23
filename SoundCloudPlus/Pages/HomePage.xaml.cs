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

namespace SoundCloudPlus.Pages
{
    public sealed partial class HomePage : Page
    {
        private HomePageViewModel _homePageViewModel;
        public HomePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

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
            Task t = Task.Run((Action) StreamScroller);
        }

        private async void StreamScroller()
        {
            var verticalOffset = svExplore.VerticalOffset;
            var maxVerticalOffset = svExplore.ScrollableHeight; //sv.ExtentHeight - sv.ViewportHeight;

            if (maxVerticalOffset < 0 || verticalOffset == maxVerticalOffset)
            {
                // Scrolled to bottom
                ObservableCollection<Track> newCollection = await App.SoundCloud.GetStream(App.SoundCloud.GetStreamNextHref().Replace("https://api-v2.soundcloud.com", ""));
                foreach (var track in newCollection)
                {
                    _homePageViewModel.StreamCollection.Add(track);
                }
            }
        }
    }
}
