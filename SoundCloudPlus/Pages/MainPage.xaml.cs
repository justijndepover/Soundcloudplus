using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using ClassLibrary.Messages;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;

namespace SoundCloudPlus.Pages
{

    public sealed partial class MainPage
    {
        public static MainPage Current;
        public MainPageViewModel MainPageViewModel;
        public string PageTitle;
        readonly DispatcherTimer _playbackTimer = new DispatcherTimer();
        public Frame MainFrame;

        public int UserId { get; set; }

        public List<int> UserIdHistory { get; set; }
        public Playlist CurrentPlaylist { get; set; }

        public MainPage()
        {
            InitializeComponent();
            LoadTheme();
            Current = this;
            NavigationCacheMode = NavigationCacheMode.Required;
            MainFrame = MyFrame;
            UserIdHistory = new List<int>();
            _playbackTimer.Interval = TimeSpan.FromMilliseconds(250);
            _playbackTimer.Tick += _playbackTimer_Tick;
        }

        private void LoadUserAvatar()
        {
            User s = App.SoundCloud.CurrentUser;
            try
            {
                MainPageViewModel.LoggedInUser = s;
            }
            catch (Exception ex)
            {
                new ErrorLogProxy(ex.ToString());
            }
        }

        private void LoadTheme()
        {
            try
            {
                App.RootFrame.RequestedTheme = (ElementTheme)ApplicationSettingsHelper.ReadRoamingSettingsValue<ElementTheme>("ElementTheme");
            }
            catch (Exception)
            {
                App.RootFrame.RequestedTheme = ElementTheme.Default;
                ApplicationSettingsHelper.SaveRoamingSettingsValue("ElementTheme", ElementTheme.Default);
            }
        }

        private void _playbackTimer_Tick(object sender, object e)
        {
            var position = App.AudioPlayer.CurrentPlayer.Position;
            PlayerPosition.Text = position.Minutes.ToString("00") + ":" + position.Seconds.ToString("00");
            PlayerProgressBar.Maximum = App.AudioPlayer.CurrentPlayer.NaturalDuration.TotalMilliseconds;
            try
            {
                PlayerProgressBar.Value = position.TotalMilliseconds;
                //PlayerProgressBar.Value = (position.TotalMilliseconds - 0) / (App.SoundCloud.AudioPlayer.CurrentPlayer.NaturalDuration.TotalMilliseconds - 0) * (100 - 0) + 0;
            }
            catch (Exception ex)
            {
                new ErrorLogProxy(ex.ToString());
                Debug.WriteLine("Error: Progressbar value is NaN");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Application.Current.Suspending += ForegroundApp_Suspending;
            Application.Current.Resuming += ForegroundApp_Resuming;

            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState,
                AppState.Active.ToString());

            if (e.NavigationMode != NavigationMode.Back)
            {
                try
                {
                    MyFrame.Navigate(typeof(HomePage));
                    MainPageViewModel =
                        (MainPageViewModel)Resources["MainPageViewModel"];
                    MusicPlayerControl.DataContext = App.AudioPlayer;
                    MainPageViewModel.PageTitle = "Home";
                    //App.AudioPlayer.CurrentPlayer.CurrentStateChanged += CurrentPlayer_CurrentStateChanged;
                    LoadUserAvatar();
                }
                catch (Exception ex)
                {
                    new ErrorLogProxy(ex.ToString());
                }
            }
            base.OnNavigatedTo(e);
        }
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.IsPaneOpen = !SplitViewMenu.IsPaneOpen;
            if (SplitViewMenu.IsPaneOpen)
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

        public void Navigate(object sender, string destination)
        {
            Page page = MyFrame?.Content as Page;
            Button b = sender as Button;
            switch (destination)
            {
                case "home":
                    if (page?.GetType() != typeof(HomePage))
                    {
                        MainPageViewModel.PageTitle = "Home";
                        MyFrame?.Navigate(typeof(HomePage));
                    }
                    break;
                case "profile":
                    if (page?.GetType() != typeof(ProfilePage))
                    {
                        MainPageViewModel.PageTitle = "Profile";
                        if (b?.Tag != null)
                        {
                            int id = (int)b.Tag;
                            UserId = id;
                            MyFrame?.Navigate(typeof(ProfilePage));
                            UserIdHistory.Add(id);
                        }
                    }
                    break;
                case "followers":
                    if (page?.GetType() != typeof(FollowerPage))
                    {
                        MainPageViewModel.PageTitle = "Followers";
                        if (b?.Tag != null)
                        {
                            int id = (int)b.Tag;
                            UserId = id;
                            MyFrame?.Navigate(typeof(FollowerPage));
                            UserIdHistory.Add(id);
                        }
                    }
                    break;
                case "following":
                    if (page?.GetType() != typeof(FollowingPage))
                    {
                        MainPageViewModel.PageTitle = "Following";
                        if (b?.Tag != null)
                        {
                            int id = (int)b.Tag;
                            UserId = id;
                            MyFrame?.Navigate(typeof(FollowingPage));
                            UserIdHistory.Add(id);
                        }
                    }
                    break;
                case "playlistview":
                    if (page?.GetType() != typeof(PlaylistViewPage))
                    {
                        MainPageViewModel.PageTitle = "Playlist";
                        MyFrame?.Navigate(typeof(PlaylistViewPage));
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
                        MainPageViewModel.PageTitle = "Home";
                        MyFrame?.Navigate(typeof(HomePage));
                    }
                    break;
                case "following":
                    if (page?.GetType() != typeof(FollowingPage))
                    {
                        MainPageViewModel.PageTitle = "Following";
                        int id = App.SoundCloud.CurrentUser.Id;
                        UserId = id;
                        MyFrame?.Navigate(typeof(FollowingPage));
                        UserIdHistory.Add(id);
                    }
                    break;
                case "followers":
                    if (page?.GetType() != typeof(FollowerPage))
                    {
                        MainPageViewModel.PageTitle = "Followers";
                        int id = App.SoundCloud.CurrentUser.Id;
                        UserId = id;
                        MyFrame?.Navigate(typeof(FollowerPage));
                        UserIdHistory.Add(id);
                    }
                    break;
                case "playlist":
                    if (page?.GetType() != typeof(PlaylistPage))
                    {
                        MainPageViewModel.PageTitle = "Playlist";
                        MyFrame?.Navigate(typeof(PlaylistPage));
                    }
                    break;
                case "like":
                    if (page?.GetType() != typeof(LikePage))
                    {
                        MainPageViewModel.PageTitle = "Like";
                        MyFrame?.Navigate(typeof(LikePage));
                    }
                    break;
                case "profile":
                    if (true)
                    {
                        MainPageViewModel.PageTitle = "Profile";
                        int id = App.SoundCloud.CurrentUser.Id;
                        UserId = id;
                        MyFrame?.Navigate(typeof(ProfilePage));
                        UserIdHistory.Add(id);
                    }
                    break;
                case "activity":
                    if (page?.GetType() != typeof(ActivityPage))
                    {
                        MainPageViewModel.PageTitle = "Activity";
                        LoadActivityPageResources();
                        MyFrame?.Navigate(typeof(ActivityPage));
                    }
                    break;
                case "setting":
                    if (page?.GetType() != typeof(SettingPage))
                    {
                        MainPageViewModel.PageTitle = "Setting";
                        MyFrame?.Navigate(typeof(SettingPage));
                    }
                    break;
                case "currentsong":
                    if (page?.GetType() != typeof(PlayingPage))
                    {
                        MainPageViewModel.PageTitle = "Now playing";
                        MyFrame?.Navigate(typeof(PlayingPage));
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
            if (!await App.SoundCloud.IsAuthenticated())
            {
                if (await App.SoundCloud.SignIn())
                {
                    UpdateActivityCollection();
                }
                else
                {
                    await new MessageDialog("There was a problem while getting some information.").ShowAsync();
                }
            }
            else
            {
                UpdateActivityCollection();
            }
        }

        private void UpdateActivityCollection()
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                int limit = Screen.GetLimitItems(height, width, 400, 800, 200, 400);
                GetActivities(limit);
            }
            catch (Exception ex)
            {
                new ErrorLogProxy(ex.ToString());
                Application.Current.Exit();
            }
        }

        private async void GetActivities(int limit)
        {
            Activity a;
            try
            {
                a = await App.SoundCloud.GetActivities(limit);
                MainPageViewModel.ActivityObject = a;
            }
            catch (Exception ex)
            {
                new ErrorLogProxy(ex.ToString());
                return;
            }


            //_mainPageViewModel.ActivityCollection;
            int l = a.Collection.Count;
            ObservableCollection<ActivityCollection> c = new ObservableCollection<ActivityCollection>();
            for (int i = 0; i < l; i++)
            {
                c.Add(a.Collection[i]);
            }
            MainPageViewModel.ActivityCollection = c;
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (App.AudioPlayer.IsMyBackgroundTaskRunning)
            {
                App.AudioPlayer.RemoveMediaPlayerEventHandlers();
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());
            }

            base.OnNavigatedFrom(e);
        }

        #region Foreground App Lifecycle Handlers
        async void ForegroundApp_Resuming(object sender, object e)
        {
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());

            // Verify the task is running
            if (App.AudioPlayer.IsMyBackgroundTaskRunning)
            {
                // If yes, it's safe to reconnect to media play handlers
                App.AudioPlayer.AddMediaPlayerEventHandlers();
                // Send message to background task that app is resumed so it can start sending notifications again
                MessageService.SendMessageToBackground(new AppResumedMessage());

                UpdateTransportControls(App.AudioPlayer.CurrentPlayer.CurrentState);

                var trackId = App.AudioPlayer.GetCurrentTrackIdAfterAppResume();
                if (trackId != null)
                {
                    App.AudioPlayer.CurrentTrack = await App.AudioPlayer.GetTrackById(trackId);
                }
                MainPageViewModel.PlayingTrack = trackId == null ? new Track() : await App.AudioPlayer.GetTrackById(trackId);
            }
        }
        void ForegroundApp_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            if (App.AudioPlayer.IsMyBackgroundTaskRunning)
            {
                App.AudioPlayer.RemoveMediaPlayerEventHandlers();
                MessageService.SendMessageToBackground(new AppSuspendedMessage());
            }
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Suspended.ToString());

            deferral.Complete();
        }
        #endregion

        #region Button and Control Click Event Handlers

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new SkipPreviousMessage());
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Play button pressed from App");
            if (App.AudioPlayer.IsMyBackgroundTaskRunning)
            {
                if (MediaPlayerState.Playing == App.AudioPlayer.CurrentPlayer.CurrentState)
                {
                    App.AudioPlayer.CurrentPlayer.Pause();
                }
                else if (MediaPlayerState.Paused == App.AudioPlayer.CurrentPlayer.CurrentState)
                {
                    App.AudioPlayer.CurrentPlayer.Play();
                }
                else if (MediaPlayerState.Closed == App.AudioPlayer.CurrentPlayer.CurrentState)
                {
                    App.AudioPlayer.StartBackgroundAudioTask();
                }
            }
            else
            {
                App.AudioPlayer.StartBackgroundAudioTask();
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

        public void UpdateTransportControls(MediaPlayerState state)
        {
            MainPageViewModel.PlayingTrack = App.AudioPlayer.CurrentTrack;
            MainPageViewModel.PlayingList = App.AudioPlayer.PlayList;

            if (state == MediaPlayerState.Playing)
            {
                MusicPlayerControl.Visibility = Visibility.Visible;
                _playbackTimer.Start();
                Playbuttonicon.Glyph = "\ue769";
                //playbuttonicon.Glyph = "| |";     // Change to pause button
            }
            else
            {
                _playbackTimer.Stop();
                Playbuttonicon.Glyph = "\ue768";
                //playbuttonicon.Glyph = ">";     // Change to play button
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

        private void PlayerProgressBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.NewValue > (e.OldValue + 5000) || e.NewValue < (e.OldValue - 5000))
            {
                TimeSpan newPos = TimeSpan.FromMilliseconds(e.NewValue);
                App.AudioPlayer.CurrentPlayer.Position = newPos;
            }
        }
    }
}