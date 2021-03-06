﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;

namespace SoundCloudPlus.Pages
{
    public sealed partial class HomePage : Page
    {
        private HomePageViewModel _homePageViewModel;
        private BackgroundWorker _bwExplore = new BackgroundWorker();
        private BackgroundWorker _bwStream = new BackgroundWorker();
        private ObservableCollection<StreamCollection> _newStreamCollection = new ObservableCollection<StreamCollection>();
        private ObservableCollection<Track> _newExploreCollection = new ObservableCollection<Track>();
        private List<String> _genre;
        private double _verticalOffsetStream;
        private double _verticalOffsetExplore;
        private double _maxVerticalOffsetStream;
        private double _maxVerticalOffsetExplore;

        public HomePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            // backgroundworker init
            InitBwStream();
            InitBwExplore();
        }

        #region BackgroundWorkerStream
        private void InitBwStream()
        {
            _bwStream.DoWork += BwStream_DoWork;
            _bwStream.WorkerSupportsCancellation = true;
            _bwStream.RunWorkerCompleted += BwStream_RunWorkerCompleted;
        }

        private void BwStream_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (StreamCollection s in _newStreamCollection)
                {
                    _homePageViewModel.StreamCollection.Add(s);
                }
            }
            catch (Exception) { }

            _bwStream.CancelAsync();
        }

        private void BwStream_DoWork(object sender, DoWorkEventArgs e)
        {
            StreamScroller();
        }
        #endregion

        #region BackgroundWorkerExplore
        private void InitBwExplore()
        {
            _bwExplore.DoWork += BwExplore_DoWork;
            _bwExplore.WorkerSupportsCancellation = true;
            _bwExplore.RunWorkerCompleted += BwExplore_RunWorkerCompleted;
        }

        private void BwExplore_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (Track t in _newExploreCollection)
                {
                    _homePageViewModel.ExploreCollection.Add(t);
                }
            }
            catch (Exception) { }

            _bwStream.CancelAsync();
        }

        private void BwExplore_DoWork(object sender, DoWorkEventArgs e)
        {
            ExploreScroller();
        }
        #endregion

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            Frame.BackStack.Clear();
            try
            {
                LoadGenres();
            }
            catch (Exception ex)
            {
                ErrorLogProxy.LogError(ex.ToString());
                ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
            }

            if (e.NavigationMode != NavigationMode.Back)
            {
                _homePageViewModel =
                    (HomePageViewModel)Resources["HomePageViewModel"];
                int attempts = 0;
                try
                {
                    while (!await App.SoundCloud.IsAuthenticated())
                    {
                        if (attempts == 2)
                        {
                            MessageDialog md = new MessageDialog("Please check your internet connection", "Sorry, we encountered an error");
                            md.Commands.Add(new UICommand("Close", Action));
                            await md.ShowAsync();
                        }
                        attempts = attempts + 1;
                        new ManualResetEvent(false).WaitOne(1500);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogProxy.LogError(ex.ToString());
                    ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                }

                //UpdateStreamExploreCollection();
            }
            base.OnNavigatedTo(e);
        }

        private void Action(IUICommand command)
        {
            Application.Current.Exit();
        }

        private void LoadGenres()
        {
            _genre = new List<string>() { "Music", "Audio" };
            //_genre = await App.SoundCloud.GetGenres();
            //foreach (string s in _genre)
            //{
                //string genre = s.Replace("+", " ").Replace("%", "").Replace("26", " & ").Replace("  ", " ");
            if (cboGenre.Items.Count < 2)
            {
                cboGenre.Items.Add("Music");
                cboGenre.Items.Add("Audio");
            }
                if (cboGenre.SelectedIndex == -1)
                {
                    cboGenre.SelectedIndex = 0;
                }
            //}
        }

        private async void UpdateStreamExploreCollection()
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                int limit = Screen.GetLimitItems(height, width, 400, 800, 200, 400);
                var a = await App.SoundCloud.GetStream(limit);
                _homePageViewModel.StreamCollection = a;
                if (_genre != null && cboGenre.SelectedIndex != -1)
                {
                    var b = await App.SoundCloud.GetExplore(limit, _genre[cboGenre.SelectedIndex]);
                    _homePageViewModel.ExploreCollection = b;
                }
            }
            catch (Exception ex)
            {
                ErrorLogProxy.LogError(ex.ToString());
                //ErrorLogProxy.NotifyError(ex.ToString());
            }
        }

        private void StreamGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Screen.MakeResponsive(e, 400, 800, StreamGridView);
        }

        private void ExploreGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 400, 800, ExploreGridView);
        }
        private void TrackGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                StreamCollection s = e.ClickedItem as StreamCollection;
                if (s == null)
                {
                    Track t = e.ClickedItem as Track;
                    App.AudioPlayer.PlayTrack(new List<Track> { t }, t);
                }
                if (s?.Track != null)
                {
                    List<Track> playList = (from streamCollection in _newStreamCollection where streamCollection.Track != null select streamCollection.Track).ToList();
                    App.AudioPlayer.PlayTrack(new List<Track> { s.Track }, s.Track);
                }
                else if (s?.Playlist != null)
                {

                    App.AudioPlayer.PlayTrack(s.Playlist.Tracks, s.Playlist.Tracks[0]);
                    MainPage.Current.Navigate(new PlaylistViewPage(), s.Playlist.Id.ToString());
                }
            }
            catch (InvalidCastException)
            {
                Track t = e.ClickedItem as Track;
                App.AudioPlayer.PlayTrack(new List<Track> { t }, t);
            }
        }

        #region StreamScroller
        private void ScrollViewerStream_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            _verticalOffsetStream = SvSteam.VerticalOffset;
            _maxVerticalOffsetStream = SvSteam.ScrollableHeight;

            if (_maxVerticalOffsetStream < 0 || _verticalOffsetStream == _maxVerticalOffsetStream)
            {
                if (_bwStream.IsBusy == false)
                {
                    _bwStream.RunWorkerAsync();
                }
            }
        }

        private async void StreamScroller()
        {
            var e = App.SoundCloud.GetStreamNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                ObservableCollection<StreamCollection> newCollection = await App.SoundCloud.GetStream(b);
                _newStreamCollection = newCollection;
            }
        }
        #endregion

        #region ExploreScroller
        private void ScrollViewerExplore_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            _verticalOffsetExplore = SvExplore.VerticalOffset;
            _maxVerticalOffsetExplore = SvExplore.ScrollableHeight;

            if (_maxVerticalOffsetExplore < 0 || _verticalOffsetExplore == _maxVerticalOffsetExplore)
            {
                if (_bwExplore.IsBusy == false)
                {
                    _bwExplore.RunWorkerAsync();
                }
            }
        }

        private async void ExploreScroller()
        {
            var e = App.SoundCloud.GetExploreNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                ObservableCollection<Track> newCollection = await App.SoundCloud.GetExplore(b);
                _newExploreCollection = newCollection;
            }
        }
        #endregion

        /*private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            b.Style = Application.Current.Resources["BtnLikeActive"] as Style;

            TextBlock txb = (TextBlock)b.Content;
            txb.Text = "\ue0a5";
        }*/

        private void cboGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStreamExploreCollection();
        }

        private void LikeButton_Click(System.Object sender, RoutedEventArgs e)
        {
            //Button b = sender as Button;
            //await App.SoundCloud.likeTrack(b.Tag.ToString());
        }
    }
}