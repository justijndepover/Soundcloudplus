using System;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    class ActivityTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string s = (string)value;

            if(s == "affiliation")
            {
                s = "started following you";
            }

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
