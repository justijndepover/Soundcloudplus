using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Enough.Storage;
using SoundCloudPlus.ViewModels;
using Windows.UI.Notifications;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        private SettingPageViewModel _settingPageViewModel;
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
                CboTheme.SelectedIndex = 1;
            }
            else if(e == ElementTheme.Dark)
            {
                CboTheme.SelectedIndex = 2;
            }
            else
            {
                CboTheme.SelectedIndex = 0;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if(e.NavigationMode != NavigationMode.Back)
            {
                _settingPageViewModel = (SettingPageViewModel)Resources["SettingViewModel"];
                _settingPageViewModel.ActiveUser = App.SoundCloud.CurrentUser;
                bool LiveTilesBool = await StorageHelper.TryLoadObjectAsync<bool>("LiveTilesEnabled");
                if (LiveTilesBool)
                {
                    LivetilesToggle.IsOn = true;
                }

                bool ToastsEnabled = await StorageHelper.TryLoadObjectAsync<bool>("ToastsEnabled");
                if (ToastsEnabled)
                {
                    ToastToggle.IsOn = true;
                }
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bool u = await App.SoundCloud.SignIn();
        }

        private async void LivetilesToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch.IsOn)
            {
                await StorageHelper.SaveObjectAsync(true, "LiveTilesEnabled");
            }
            else
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
                await StorageHelper.SaveObjectAsync(false, "LiveTilesEnabled");
            }
        }

        private async void ToastToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch.IsOn)
            {
                await StorageHelper.SaveObjectAsync(true, "ToastsEnabled");
            }
            else
            {
                await StorageHelper.SaveObjectAsync(false, "ToastsEnabled");
            }
        }
    }
}
