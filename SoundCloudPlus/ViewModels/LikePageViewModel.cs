using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class LikePageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Track> _trackLikesCollection;
        public ObservableCollection<Track> TrackLikesCollection
        {
            get { return _trackLikesCollection; }
            set { _trackLikesCollection = value; OnPropertyChanged(nameof(TrackLikesCollection)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
