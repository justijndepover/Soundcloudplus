using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using ClassLibrary.Models;

namespace SoundCloudPlus.Converters
{
    public class PlaylistToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                Playlist p = (Playlist)value;
                if (p.ArtworkUrl != null)
                {
                    return p.ArtworkUrl;
                }
                return p.User.AvatarUrl;
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
