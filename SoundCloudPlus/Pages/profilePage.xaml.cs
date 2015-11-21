﻿using System;
using System.Collections.Generic;
using Windows.Security.Authentication.OnlineId;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.API;
using SoundCloudPlus.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        private int _userId;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public ProfilePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }
        private ProfilePageViewModel _profilePageViewModel;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _profilePageViewModel =
                    (ProfilePageViewModel)Resources["ProfilePageViewModel"];
                if (App.SoundCloud.IsAuthenticated)
                {
                    
                    int id = MainPage.Current.UserId;

                    _profilePageViewModel.UserObject = await App.SoundCloud.GetUser(id);

                    try { _profilePageViewModel.PlaylistCollection = await App.SoundCloud.GetOwnPlaylists(id); }
                    catch (Exception) { _profilePageViewModel.PlaylistCollection = null; }

                    try { _profilePageViewModel.RepostCollection = await App.SoundCloud.GetReposts(id); }
                    catch (Exception) { _profilePageViewModel.RepostCollection = null; }

                    try { _profilePageViewModel.TrackCollection = await App.SoundCloud.GetTracks(id); }
                    catch (Exception) { _profilePageViewModel.TrackCollection = null; }
                }
            }
            base.OnNavigatedTo(e);
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            List<int> l = MainPage.Current.UserIdHistory;
            int prevId = l[l.Count - 1];
            MainPage.Current.UserId = prevId;
            if (Frame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            
            currentView.BackRequested -= CurrentView_BackRequested;
        }
        
        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        {}

        private void MakeVisibleInvisible(bool all, bool trackCollection, bool playlistCollection, bool repostCollection)
        {
            if (all)
            {
                TrackCollectionGridView.Visibility = Visibility.Visible;
                PlaylistCollectionGridView.Visibility = Visibility.Visible;
                RepostCollectionGridView.Visibility = Visibility.Visible;
            }
            else
            {
                TrackCollectionGridView.Visibility = Visibility.Collapsed;
                PlaylistCollectionGridView.Visibility = Visibility.Collapsed;
                RepostCollectionGridView.Visibility = Visibility.Collapsed;

                if (trackCollection)
                {
                    TrackCollectionGridView.Visibility = Visibility.Visible;
                }

                if (playlistCollection)
                {
                    PlaylistCollectionGridView.Visibility = Visibility.Visible;
                }

                if (repostCollection)
                {
                    RepostCollectionGridView.Visibility = Visibility.Visible;
                }
            }
        }

        private async void RepostPlaylist(object sender, RoutedEventArgs e)
        {
            Button b = (Button) sender;
            int playlistId = Convert.ToInt32(b.Tag);
            //repost niet mogelijk bij eigen user!!
            try
            {
                ApiResponse aR = await App.SoundCloud.RepostPlaylist(playlistId);
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
