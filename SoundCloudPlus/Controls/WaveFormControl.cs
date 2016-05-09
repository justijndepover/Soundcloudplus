using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using ClassLibrary.Common;
using ClassLibrary.Models;
using SoundCloudPlus.Pages;

namespace SoundCloudPlus.Controls
{
    public class WaveFormControl : Grid
    {
        readonly DispatcherTimer _playbackTimer = new DispatcherTimer();
        readonly Slider _slider = new Slider();

        public WaveFormControl()
        {
            _playbackTimer.Interval = TimeSpan.FromMilliseconds(250);
            _playbackTimer.Tick += _playbackTimer_Tick;
        }
        private void _playbackTimer_Tick(object sender, object e)
        {
            var position = App.AudioPlayer.CurrentPlayer.Position;
            _slider.Maximum = App.AudioPlayer.CurrentPlayer.NaturalDuration.TotalMilliseconds;
            try
            {
                _slider.Value = position.TotalMilliseconds;
            }
            catch (Exception ex)
            {
                ErrorLogProxy.LogError(ex.ToString());
                ErrorLogProxy.NotifyError(ex.ToString());
            }
        }
        public void FillWaveForm(WaveForm wave)
        {
            Children.Clear();
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            if (wave != null)
            {
                //create the waveform
                int index = 0;
                int sample;
                RowDefinition row1 = new RowDefinition();
                row1.Height = new GridLength(2, GridUnitType.Star);
                RowDefinitions.Add(row1);
                RowDefinition row2 = new RowDefinition();
                row1.Height = new GridLength(1, GridUnitType.Star);
                RowDefinitions.Add(row2);
                for (sample = 0; sample < wave.Samples.Length; sample = sample + 6)
                {
                    int s = sample / 6;
                    if((s & 1) == 0)
                    {
                        int i = wave.Samples[sample];
                        ColumnDefinition col = new ColumnDefinition();
                        col.Width = new GridLength(1, GridUnitType.Star);
                        ColumnDefinitions.Add(col);
                        Thickness margin = new Thickness();
                        margin.Bottom = 1;
                        margin.Top = 1;
                        Rectangle r = new Rectangle
                        {
                            Height = i / 2,
                            Margin = margin,
                            VerticalAlignment = VerticalAlignment.Bottom
                        };
                        Rectangle r2 = new Rectangle
                        {
                            Height = i / 4,
                            Margin = margin,
                            VerticalAlignment = VerticalAlignment.Top
                        };
                        r.SetValue(ColumnProperty, index);
                        r.SetValue(RowProperty, 0);
                        r2.SetValue(ColumnProperty, index);
                        r2.SetValue(RowProperty, 1);
                        r.Fill = new SolidColorBrush(Colors.White);
                        r2.Fill = new SolidColorBrush(Colors.White);
                        Children.Add(r);
                        Children.Add(r2);
                        index = index + 2;
                    }
                    else
                    {
                        ColumnDefinition col = new ColumnDefinition();
                        col.Width = new GridLength(1, GridUnitType.Star);
                        ColumnDefinitions.Add(col);
                    }
                    
                }
                _slider.Minimum = 0;
                _slider.StepFrequency = 1;
                _slider.IsThumbToolTipEnabled = false;
                _slider.Style = (Style) Application.Current.Resources["WaveFormSlider"];
                _slider.SetValue(ColumnSpanProperty, index);
                _slider.ValueChanged += Slider_ValueChanged;
                Children.Add(_slider);
                _playbackTimer.Start();
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.NewValue > e.OldValue + 5000 || e.NewValue < e.OldValue - 5000)
            {
                TimeSpan newPos = TimeSpan.FromMilliseconds(e.NewValue);
                //MainPage.Current.CurrentPlayer.Position = newPos;
            }
        }
    }
}
