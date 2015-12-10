using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ClassLibrary;
using ClassLibrary.Common;
using Microsoft.ApplicationInsights;
using SoundCloudPlus.Pages;

namespace SoundCloudPlus
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App
    {
        public static SoundCloud SoundCloud { get; set; }
        public static ContinuationManager ContinuationManager { get; private set; }
        public static Frame RootFrame { get; set; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync(
                WindowsCollectors.Metadata |
                WindowsCollectors.Session);
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached && e.PrelaunchActivated)
            {
                DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            SoundCloud = new SoundCloud();

            CreateRootFrame();

            if (RootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                RootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
        protected async override void OnActivated(IActivatedEventArgs e)
        {

            ContinuationManager = new ContinuationManager();

            CreateRootFrame();

            // Restore the saved session state only when appropriate
            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                try
                {
                    await SuspensionManager.RestoreAsync();
                }
                catch (SuspensionManagerException)
                {
                    //Something went wrong restoring state.
                    //Assume there is no state and continue
                }
            }

            //Check if this is a continuation
            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null)
            {
                ContinuationManager.Continue(continuationEventArgs);
            }

            Window.Current.Activate();
        }
        private void CreateRootFrame()
        {
            if (RootFrame != null)
                return;
            
            RootFrame = new Frame();
                         
            SuspensionManager.RegisterFrame(RootFrame, "AppFrame");

            RootFrame.Language = ApplicationLanguages.Languages[0];

            RootFrame.NavigationFailed += OnNavigationFailed;
            
            Window.Current.Content = RootFrame;
        }

    }
}
