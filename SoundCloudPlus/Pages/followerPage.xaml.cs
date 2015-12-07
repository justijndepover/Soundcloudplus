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
    public sealed partial class FollowerPage : Page
    {
        private BackgroundWorker bwFollower = new BackgroundWorker();
        private ObservableCollection<User> newFollowerCollection = new ObservableCollection<User>();
        private double verticalOffsetFollower;
        private double maxVerticalOffsetFollower;
        private FollowerPageViewModel _followerViewModel;
        public FollowerPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            initBwFollower();
        }

        #region BackgroundWorkerFollower

        private void initBwFollower()
        {
            bwFollower.DoWork += BwFollower_DoWork;
            bwFollower.WorkerSupportsCancellation = true;
            bwFollower.RunWorkerCompleted += BwFollower_RunWorkerCompleted;
        }

        private void BwFollower_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (User u in newFollowerCollection)
                {
                    _followerViewModel.FollowersCollection.Add(u);
                }
                newFollowerCollection.Clear();
            }
            catch (Exception) { }

            bwFollower.CancelAsync();
        }

        private void BwFollower_DoWork(object sender, DoWorkEventArgs e)
        {
            FollowerScroller();
        }

        #endregion

        #region FollowerScroller

        private async void FollowerScroller()
        {
            var e = App.SoundCloud.GetFollowerNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api.soundcloud.com", "");
                ObservableCollection<User> newCollection = await App.SoundCloud.GetFollowers(App.SoundCloud.CurrentUser.Id, b);
                newFollowerCollection = newCollection;
            }
        }

        private void SvFollower_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            verticalOffsetFollower = svFollower.VerticalOffset;
            maxVerticalOffsetFollower = svFollower.ScrollableHeight;

            if (maxVerticalOffsetFollower < 0 || verticalOffsetFollower == maxVerticalOffsetFollower)
            {
                if (bwFollower.IsBusy == false)
                {
                    bwFollower.RunWorkerAsync();
                }
            }
        }

        #endregion
        
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _followerViewModel =
                    (FollowerPageViewModel)Resources["FollowerViewModel"];
                if (await App.SoundCloud.IsAuthenticated())
                {
                    _followerViewModel.FollowersCollection = await App.SoundCloud.GetFollowers(App.SoundCloud.CurrentUser.Id);
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

        private void FollowerGridView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 200, 400, FollowerGridView);
        }

        private void onAvatarClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int userId = (int)b.Tag;

            MainPage.Current.Navigate(sender, "profile");
        }
    }
}
