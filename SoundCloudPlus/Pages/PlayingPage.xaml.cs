﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Graphics.Display;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using ClassLibrary.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SoundCloudPlus.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{


    public sealed partial class PlayingPage : Page
    {
        private CanvasControl _canvasControl;
        private CanvasBitmap _image;
        private bool _imageLoaded;
        private ScaleEffect _scaleEffect;
        private GaussianBlurEffect _blurEffect;
        private MainPageViewModel _mainPageViewModel;
        private Track _currentTrack;
        DisplayRequest displayRequest = new DisplayRequest();
        public PlayingPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            displayRequest.RequestActive(); //to request keep display on

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                try
                {
                    _mainPageViewModel = MainPage.Current.MainPageViewModel;
                    LayoutRoot.DataContext = _mainPageViewModel;
                    _currentTrack = _mainPageViewModel.PlayingTrack;
                    _canvasControl = new CanvasControl();
                    _canvasControl.Draw += _canvasControl_Draw;
                    App.AudioPlayer.CurrentTrackChanged += AudioPlayer_CurrentTrackChanged;
                    ContentPresenter.Content = _canvasControl;
                    CreateWaveForm();
                }
                catch (Exception ex)
                {
                    ErrorLogProxy.LogError(ex.ToString());
                    ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                }
            }
        }

        private void AudioPlayer_CurrentTrackChanged(object sender, EventArgs e)
        {
            List<Track> playlist = App.AudioPlayer.PlayList;
            Track track = App.AudioPlayer.CurrentTrack;
            int index;
            for (index = 0; index < playlist.Count; index++)
            {
                if (track.Id.Equals(playlist[index].Id))
                {
                    try
                    {
                        PlayListView.SelectedIndex = index;
                        _currentTrack = track;
                        _canvasControl.Invalidate();
                        return;
                    }
                    catch (Exception ex)
                    {
                        ErrorLogProxy.LogError(ex.ToString());
                        ErrorLogProxy.NotifyErrorInDebug(ex.ToString());
                    }
                }
            }
        }

        private async void CreateWaveForm()
        {
            var url = _mainPageViewModel.PlayingTrack.WaveformUrl;
            url = url.Replace(".png", ".json");
            if (url != null)
            {
                WaveForm wave = await App.SoundCloud.GetWaveForm(url);
                WaveFormControl.FillWaveForm(wave);
            }
            else
            {
                WaveFormControl.Visibility = Visibility.Collapsed;
            }
        }

        private void _canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_currentTrack?.ArtworkUrl != null)
            {
                var uri = _currentTrack.ArtworkUrl.ToString();
                uri = uri.Replace("large.jpg", "t500x500.jpg");
                if (!string.IsNullOrWhiteSpace(uri))
                {

                    _scaleEffect = new ScaleEffect();
                    _blurEffect = new GaussianBlurEffect();

                    _image = CanvasBitmap.LoadAsync(sender.Device,
                        new Uri(uri)).GetAwaiter().GetResult();

                    _imageLoaded = true;

                    //sender.Invalidate();
                }
            }
            else
            {_scaleEffect = new ScaleEffect();
                    _blurEffect = new GaussianBlurEffect();

                    _image = CanvasBitmap.LoadAsync(sender.Device,
                        new Uri("ms-appx:///Assets/10SoundBackground.png")).GetAwaiter().GetResult();

                    _imageLoaded = true;
            }
            if (_imageLoaded)
            {
                using (var session = args.DrawingSession)
                {
                    session.Units = CanvasUnits.Pixels;

                    double displayScaling = DisplayInformation.GetForCurrentView().LogicalDpi / 96.0;
                    double pixelWidth;
                    if (sender.ActualWidth > sender.ActualHeight)
                    {
                        pixelWidth = sender.ActualWidth * displayScaling;
                    }
                    else
                    {
                        pixelWidth = sender.ActualHeight * displayScaling;
                    }

                    var scalefactor = pixelWidth / _image.Size.Width;

                    _scaleEffect.Source = _image;
                    _scaleEffect.Scale = new Vector2
                    {
                        X = (float)scalefactor,
                        Y = (float)scalefactor
                    };

                    _blurEffect.Source = _scaleEffect;
                    if(Window.Current.Bounds.Width < 720)
                    {
                        _blurEffect.BlurAmount = 0;
                    }
                    else
                    {
                        _blurEffect.BlurAmount = 100;
                    }
                    
                    session.DrawImage(_blurEffect, 2.0f, 2.0f);

                }
            }
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            displayRequest.RequestRelease(); //to request keep display on
            if (MainPage.Current.MainFrame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            currentView.BackRequested -= CurrentView_BackRequested;
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            _currentTrack = _mainPageViewModel.PlayingTrack = (Track)e.ClickedItem;
            App.AudioPlayer.PlayTrack(MainPage.Current.MainPageViewModel.PlayingList, _currentTrack);
            _canvasControl.Invalidate();
        }
    }
}