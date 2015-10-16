using System;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    public class ImageToHighResConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string source = (string) value;
            return source.Replace("large.jpg", "t500x500.jpg");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
