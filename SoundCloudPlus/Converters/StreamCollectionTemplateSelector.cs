using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using ClassLibrary.Models;

namespace SoundCloudPlus.Converters
{
    public class StreamCollectionTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element != null && item is StreamCollection)
            {
                StreamCollection stream = (StreamCollection) item;
                if (stream.Playlist != null)
                {
                    element.DataContext = stream.Playlist;
                    return Application.Current.Resources["PlaylistDataTemplate"] as DataTemplate;
                }
                if (stream.Track != null)
                {
                    element.DataContext = stream.Track;
                    return Application.Current.Resources["TrackDataTemplate"] as DataTemplate;
                }
            }
            return null;
        }
    }
}
