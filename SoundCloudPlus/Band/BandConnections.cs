using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ClassLibrary.Common;
using ClassLibrary.Messages;
using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.Band
{
    public class BandConnections : INotifyPropertyChanged
    {
        private Guid _bandTileId;
        public IBandInfo BandInfo { get; set; }
        public IBandClient BandClient { get; set; }
        public static Guid BandTileId { get; set; }
        public static Guid BandPageId { get; set; }
        public BandConnections()
        {
            BandTileId = new Guid("0c35a36f-1bd9-4760-8536-2ba4883ac49c");
            BandPageId = new Guid("ca6c9923-908b-420c-8ed6-a9b05af34a66");
        }

        public async Task ConnectBand()
        {
            if (BandInfo == null)
            {
                await FindBands();
            }
        }

        private async Task FindBands()
        {
            try
            {
                IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    return;
                }
                else
                {
                    IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);
                    BandClient = bandClient;
                    BandInfo = pairedBands[0];
                }
            }
            catch (Exception ex)
            {
                ErrorLogProxy.LogError(ex.Message);
                ErrorLogProxy.NotifyErrorInDebug(ex.Message);
            }
        }
        public async Task CreateAndPushTileAsync(string name)
        {
            if (BandClient == null)
            {
                return;
            }
            try
            {
                var designed = new BandTileLayout();
                BandTile myTile = new BandTile(BandTileId)
                {
                    Name = name,
                    SmallIcon = await LoadIcon("ms-appx:///Band/bandsmallicon.png"),
                    TileIcon = await LoadIcon("ms-appx:///Band/bandicon.png")
                };
                myTile.PageLayouts.Add(designed.Layout);

                _bandTileId = BandTileId;
                if (BandClient.TileManager.TileInstalledAndOwned(ref _bandTileId, CancellationToken.None))
                {
                    await BandClient.TileManager.SetPagesAsync(myTile.TileId, new PageData(BandPageId, 0, designed.Data.All));
                    BandClient.TileManager.TileButtonPressed += TileManager_TileButtonPressed;
                    await BandClient.TileManager.StartReadingsAsync();
                }
                else
                {
                    var tilecapacity = await BandClient.TileManager.GetRemainingTileCapacityAsync();
                    if (tilecapacity == 0)
                    {
                        ErrorLogProxy.NotifyError("No space on Band for tile");
                        return;
                    }
                    await BandClient.TileManager.RemoveTileAsync(myTile.TileId);
                    await BandClient.TileManager.AddTileAsync(myTile);
                    await BandClient.TileManager.SetPagesAsync(myTile.TileId, new PageData(BandPageId, 0, designed.Data.All));
                    BandClient.TileManager.TileButtonPressed += TileManager_TileButtonPressed;
                    await BandClient.TileManager.StartReadingsAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorLogProxy.LogError(ex.Message);
                ErrorLogProxy.NotifyErrorInDebug(ex.Message);
            }
        }

        private void TileManager_TileButtonPressed(object sender, BandTileEventArgs<IBandTileButtonPressedEvent> e)
        {
            Debug.WriteLine(e.TileEvent.TileId + " ----- " + e.TileEvent.ElementId);
            switch ((int) e.TileEvent.ElementId)
            {
                case 2:
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
                    break;
                case 3:
                    MessageService.SendMessageToBackground(new SkipPreviousMessage());
                    break;
                case 4:
                    MessageService.SendMessageToBackground(new SkipNextMessage());
                    break;
                default:
                    break;
            }
        }

        private static async Task<BandIcon> LoadIcon(string uri)
        {
            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}