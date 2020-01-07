using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Music_thing
{
    public sealed class Media : INotifyPropertyChanged
        //This class manages the currently playing song and the queued songs.
    {
        private static readonly Media instance = new Media();

        public MediaPlayer mediaPlayer = new MediaPlayer();

        public MediaPlaybackList Playlist = new MediaPlaybackList();

        public ImageSource currentart; //Might need a placeholder one
        public string currenttitle = "";
        public string currentartist = "";

        private bool hasFixedvol = true;

        public int globalVol = 10; //TODO: Get it from the settings

        private int chosenVol = 50;

        public event PropertyChangedEventHandler PropertyChanged;





        //public Song CurrentSong;

        static Media() { }

        private Media()
        {
            mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;

            mediaPlayer.Source = Playlist;

            Playlist.CurrentItemChanged += Playlist_CurrentItemChanged;

            mediaPlayer.Volume = 0.01;

            mediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;

            //The placeholder album art TODO: make it work?
            BitmapImage bitmapImage =
                     new BitmapImage(new Uri("ms-appx:///[Music_thing]/Assets/StoreLogo.png"));

            Currentart = bitmapImage;
            NotifyPropertyChanged();

            //CurrentSong = new StorageFile();
        }

        //Used to inform the UI that something about the currently playing song has changed.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {

            // Your UI update code goes here!
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        //Changes the volume and notifies of this.
        private void MediaPlayer_VolumeChanged(MediaPlayer sender, object args)
        {
            chosenVol = (int)(mediaPlayer.Volume * 100);
            VolChanged();
        }

        //Dodgy volume stuff
        public void VolChanged()
        {
            if (!hasFixedvol)
            {
                hasFixedvol = true;
                mediaPlayer.Volume = chosenVol * (globalVol / 10000.0);
            }
            else
            {
                hasFixedvol = false;
            }

        }


        //Updates song details when the song changes.
        private void Playlist_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            uint thign = Playlist.CurrentItemIndex;
            if (thign == 4294967295) //Magic number?? Perfectly totient.
            {
                SongListStorage.CurrentPlaceInPlaylist = 0;
            }
            else
            {
                SongListStorage.CurrentPlaceInPlaylist = (int)Playlist.CurrentItemIndex;
            }


            Currentart = SongListStorage.GetCurrentSongArt();
            Currenttitle = SongListStorage.GetCurrentSongName();
            Currentartist = SongListStorage.GetCurrentArtistName();
        }

        //Allows the following properties to be accessed


        public string Currenttitle
        {
            get
            {
                return this.currenttitle;
            }
            set
            {
                if (value != this.currenttitle)
                {
                    this.currenttitle = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Currentartist
        {
            get
            {
                return this.currentartist;
            }
            set
            {
                if (value != this.currentartist)
                {
                    this.currentartist = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ImageSource Currentart 
        {
            get
            {
                return this.currentart;
            }
            set
            {
                if (value != this.currentart)
                {
                    this.currentart = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //Returns the static instance of this class, as only one instance can run at a time.
        public static Media Instance
        {
            get
            {
                return instance;
            }
        }

        //Plays the specified song
        public void playSong(string songid)
        {
            //mediaPlayer.Source = MediaSource.CreateFromStorageFile(song);
            Playlist.Items.Clear();
            //StorageFile file = SongListStorage.Songs[song].File;
            var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(SongListStorage.SongDict[songid].File));
            Playlist.Items.Add(mediaPlaybackItem);
            mediaPlayer.Play();
            SongListStorage.PlaylistRepresentation.Clear();
            SongListStorage.PlaylistRepresentation.Add(SongListStorage.SongDict[songid]);

        }

        //Plays the playlist
        public void PlayPlaylist(ObservableCollection<Song> Songs, int Pos)
        {
            Playlist.Items.Clear(); //Clears the playlist
            SongListStorage.PlaylistRepresentation.Clear(); //MAY BE BAD?
            foreach (Song song in Songs)
            {
                addSong(song.id);

            }
            Playlist.MoveTo((uint)Pos - 1);

        }

        //Appends a song to the playlist.
        public void addSong(string songid)
        {
            var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(SongListStorage.SongDict[songid].File));
            Playlist.Items.Add(mediaPlaybackItem);
            mediaPlayer.Play();
            SongListStorage.PlaylistRepresentation.Add(SongListStorage.SongDict[songid]);
        }


        



    }
}
