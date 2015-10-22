using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private Activity _activityObject;
        public Activity ActivityObject
        {
            get { return _activityObject; }
            set { _activityObject = value; OnPropertyChanged(nameof(ActivityObject)); }
        }
        private ObservableCollection<Collection> _activityCollection;
        public ObservableCollection<Collection> ActivityCollection
        {
            get { return _activityCollection; }
            set { _activityCollection = value; OnPropertyChanged(nameof(ActivityCollection)); }
        }

        private Uri _playingTrack;

        public Uri PlayingTrack
        {
            get { return _playingTrack; }
            set { _playingTrack = value; OnPropertyChanged(nameof(PlayingTrack)); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
