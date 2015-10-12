using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Track> _streamCollection;
        public ObservableCollection<Track> StreamCollection
        {
            get { return _streamCollection; }
            set { _streamCollection = value; OnPropertyChanged(nameof(StreamCollection)); }
        }

        public MainPageViewModel()
        {
            //StreamCollection = new ObservableCollection<Track>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}