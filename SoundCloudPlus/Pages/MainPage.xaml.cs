using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ClassLibrary.Models;
using SoundCloudPlus.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SoundCloudPlus.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private MainPageViewModel mainPageViewModel;
        public MainPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainPageViewModel =
                (MainPageViewModel)Resources["MainPageViewModel"];
            mainPageViewModel.ExploreCollection = new ObservableCollection<Track>(await App.SoundCloud.GetExplore());
            base.OnNavigatedTo(e);
        }

        private void NavButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SplitView.IsPaneOpen = !SplitView.IsPaneOpen;
        }
    }
}
