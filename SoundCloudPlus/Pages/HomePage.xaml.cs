using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
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

        private void StreamGridView_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            ItemsWrapGrid myItemsPanel = (ItemsWrapGrid)StreamGridView.ItemsPanelRoot;
            double screenWidth = e.NewSize.Width;
            int? itemsNumber = StreamGridView.Items?.Count;
            if (itemsNumber > 0)
            {
                if (myItemsPanel != null) myItemsPanel.ItemWidth = (screenWidth / GetNumberOfColumns(screenWidth));
            }
        }

        private int GetNumberOfColumns(double screenWidth)
        {
            int w = 799;
            int c = 1;
            while (true)
            {
                if (screenWidth <= w)
                {
                    return c;
                }
                c++;
                w += 400;
            }
        }

        private void ExploreGridView_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            ItemsWrapGrid myItemsPanel = (ItemsWrapGrid)ExploreGridView.ItemsPanelRoot;
            double screenWidth = e.NewSize.Width;
            int? itemsNumber = ExploreGridView.Items?.Count;
            if (itemsNumber > 0)
            {
                if (myItemsPanel != null) myItemsPanel.ItemWidth = (screenWidth / GetNumberOfColumns(screenWidth));
            }
        }
    }
}
