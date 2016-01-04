﻿using ClassLibrary.Common;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    public sealed partial class PlaylistViewPage : Page
    {
        private PlaylistViewPageViewModel _playlistViewPageViewModel;

        public PlaylistViewPage()
        {
            this.InitializeComponent();
        }

        private void PlaylistGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            /*GridView gridView = (GridView)sender;
            ObservableCollection<PlaylistCollection> playlistCollections = (ObservableCollection<PlaylistCollection>)gridView.ItemsSource;
            PlaylistCollection p = e.ClickedItem as PlaylistCollection;
            if (p?.Playlist.Tracks != null)
            {
                List<Track> playList = (from playlistCollection in playlistCollections where playlistCollections. != null select streamCollection.Track).ToList();
                App.SoundCloud.AudioPlayer.PlayTrack(playList, p.Track);
            }*/
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
            if (Frame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _playlistViewPageViewModel =
                    (PlaylistViewPageViewModel)Resources["PlaylistViewPageViewModel"];
                _playlistViewPageViewModel.Playlist = (Playlist) MainPage.Current.CurrentPlaylist;
            }
            base.OnNavigatedTo(e);
        }

    }
}
