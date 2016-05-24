using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ClassLibrary.Models;

namespace SoundCloudPlus.Converters
{
    public class SearchCollectionTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element != null && item is SearchCollection)
            {
                SearchCollection stream = (SearchCollection) item;
                if (stream.Playlist != null)
                {
                    return  Application.Current.Resources["PlaylistDataTemplateContentPresenter"] as DataTemplate;
                }
                if (stream.Track != null)
                {
                    return Application.Current.Resources["TrackDataTemplateContentPresenter"] as DataTemplate;
                }
                if (stream.User != null)
                {
                    return Application.Current.Resources["UserDataTemplateContentPresenter"] as DataTemplate;
                }
            }
            return null;
        }
    }
}
