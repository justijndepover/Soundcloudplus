using System;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    public class IntToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int val = (int)value;
            DateTime time = new DateTime();
            time = time.AddMilliseconds(val);
            return time.Minute + ":" + time.Second;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
