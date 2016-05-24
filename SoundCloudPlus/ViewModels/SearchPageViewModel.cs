using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class SearchPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<SearchCollection> _trackSearchCollection;
        public ObservableCollection<SearchCollection> TrackSearchCollection
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
