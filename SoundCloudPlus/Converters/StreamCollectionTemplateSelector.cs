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
                    return  Application.Current.Resources["PlaylistDataTemplateContentPresenter"] as DataTemplate;
                }
                if (stream.Track != null)
                {
                    return Application.Current.Resources["TrackDataTemplateContentPresenter"] as DataTemplate;
                }
            }
            return null;
        }
    }
}
