using System;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Media.Core;
using System.Collections.Generic;
using System.Net.Http;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using ClassLibrary.API;
using ClassLibrary.Common;
using ClassLibrary.Messages;
using ClassLibrary.Models;
using Newtonsoft.Json;

namespace BackgroundTasks
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        #region Private fields, properties

        private const string TrackIdKey = "trackid";
        private const string TitleKey = "title";
        private const string AlbumArtKey = "albumart";

        private SystemMediaTransportControls _smtc;
        private MediaPlaybackList _playbackList = new MediaPlaybackList();
        private BackgroundTaskDeferral _deferral; // Used to keep task alive
        private AppState _foregroundAppState = AppState.Unknown;
        private readonly ManualResetEvent _backgroundTaskStarted = new ManualResetEvent(false);
        private bool _playbackStartedPreviously;
        private const string ClientId = "776ca412db7b101b1602c6a67b1a0579";

        #endregion

        #region Helper methods
        int GetCurrentTrackId()
        {
            if (_playbackList == null)
                return 0;

            return GetTrackId(_playbackList.CurrentItem);
        }
        int GetTrackId(MediaPlaybackItem item)
        {
            if (item == null)
                return 0; // no track playing

            return (int)item.Source.CustomProperties[TrackIdKey];
        }
        #endregion

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                Debug.WriteLine("Background Audio Task " + taskInstance.Task.Name + " starting...");
                // Initialize SystemMediaTransportControls (SMTC) for integration with
                // the Universal Volume Control (UVC).
                //
                // The UI for the UVC must update even when the foreground process has been terminated
                // and therefore the SMTC is configured and updated from the background task.
                _smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
                _smtc.ButtonPressed += Smtc_ButtonPressed;
                _smtc.PropertyChanged += Smtc_PropertyChanged;
                _smtc.IsEnabled = true;
                _smtc.IsPauseEnabled = true;
                _smtc.IsPlayEnabled = true;
                _smtc.IsNextEnabled = true;
                _smtc.IsPreviousEnabled = true;

                var value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.AppState);
                _foregroundAppState = value == null ? AppState.Unknown : EnumHelper.Parse<AppState>(value.ToString());

                BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
                BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

                if (_foregroundAppState != AppState.Suspended)
                    MessageService.SendMessageToForeground(new BackgroundAudioTaskStartedMessage());

                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());

                _deferral = taskInstance.GetDeferral();
                _backgroundTaskStarted.Set();

                taskInstance.Task.Completed += Task_Completed;
                taskInstance.Canceled += TaskInstance_Canceled;
            }
            catch (Exception)
            {
                _deferral = taskInstance.GetDeferral();
                _backgroundTaskStarted.Set();

                taskInstance.Task.Completed += Task_Completed;
                taskInstance.Canceled += TaskInstance_Canceled;
            }
        }

        private void Task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("MyBackgroundAudioTask " + sender.TaskId + " Completed...");
            _deferral.Complete();
        }
        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Debug.WriteLine("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");
            try
            {
                // immediately set not running
                _backgroundTaskStarted.Reset();

                // save state
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, GetCurrentTrackId() == 0 ? null : GetCurrentTrackId().ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, BackgroundMediaPlayer.Current.Position.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Canceled.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, Enum.GetName(typeof(AppState), _foregroundAppState));

                // unsubscribe from list changes
                if (_playbackList != null)
                {
                    _playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
                    _playbackList = null;
                }

                // unsubscribe event handlers
                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                _smtc.ButtonPressed -= Smtc_ButtonPressed;
                _smtc.PropertyChanged -= Smtc_PropertyChanged;

                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            _deferral.Complete(); // signals task completion. 
            Debug.WriteLine("MyBackgroundAudioTask Cancel complete...");
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            AppSuspendedMessage appSuspendedMessage;
            if (MessageService.TryParseMessage(e.Data, out appSuspendedMessage))
            {
                Debug.WriteLine("App suspending"); // App is suspended, you can save your task state at this point
                _foregroundAppState = AppState.Suspended;
                var currentTrackId = GetCurrentTrackId();
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, currentTrackId == 0 ? null : currentTrackId.ToString());
                return;
            }

            AppResumedMessage appResumedMessage;
            if (MessageService.TryParseMessage(e.Data, out appResumedMessage))
            {
                Debug.WriteLine("App resuming"); // App is resumed, now subscribe to message channel
                _foregroundAppState = AppState.Active;
                return;
            }

            StartPlaybackMessage startPlaybackMessage;
            if (MessageService.TryParseMessage(e.Data, out startPlaybackMessage))
            {
                //Foreground App process has signalled that it is ready for playback
                Debug.WriteLine("Starting Playback");
                StartPlayback();
                return;
            }

            SkipNextMessage skipNextMessage;
            if (MessageService.TryParseMessage(e.Data, out skipNextMessage))
            {
                // User has chosen to skip track from app context.
                Debug.WriteLine("Skipping to next");
                SkipToNext();
                return;
            }

            SkipPreviousMessage skipPreviousMessage;
            if (MessageService.TryParseMessage(e.Data, out skipPreviousMessage))
            {
                // User has chosen to skip track from app context.
                Debug.WriteLine("Skipping to previous");
                SkipToPrevious();
                return;
            }

            TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                var index = _playbackList.Items.ToList().FindIndex(i => (int)i.Source.CustomProperties[TrackIdKey] == trackChangedMessage.TrackId);
                Debug.WriteLine("Skipping to track " + index);
                _smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                _playbackList.MoveTo((uint)index);
                return;
            }

            UpdatePlaylistMessage updatePlaylistMessage;
            if (MessageService.TryParseMessage(e.Data, out updatePlaylistMessage))
            {
                CreatePlaybackList(updatePlaylistMessage.PlayList);
            }
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                _smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
            else if (sender.CurrentState == MediaPlayerState.Closed)
            {
                _smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
            }
        }

        private void Smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // If soundlevel turns to muted, app can choose to pause the music
        }

        private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Debug.WriteLine("UVC play button pressed");

                    // When the background task has been suspended and the SMTC
                    // starts it again asynchronously, some time is needed to let
                    // the task startup process in Run() complete.

                    // Wait for task to start. 
                    // Once started, this stays signaled until shutdown so it won't wait
                    // again unless it needs to.
                    bool result = _backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                        throw new Exception("Background Task didnt initialize in time");

                    StartPlayback();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Debug.WriteLine("UVC pause button pressed");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Debug.WriteLine("UVC next button pressed");
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Debug.WriteLine("UVC previous button pressed");
                    SkipToPrevious();
                    break;
            }
        }
        private void UpdateUvcOnNewTrack(MediaPlaybackItem item)
        {
            try
            {
                if (item == null)
                {
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    _smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
                    _smtc.DisplayUpdater.Update();
                    return;
                }

                _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                _smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
                _smtc.DisplayUpdater.MusicProperties.Title = (string)item.Source.CustomProperties[TitleKey];

                var albumArtUri = item.Source.CustomProperties[AlbumArtKey] as Uri;
                _smtc.DisplayUpdater.Thumbnail = albumArtUri != null ? RandomAccessStreamReference.CreateFromUri(albumArtUri) : null;

                _smtc.DisplayUpdater.Update();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        private void StartPlayback()
        {
            try
            {
                // If playback was already started once we can just resume playing.
                if (!_playbackStartedPreviously)
                {
                    _playbackStartedPreviously = true;

                    // If the task was cancelled we would have saved the current track and its position. We will try playback from there.
                    int currentTrackId = (int)ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.TrackId);
                    var currentTrackPosition = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.Position);
                    if (currentTrackId != 0)
                    {
                        // Find the index of the item by name
                        var index = _playbackList.Items.ToList().FindIndex(item =>
                            GetTrackId(item) == currentTrackId);

                        if (currentTrackPosition == null)
                        {
                            // Play from start if we dont have position
                            Debug.WriteLine("StartPlayback: Switching to track " + index);
                            _playbackList.MoveTo((uint)index);

                            // Begin playing
                            BackgroundMediaPlayer.Current.Play();
                        }
                        else
                        {
                            // Play from exact position otherwise
                            TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler = null;
                            handler = (list, args) =>
                            {
                                if (args.NewItem == _playbackList.Items[index])
                                {
                                    // Unsubscribe because this only had to run once for this item
                                    _playbackList.CurrentItemChanged -= handler;

                                    // Set position
                                    var position = TimeSpan.Parse((string)currentTrackPosition);
                                    Debug.WriteLine("StartPlayback: Setting Position " + position);
                                    BackgroundMediaPlayer.Current.Position = position;

                                    // Begin playing
                                    BackgroundMediaPlayer.Current.Play();
                                }
                            };
                            _playbackList.CurrentItemChanged += handler;

                            // Switch to the track which will trigger an item changed event
                            Debug.WriteLine("StartPlayback: Switching to track " + index);
                            _playbackList.MoveTo((uint)index);
                        }
                    }
                    else
                    {
                        // Begin playing
                        BackgroundMediaPlayer.Current.Play();
                    }
                }
                else
                {
                    // Begin playing
                    BackgroundMediaPlayer.Current.Play();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Raised when playlist changes to a new track
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            try
            {
                // Get the new item
                var item = args.NewItem;
                Debug.WriteLine("PlaybackList_CurrentItemChanged: " + (item == null ? "null" : GetTrackId(item).ToString()));

                // Update the system view
                UpdateUvcOnNewTrack(item);

                // Get the current track
                int currentTrackId = 0;
                if (item != null)
                    currentTrackId = (int)item.Source.CustomProperties[TrackIdKey];

                // Notify foreground of change or persist for later
                if (_foregroundAppState == AppState.Active)
                    MessageService.SendMessageToForeground(new TrackChangedMessage(currentTrackId));
                else
                    ApplicationSettingsHelper.SaveSettingsValue(TrackIdKey, currentTrackId == 0 ? 0 : currentTrackId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToPrevious()
        {
            _smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            _playbackList.MovePrevious();
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToNext()
        {
            _smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            _playbackList.MoveNext();
        }

        void CreatePlaybackList(List<Track> playlist)
        {
            try
            {
                // Make a new list and enable looping
                _playbackList = new MediaPlaybackList { AutoRepeatEnabled = true };

                // Add playback items to the list
                foreach (Track track in playlist)
                {
                    if (track.Id.HasValue)
                    {
                        var source = MediaSource.CreateFromUri(GetMusicFile(track.Id.Value));
                        source.CustomProperties[TrackIdKey] = track.Id.Value;
                        source.CustomProperties[TitleKey] = track.Title;
                        source.CustomProperties[AlbumArtKey] = track.ArtworkUrl;
                        _playbackList.Items.Add(new MediaPlaybackItem(source));
                    }
                }

                // Don't auto start
                BackgroundMediaPlayer.Current.AutoPlay = false;

                // Assign the list to the player
                BackgroundMediaPlayer.Current.Source = _playbackList;

                // Add handler for future playlist item changes
                _playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        public Uri GetMusicFile(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync("https://api.soundcloud.com/tracks/" + id + "/streams?client_id=" + ClientId).Result;
                if (response.IsSuccessStatusCode)
                {
                    ApiResponse apiResponse = new ApiResponse
                    {
                        Data = JsonConvert.DeserializeObject<dynamic>(AsyncHelper.RunSync(() => response.Content.ReadAsStringAsync()))
                    };

                    string mp3;
                    try
                    {
                        mp3 = apiResponse.Data["http_mp3_128_url"].Value;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            mp3 = apiResponse.Data["hls_mp3_128_url"].Value;
                            UpdateToastMessage("Sorry, we can't play this song. It uses the HLS protocol, and the app does not support it yet.");
                        }
                        catch (Exception ex)
                        {
                            var a = new ErrorLogProxy(ex.ToString());
                            Debug.WriteLine(a);
                            return null;
                        }
                    }
                    return new Uri(mp3);
                }
            }
            return null;
        }
        void UpdateToastMessage(string message)
        {
            var template = ToastTemplateType.ToastImageAndText02;
            var xml = ToastNotificationManager.GetTemplateContent(template);
            xml.DocumentElement.SetAttribute("launch", "Args");

            var node = xml.CreateTextNode(message);
            var elements = xml.GetElementsByTagName("text");
            elements[1].AppendChild(node);

            var toast = new ToastNotification(xml);
            var notifier = ToastNotificationManager.CreateToastNotifier("App");
            notifier.Show(toast);
        }
    }
}
