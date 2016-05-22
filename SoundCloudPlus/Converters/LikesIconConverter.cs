using System;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    public class LikesIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isLiked = (bool) value;
            if (isLiked)
            {
                return "\xE0A5";
            }
            return "\xE006";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
