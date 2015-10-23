using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class FollowingViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<User> _followingsCollection;
        public ObservableCollection<User> FollowingsCollection
        {
            get { return _followingsCollection; }
            set { _followingsCollection = value; OnPropertyChanged(nameof(FollowingsCollection)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
