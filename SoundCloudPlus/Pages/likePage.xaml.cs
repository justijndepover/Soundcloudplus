using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
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
        public LikePage()
        {
            this.InitializeComponent();
        }
        private LikeViewModel _likePageViewModel;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _likePageViewModel =
                    (LikeViewModel)Resources["LikeViewModel"];
                if (App.SoundCloud.IsAuthenticated)
                {
                    _likePageViewModel.TrackLikesCollection = await App.SoundCloud.GetLikes(App.SoundCloud.CurrentUser.Id);
                }
            }
            base.OnNavigatedTo(e);
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

        private void TrackLikesGridView_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            ItemsWrapGrid myItemsPanel = (ItemsWrapGrid)LikesGridView.ItemsPanelRoot;
            double screenWidth = e.NewSize.Width;
            int? itemsNumber = LikesGridView.Items?.Count;
            if (itemsNumber > 0)
            {
                if (myItemsPanel != null) myItemsPanel.ItemWidth = (screenWidth / GetNumberOfColumns(screenWidth));
            }
        }

        private async void TrackLikesGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            MainPageViewModel a = (MainPageViewModel)this.DataContext;
            Track t = e.ClickedItem as Track;
            a.PlayingTrack = await App.SoundCloud.GetMusicFile(t.Id.Value);
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
