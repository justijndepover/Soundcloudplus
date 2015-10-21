using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SoundCloudPlus.ViewModels;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace SoundCloudPlus.Pages
{
    public sealed partial class MainPage
    {
        private HomePageViewModel _mainPageViewModel;
        public MainPage()
        {
            InitializeComponent();
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

        private void Navigation_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var frame = this.SplitViewMenu.Content as Frame;
            Page page = frame?.Content as Page;
            Button b = sender as Button;
            switch (b.Tag.ToString())
            {
                case "recent":
                    if (page?.GetType() != typeof(recentPage))
                    {
                        frame.Navigate(typeof(recentPage));
                    }
                    break;
                case "artist":
                    if (page?.GetType() != typeof(artistPage))
                    {
                        frame.Navigate(typeof(artistPage));
                    }
                    break;
                case "genre":
                    if (page?.GetType() != typeof(genrePage))
                    {
                        frame.Navigate(typeof(genrePage));
                    }
                    break;
                case "following":
                    if (page?.GetType() != typeof(followingPage))
                    {
                        frame.Navigate(typeof(followingPage));
                    }
                    break;
                case "followers":
                    if (page?.GetType() != typeof(followerPage))
                    {
                        frame.Navigate(typeof(followerPage));
                    }
                    break;
                case "playlist":
                    if (page?.GetType() != typeof(playlistPage))
                    {
                        frame.Navigate(typeof(playlistPage));
                    }
                    break;
                case "like":
                    if (page?.GetType() != typeof(likePage))
                    {
                        frame.Navigate(typeof(likePage));
                    }
                    break;
                case "profile":
                    if (page?.GetType() != typeof(profilePage))
                    {
                        frame.Navigate(typeof(profilePage));
                    }
                    break;
                case "activity":
                    if (page?.GetType() != typeof(activityPage))
                    {
                        frame.Navigate(typeof(activityPage));
                    }
                    break;
                case "setting":
                    if (page?.GetType() != typeof(settingPage))
                    {
                        frame.Navigate(typeof(settingPage));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}