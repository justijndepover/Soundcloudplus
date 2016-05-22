﻿using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Notifications;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var currentView = SystemNavigationManager.GetForCurrentView();

            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentView_BackRequested;

            if(e.NavigationMode != NavigationMode.Back)
            {
                var version = Package.Current.Id.Version;
                VersionTextBlock.Text = "Version: "+ version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
                _settingPageViewModel = (SettingPageViewModel)Resources["SettingViewModel"];
                _settingPageViewModel.ActiveUser = App.SoundCloud.CurrentUser;
                bool liveTilesBool = (bool) ApplicationSettingsHelper.ReadRoamingSettingsValue<bool>("LiveTilesEnabled");
                if (liveTilesBool)
                {
                    LivetilesToggle.IsOn = true;
                }

                bool toastsEnabled = (bool) ApplicationSettingsHelper.ReadRoamingSettingsValue<bool>("ToastsEnabled");
                if (toastsEnabled)
                {
                    ToastToggle.IsOn = true;
                }

                bool bandTilesEnabled = (bool)ApplicationSettingsHelper.ReadLocalSettingsValue<bool>("BandTilesEnabled");
                if (bandTilesEnabled)
                {
                    BandToggle.IsOn = true;
                }

                bool debugModeEnabled = (bool)ApplicationSettingsHelper.ReadLocalSettingsValue<bool>("DebugModeEnabled");
                if (debugModeEnabled)
                {
                    DebugToggle.IsOn = true;
                }
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            ApplicationSettingsHelper.SaveRoamingSettingsValue("ElementTheme", App.RootFrame.RequestedTheme);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bool u = await App.SoundCloud.SignIn();
        }

        private void LivetilesToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch.IsOn)
            {
                ApplicationSettingsHelper.SaveRoamingSettingsValue("LiveTilesEnabled", true);
            }
            else
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
                ApplicationSettingsHelper.SaveRoamingSettingsValue("LiveTilesEnabled", false);
            }
        }

        private void ToastToggle_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch.IsOn)
            {
                ApplicationSettingsHelper.SaveRoamingSettingsValue("ToastsEnabled", true);
            }
            else
            {
                ApplicationSettingsHelper.SaveRoamingSettingsValue("ToastsEnabled", false);
            }
        }

        private async void BandToggle_OnToggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null && toggleSwitch.IsOn)
            {
                await App.BandConnections.ConnectBand();
                if (App.BandConnections.BandClient != null)
                {
                    await App.BandConnections.CreateAndPushTileAsync("10Sound");
                }
                ApplicationSettingsHelper.SaveLocalSettingsValue("BandTilesEnabled", true);
            }
            else
            {
                ApplicationSettingsHelper.SaveLocalSettingsValue("BandTilesEnabled", false);
            }
        }

        private void DebugToggle_OnToggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null && toggleSwitch.IsOn)
            {
                ApplicationSettingsHelper.SaveLocalSettingsValue("DebugModeEnabled", true);
            }
            else
            {
                ApplicationSettingsHelper.SaveLocalSettingsValue("DebugModeEnabled", false);
            }
        }
    }
}
