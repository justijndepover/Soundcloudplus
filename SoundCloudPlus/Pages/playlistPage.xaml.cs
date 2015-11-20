using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Common;
using SoundCloudPlus.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistPage : Page
    {
        public PlaylistPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private PlaylistPageViewModel _playlistViewModel;
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if (e.NavigationMode != NavigationMode.Back)
            {
                _playlistViewModel =
                    (PlaylistPageViewModel)Resources["PlaylistViewModel"];
                if (App.SoundCloud.IsAuthenticated)
                {
                    _playlistViewModel.PlaylistCollection = await App.SoundCloud.GetPlaylists(App.SoundCloud.CurrentUser.Id);
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

        private void PlaylistGridView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Screen.MakeResponsive(e, 200, 400, PlaylistGridView);
        }
    }
}
