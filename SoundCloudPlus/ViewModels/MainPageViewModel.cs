using System.Collections.ObjectModel;
using ClassLibrary.Models;

namespace SoundCloudPlus.ViewModels
{
    public class MainPageViewModel
    {
        public ObservableCollection<Track> ExploreCollection { get; set; }

        public MainPageViewModel()
        {
            
        }

    }
}