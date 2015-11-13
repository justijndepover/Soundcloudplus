﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClassLibrary.Models;
using SoundCloudPlus.Annotations;

namespace SoundCloudPlus.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private Activity _activityObject;
        public Activity ActivityObject
        {
            get { return _activityObject; }
            set { _activityObject = value; OnPropertyChanged(nameof(ActivityObject)); }
        }
        private ObservableCollection<ActivityCollection> _activityCollection;
        public ObservableCollection<ActivityCollection> ActivityCollection
        {
            get { return _activityCollection; }
            set { _activityCollection = value; OnPropertyChanged(nameof(ActivityCollection)); }
        }

        private Track _playingTrack;

        public Track PlayingTrack
        {
            get { return _playingTrack; }
            set { _playingTrack = value; OnPropertyChanged(nameof(PlayingTrack)); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
