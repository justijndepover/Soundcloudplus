using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SoundCloudPlus.ViewModels;
using ClassLibrary.Models;

namespace SoundCloudPlus.Pages
{
    public sealed partial class MainPage
    {
        private MainPageViewModel _mainPageViewModel;
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            if (e.NavigationMode != NavigationMode.Back)
            {
                MyFrame.Navigate(typeof(HomePage));
                _mainPageViewModel =
                    (MainPageViewModel)Resources["MainPageViewModel"];
            }
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
                  //_mainPageViewModel.StreamCollection = await App.SoundCloud.GetStream();
                  //_mainPageViewModel.ExploreCollection = await App.SoundCloud.GetExplore();
                }
              else
              {
                  await new MessageDialog("There was a problem signing you in").ShowAsync();
              }
            }
            else
            {
                await new MessageDialog("You are already signed in").ShowAsync();
                //_mainPageViewModel.StreamCollection = await App.SoundCloud.GetStream();
                //_mainPageViewModel.ExploreCollection = await App.SoundCloud.GetExplore();
            }
        }

        private async void Navigation_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Page page = MyFrame?.Content as Page;
            Button b = sender as Button;
            switch (b?.Tag.ToString())
            {
                case "recent":
                    if (page?.GetType() != typeof(RecentPage))
                    {
                        MyFrame?.Navigate(typeof(RecentPage));
                    }
                    break;
                case "artist":
                    if (page?.GetType() != typeof(ArtistPage))
                    {
                        MyFrame?.Navigate(typeof(ArtistPage));
                    }
                    break;
                case "genre":
                    if (page?.GetType() != typeof(GenrePage))
                    {
                        MyFrame?.Navigate(typeof(GenrePage));
                    }
                    break;
                case "following":
                    if (page?.GetType() != typeof(FollowingPage))
                    {
                        MyFrame?.Navigate(typeof(FollowingPage));
                    }
                    break;
                case "followers":
                    if (page?.GetType() != typeof(FollowerPage))
                    {
                        MyFrame?.Navigate(typeof(FollowerPage));
                    }
                    break;
                case "playlist":
                    if (page?.GetType() != typeof(PlaylistPage))
                    {
                        MyFrame?.Navigate(typeof(PlaylistPage));
                    }
                    break;
                case "like":
                    if (page?.GetType() != typeof(LikePage))
                    {
                        MyFrame?.Navigate(typeof(LikePage));
                    }
                    break;
                case "profile":
                    if (page?.GetType() != typeof(ProfilePage))
                    {
                        MyFrame?.Navigate(typeof(ProfilePage));
                    }
                    break;
                case "activity":
                    if (page?.GetType() != typeof(ActivityPage))
                    {
                        loadActivityPageResources();
                        MyFrame?.Navigate(typeof(ActivityPage));
                    }
                    break;
                case "setting":
                    if (page?.GetType() != typeof(SettingPage))
                    {
                        MyFrame?.Navigate(typeof(SettingPage));
                    }
                    break;
            }
        }

        #region Activity

        private async void loadActivityPageResources()
        {
            if (!App.SoundCloud.IsAuthenticated)
            {
                if (await App.SoundCloud.SignIn())
                {
                    getActivities();
                }
                else
                {
                    await new MessageDialog("There was a problem while getting some information.").ShowAsync();
                }
            }
            else
            {
                getActivities();
            }
        }

        private async void getActivities()
        {
            Activity a;
            try
            {
                a = await App.SoundCloud.GetActivities();
                _mainPageViewModel.ActivityObject = a;
            }
            catch (Exception)
            {
                return;
            }

            
            //_mainPageViewModel.ActivityCollection;
            int l = a.Collection.Count;
            ObservableCollection<Collection> c = new ObservableCollection<Collection>();
            for (int i = 0; i < l; i++)
            {
                c.Add(a.Collection[i]);
            }
            Debug.WriteLine(c);
            _mainPageViewModel.ActivityCollection = c;
        }
        #endregion

    }
}