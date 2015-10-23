﻿using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SoundCloudPlus.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FollowingPage : Page
    {
        public FollowingPage()
        {
            this.InitializeComponent();
        }
        private FollowingViewModel _followingPageViewModel;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _followingPageViewModel =
                    (FollowingViewModel)Resources["FollowingViewModel"];
                if (App.SoundCloud.IsAuthenticated)
                {
                    _followingPageViewModel.FollowingsCollection = await App.SoundCloud.GetFollowings(App.SoundCloud.CurrentUser.Id);
                }
            }
            base.OnNavigatedTo(e);
        }
        
        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            currentView.BackRequested -= CurrentView_BackRequested;
        }
    }
}
