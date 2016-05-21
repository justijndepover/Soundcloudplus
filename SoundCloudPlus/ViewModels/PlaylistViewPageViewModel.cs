using ClassLibrary.Models;
using SoundCloudPlus.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SoundCloudPlus.ViewModels
{
    public class PlaylistViewPageViewModel : INotifyPropertyChanged
    {
        private Playlist _playlist;
        public Playlist Playlist
        {
            get { return _playlist; }
            set { _playlist = value; OnPropertyChanged(nameof(Playlist)); }
        }

        private Track _selectedTrack;
        public Track SelectedTrack
        {
            get { return _selectedTrack; }
            set { _selectedTrack = value; OnPropertyChanged(nameof(SelectedTrack)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
