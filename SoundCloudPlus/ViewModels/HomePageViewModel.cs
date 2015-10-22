using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;
using GalaSoft.MvvmLight.Command;

namespace SoundCloudPlus.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Track> _streamCollection;
        public ObservableCollection<Track> StreamCollection
        {
            get { return _streamCollection; }
            set { _streamCollection = value; OnPropertyChanged(nameof(StreamCollection)); }
        }

        private ObservableCollection<Track> _exploreCollection;
        private Track _selectedExploreTrack;

        public ObservableCollection<Track> ExploreCollection
        {
            get { return _exploreCollection; }
            set { _exploreCollection = value; OnPropertyChanged(nameof(ExploreCollection)); }
        }

        private Activity _activityCollection;
        public Activity ActivityCollection
        {
            get { return _activityCollection; }
            set { _activityCollection = value; OnPropertyChanged(nameof(ActivityCollection)); }
        }

        public Track SelectedExploreTrack
        {
            get { return _selectedExploreTrack; }
            set { _selectedExploreTrack = value; OnPropertyChanged(nameof(SelectedExploreTrack));}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}