using System.ComponentModel;
using System.Runtime.CompilerServices;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class ProfilePageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
