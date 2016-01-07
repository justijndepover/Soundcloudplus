using System;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    public class IntToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int val = System.Convert.ToInt32(value);
            DateTime time = new DateTime();
            time = time.AddMilliseconds(val);
            if(time.Hour != 0)
            {
                return time.ToString("hh:mm:ss");
            }
            return time.ToString("mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
