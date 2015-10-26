using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class PlaylistViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private PlaylistObject _playlistObject;
        public PlaylistObject PlaylistObject
        {
            get { return _playlistObject; }
            set { _playlistObject = value; OnPropertyChanged(nameof(PlaylistObject)); }
        }

        private ObservableCollection<PlaylistCollection> _playlistCollection;
        public ObservableCollection<PlaylistCollection> PlaylistCollection
        {
            get { return _playlistCollection; }
            set { _playlistCollection = value; OnPropertyChanged(nameof(PlaylistCollection)); }
        }
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
