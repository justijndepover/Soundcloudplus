using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SoundCloudPlus.ViewModels;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.ViewManagement;

namespace SoundCloudPlus.Pages
{
    public sealed partial class MainPage
    {
        private HomePageViewModel _mainPageViewModel;
        public MainPage()
        {
            InitializeComponent();
            InitializeTitleBar();
        }

        void InitializeTitleBar()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            // Title bar colors. Alpha must be 255.
            titleBar.BackgroundColor = new Color() { A = 255, R = 255, G = 102, B = 25 };
            titleBar.ForegroundColor = new Color() { A = 255, R = 255, G = 255, B = 255 };
            titleBar.ButtonBackgroundColor = new Color() { A = 255, R = 255, G = 102, B = 25 };

            titleBar.ButtonForegroundColor = new Color() { A = 255, R = 255, G = 255, B = 255 };
            titleBar.ButtonHoverBackgroundColor = new Color() { A = 255, R = 255, G = 112, B = 41 };
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            _mainPageViewModel =
                (HomePageViewModel)Resources["MainPageViewModel"];
            if (App.SoundCloud.IsAuthenticated)
            {
                _mainPageViewModel.StreamCollection = await App.SoundCloud.GetStream();
                _mainPageViewModel.ExploreCollection = await App.SoundCloud.GetExplore();
            }
            MyFrame.Navigate(typeof (HomePage));
            base.OnNavigatedTo(e);
        }

        private void NavButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SplitViewMenu.IsPaneOpen = !SplitViewMenu.IsPaneOpen;
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
    }
}