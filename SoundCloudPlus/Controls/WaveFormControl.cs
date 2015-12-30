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
            var position = App.SoundCloud.AudioPlayer.CurrentPlayer.Position;
            _slider.Maximum = App.SoundCloud.AudioPlayer.CurrentPlayer.NaturalDuration.TotalMilliseconds;
            try
            {
                _slider.Value = position.TotalMilliseconds;
            }
            catch (Exception ex)
            {
                new ErrorLogProxy(ex.ToString());
                Debug.WriteLine("Error: Progressbar value is NaN");
            }
        }
        public void FillWaveForm(WaveForm wave)
        {
            if (wave != null)
            {
                //create the waveform
                int index = 0;
                int sample;
                for (sample = 0; sample < wave.Samples.Length; sample = sample + 6)
                {
                    int i = wave.Samples[sample];
                    ColumnDefinitions.Add(new ColumnDefinition());
                    Rectangle r = new Rectangle
                    {
                        Height = i
                    };
                    r.SetValue(ColumnProperty, index);
                    r.Fill = new SolidColorBrush(Colors.White);
                    Children.Add(r);
                    index++;
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
                App.SoundCloud.AudioPlayer.CurrentPlayer.Position = newPos;
            }
        }
    }
}
