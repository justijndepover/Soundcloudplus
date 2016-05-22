using System;
using Windows.UI.Xaml.Data;
using ClassLibrary.Models;

namespace SoundCloudPlus.Converters
{
    public class PlaylistToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                Playlist p = (Playlist) value;
                try
                {
                    if (p.ArtworkUrl != null)
                    {
                        return p.ArtworkUrl;
                    }
                    return p.User?.AvatarUrl;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
