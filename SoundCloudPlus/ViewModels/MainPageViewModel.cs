using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using ClassLibrary.Common;
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
        private List<Track> _playingList;
        public List<Track> PlayingList
        {
            get { return _playingList; }
            set { _playingList = value; OnPropertyChanged(nameof(PlayingList)); }
        }

        private string _pageTitle;

        public string PageTitle
        {
            get { return _pageTitle; }
            set { _pageTitle = value; OnPropertyChanged(nameof(PageTitle)); }
        }

        private User _loggedInUser;

        public User LoggedInUser
        {
            get { return _loggedInUser; }
            set { _loggedInUser = value; OnPropertyChanged(nameof(LoggedInUser)); }
        }

        private Visibility _pinButtonVisibility;
        public Visibility PinButtonVisibility
        {
            get { return _pinButtonVisibility; }
            set { _pinButtonVisibility = value; OnPropertyChanged(nameof(PinButtonVisibility));}
        }

        private Visibility _bandIconVisibility;
        public Visibility BandIconVisibility
        {
            get { return _bandIconVisibility; }
            set { _bandIconVisibility = value; OnPropertyChanged(nameof(PinButtonVisibility)); }
        }

        private float _bandIconOpacity;
        public float BandIconOpacity
        {
            get { return _bandIconOpacity; }
            set { _bandIconOpacity = value; OnPropertyChanged(nameof(BandIconOpacity)); }
        }


        public MainPageViewModel()
        {
            PinButtonVisibility = Visibility.Collapsed;
            BandIconOpacity = (float)0.2;
            if ((bool)ApplicationSettingsHelper.ReadLocalSettingsValue<bool>("BandTilesEnabled"))
            {
                BandIconVisibility = Visibility.Visible;
            }
            else
            {
                BandIconVisibility = Visibility.Collapsed;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
