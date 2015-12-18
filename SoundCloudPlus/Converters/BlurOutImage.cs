using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using ClassLibrary.Common;

namespace SoundCloudPlus.Converters
{
    class BlurOutImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Image v = value as Image;
            //BitmapImage b = await BlurImage.BlurOutImage(v);
            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
