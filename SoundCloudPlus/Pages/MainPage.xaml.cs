﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
        private MainPageViewModel _mainPageViewModel;
        private AutoResetEvent backgroundAudioTaskStarted;
        private bool isMyBackgroundTaskRunning = false;
        private Dictionary<string, BitmapImage> albumArtCache = new Dictionary<string, BitmapImage>();
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
        public List<Track> PlayList { get; set; }

        /// <summary>
        /// Gets the information about background task is running or not by reading the setting saved by background task.
        /// This is used to determine when to start the task and also when to avoid sending messages.
        /// </summary>
        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                if (isMyBackgroundTaskRunning)
                    return true;

                string value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.BackgroundTaskState) as string;
                if (value == null)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        isMyBackgroundTaskRunning = EnumHelper.Parse<BackgroundTaskState>(value) == BackgroundTaskState.Running;
                    }
                    catch (ArgumentException)
                    {
                        isMyBackgroundTaskRunning = false;
                    }
                    return isMyBackgroundTaskRunning;
                }
            }
        }
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            Current = this;
            backgroundAudioTaskStarted = new AutoResetEvent(false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Application.Current.Suspending += ForegroundApp_Suspending;
            Application.Current.Resuming += ForegroundApp_Resuming;
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());
            if (e.NavigationMode != NavigationMode.Back)
            {
                MyFrame.Navigate(typeof(HomePage));
                _mainPageViewModel =
                    (MainPageViewModel)Resources["MainPageViewModel"];
            }
            base.OnNavigatedTo(e);
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            SplitViewMenu.IsPaneOpen = !SplitViewMenu.IsPaneOpen;
        }

        private async void AccountButton_Click(object sender, RoutedEventArgs e)
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

        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            Page page = MyFrame?.Content as Page;
            Button b = sender as Button;
            switch (b?.Tag.ToString())
            {
                case "home":
                    if (page?.GetType() != typeof(HomePage))
                    {
                        MyFrame?.Navigate(typeof(HomePage));
                    }
                    break;
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

        /// <summary>
        /// You should never cache the MediaPlayer and always call Current. It is possible
        /// for the background task to go away for several different reasons. When it does
        /// an RPC_S_SERVER_UNAVAILABLE error is thrown. We need to reset the foreground state
        /// and restart the background task.
        /// </summary>
        private MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;

                try
                {
                    mp = BackgroundMediaPlayer.Current;
                }
                catch (Exception ex)
                {
                    if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                    {
                        // The foreground app uses RPC to communicate with the background process.
                        // If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE
                        // is returned when calling Current.
                        ResetAfterLostBackground();
                        StartBackgroundAudioTask();
                    }
                }

                if (mp == null)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }

                return mp;
            }
        }

        /// <summary>
        /// The background task did exist, but it has disappeared. Put the foreground back into an initial state.
        /// </summary>
        private void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            isMyBackgroundTaskRunning = false;
            backgroundAudioTaskStarted.Reset();
            //prevButton.IsEnabled = true;
            //nextButton.IsEnabled = true;
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Unknown.ToString());
            //playButton.Content = "| |";

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }
            }
        }
        /*

        public Scenario1()
        {
            this.InitializeComponent();

            // Always use the cached page
            this.NavigationCacheMode = NavigationCacheMode.Required;

            // Create a static song list
            InitializeSongs();

            // Setup the initialization lock
            backgroundAudioTaskStarted = new AutoResetEvent(false);
        }

        void InitializeSongs()
        {
            // Album art attribution
            // Ring01.jpg      | Autumn Yellow Leaves           | George Hodan
            // Ring02.jpg      | Abstract Background            | Larisa Koshkina
            // Ring03Part1.jpg | Snow Covered Mountains         | Petr Kratochvil
            // Ring03Part2.jpg | Tropical Beach With Palm Trees | Petr Kratochvil
            // Ring03Part3.jpg | Alyssum Background             | Anne Lowe

            // Initialize the playlist data/view model.
            // In a production app your data would be sourced from a data store or service.

            // Add complete tracks
            var song1 = new SongModel();
            song1.Title = "Ring 1";
            song1.MediaUri = new Uri("ms-appx:///Assets/Media/Ring01.wma");
            song1.AlbumArtUri = new Uri("ms-appx:///Assets/Media/Ring01.jpg");
            playlistView.Songs.Add(song1);

            var song2 = new SongModel();
            song2.Title = "Ring 2";
            song2.MediaUri = new Uri("ms-appx:///Assets/Media/Ring02.wma");
            song2.AlbumArtUri = new Uri("ms-appx:///Assets/Media/Ring02.jpg");
            playlistView.Songs.Add(song2);

            // Add gapless
            for (int i = 1; i <= 3; ++i)
            {
                var segment = new SongModel();
                segment.Title = "Ring 3 Part " + i;
                segment.MediaUri = new Uri("ms-appx:///Assets/Media/Ring03Part" + i + ".wma");
                segment.AlbumArtUri = new Uri("ms-appx:///Assets/Media/Ring03Part" + i + ".jpg");
                playlistView.Songs.Add(segment);
            }

            // Pre-cache all album art to facilitate smooth gapless transitions.
            // A production app would have a more sophisticated object cache.
            foreach (var song in playlistView.Songs)
            {
                var bitmap = new BitmapImage();
                bitmap.UriSource = song.AlbumArtUri;
                albumArtCache[song.AlbumArtUri.ToString()] = bitmap;
            }
        }*/

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (isMyBackgroundTaskRunning)
            {
                RemoveMediaPlayerEventHandlers();
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());
            }

            base.OnNavigatedFrom(e);
        }

        #region Foreground App Lifecycle Handlers
        /// <summary>
        /// Read persisted current track information from application settings
        /// </summary>
        private Uri GetCurrentTrackIdAfterAppResume()
        {
            object value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.TrackId);
            if (value != null)
                return new Uri((String)value);
            else
                return null;
        }

        /// <summary>
        /// Sends message to background informing app has resumed
        /// Subscribe to MediaPlayer events
        /// </summary>
        void ForegroundApp_Resuming(object sender, object e)
        {
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());

            // Verify the task is running
            if (IsMyBackgroundTaskRunning)
            {
                // If yes, it's safe to reconnect to media play handlers
                AddMediaPlayerEventHandlers();

                // Send message to background task that app is resumed so it can start sending notifications again
                MessageService.SendMessageToBackground(new AppResumedMessage());

                UpdateTransportControls(CurrentPlayer.CurrentState);

                var trackId = GetCurrentTrackIdAfterAppResume();
                //txtCurrentTrack.Text = trackId == null ? string.Empty : playlistView.GetSongById(trackId).Title;
                //txtCurrentState.Text = CurrentPlayer.CurrentState.ToString();
            }
            else
            {
                //playButton.Content = ">";     // Change to play button
                //txtCurrentTrack.Text = string.Empty;
                //txtCurrentState.Text = "Background Task Not Running";
            }
        }

        /// <summary>
        /// Send message to Background process that app is to be suspended
        /// Stop clock and slider when suspending
        /// Unsubscribe handlers for MediaPlayer events
        /// </summary>
        void ForegroundApp_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // Only if the background task is already running would we do these, otherwise
            // it would trigger starting up the background task when trying to suspend.
            if (IsMyBackgroundTaskRunning)
            {
                // Stop handling player events immediately
                RemoveMediaPlayerEventHandlers();

                // Tell the background task the foreground is suspended
                MessageService.SendMessageToBackground(new AppSuspendedMessage());
            }

            // Persist that the foreground app is suspended
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Suspended.ToString());

            deferral.Complete();
        }
        #endregion

        #region Background MediaPlayer Event handlers
        /// <summary>
        /// MediaPlayer state changed event handlers. 
        /// Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Update state label
                //txtCurrentState.Text = currentState.ToString();

                // Update controls
                UpdateTransportControls(currentState);
            });
        }

        /// <summary>
        /// This event is raised when a message is recieved from BackgroundAudioTask
        /// </summary>
        async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                // When foreground app is active change track based on background message
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // If playback stopped then clear the UI
                    if (!trackChangedMessage.TrackId.HasValue)
                    {
                        //playlistView.SelectedIndex = -1;
                        //albumArt.Source = null;
                        //txtCurrentTrack.Text = string.Empty;
                        //prevButton.IsEnabled = false;
                        //nextButton.IsEnabled = false;
                        return;
                    }

                    //var songIndex = playlistView.GetSongIndexById(trackChangedMessage.TrackId);
                    var song = PlayList.Where(track =>
                    {
                        Debug.Assert(track.Id != null, "track.Id != null");
                        return track.Id.Value.Equals(trackChangedMessage.TrackId.Value);
                    });

                    // Update list UI
                    //playlistView.SelectedIndex = songIndex;

                    // Update the album art
                    //albumArt.Source = albumArtCache[song.AlbumArtUri.ToString()];

                    // Update song title
                    //txtCurrentTrack.Text = song.Title;

                    // Ensure track buttons are re-enabled since they are disabled when pressed
                    //prevButton.IsEnabled = true;
                    //nextButton.IsEnabled = true;
                });
                return;
            }

            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                // and ready to receive messages
                Debug.WriteLine("BackgroundAudioTask started");
                backgroundAudioTaskStarted.Set();
                return;
            }
        }

        #endregion

        public void PlayTrack(Track track)
        {
            var song = track;
            if (PlayList == null)
            {
                PlayList = new List<Track>();
            }
            if (!PlayList.Contains(track))
            {
                PlayList.Add(track);
            }
            Debug.WriteLine("Clicked item from App: " + song.Id.ToString());

            // Start the background task if it wasn't running
            if (!IsMyBackgroundTaskRunning || MediaPlayerState.Closed == CurrentPlayer.CurrentState)
            {
                // First update the persisted start track
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, song.Id);
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, new TimeSpan().ToString());

                // Start task
                StartBackgroundAudioTask();
            }
            else
            {
                // Switch to the selected track
                MessageService.SendMessageToBackground(new TrackChangedMessage(song.Id));
            }

            if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
            {
                CurrentPlayer.Play();
            }
        }
        #region Button and Control Click Event Handlers

        /// <summary>
        /// Sends message to the background task to skip to the previous track.
        /// </summary>
        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new SkipPreviousMessage());

            // Prevent the user from repeatedly pressing the button and causing 
            // a backlong of button presses to be handled. This button is re-eneabled 
            // in the TrackReady Playstate handler.
            //prevButton.IsEnabled = false;
        }

        /// <summary>
        /// If the task is already running, it will just play/pause MediaPlayer Instance
        /// Otherwise, initializes MediaPlayer Handlers and starts playback
        /// track or to pause if we're already playing.
        /// </summary>
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Play button pressed from App");
            if (IsMyBackgroundTaskRunning)
            {
                if (MediaPlayerState.Playing == CurrentPlayer.CurrentState)
                {
                    CurrentPlayer.Pause();
                }
                else if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
                {
                    CurrentPlayer.Play();
                }
                else if (MediaPlayerState.Closed == CurrentPlayer.CurrentState)
                {
                    StartBackgroundAudioTask();
                }
            }
            else
            {
                StartBackgroundAudioTask();
            }
        }

        /// <summary>
        /// Tells the background audio agent to skip to the next track.
        /// </summary>
        /// <param name="sender">The button</param>
        /// <param name="e">Click event args</param>
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new SkipNextMessage());

            // Prevent the user from repeatedly pressing the button and causing 
            // a backlong of button presses to be handled. This button is re-eneabled 
            // in the TrackReady Playstate handler.
            //nextButton.IsEnabled = false;
        }

        private void speedButton_Click(object sender, RoutedEventArgs e)
        {
            // Create menu and add commands
            var popupMenu = new PopupMenu();

            popupMenu.Commands.Add(new UICommand("4.0x", command => CurrentPlayer.PlaybackRate = 4.0));
            popupMenu.Commands.Add(new UICommand("2.0x", command => CurrentPlayer.PlaybackRate = 2.0));
            popupMenu.Commands.Add(new UICommand("1.5x", command => CurrentPlayer.PlaybackRate = 1.5));
            popupMenu.Commands.Add(new UICommand("1.0x", command => CurrentPlayer.PlaybackRate = 1.0));
            popupMenu.Commands.Add(new UICommand("0.5x", command => CurrentPlayer.PlaybackRate = 0.5));

            // Get button transform and then offset it by half the button
            // width to center. This will show the popup just above the button.
            var button = (Button)sender;
            var transform = button.TransformToVisual(null);
            var point = transform.TransformPoint(new Point(button.Width / 2, 0));

            // Show popup
            var ignoreAsyncResult = popupMenu.ShowAsync(point);

        }
        #endregion Button Click Event Handlers

        #region Media Playback Helper methods
        private void UpdateTransportControls(MediaPlayerState state)
        {
            if (state == MediaPlayerState.Playing)
            {
                //playButton.Content = "| |";     // Change to pause button
            }
            else
            {
                //playButton.Content = ">";     // Change to play button
            }
        }

        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void RemoveMediaPlayerEventHandlers()
        {
            CurrentPlayer.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // do nothing
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        private void AddMediaPlayerEventHandlers()
        {
            CurrentPlayer.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // Internally MessageReceivedFromBackground calls Current which can throw RPC_S_SERVER_UNAVAILABLE
                    ResetAfterLostBackground();
                }
            }
        }

        /// <summary>
        /// Initialize Background Media Player Handlers and starts playback
        /// </summary>
        private void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            var startResult = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool result = backgroundAudioTaskStarted.WaitOne(10000);
                //Send message to initiate playback
                if (result == true)
                {
                    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(PlayList));
                    MessageService.SendMessageToBackground(new StartPlaybackMessage());
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            });
            startResult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
            {
                Debug.WriteLine("Background Audio Task initialized");
            }
            else if (status == AsyncStatus.Error)
            {
                Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
            }
        }
        #endregion
    }
}