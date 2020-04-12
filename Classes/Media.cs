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

        public double globalVol; //TODO: Get it from the settings

        public double chosenVol;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Muted = false;



        //public Song CurrentSong;

        static Media() { }

        private Media()
        {
            mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;

            mediaPlayer.Source = Playlist;

            Playlist.CurrentItemChanged += Playlist_CurrentItemChanged;

            try
            {
                globalVol = (double)Windows.Storage.ApplicationData.Current.LocalSettings.Values["globalvol"];
                chosenVol = (double)Windows.Storage.ApplicationData.Current.LocalSettings.Values["chosenVol"];
            }
            catch
            {
                globalVol = 1.0;
                chosenVol = 0.3;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["globalvol"] = globalVol;
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["chosenVol"] = chosenVol;
            }
            ChangeVolume();
            //mediaPlayer.Volume = chosenVol * globalVol;

            //mediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;

            //The placeholder album art TODO: make it work?
            BitmapImage bitmapImage =
                     new BitmapImage(new Uri("ms-appx:///Assets/Album.png"));

            Currentart = bitmapImage;
            NotifyPropertyChanged();

            //CurrentSong = new StorageFile();
        }

        //Used to inform the UI that something about the currently playing song has changed.
        private async void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {

            // Your UI update code goes here!
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        //Changes the volume and notifies of this.
        /*private void MediaPlayer_VolumeChanged(MediaPlayer sender, object args)
        {
            chosenVol = (int)(mediaPlayer.Volume * 100);
            VolChanged();
        }*/
        public void ToggleMute()
        {
            if (!Muted)
            {
                mediaPlayer.Volume = 0.0;
                Muted = true;
            }
            else
            {
                ChangeVolume();
                Muted = false;
            }
        }

        public void ChangeVolume()
        {
            mediaPlayer.Volume = chosenVol * globalVol;
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
            SongListStorage.SaveVolume();

        }

        public TimeSpan GetSongTime()
        {
            return mediaPlayer.PlaybackSession.Position;
        }

        public void SetSongTime(TimeSpan songtime)
        {
            mediaPlayer.PlaybackSession.Position = songtime;
        }


        //Updates song details when the song changes.
        private async void Playlist_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
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

            if (SongListStorage.PlaylistRepresentation.Count > 0)
            {

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    Currentart = await SongListStorage.GetCurrentSongArt(100);

                }
                );
                Currenttitle = SongListStorage.GetCurrentSongName();
                Currentartist = SongListStorage.GetCurrentArtistName();

                //set the system transport controls
                //mediaPlayer.SystemMediaTransportControls.IsEnabled = true;
                var props = mediaPlayer.SystemMediaTransportControls.DisplayUpdater;
                props.Type = Windows.Media.MediaPlaybackType.Music;
                //props.AppMediaId = "TOAST";
                //props.MusicProperties.Title = "TEST";
                //props.Update();
                //var musicprops = props.MusicProperties;
                var file = await SongListStorage.GetCurrentSongFile();
                bool ok = await props.CopyFromFileAsync(Windows.Media.MediaPlaybackType.Music,file);
                //props.AppMediaId = "dwioahjdioaw";
                //props.Type = Windows.Media.MediaPlaybackType.Music;
                props.Update();


                //musicprops.Title = Currenttitle;
                //musicprops.Artist = Currentartist;
                //var thing = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(await SongListStorage.GetCurrentSongArtURI());
                //props.Thumbnail = thing;

            }
            //await SongListStorage.SaveNowPlaying();
            SongListStorage.SavePlace();
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
        public async Task PlaySong(string songid)
        {
            Playlist.Items.Clear();
            var song = SongListStorage.SongDict[songid];
            var file = await song.GetFile();
            var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(await song.GetFile()));
            Playlist.Items.Add(mediaPlaybackItem);
            mediaPlayer.Play();
            SongListStorage.PlaylistRepresentation.Clear();
            SongListStorage.PlaylistRepresentation.Add(SongListStorage.SongDict[songid]);
            await SongListStorage.SaveNowPlaying();

        }

        //Plays the playlist
        public async Task PlayPlaylist(ObservableCollection<Song> Songs, int Pos, string songid, bool play)
        {
            mediaPlayer.Pause();
            Playlist.Items.Clear(); //Clears the playlist
            SongListStorage.CurrentPlaceInPlaylist = 0;
            SongListStorage.PlaylistRepresentation.Clear(); //MAY BE BAD?
            foreach (Song song in Songs)
            {
                await AddSong(song.ID, false);
                if (play) mediaPlayer.Play(); else mediaPlayer.Pause();
            }
            if (Pos > 1 && SongListStorage.PlaylistRepresentation.Count >= Pos)
            {
                Playlist.MoveTo((uint)Pos - 1);
            }
            else
            {
                if (songid != null && songid != "")
                {
                    for (int i = 0; i < SongListStorage.PlaylistRepresentation.Count; i++)
                    {
                        if (SongListStorage.PlaylistRepresentation[i].ID == songid)
                        {
                            Playlist.MoveTo((uint)i);
                            break;
                        }

                    }
                }
            }
            if (play) mediaPlayer.Play(); else mediaPlayer.Pause();
            //if (!play) mediaPlayer.Pause();
            await SongListStorage.SaveNowPlaying();
        }

        //Appends a song to the playlist.
        public async Task AddSong(string songid, bool save)
        {
            var song = SongListStorage.SongDict[songid];
            var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(await song.GetFile()));
            Playlist.Items.Add(mediaPlaybackItem);
            SongListStorage.PlaylistRepresentation.Add(SongListStorage.SongDict[songid]);
            if (save) await SongListStorage.SaveNowPlaying();
        }


        



    }
}
