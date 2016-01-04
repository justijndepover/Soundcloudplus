using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                return "Visible";
            }
            else
            {
                return "Collapsed";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
