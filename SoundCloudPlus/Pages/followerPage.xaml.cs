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
        private BackgroundWorker _bwFollower = new BackgroundWorker();
        private ObservableCollection<User> _newFollowerCollection = new ObservableCollection<User>();
        private double _verticalOffsetFollower;
        private double _maxVerticalOffsetFollower;
        private FollowerPageViewModel _followerViewModel;
        public FollowerPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            InitBwFollower();
        }

        #region BackgroundWorkerFollower

        private void InitBwFollower()
        {
            _bwFollower.DoWork += BwFollower_DoWork;
            _bwFollower.WorkerSupportsCancellation = true;
            _bwFollower.RunWorkerCompleted += BwFollower_RunWorkerCompleted;
        }

        private void BwFollower_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (User u in _newFollowerCollection)
                {
                    _followerViewModel.FollowersCollection.Add(u);
                }
                _newFollowerCollection.Clear();
            }
            catch (Exception) { }

            _bwFollower.CancelAsync();
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
                _newFollowerCollection = newCollection;
            }
        }

        private void SvFollower_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            _verticalOffsetFollower = SvFollower.VerticalOffset;
            _maxVerticalOffsetFollower = SvFollower.ScrollableHeight;

            if (_maxVerticalOffsetFollower < 0 || _verticalOffsetFollower == _maxVerticalOffsetFollower)
            {
                if (_bwFollower.IsBusy == false)
                {
                    _bwFollower.RunWorkerAsync();
                }
            }
        }

        #endregion
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _followerViewModel =
                    (FollowerPageViewModel)Resources["FollowerViewModel"];
                int uId = MainPage.Current.UserId;
                if (await App.SoundCloud.IsAuthenticated())
                {
                    UpdateFollowerCollection(uId);
                }
            }
            base.OnNavigatedTo(e);
        }

        private async void UpdateFollowerCollection(int id)
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                int limit = Screen.GetLimitItems(height, width, 200, 400, 200, 400);
                _followerViewModel.FollowersCollection = await App.SoundCloud.GetFollowers(id, limit);
            }
            catch (Exception)
            {
                Application.Current.Exit();
            }
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

        private void OnAvatarClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int userId = (int)b.Tag;

            MainPage.Current.Navigate(sender, "profile");
        }
    }
}
