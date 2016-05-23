using ClassLibrary.Common;
using ClassLibrary.Models;

namespace SoundCloudPlus.ViewModels
{
    public class CommandsViewModel
    {
        public Command LikeCommand { get; set; }
        public CommandsViewModel()
        {
            LikeCommand = new Command();
            LikeCommand.CanExecuteFunc = obj => true;
            LikeCommand.ExecuteFunc = LikeUnlikeTrack;
        }
        public async void LikeUnlikeTrack(object parameter)
        {
            Track track = (Track) parameter;

            if (track.IsLiked)
            {
                if (await App.SoundCloud.UnlikeTrack(track.Id))
                {
                    track.IsLiked = false;
                }
            }
            else
            {
                if (await App.SoundCloud.LikeTrack(track.Id))
                {
                    track.IsLiked = true;
                }
            }
        }
    }
}
