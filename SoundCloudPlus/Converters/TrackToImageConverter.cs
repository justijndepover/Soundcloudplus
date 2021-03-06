﻿using System;
using Windows.UI.Xaml.Data;
using ClassLibrary.Models;

namespace SoundCloudPlus.Converters
{
    public class TrackToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                Track t = (Track)value;
                if (t.ArtworkUrl != null)
                {
                    return t.ArtworkUrl;
                }
                return t.User.AvatarUrl;
            }
            catch
            {
                return null;
            }
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
