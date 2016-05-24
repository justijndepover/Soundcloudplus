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
            if (parameter is Track)
            {
                Track track = (Track)parameter;

                if (track.IsLiked)
                {
                    if (await App.SoundCloud.UnlikeTrack(track.Id))
                    {
                        track.LikesCount--;
                        track.IsLiked = false;
                    }
                }
                else
                {
                    if (await App.SoundCloud.LikeTrack(track.Id))
                    {
                        track.LikesCount++;
                        track.IsLiked = true;
                    }
                }
            }else if(parameter is Playlist)
            {
                Playlist playlist = (Playlist)parameter;

                if (playlist.IsLiked)
                {
                    if (await App.SoundCloud.UnlikePlaylist(playlist.Id))
                    {
                        playlist.TrackCount--;
                        playlist.IsLiked = false;
                    }
                }
                else
                {
                    if (await App.SoundCloud.LikePlaylist(playlist.Id))
                    {
                        playlist.TrackCount++;
                        playlist.IsLiked = true;
                    }
                }
            }
        }
    }
}
