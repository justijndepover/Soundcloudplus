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
    public class AcitivityViewModel: INotifyPropertyChanged
    {
        private ObservableCollection<Activity> _activityCollection;
        public ObservableCollection<Activity> ActivityCollection
        {
            get { return _activityCollection; }
            set { _activityCollection = value; OnPropertyChanged(nameof(ActivityCollection)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
