using System.Collections.ObjectModel;
using ClassLibrary.Models;

namespace SoundCloudPlus.ViewModels
{
    public class MainPageViewModel
    {
        public ObservableCollection<string> NavigationCollection { get; set; }
        public ObservableCollection<Track> ExploreCollection { get; set; }

        public MainPageViewModel()
        {
            NavigationCollection = new ObservableCollection<string>() {"Sign in", "Settings"};
        }

    }
}