using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class FollowingPage : Page
    {
        private BackgroundWorker bwFollowing = new BackgroundWorker();
        private ObservableCollection<User> newFollowingCollection = new ObservableCollection<User>();
        private double verticalOffsetStream;
        private double maxVerticalOffsetStream;
        public FollowingPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        #region BackgroundWorkerStream
        private void initBwStream()
        {
            bwFollowing.DoWork += BwFollowing_DoWork;
            bwFollowing.WorkerSupportsCancellation = true;
            bwFollowing.RunWorkerCompleted += BwFollowing_RunWorkerCompleted;
        }

        private void BwFollowing_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (User u in newFollowingCollection)
                {
                    _followingPageViewModel.FollowingsCollection.Add(u);
                }
            }
            catch (Exception) { }

            bwFollowing.CancelAsync();
        }

        private void BwFollowing_DoWork(object sender, DoWorkEventArgs e)
        {
            FollowingScroller();
        }
        #endregion

        private FollowingPageViewModel _followingPageViewModel;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _followingPageViewModel =
                    (FollowingPageViewModel)Resources["FollowingViewModel"];
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

        private void FollowingGridView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 200, 400, FollowingGridView);
        }

        private void onAvatarClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int userId = (int)b.Tag;

            MainPage.Current.Navigate(sender, "profile");
        }

        #region StreamScroller
        private async void FollowingScroller()
        {
            var e = App.SoundCloud.GetStreamNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api-v2.soundcloud.com", "");
                ObservableCollection<User> newCollection = await App.SoundCloud.GetFollowings(App.SoundCloud.CurrentUser.Id, b);
                newFollowingCollection = newCollection;
            }
        }

        private void SvFollowing_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            verticalOffsetStream = svFollowing.VerticalOffset;
            maxVerticalOffsetStream = svFollowing.ScrollableHeight;

            if (maxVerticalOffsetStream < 0 || verticalOffsetStream == maxVerticalOffsetStream)
            {
                if (bwFollowing.IsBusy == false)
                {
                    bwFollowing.RunWorkerAsync();
                }
            }
        }
        #endregion


    }
}
