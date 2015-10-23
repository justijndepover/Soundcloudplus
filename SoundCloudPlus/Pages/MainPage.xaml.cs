using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.UI.Xaml;
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
            //MusicPlayer.Source =
                //new Uri(@"https://cf-media.sndcdn.com/G0ddTXDYRX48.128.mp3?Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiKjovL2NmLW1lZGlhLnNuZGNkbi5jb20vRzBkZFRYRFlSWDQ4LjEyOC5tcDMiLCJDb25kaXRpb24iOnsiRGF0ZUxlc3NUaGFuIjp7IkFXUzpFcG9jaFRpbWUiOjE0NDU1NDczNzF9fX1dfQ__&Signature=h5iThUINJUr3cYqfVDT41KAaW9tlX4Gb0EHHDHCxxjHoPyDc8-KSB0XgDzQIESvA2lZhPpjnHYNrVad6XLqmkHv9MU6K3sf6rrLwF3MUWuhVMOoOSAxg777f5TPYkoO7yWhPqGjjZrxDWuCdTljzmdKtLvDGDucr-xqHwZe3VbVzgvld2xIYwaNB8ghZOZSAiq4gkjPoFDmDzEhCtfkbVqM8ryziO5ifI87alSGLXeHIrsV5oWzWwhEyV~zmDs9kdXPiAoL1CuLCNV8ISjJJ9LcuvCwd0xcKBes4Iit5G76c3X7bBD6ATc~z2Xgcy~YQzyNPfoiduoelYCa6ePtAIg__&Key-Pair-Id=APKAJAGZ7VMH2PFPW6UQ");
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

        private void Navigation_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
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
                        LoadActivityPageResources();
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

            if (this.Width < 720 && SplitViewMenu.IsPaneOpen == true)
            {
                SplitViewMenu.IsPaneOpen = false;
            }
        }

        #region Activity

        private async void LoadActivityPageResources()
        {
            if (!App.SoundCloud.IsAuthenticated)
            {
                if (await App.SoundCloud.SignIn())
                {
                    GetActivities();
                }
                else
                {
                    await new MessageDialog("There was a problem while getting some information.").ShowAsync();
                }
            }
            else
            {
                GetActivities();
            }
        }

        private async void GetActivities()
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

        private void MyFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
             var content = MyFrame.Content as FrameworkElement;
            if (content == null)
                return;
            content.DataContext = MyFrame.DataContext;
        }
    }
}