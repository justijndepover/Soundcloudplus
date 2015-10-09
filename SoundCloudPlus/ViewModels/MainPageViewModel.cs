using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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