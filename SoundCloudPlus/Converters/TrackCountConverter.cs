using System;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    class TrackCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int val = (int)value;
            if (val < 2)
            {
                return val + " Track";
            }
            return val + " Tracks";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
