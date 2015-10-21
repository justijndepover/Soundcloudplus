using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace ClassLibrary.Models
{
    public class TitleBar
    {
        #region ForegroundColor
        public static readonly DependencyProperty ForegroundColorProperty =
        DependencyProperty.RegisterAttached("ForegroundColor", typeof(Color),
        typeof(TitleBar),
        new PropertyMetadata(null, OnForegroundColorPropertyChanged));

        public static Color GetForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ForegroundColorProperty);
        }

        public static void SetForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ForegroundColorProperty, value);
        }

        private static void OnForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = color;
        }
        #endregion

        #region BackgroundColor
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.RegisterAttached("BackgroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnBackgroundColorPropertyChanged));

        public static Color GetBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(BackgroundColorProperty);
        }

        public static void SetBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(BackgroundColorProperty, value);
        }

        private static void OnBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = color;
        }
        #endregion

        #region ButtonForeground
        public static readonly DependencyProperty ButtonForegroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonForegroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonForegroundColorPropertyChanged));

        public static Color GetButtonForegroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonForegroundColorProperty);
        }

        public static void SetButtonForegroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonForegroundColorProperty, value);
        }

        private static void OnButtonForegroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = color;
        }
        #endregion

        #region ButtonBackground
        public static readonly DependencyProperty ButtonBackgroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonBackgroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonBackgroundColorPropertyChanged));

        public static Color GetButtonBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonBackgroundColorProperty);
        }

        public static void SetButtonBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonBackgroundColorProperty, value);
        }

        private static void OnButtonBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = color;
        }
        #endregion

        #region ButtonHoverBackground
        public static readonly DependencyProperty ButtonHoverBackgroundColorProperty =
            DependencyProperty.RegisterAttached("ButtonHoverBackgroundColor", typeof(Color),
            typeof(TitleBar),
            new PropertyMetadata(null, OnButtonHoverBackgroundColorPropertyChanged));

        public static Color GetButtonHoverBackgroundColor(DependencyObject d)
        {
            return (Color)d.GetValue(ButtonHoverBackgroundColorProperty);
        }

        public static void SetButtonHoverBackgroundColor(DependencyObject d, Color value)
        {
            d.SetValue(ButtonHoverBackgroundColorProperty, value);
        }

        private static void OnButtonHoverBackgroundColorPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var color = (Color)e.NewValue;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonHoverBackgroundColor = color;
        }
        #endregion
    }
}
