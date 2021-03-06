﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    class PlaylistConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var o = value;
            if(o == null)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
