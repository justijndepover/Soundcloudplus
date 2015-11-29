using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class SearchPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Track> _trackSearchCollection;
        public ObservableCollection<Track> TrackSearchCollection
        {
            get { return _trackSearchCollection; }
            set { _trackSearchCollection = value; OnPropertyChanged(nameof(TrackSearchCollection)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
