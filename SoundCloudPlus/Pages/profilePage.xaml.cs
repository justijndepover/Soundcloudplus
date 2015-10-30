using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            this.InitializeComponent();
        }
        private ProfilePageViewModel _profilePageViewModel;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _profilePageViewModel =
                    (ProfilePageViewModel)Resources["ProfilePageViewModel"];
                if (App.SoundCloud.IsAuthenticated)
                {
                    _profilePageViewModel.UserObject = App.SoundCloud.CurrentUser;
                    int id = _profilePageViewModel.UserObject.Id;
                    _profilePageViewModel.PlaylistCollection =
                        await App.SoundCloud.GetOwnPlaylists(id);
                    _profilePageViewModel.RepostCollection =
                        await App.SoundCloud.GetReposts(id);
                    _profilePageViewModel.TrackCollection = await App.SoundCloud.GetTracks(id);
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
        
        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            /*Button b = sender as Button;
            SolidColorBrush sCoBr = new SolidColorBrush(Colors.Orange);
            ChangeColorButtons();
            switch (b.Name)
            {
                case "btnAll":
                    makeVisibleInvisible(true, false, false, false);
                    b.Background = sCoBr;
                    break;
                case "btnTracks":
                    makeVisibleInvisible(false, true, false, false);
                    b.Background = sCoBr;
                    break;
                case "btnPlaylist":
                    makeVisibleInvisible(false, false, true, false);
                    b.Background = sCoBr;
                    break;
                case "btnReposts":
                    makeVisibleInvisible(false, false, false, true);
                    b.Background = sCoBr;
                    break;
            }*/
        }

       /* private void ChangeColorButtons()
        {
            SolidColorBrush c = new SolidColorBrush(Colors.White);
            btnTracks.Background = btnAll.Background = btnReposts.Background = btnPlaylist.Background = c;
        }*/

        private void makeVisibleInvisible(bool all, bool trackCollection, bool playlistCollection, bool repostCollection)
        {
            if (all)
            {
                TrackCollectionGridView.Visibility = Visibility.Visible;
                PlaylistCollectionGridView.Visibility = Visibility.Visible;
                RepostCollectionGridView.Visibility = Visibility.Visible;
            }
            else
            {
                TrackCollectionGridView.Visibility = Visibility.Collapsed;
                PlaylistCollectionGridView.Visibility = Visibility.Collapsed;
                RepostCollectionGridView.Visibility = Visibility.Collapsed;

                if (trackCollection)
                {
                    TrackCollectionGridView.Visibility = Visibility.Visible;
                }

                if (playlistCollection)
                {
                    PlaylistCollectionGridView.Visibility = Visibility.Visible;
                }

                if (repostCollection)
                {
                    RepostCollectionGridView.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
