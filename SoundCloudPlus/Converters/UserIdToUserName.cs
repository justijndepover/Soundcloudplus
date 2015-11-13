using System;
using Windows.UI.Xaml.Data;

namespace SoundCloudPlus.Converters
{
    public class UserIdToUserName: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int val = (int)value;

            /*var task = Task.Run(async () =>
            {*/
                //return await App.SoundCloud.GetUser(val);
            /*});
            return new TaskCompletionNotifier<string>(task);*/
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
