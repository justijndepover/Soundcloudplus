using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Enough.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            SetThemeIndex();
        }

        private void SetThemeIndex()
        {
            ElementTheme e = App.RootFrame.RequestedTheme;
            if(e == ElementTheme.Light)
            {
                cboTheme.SelectedIndex = 1;
            }
            else if(e == ElementTheme.Dark)
            {
                cboTheme.SelectedIndex = 2;
            }
            else
            {
                cboTheme.SelectedIndex = 0;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;
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

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox c = sender as ComboBox;
            if(c.SelectedIndex == 0)
            {
                App.RootFrame.RequestedTheme = ElementTheme.Default;
            }
            else if(c.SelectedIndex == 1)
            {
                App.RootFrame.RequestedTheme = ElementTheme.Light;
            }
            else if (c.SelectedIndex == 2)
            {
                App.RootFrame.RequestedTheme = ElementTheme.Dark;
            }

            await StorageHelper.SaveObjectAsync(App.RootFrame.RequestedTheme);
        }
    }
}
