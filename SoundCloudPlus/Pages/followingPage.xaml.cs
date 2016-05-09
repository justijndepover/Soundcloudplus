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
        private BackgroundWorker _bwFollowing = new BackgroundWorker();
        private ObservableCollection<User> _newFollowingCollection = new ObservableCollection<User>();
        private double _verticalOffsetFollowing;
        private double _maxVerticalOffsetFollowing;
        public FollowingPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            InitBwFollowing();
        }

        #region BackgroundWorkerFollowing
        private void InitBwFollowing()
        {
            _bwFollowing.DoWork += BwFollowing_DoWork;
            _bwFollowing.WorkerSupportsCancellation = true;
            _bwFollowing.RunWorkerCompleted += BwFollowing_RunWorkerCompleted;
        }

        private void BwFollowing_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                foreach (User u in _newFollowingCollection)
                {
                    _followingPageViewModel.FollowingsCollection.Add(u);
                }
                _newFollowingCollection.Clear();
            }
            catch (Exception) { }

            _bwFollowing.CancelAsync();
        }

        private void BwFollowing_DoWork(object sender, DoWorkEventArgs e)
        {
            FollowingScroller();
        }
        #endregion

        private FollowingPageViewModel _followingPageViewModel;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _followingPageViewModel =
                    (FollowingPageViewModel)Resources["FollowingViewModel"];
                int uId = MainPage.Current.UserId;
                if (await App.SoundCloud.IsAuthenticated())
                {
                    UpdateFollowingCollection(uId);
                }
            }
            base.OnNavigatedTo(e);
        }

        private async void UpdateFollowingCollection(int id)
        {
            try
            {
                var bounds = Window.Current.Bounds;
                double height = bounds.Height;
                double width = bounds.Width;
                int limit = Screen.GetLimitItems(height, width, 200, 400, 200, 400);
                _followingPageViewModel.FollowingsCollection = await App.SoundCloud.GetFollowings(id, limit);
            }
            catch (Exception ex)
            {
                ErrorLogProxy.LogError(ex.ToString());
                ErrorLogProxy.NotifyError(ex.ToString());
            }
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            if (MainPage.Current.MainFrame.CanGoBack) Frame.GoBack();
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

        private void OnAvatarClick(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            int userId = (int)b.Tag;
            MainPage.Current.Navigate(sender, "profile");
        }

        #region FollowingScroller
        private async void FollowingScroller()
        {
            var e = App.SoundCloud.GetFollowingNextHref();
            if (e != null)
            {
                var b = e.Replace("https://api.soundcloud.com", "");
                ObservableCollection<User> newCollection = await App.SoundCloud.GetFollowings(App.SoundCloud.CurrentUser.Id, b);
                _newFollowingCollection = newCollection;
            }
        }

        private void SvFollowing_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Laad nieuwe items in wanneer scrollviewer op einde is...
            _verticalOffsetFollowing = SvFollowing.VerticalOffset;
            _maxVerticalOffsetFollowing = SvFollowing.ScrollableHeight;

            if (_maxVerticalOffsetFollowing < 0 || _verticalOffsetFollowing == _maxVerticalOffsetFollowing)
            {
                if (_bwFollowing.IsBusy == false)
                {
                    _bwFollowing.RunWorkerAsync();
                }
            }
        }
        #endregion


    }
}
