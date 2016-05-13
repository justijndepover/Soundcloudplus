using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class SearchPage : Page
    {
        private string query;
        public SearchPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }
        private SearchPageViewModel _searchPageViewModel;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _searchPageViewModel =
                    (SearchPageViewModel)Resources["SearchPageViewModel"];
                if (await App.SoundCloud.IsAuthenticated())
                {
                    query = e.Parameter as string;
                    _searchPageViewModel.TrackSearchCollection = await App.SoundCloud.Search(query);
                }

                MainPage.Current.MainPageViewModel.PinButtonVisibility = Visibility.Visible;
                MainPage.Current.PinToStartButton.Click += PinToStartButton_Click;
            }
            base.OnNavigatedTo(e);
        }

        private void PinToStartButton_Click(object sender, RoutedEventArgs e)
        {
            TileService.CreateTileLinkedToPage("10Sound", query, new[] {"search", query});
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            if (MainPage.Current.MainFrame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            MainPage.Current.MainPageViewModel.PinButtonVisibility = Visibility.Collapsed;

            currentView.BackRequested -= CurrentView_BackRequested;
        }
        private void SearchGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, SearchGridView);
        }

        private void SearchGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            App.AudioPlayer.PlayTrack(new List<Track>((ObservableCollection<Track>) SearchGridView.ItemsSource), e.ClickedItem as Track);
        }
    }
}