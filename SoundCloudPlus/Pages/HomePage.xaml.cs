using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;

namespace SoundCloudPlus.Pages
{
    public sealed partial class HomePage : Page
    {
        private HomePageViewModel _homePageViewModel;
        public HomePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
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
                }   _homePageViewModel.ExploreCollection = await App.SoundCloud.GetExplore();
            }
            base.OnNavigatedTo(e);
        }

        private void StreamGridView_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            ItemsWrapGrid myItemsPanel = (ItemsWrapGrid)StreamGridView.ItemsPanelRoot;
            double screenWidth = e.NewSize.Width;
            int? itemsNumber = StreamGridView.Items?.Count;
            if (itemsNumber > 0)
            {
                if (myItemsPanel != null) myItemsPanel.ItemWidth = (screenWidth / GetNumberOfColumns(screenWidth));
            }
        }

        private int GetNumberOfColumns(double screenWidth)
        {
            int w = 799;
            int c = 1;
            while (true)
            {
                if (screenWidth <= w)
                {
                    return c;
                }
                c++;
                w += 400;
            }
        }

        private void ExploreGridView_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            ItemsWrapGrid myItemsPanel = (ItemsWrapGrid)ExploreGridView.ItemsPanelRoot;
            double screenWidth = e.NewSize.Width;
            int? itemsNumber = ExploreGridView.Items?.Count;
            if (itemsNumber > 0)
            {
                if (myItemsPanel != null) myItemsPanel.ItemWidth = (screenWidth / GetNumberOfColumns(screenWidth));
            }
        }

        private async void TrackGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            MainPageViewModel _mainPageViewModel =
                    (MainPageViewModel)Resources["MainPageViewModel"];
            Track t = e.ClickedItem as Track;
            _mainPageViewModel.PlayingTrack = await App.SoundCloud.GetMusicFile(t.Id.Value);
        }
    }
}
