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

        static Media() { }

        private Media()
        {
            mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;

            mediaPlayer.Source = Playlist;

            Playlist.CurrentItemChanged += Playlist_CurrentItemChanged;

            mediaPlayer.Volume = 0.001;

            mediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;

            //CurrentSong = new StorageFile();
        }

        private void MediaPlayer_VolumeChanged(MediaPlayer sender, object args)
        {
            if (!((mediaPlayer.Volume * 1000.0) % 10.0 == 3.0))
            {
                mediaPlayer.Volume = mediaPlayer.Volume / 10.0 + 0.003;
            }
            
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
            //StorageFile file = SongListStorage.Songs[song].File;
            var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(SongListStorage.SongDict[song].File));
            Playlist.Items.Add(mediaPlaybackItem);
            mediaPlayer.Play();
            SongListStorage.PlaylistRepresentation.Clear();
            SongListStorage.PlaylistRepresentation.Add(SongListStorage.SongDict[song]);

        }

        public void PlayPlaylist(ObservableCollection<Song> Songs, int Pos)
        {
            Playlist.Items.Clear(); //Clears the playlist
            foreach (Song song in Songs)
            {
                //Media.Instance.
                addSong(song.id);

            }
            Playlist.MoveTo((uint)Pos - 1);

        }

        public void addSong(int song)
        {
            var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(SongListStorage.SongDict[song].File));
            Playlist.Items.Add(mediaPlaybackItem);
            mediaPlayer.Play();
            SongListStorage.PlaylistRepresentation.Add(SongListStorage.SongDict[song]);
            //mediaPlayer.Source = MediaSource.CreateFromStorageFile(song);
        }


        



    }
}
