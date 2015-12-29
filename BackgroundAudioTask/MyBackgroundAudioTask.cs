using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using ClassLibrary.API;
using ClassLibrary.Common;
using ClassLibrary.Messages;
using ClassLibrary.Models;
using Newtonsoft.Json;

// ReSharper disable AccessToDisposedClosure

namespace BackgroundAudioTask
{
    public sealed class MyBackgroundAudioTask : IBackgroundTask
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
        private readonly string _clientId = "776ca412db7b101b1602c6a67b1a0579";

        #endregion

        #region Helper methods
        int? GetCurrentTrackId()
        {
            if (_playbackList == null)
                return null;

            return GetTrackId(_playbackList.CurrentItem);
        }

        int? GetTrackId(MediaPlaybackItem item)
        {
            return (int?) item?.Source.CustomProperties[TrackIdKey];
        }
        #endregion

        #region IBackgroundTask and IBackgroundTaskInstance Interface Members and handlers
        /// <summary>
        /// The Run method is the entry point of a background task. 
        /// </summary>
        /// <param name="taskInstance"></param>
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
            _smtc.ButtonPressed += smtc_ButtonPressed;
            _smtc.PropertyChanged += smtc_PropertyChanged;
            _smtc.IsEnabled = true;
            _smtc.IsPauseEnabled = true;
            _smtc.IsPlayEnabled = true;
            _smtc.IsNextEnabled = true;
            _smtc.IsPreviousEnabled = true;

            // Read persisted state of foreground app
            var value = ApplicationSettingHelper.ReadLocalSettingsValue<string>(ApplicationSettingsConstants.AppState);
            _foregroundAppState = value == null ? AppState.Unknown : EnumHelper.Parse<AppState>(value.ToString());

            // Add handlers for MediaPlayer
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

            // Initialize message channel 
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;

            // Send information to foreground that background task has been started if app is active
            if (_foregroundAppState != AppState.Suspended)
                MessageService.SendMessageToForeground(new BackgroundAudioTaskStartedMessage());

            ApplicationSettingHelper.SaveLocalSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());

            _deferral = taskInstance.GetDeferral(); // This must be retrieved prior to subscribing to events below which use it

            // Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)
            _backgroundTaskStarted.Set();

            // Associate a cancellation and completed handlers with the background task.
            taskInstance.Task.Completed += TaskCompleted;
            taskInstance.Canceled += OnCanceled; // event may raise immediately before continung thread excecution so must be at the end

            }
            catch (Exception ex)
            {
                new ErrorLogProxy(ex.ToString());
            }
        }

        /// <summary>
        /// Indicate that the background task is completed.
        /// </summary>       
        void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("MyBackgroundAudioTask " + sender.TaskId + " Completed...");
            _deferral.Complete();
        }

        /// <summary>
        /// Handles background task cancellation. Task cancellation happens due to:
        /// 1. Another Media app comes into foreground and starts playing music 
        /// 2. Resource pressure. Your task is consuming more CPU and memory than allowed.
        /// In either case, save state so that if foreground app resumes it can know where to start.
        /// </summary>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // You get some time here to save your state before process and resources are reclaimed
            Debug.WriteLine("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");
            try
            {
                // immediately set not running
                _backgroundTaskStarted.Reset();

                // save state
                ApplicationSettingHelper.SaveLocalSettingsValue(ApplicationSettingsConstants.TrackId, GetCurrentTrackId() == null ? null : GetCurrentTrackId().ToString());
                ApplicationSettingHelper.SaveLocalSettingsValue(ApplicationSettingsConstants.Position, BackgroundMediaPlayer.Current.Position.ToString());
                ApplicationSettingHelper.SaveLocalSettingsValue(ApplicationSettingsConstants.BackgroundTaskState,
                    BackgroundTaskState.Canceled.ToString());
                ApplicationSettingHelper.SaveLocalSettingsValue(ApplicationSettingsConstants.AppState,
                    Enum.GetName(typeof (AppState), _foregroundAppState));

                // unsubscribe from list changes
                if (_playbackList != null)
                {
                    _playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
                    _playbackList = null;
                }

                // unsubscribe event handlers
                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                _smtc.ButtonPressed -= smtc_ButtonPressed;
                _smtc.PropertyChanged -= smtc_PropertyChanged;

                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            _deferral.Complete(); // signals task completion. 
            Debug.WriteLine("MyBackgroundAudioTask Cancel complete...");
        }
        #endregion

        #region SysteMediaTransportControls related functions and handlers
        /// <summary>
        /// Update Universal Volume Control (UVC) using SystemMediaTransPortControl APIs
        /// </summary>
        private void UpdateUvcOnNewTrack(MediaPlaybackItem item)
        {
            if (item == null)
            {
                //_smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                _smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
                _smtc.DisplayUpdater.Update();
                return;
            }

            //_smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            _smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            // ReSharper disable once AssignNullToNotNullAttribute
            _smtc.DisplayUpdater.MusicProperties.Title = item.Source.CustomProperties[TitleKey] as string;

            var albumArt = item.Source.CustomProperties[AlbumArtKey] as string;
            Uri albumArtUri = null;
            if (albumArt != null)
            {
                albumArtUri = new Uri(albumArt);
            }
            if (albumArtUri != null)
                _smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumArtUri);

            _smtc.DisplayUpdater.Update();
        }

        /// <summary>
        /// Fires when any SystemMediaTransportControl property is changed by system or user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            // TODO: If soundlevel turns to muted, app can choose to pause the music
        }

        /// <summary>
        /// This function controls the button events from UVC.
        /// This code if not run in background process, will not be able to handle button pressed events when app is suspended.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
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



        #endregion

        #region Playlist management functions and handlers
        /// <summary>
        /// Start playlist and change UVC state
        /// </summary>
        private void StartPlayback()
        {
            try
            {
                // If playback was already started once we can just resume playing.
                if (!_playbackStartedPreviously)
                {
                    _playbackStartedPreviously = true;

                    // If the task was cancelled we would have saved the current track and its position. We will try playback from there.
                    var currentTrackId = ApplicationSettingHelper.ReadLocalSettingsValue<int>(ApplicationSettingsConstants.TrackId);
                    var currentTrackPosition = ApplicationSettingHelper.ReadLocalSettingsValue<string>(ApplicationSettingsConstants.Position);
                    if (currentTrackId != null)
                    {
                        // Find the index of the item by name
                        var index = _playbackList.Items.ToList().FindIndex(item =>
                            GetTrackId(item) == (int)currentTrackId);

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
                                if (index != -1 && args.NewItem == _playbackList.Items[index])
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
            // Get the new item
            var item = args.NewItem;
            //Debug.WriteLine("PlaybackList_CurrentItemChanged: " + (item == null ? "null" : GetTrackId(item).ToString()));

            // Update the system view
            UpdateUvcOnNewTrack(item);

            // Get the current track
            int currentTrackId = 0;
            if (item != null)
                currentTrackId = item.Source.CustomProperties[TrackIdKey] as int? ?? 0;

            // Notify foreground of change or persist for later
            if (_foregroundAppState == AppState.Active)
                MessageService.SendMessageToForeground(new TrackChangedMessage(currentTrackId));
            else
                ApplicationSettingHelper.SaveLocalSettingsValue(TrackIdKey, currentTrackId);
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToPrevious()
        {
            try
            {
                _smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                _playbackList.MovePrevious();
            }
            catch (Exception)
            {
                Debug.WriteLine("ERROR");
            }

            // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
            BackgroundMediaPlayer.Current.Play();
        }

        /// <summary>
        /// Skip track and update UVC via SMTC
        /// </summary>
        private void SkipToNext()
        {
            try
            {
                _smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                _playbackList.MoveNext();
            }
            catch (Exception)
            {
                Debug.WriteLine("ERROR");
            }

            // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
            BackgroundMediaPlayer.Current.Play();

        }
        #endregion

        #region Background Media Player Handlers
        void Current_CurrentStateChanged(MediaPlayer sender, object args)
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

        /// <summary>
        /// Raised when a message is recieved from the foreground app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            AppSuspendedMessage appSuspendedMessage;
            if (MessageService.TryParseMessage(e.Data, out appSuspendedMessage))
            {
                Debug.WriteLine("App suspending"); // App is suspended, you can save your task state at this point
                _foregroundAppState = AppState.Suspended;
                var currentTrackId = GetCurrentTrackId();
                ApplicationSettingHelper.SaveLocalSettingsValue(ApplicationSettingsConstants.TrackId, currentTrackId?.ToString());
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
                var index = _playbackList.Items.ToList().FindIndex(i => (int) i.Source.CustomProperties[TrackIdKey] == trackChangedMessage.TrackId);
                Debug.WriteLine("Skipping to track " + index);
                ChangeTrackInPlaylist((uint)index);

                // TODO: Work around playlist bug that doesn't continue playing after a switch; remove later
                BackgroundMediaPlayer.Current.Play();
                return;
            }

            UpdatePlaylistMessage updatePlaylistMessage;
            if (MessageService.TryParseMessage(e.Data, out updatePlaylistMessage))
            {
                CreatePlaybackList(updatePlaylistMessage.Songs);
            }
        }

        void ChangeTrackInPlaylist(uint index)
        {
            try
            {
                _smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
                MessageService.SendMessageToForeground(new TrackChangedMessage((int) index));
                _playbackList.MoveTo(index);
            }
            catch (Exception)
            {
                ChangeTrackInPlaylist(index);
            }
        }

        /// <summary>
        /// Create a playback list from the list of songs received from the foreground app.
        /// </summary>
        /// <param name="songs"></param>
        void CreatePlaybackList(IEnumerable<Track> songs)
        {
            // Make a new list and enable looping
            _playbackList = new MediaPlaybackList {AutoRepeatEnabled = true};
            if (songs!= null)
            {
                // Add playback items to the list
                foreach (var song in songs)
                {
                    if (song.Id != null)
                    {
                        var file = GetMusicFile(song.Id.Value);
                        if (file != null)
                        {
                            var source = MediaSource.CreateFromUri(file);
                            source.CustomProperties[TrackIdKey] = song.Id;
                            source.CustomProperties[TitleKey] = song.Title;
                            source.CustomProperties[AlbumArtKey] = song.ArtworkUrl;
                            _playbackList.Items.Add(new MediaPlaybackItem(source));
                        }
                    }
                }
            }

            // Don't auto start
            BackgroundMediaPlayer.Current.AutoPlay = false;

            // Assign the list to the player
            BackgroundMediaPlayer.Current.Source = _playbackList;

            // Add handler for future playlist item changes
            _playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
        }
        #endregion


        public Uri GetMusicFile(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = AsyncHelper.RunSync(() =>
                {
                    Debug.Assert(client != null, "client != null");
                    return client.GetAsync("https://api.soundcloud.com/tracks/" + id + "/streams?client_id=" + _clientId);
                });
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
                            new ErrorLogProxy(ex.ToString());
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