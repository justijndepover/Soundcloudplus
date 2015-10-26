using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class FollowerPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<User> _followersCollection;
        public ObservableCollection<User> FollowersCollection
        {
            get { return _followersCollection; }
            set { _followersCollection = value; OnPropertyChanged(nameof(FollowersCollection)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
