using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using ClassLibrary.Messages;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;
using TilesAndNotifications.Services;

namespace SoundCloudPlus.Pages
{
    public sealed partial class MainPage
    {
        public static MainPage Current;
        private MainPageViewModel _mainPageViewModel;
        public List<Track> PlayList { get; set; }
        public Track CurrentTrack { get; set; }
        public string PageTitle;
        readonly DispatcherTimer _playbackTimer = new DispatcherTimer();

        public int UserId { get; set; }

        public List<int> UserIdHistory { get; set; }

        public MainPage()
        {
            InitializeComponent();
            Current = this;
            NavigationCacheMode = NavigationCacheMode.Required;
            UserIdHistory = new List<int>();
            _playbackTimer.Interval = TimeSpan.FromMilliseconds(250);
            _playbackTimer.Tick += _playbackTimer_Tick;
        }

        private void _playbackTimer_Tick(object sender, object e)
        {
            var position = App.SoundCloud.AudioPlayer.CurrentPlayer.Position;
            PlayerPosition.Text = position.Minutes + ":" + position.Seconds;
            try
            {
                PlayerProgressBar.Value = (position.TotalMilliseconds - 0) / (App.SoundCloud.AudioPlayer.CurrentPlayer.NaturalDuration.TotalMilliseconds - 0) * (100 - 0) + 0;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error: Progressbar value is NaN");
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Application.Current.Suspending += ForegroundApp_Suspending;
            Application.Current.Resuming += ForegroundApp_Resuming;
            await
                Enough.Storage.StorageHelper.SaveObjectAsync(AppState.Active.ToString(),
                    ApplicationSettingsConstants.AppState);
            if (e.NavigationMode != NavigationMode.Back)
            {
                MyFrame.Navigate(typeof(HomePage));
                _mainPageViewModel =
                    (MainPageViewModel)Resources["MainPageViewModel"];
                _mainPageViewModel.PageTitle = "Home";
                App.SoundCloud.AudioPlayer.CurrentPlayer.CurrentStateChanged += CurrentPlayer_CurrentStateChanged;
            }
            base.OnNavigatedTo(e);
        }

        private async void CurrentPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                UpdateTransportControls(currentState);
            });
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.IsPaneOpen = !SplitViewMenu.IsPaneOpen;
            if (SplitViewMenu.IsPaneOpen == true)
            {
                SearchBox.Visibility = Visibility.Visible;
                SearchButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchBox.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Visible;
            }
        }

        private async void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (!App.SoundCloud.IsAuthenticated)
            {
              if (await App.SoundCloud.SignIn())
              {
                  App.SoundCloud.IsAuthenticated = true;
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

        public void Navigate(object sender, string destination)
        {
            Page page = MyFrame?.Content as Page;
            Button b = sender as Button;
            switch (destination)
            {
                case "home":
                    if (page?.GetType() != typeof (HomePage))
                    {
                        _mainPageViewModel.PageTitle = "Home";
                        MyFrame?.Navigate(typeof (HomePage));
                    }
                    break;
                case "profile":
                    if (page?.GetType() != typeof (ProfilePage))
                    {
                        _mainPageViewModel.PageTitle = "Profile";
                        if (b?.Tag != null)
                        {
                            int id = (int)b.Tag;
                            UserId = id;
                            MyFrame?.Navigate(typeof (ProfilePage));
                            UserIdHistory.Add(id);
                        }
                    }
                    break;
            }
            if (ActualWidth < 720 && SplitViewMenu.IsPaneOpen)
            {
                SplitViewMenu.IsPaneOpen = false;
            }
        }

        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            Page page = MyFrame?.Content as Page;
            Button b = sender as Button;
            switch (b?.Tag.ToString())
            {
                case "home":
                    if (page?.GetType() != typeof(HomePage))
                    {
                        _mainPageViewModel.PageTitle = "Home";
                        MyFrame?.Navigate(typeof(HomePage));
                    }
                    break;
                case "recent":
                    if (page?.GetType() != typeof(RecentPage))
                    {
                        _mainPageViewModel.PageTitle = "Recent";
                        MyFrame?.Navigate(typeof(RecentPage));
                    }
                    break;
                case "artist":
                    if (page?.GetType() != typeof(ArtistPage))
                    {
                        _mainPageViewModel.PageTitle = "Artist";
                        MyFrame?.Navigate(typeof(ArtistPage));
                    }
                    break;
                case "genre":
                    if (page?.GetType() != typeof(GenrePage))
                    {
                        _mainPageViewModel.PageTitle = "Genre";
                        MyFrame?.Navigate(typeof(GenrePage));
                    }
                    break;
                case "following":
                    if (page?.GetType() != typeof(FollowingPage))
                    {
                        _mainPageViewModel.PageTitle = "Following";
                        MyFrame?.Navigate(typeof(FollowingPage));
                    }
                    break;
                case "followers":
                    if (page?.GetType() != typeof(FollowerPage))
                    {
                        _mainPageViewModel.PageTitle = "Followers";
                        MyFrame?.Navigate(typeof(FollowerPage));
                    }
                    break;
                case "playlist":
                    if (page?.GetType() != typeof(PlaylistPage))
                    {
                        _mainPageViewModel.PageTitle = "Playlist";
                        MyFrame?.Navigate(typeof(PlaylistPage));
                    }
                    break;
                case "like":
                    if (page?.GetType() != typeof(LikePage))
                    {
                        _mainPageViewModel.PageTitle = "Like";
                        MyFrame?.Navigate(typeof(LikePage));
                    }
                    break;
                case "profile":
                    if (true)
                    {
                        _mainPageViewModel.PageTitle = "Profile";
                        int id = App.SoundCloud.CurrentUser.Id;
                        UserId = id;
                        MyFrame?.Navigate(typeof(ProfilePage));
                        UserIdHistory.Add(id);
                    }
                    break;
                case "activity":
                    if (page?.GetType() != typeof(ActivityPage))
                    {
                        _mainPageViewModel.PageTitle = "Activity";
                        LoadActivityPageResources();
                        MyFrame?.Navigate(typeof(ActivityPage));
                    }
                    break;
                case "setting":
                    if (page?.GetType() != typeof(SettingPage))
                    {
                        _mainPageViewModel.PageTitle = "Setting";
                        MyFrame?.Navigate(typeof(SettingPage));
                    }
                    break;
            }

            if (ActualWidth < 720 && SplitViewMenu.IsPaneOpen)
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
            ObservableCollection<ActivityCollection> c = new ObservableCollection<ActivityCollection>();
            for (int i = 0; i < l; i++)
            {
                c.Add(a.Collection[i]);
            }
            _mainPageViewModel.ActivityCollection = c;
        }
        #endregion

        #region Likes

        /*private async void LoadLikePageResources()
        {
            getLikes(178017941);
        }

        private async void getLikes(int userId)
        {
            LikeViewModel _likePageViewModel = (LikeViewModel)Resources["LikeViewModel"];
            _likePageViewModel.TrackLikesCollection = await App.SoundCloud.GetLikes(userId);
        }*/

        #endregion
        private void MyFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
             var content = MyFrame.Content as FrameworkElement;
            if (content == null)
                return;
            content.DataContext = MyFrame.DataContext;
        }
        
        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (App.SoundCloud.AudioPlayer.IsMyBackgroundTaskRunning)
            {
                await Enough.Storage.StorageHelper.SaveObjectAsync(BackgroundTaskState.Running.ToString(), ApplicationSettingsConstants.BackgroundTaskState);
            }

            base.OnNavigatedFrom(e);
        }

        #region Foreground App Lifecycle Handlers
        async void ForegroundApp_Resuming(object sender, object e)
        {
            await Enough.Storage.StorageHelper.SaveObjectAsync(AppState.Active.ToString(), ApplicationSettingsConstants.AppState);

            // Verify the task is running
            if (App.SoundCloud.AudioPlayer.IsMyBackgroundTaskRunning)
            {
                // If yes, it's safe to reconnect to media play handlers
                App.SoundCloud.AudioPlayer.AddMediaPlayerEventHandlers();

                // Send message to background task that app is resumed so it can start sending notifications again
                MessageService.SendMessageToBackground(new AppResumedMessage());

                UpdateTransportControls(App.SoundCloud.AudioPlayer.CurrentPlayer.CurrentState);

                //var trackId = GetCurrentTrackIdAfterAppResume();
                //txtCurrentTrack.Text = trackId == null ? string.Empty : playlistView.GetSongById(trackId).Title;
                //txtCurrentState.Text = CurrentPlayer.CurrentState.ToString();
            }
        }
        async void ForegroundApp_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            if (App.SoundCloud.AudioPlayer.IsMyBackgroundTaskRunning)
            {
                App.SoundCloud.AudioPlayer.RemoveMediaPlayerEventHandlers();
                MessageService.SendMessageToBackground(new AppSuspendedMessage());
            }
            await Enough.Storage.StorageHelper.SaveObjectAsync(AppState.Suspended.ToString(), ApplicationSettingsConstants.AppState);

            deferral.Complete();
        }
        #endregion


        void UpdateLiveTile(Track t)
        {
            try
            {
                var xmlDoc = TileService.CreateTiles(t);
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                TileNotification notification = new TileNotification(xmlDoc); updater.Update(notification);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        

        #region Button and Control Click Event Handlers
        
        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new SkipPreviousMessage());
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Play button pressed from App");
            if (App.SoundCloud.AudioPlayer.IsMyBackgroundTaskRunning)
            {
                if (MediaPlayerState.Playing == App.SoundCloud.AudioPlayer.CurrentPlayer.CurrentState)
                {
                    App.SoundCloud.AudioPlayer.CurrentPlayer.Pause();
                }
                else if (MediaPlayerState.Paused == App.SoundCloud.AudioPlayer.CurrentPlayer.CurrentState)
                {
                    App.SoundCloud.AudioPlayer.CurrentPlayer.Play();
                }
                else if (MediaPlayerState.Closed == App.SoundCloud.AudioPlayer.CurrentPlayer.CurrentState)
                {
                    App.SoundCloud.AudioPlayer.StartBackgroundAudioTask();
                }
            }
            else
            {
                App.SoundCloud.AudioPlayer.StartBackgroundAudioTask();
            }
        }
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new SkipNextMessage());

            // Prevent the user from repeatedly pressing the button and causing 
            // a backlong of button presses to be handled. This button is re-eneabled 
            // in the TrackReady Playstate handler.
            //nextButton.IsEnabled = false;
        }

        private void UpdateTransportControls(MediaPlayerState state)
        {
            if (state == MediaPlayerState.Playing)
            {
                MusicPlayerControl.Visibility = Visibility.Visible;
                _playbackTimer.Start();
                playbuttonicon.Glyph = "\ue769";
                playbuttonicon.Glyph = "| |";     // Change to pause button
            }
            else
            {
                _playbackTimer.Stop();
                playbuttonicon.Glyph = "\ue768";
                playbuttonicon.Glyph = ">";     // Change to play button
            }
        }
        #endregion Button Click Event Handlers

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            MyFrame?.Navigate(typeof(SearchPage), args.QueryText);
        }

        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string value = SearchBox.Text;
                SearchBox.ItemsSource = await App.SoundCloud.AutoSuggest(value);
                SearchBox.IsSuggestionListOpen = true;
            }
        }

        private void SplitViewMenu_PaneClosed(SplitView sender, object args)
        {
            SearchBox.Visibility = Visibility.Collapsed;
            SearchButton.Visibility = Visibility.Visible;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.IsPaneOpen = true;
            SearchBox.Visibility = Visibility.Visible;
            SearchButton.Visibility = Visibility.Collapsed;
            SearchBox.Focus(FocusState.Keyboard);
        }
    }
}