using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class ProfilePageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private User _userObject;
        public User UserObject
        {
            get { return _userObject; }
            set { _userObject = value; OnPropertyChanged(nameof(UserObject)); }
        }

        private ObservableCollection<RepostCollection> _repostCollection;
        public ObservableCollection<RepostCollection> RepostCollection
        {
            get { return _repostCollection; }
            set { _repostCollection = value; OnPropertyChanged(nameof(RepostCollection)); }
        }

        private ObservableCollection<Playlist> _playlistCollection;
        public ObservableCollection<Playlist> PlaylistCollection
        {
            get { return _playlistCollection; }
            set { _playlistCollection = value; OnPropertyChanged(nameof(PlaylistCollection)); }
        }

        private ObservableCollection<Track> _trackCollection;
        public ObservableCollection<Track> TrackCollection
        {
            get { return _trackCollection; }
            set { _trackCollection = value; OnPropertyChanged(nameof(TrackCollection)); }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; OnPropertyChanged(nameof(CurrentUser)); }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
