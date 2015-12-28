using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class SettingPageViewModel : INotifyPropertyChanged
    {
        private User _activeUser;

        public User ActiveUser
        {
            get { return _activeUser; }
            set { _activeUser = value; OnPropertyChanged(nameof(ActiveUser)); }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
