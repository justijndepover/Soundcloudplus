using System;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    public class ImageToHighResConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string source = (string) value;
            try
            {
                return source?.Replace("large.jpg", "t500x500.jpg");
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
