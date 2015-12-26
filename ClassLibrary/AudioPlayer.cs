using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Notifications;
using ClassLibrary.Common;
using ClassLibrary.Messages;
using ClassLibrary.Models;
using Enough.Storage;
using TilesAndNotifications.Services;
using System.Text;
using System.Xml;

namespace ClassLibrary
{
    public class AudioPlayer : INotifyPropertyChanged
    {
        private const int RpcSServerUnavailable = -2147023174; // 0x800706BA
        private readonly AutoResetEvent _backgroundAudioTaskStarted;
        private Track _currentTrack;
        public List<Track> PlayList { get; set; }
        public Track CurrentTrack
        {
            get { return _currentTrack; }
            set { _currentTrack = value; OnPropertyChanged(nameof(CurrentTrack)); }
        }

        public bool IsMyBackgroundTaskRunning
        {
            get
            {
                bool isMyBackgroundTaskRunning;

                string value =
                    AsyncHelper.RunSync(
                        () =>
                            StorageHelper.TryLoadObjectAsync<string>(
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
        }
        private async void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            _backgroundAudioTaskStarted.Reset();
            await
                StorageHelper.SaveObjectAsync(BackgroundTaskState.Unknown.ToString(),
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

            var startResult = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                MessageService.SendMessageToBackground(new UpdatePlaylistMessage(PlayList));
                MessageService.SendMessageToBackground(new StartPlaybackMessage());
                //bool result = _backgroundAudioTaskStarted.WaitOne(10000);
                ////Send message to initiate playback
                //if (result)
                //{
                //    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(PlayList));
                //    MessageService.SendMessageToBackground(new StartPlaybackMessage());
                //}
                //else
                //{
                //    throw new Exception("Background Audio Task didn't start in expected time");
                //}
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
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    if (!trackChangedMessage.TrackId.HasValue)
                    {
                        return;
                    }
                    
                    CurrentTrack = PlayList.Where(track =>
                    {
                        Debug.Assert(track.Id != null, "track.Id != null");
                        return track.Id.Value.Equals(trackChangedMessage.TrackId.Value);
                    }).FirstOrDefault();
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

        void UpdateToastMessage(Track t)
        {
            var template = ToastTemplateType.ToastImageAndText02;
            var xml = ToastNotificationManager.GetTemplateContent(template);
            xml.DocumentElement.SetAttribute("launch", "Args");

            var title = xml.CreateTextNode(t.Title);
            var artist = xml.CreateTextNode(t.User.Username);
            var elements = xml.GetElementsByTagName("text");
            elements[0].AppendChild(artist);
            elements[1].AppendChild(title);

            var imageElement = xml.GetElementsByTagName("image");
            imageElement[0].Attributes[1].NodeValue = t.ArtworkUrl;

            var toast = new ToastNotification(xml);
            var notifier = ToastNotificationManager.CreateToastNotifier();
            notifier.Show(toast);
        }

        public async void PlayTrack(Track track)
        {
            var song = track;
            bool trackAlreadyInPlaylist = true;
            bool LiveTile = await StorageHelper.TryLoadObjectAsync<bool>("LiveTilesEnabled");
            if (LiveTile)
            {
                UpdateLiveTile(track);
            }
            bool Toast = await StorageHelper.TryLoadObjectAsync<bool>("ToastsEnabled");
            if (Toast)
            {
                UpdateToastMessage(track);
            }
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

            Debug.WriteLine(CurrentPlayer.CurrentState);
            // Start the background task if it wasn't running
            if (!IsMyBackgroundTaskRunning || MediaPlayerState.Closed == CurrentPlayer.CurrentState)
            {
                // First update the persisted start track
                await StorageHelper.SaveObjectAsync(song.Id, ApplicationSettingsConstants.TrackId);
                await
                    StorageHelper.SaveObjectAsync(new TimeSpan().ToString(),
                        ApplicationSettingsConstants.Position);
                
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}