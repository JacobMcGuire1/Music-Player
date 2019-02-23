using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace Music_thing
{
    public sealed class Media
    {
        private static readonly Media instance = new Media();

        public MediaPlayer mediaPlayer = new MediaPlayer();

        public MediaPlaybackList Playlist = new MediaPlaybackList();

        

        

        //public Song CurrentSong;

        static Media(){}

        private Media()
        {
            mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;

            mediaPlayer.Source = Playlist;

            Playlist.CurrentItemChanged += Playlist_CurrentItemChanged;

            //CurrentSong = new StorageFile();
        }

        private void Playlist_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            SongListStorage.CurrentPlaceInPlaylist = (int)Playlist.CurrentItemIndex;
            //CurrentSong = Song;
            //throw new NotImplementedException();

        }

        public static Media Instance
        {
            get
            {
                return instance;
            }
        }
    

        public void playSong(int song)
        {
            //mediaPlayer.Source = MediaSource.CreateFromStorageFile(song);
            Playlist.Items.Clear();
            var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(SongListStorage.Songs[song].File));
            Playlist.Items.Add(mediaPlaybackItem);
            mediaPlayer.Play();
            SongListStorage.PlaylistRepresentation.Clear();
            SongListStorage.PlaylistRepresentation.Add(SongListStorage.Songs[song]);

        }

        public void addSong(int song)
        {
            var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(SongListStorage.Songs[song].File));
            Playlist.Items.Add(mediaPlaybackItem);
            mediaPlayer.Play();
            SongListStorage.PlaylistRepresentation.Add(SongListStorage.Songs[song]);
            //mediaPlayer.Source = MediaSource.CreateFromStorageFile(song);
        }



    }
}
