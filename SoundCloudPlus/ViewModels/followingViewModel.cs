﻿using SoundCloudPlus.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SoundCloudPlus.ViewModels
{
    public class FollowingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
