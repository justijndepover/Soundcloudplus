using SoundCloudPlus.ViewModels;
using System;
using System.Diagnostics;
using System.Numerics;
using Windows.Foundation.Numerics;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using ClassLibrary.Models;

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
        public PlayingPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
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
                    LayoutRoot.DataContext = _mainPageViewModel;
                    _canvasControl = new CanvasControl();
                    _canvasControl.Draw += _canvasControl_Draw;
                    _canvasControl.CreateResources += _canvasControl_CreateResources;
                    ContentPresenter.Content = _canvasControl;
                    createWaveForm();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private async void createWaveForm()
        {
            var url = _mainPageViewModel.PlayingTrack.WaveformUrl;
            WaveForm wave = await App.SoundCloud.getWaveForm(url);
            if(wave != null)
            {
                //create the waveform
            }
        }

        private void _canvasControl_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            if (_mainPageViewModel.PlayingTrack.ArtworkUrl != null)
            {
                var uri = _mainPageViewModel.PlayingTrack.ArtworkUrl.ToString();
                uri = uri.Replace("large.jpg", "t500x500.jpg");
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    _scaleEffect = new ScaleEffect();
                    _blurEffect = new GaussianBlurEffect();

                    _image = CanvasBitmap.LoadAsync(sender.Device,
                        new Uri(uri)).GetAwaiter().GetResult();

                    _imageLoaded = true;

                    sender.Invalidate();
                }
            }
        }

        private void _canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_imageLoaded)
            {
                using (var session = args.DrawingSession)
                {
                    session.Units = CanvasUnits.Pixels;

                    double displayScaling = DisplayInformation.GetForCurrentView().LogicalDpi / 96.0;

                    double pixelWidth = sender.ActualWidth * displayScaling;

                    var scalefactor = pixelWidth / _image.Size.Width;

                    _scaleEffect.Source = this._image;
                    _scaleEffect.Scale = new Vector2
                    {
                        X = (float)scalefactor,
                        Y = (float)scalefactor
                    };

                    _blurEffect.Source = _scaleEffect;
                    _blurEffect.BlurAmount = 15;

                    session.DrawImage(_blurEffect, 0.0f, 0.0f);
                }
            }

        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            currentView.BackRequested -= CurrentView_BackRequested;
        }

    }
}