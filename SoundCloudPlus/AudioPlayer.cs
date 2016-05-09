using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Notifications;
using ClassLibrary.API;
using ClassLibrary.Common;
using ClassLibrary.Messages;
using ClassLibrary.Models;
using Newtonsoft.Json;
using SoundCloudPlus.Pages;
using TilesAndNotifications.Services;

namespace SoundCloudPlus
{
    public class AudioPlayer : INotifyPropertyChanged
    {
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
        private AutoResetEvent backgroundAudioTaskStarted;
        private bool _isMyBackgroundTaskRunning = false;
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
                if (_isMyBackgroundTaskRunning)
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
                        _isMyBackgroundTaskRunning = EnumHelper.Parse<BackgroundTaskState>(value) == BackgroundTaskState.Running;
                    }
                    catch (ArgumentException)
                    {
                        _isMyBackgroundTaskRunning = false;
                    }
                    return _isMyBackgroundTaskRunning;
                }
            }
        }
        /// <summary>
        /// You should never cache the MediaPlayer and always call Current. It is possible
        /// for the background task to go away for several different reasons. When it does
        /// an RPC_S_SERVER_UNAVAILABLE error is thrown. We need to reset the foreground state
        /// and restart the background task.
        /// </summary>
        public MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;
                int retryCount = 2;

                while (mp == null && --retryCount >= 0)
                {
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
                            // is returned when calling Current. We must restart the task, the while loop will retry to set mp.
                            ResetAfterLostBackground();
                            StartBackgroundAudioTask();
                        }
                        else
                        {
                            throw;
                        }
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
            backgroundAudioTaskStarted = new AutoResetEvent(false);
        }
        private void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            backgroundAudioTaskStarted.Reset();
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Unknown.ToString());

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

        public void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();

            var startResult = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool result = backgroundAudioTaskStarted.WaitOne(10000);
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
        public void AddMediaPlayerEventHandlers()
        {
            CurrentPlayer.CurrentStateChanged += CurrentPlayer_CurrentStateChanged;
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
                else
                {
                    throw;
                }
            }
        }

        private async void CurrentPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState; // cache outside of completion or you might get a different value
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                MainPage.Current.UpdateTransportControls(currentState);
            });
        }

        public void RemoveMediaPlayerEventHandlers()
        {
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
                    if (trackChangedMessage.TrackId == null)
                    {
                        return;
                    }

                    CurrentTrack = AsyncHelper.RunSync(() => GetTrackById(trackChangedMessage.TrackId));
                });
                return;
            }

            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                Debug.WriteLine("BackgroundAudioTask started");
                backgroundAudioTaskStarted.Set();
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
                new ErrorLogProxy(ex.ToString());
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

        public void PlayTrack(List<Track> playList, Track track)
        {
            var oldPlaylist = PlayList;
            PlayList = playList;

            bool liveTile = (bool)ApplicationSettingsHelper.ReadRoamingSettingsValue<bool>("LiveTilesEnabled");
            if (liveTile)
            {
                UpdateLiveTile(track);
            }
            bool toast = (bool)ApplicationSettingsHelper.ReadRoamingSettingsValue<bool>("ToastsEnabled");
            if (toast)
            {
                UpdateToastMessage(track);
            }
            Debug.WriteLine("Clicked item from App: " + track.Id);
            // Start the background task if it wasn't running
            if (!IsMyBackgroundTaskRunning || MediaPlayerState.Closed == CurrentPlayer.CurrentState)
            {
                // First update the persisted start track
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, track.Id);
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, new TimeSpan().ToString());

                StartBackgroundAudioTask();
            }
            else
            {

                if (!oldPlaylist.Equals(playList))
                {
                    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(playList));
                }
                MessageService.SendMessageToBackground(new TrackChangedMessage(track.Id));
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

        public string GetCurrentTrackIdAfterAppResume()
        {
            object value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.TrackId);
            if (value != null)
                return (string)value;
            return null;
        }

        public async Task<Track> GetTrackById(string trackId)
        {
            if (trackId == null)
            {
                throw new Exception();
            }
            foreach (var track in PlayList)
            {
                if (track.Id == trackId)
                {
                    return track;
                }
            }
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://api.soundcloud.com/tracks/" + trackId + "?client_id=776ca412db7b101b1602c6a67b1a0579");
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<Track>(await response.Content.ReadAsStringAsync());
                }
                return new Track();
            }
        }
    }
}