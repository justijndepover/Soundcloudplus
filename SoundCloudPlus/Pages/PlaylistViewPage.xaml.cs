using ClassLibrary.Common;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    public sealed partial class PlaylistViewPage : Page
    {
        private PlaylistViewPageViewModel _playlistViewPageViewModel;
        private MainPageViewModel _mainPageViewModel;
        private string _playlistName;
        private string _playlistId;

        public PlaylistViewPage()
        {
            this.InitializeComponent();
        }

        private void PlaylistGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(_mainPageViewModel != null)
            {
                Track t = (Track)e.ClickedItem;
                App.AudioPlayer.PlayTrack(_mainPageViewModel.PlayingList, t);
            }
        }

        private void PlaylistGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, PlaylistGridView);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            currentView.BackRequested -= CurrentView_BackRequested;
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            if (MainPage.Current.MainFrame.CanGoBack) Frame.GoBack();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                try
                {
                    _mainPageViewModel = MainPage.Current.MainPageViewModel;
                    _playlistViewPageViewModel =
                    (PlaylistViewPageViewModel)Resources["PlaylistViewPageViewModel"];
                    if (e.Parameter != null && !ReferenceEquals(e.Parameter, string.Empty))
                    {
                        _playlistId = e.Parameter.ToString();
                        _playlistViewPageViewModel.Playlist = await App.SoundCloud.GetPlaylist(Convert.ToInt32(_playlistId));
                        _playlistName = _playlistViewPageViewModel.Playlist.Title;
                    }
                    MainPage.Current.MainPageViewModel.PinButtonVisibility = Visibility.Visible;
                    MainPage.Current.PinToStartButton.Click += PinToStartButton_Click;
                }
                catch(Exception ex)
                {
                    ErrorLogProxy.LogError(ex.ToString());
                    ErrorLogProxy.NotifyError(ex.ToString());
                }
                
            }
            base.OnNavigatedTo(e);
        }
        private void PinToStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (false)//_playlistViewPageViewModel.Playlist.ArtworkUrl != null)
            {
                TileService.CreateTileLinkedToPage("10Sound", _playlistName, new[] { "playlist", _playlistId }, _playlistViewPageViewModel.Playlist.ArtworkUrl);
            }
            else
            {
                TileService.CreateTileLinkedToPage("10Sound", _playlistName, new[] { "playlist", _playlistId });
            }
        }
    }
}
