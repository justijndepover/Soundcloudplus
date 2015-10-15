using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SoundCloudPlus.ViewModels;

namespace SoundCloudPlus.Pages
{
    public sealed partial class MainPage
    {
        private MainPageViewModel _mainPageViewModel;
        public MainPage()
        {
            InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            _mainPageViewModel =
                (MainPageViewModel)Resources["MainPageViewModel"];
            if (App.SoundCloud.IsAuthenticated)
            {
                _mainPageViewModel.StreamCollection = await App.SoundCloud.GetStream();
                _mainPageViewModel.ExploreCollection = await App.SoundCloud.GetExplore();
            }
            base.OnNavigatedTo(e);
        }

        private void NavButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }

        private async void AccountButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!App.SoundCloud.IsAuthenticated)
            {
              if (await App.SoundCloud.SignIn())
              {
                  _mainPageViewModel.StreamCollection = await App.SoundCloud.GetStream();
                  _mainPageViewModel.ExploreCollection = await App.SoundCloud.GetExplore();
                }
              else
              {
                  await new MessageDialog("There was a problem signing you in").ShowAsync();
              }
            }
            else
            {
                await new MessageDialog("You are already signed in").ShowAsync();
                _mainPageViewModel.StreamCollection = await App.SoundCloud.GetStream();
                _mainPageViewModel.ExploreCollection = await App.SoundCloud.GetExplore();
            }
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
    }
}