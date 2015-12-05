﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using ClassLibrary.Common;
using ClassLibrary.Messages;
using ClassLibrary.Models;

namespace ClassLibrary
{
    public class AudioPlayer
    {
        private const int RpcSServerUnavailable = -2147023174; // 0x800706BA
        private readonly AutoResetEvent _backgroundAudioTaskStarted;
        public List<Track> PlayList { get; set; }
        public bool IsMyBackgroundTaskRunning
        {
            get
            {
                bool isMyBackgroundTaskRunning;

                string value =
                    AsyncHelper.RunSync(
                        () =>
                            Enough.Storage.StorageHelper.TryLoadObjectAsync<string>(
                                ApplicationSettingsConstants.BackgroundTaskState));
                if (value == null)
                {
                    return false;
                }
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
        public MediaPlayer CurrentPlayer
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
                    if (ex.HResult == RpcSServerUnavailable)
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
        public AudioPlayer()
        {
            _backgroundAudioTaskStarted = new AutoResetEvent(false);
            StartBackgroundAudioTask();
        }
        private async void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            _backgroundAudioTaskStarted.Reset();
            await
                Enough.Storage.StorageHelper.SaveObjectAsync(BackgroundTaskState.Unknown.ToString(),
                    ApplicationSettingsConstants.BackgroundTaskState);

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RpcSServerUnavailable)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }
            }
        }
        public void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            var startResult = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                bool result = _backgroundAudioTaskStarted.WaitOne(10000);
                //Send message to initiate playback
                if (result)
                {
                    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(PlayList));
                    MessageService.SendMessageToBackground(new StartPlaybackMessage());
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            });
            startResult.Completed = BackgroundTaskInitializationCompleted;
        }
        public void RemoveMediaPlayerEventHandlers()
        {
            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RpcSServerUnavailable)
                {
                    // do nothing
                }
                else
                {
                    throw;
                }
            }
        }
        public void AddMediaPlayerEventHandlers()
        {
            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RpcSServerUnavailable)
                {
                    // Internally MessageReceivedFromBackground calls Current which can throw RPC_S_SERVER_UNAVAILABLE
                    ResetAfterLostBackground();
                }
            }
        }
        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
            {
                Debug.WriteLine("Background Audio Task initialized");
            }
            else if (status == AsyncStatus.Error)
            {
                Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode);
            }
        }
        async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    if (!trackChangedMessage.TrackId.HasValue)
                    {
                        return;
                    }
                    
                    var song = PlayList.Where(track =>
                    {
                        Debug.Assert(track.Id != null, "track.Id != null");
                        return track.Id.Value.Equals(trackChangedMessage.TrackId.Value);
                    });
                });
                return;
            }

            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                Debug.WriteLine("BackgroundAudioTask started");
                _backgroundAudioTaskStarted.Set();
            }
        }
        public async void PlayTrack(Track track)
        {
            var song = track;
            bool trackAlreadyInPlaylist = true;
            if (PlayList == null)
            {
                PlayList = new List<Track>();
            }
            if (!PlayList.Contains(track))
            {
                PlayList.Add(track);
                trackAlreadyInPlaylist = false;
            }
            Debug.WriteLine("Clicked item from App: " + song.Id);

            // Start the background task if it wasn't running
            if (!IsMyBackgroundTaskRunning || MediaPlayerState.Closed == CurrentPlayer.CurrentState)
            {
                // First update the persisted start track
                await Enough.Storage.StorageHelper.SaveObjectAsync(song.Id, ApplicationSettingsConstants.TrackId);
                await
                    Enough.Storage.StorageHelper.SaveObjectAsync(new TimeSpan().ToString(),
                        ApplicationSettingsConstants.Position);

                // Start task
                StartBackgroundAudioTask();
            }
            else
            {
                // Switch to the selected track
                if (!trackAlreadyInPlaylist)
                {
                    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(PlayList));
                }
                MessageService.SendMessageToBackground(new TrackChangedMessage(song.Id));
            }

            if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
            {
                CurrentPlayer.Play();
            }
        }
    }
}