using Music_thing.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
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

        public BitmapImage DefaultArt;

        int Listeneventcount = 0;
        bool SongListenInDB = false;
        bool initialload = true;

        static Media() { }

        private Media()
        {
            try
            {
                DefaultArt = new BitmapImage(new Uri("ms-appx:///Assets/DefaultAlbumArt.png"));
            }
            catch { }
            mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;

            mediaPlayer.Source = Playlist;

            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;

            Playlist.CurrentItemChanged += Playlist_CurrentItemChanged;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values.ContainsKey("globalvol"))
            {
                globalVol = (double)localSettings.Values["globalvol"];
            }
            else
            {
                globalVol = 1.0;
                localSettings.Values["globalvol"] = globalVol;
            }

            if (localSettings.Values.ContainsKey("chosenVol"))
            {
                chosenVol = (double)localSettings.Values["chosenVol"];
            }
            else
            {
                chosenVol = 0.3;
                localSettings.Values["chosenVol"] = chosenVol;
            }

            ChangeVolume();

            SetDiscordRichPresence();
            saveRichPresenceInfo();

            //Load DB Save info for this song
            if (localSettings.Values.ContainsKey("Listeneventcount") && localSettings.Values.ContainsKey("SongListenInDB"))
            {
                Listeneventcount = (int)localSettings.Values["Listeneventcount"];
                SongListenInDB = (bool)localSettings.Values["SongListenInDB"];
            }
                //mediaPlayer.Volume = chosenVol * globalVol;

                //mediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;

                //The placeholder album art TODO: make it work?


                Currentart = DefaultArt;
            NotifyPropertyChanged();
            

            //CurrentSong = new StorageFile();
        }

        private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            var totalticks = SongListStorage.GetCurrentSong().Duration.TotalSeconds * 4;
            if (++Listeneventcount > totalticks / 2 && !SongListenInDB)
            {
                AddListenToDB(SongListStorage.GetCurrentSong().ID);
                SongListenInDB = true;
            }
            SongListStorage.SaveDBInfo(Listeneventcount, SongListenInDB);
        }

        private void AddListenToDB(string songid)
        {
            SongLog.AddListen(songid);
        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            MoveTo(0);
            mediaPlayer.Pause();
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
        public async void Playlist_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (!initialload)
            {
                SongListenInDB = false;
                Listeneventcount = 0;
            }
            else
            {
                initialload = false;
            }
            
            await UpdateNowPlaying();
        }

        public async Task UpdateNowPlaying()
        {
            uint position = Playlist.CurrentItemIndex;
            if (position == 4294967295) //Magic number?? Perfectly totient.
            {
                SongListStorage.CurrentPlaceInPlaylist = 0;
            }
            else
            {
                SongListStorage.CurrentPlaceInPlaylist = (int)position;
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
                bool ok = await props.CopyFromFileAsync(Windows.Media.MediaPlaybackType.Music, file);
                //props.AppMediaId = "dwioahjdioaw";
                //props.Type = Windows.Media.MediaPlaybackType.Music;
                props.Update();
            }
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

        public async Task RemoveSong(int index)
        {
            if (SongListStorage.CurrentPlaceInPlaylist == index)
            {
                Playlist.MovePrevious();
            }
            
            Playlist.Items.RemoveAt(index);
            SongListStorage.PlaylistRepresentation.RemoveAt(index);
            int place = (int)Playlist.CurrentItemIndex;
            if (Playlist.CurrentItemIndex == 4294967295) //Magic number?? Perfectly totient.
            {
                SongListStorage.CurrentPlaceInPlaylist = 0;
            }
            else
            {
                SongListStorage.CurrentPlaceInPlaylist = place;
            }
            await SongListStorage.SaveNowPlaying();
            /*if (SongListStorage.PlaylistRepresentation.Count == 0)
            {
                mediaPlayer.Source = new MediaPlaybackList();
                Currentart = DefaultArt;
                Currentartist = "";
                Currenttitle = "";
            }*/
        }

        public bool MoveTo(int index)
        {
            if (index >= 0 && index < SongListStorage.PlaylistRepresentation.Count)
            {
                Playlist.MoveTo((uint)index);
                mediaPlayer.Play();
                SongListStorage.SavePlace();
                return true;
            }
            return false;
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

        //Plays the playlist
        public async Task LoadNowPlaying(ObservableCollection<Song> Songs, int Pos, TimeSpan time)
        {
            mediaPlayer.Pause();
            Playlist.Items.Clear(); //Clears the playlist
            SongListStorage.CurrentPlaceInPlaylist = 0;
            SongListStorage.PlaylistRepresentation.Clear(); //MAY BE BAD?

            Song currentsong = Songs[Pos - 1];

            await AddSong(currentsong.ID, false);
            SetSongTime(time);
            bool b = mediaPlayer.PlaybackSession.CanPause;
            //mediaPlayer.Pause();

            await UpdateNowPlaying();
            //bool k .CanPause;
            

            int counter = 0;
            for (int i = 0; i < Songs.Count; i++)
            {
                var song = Songs[i];
                var title = song.Title;

                var file = await song.GetFile();

                if (file != null)
                {
                    var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));

                    if (i < Pos - 1)
                    {
                        Playlist.Items.Insert(counter, mediaPlaybackItem);
                        SongListStorage.PlaylistRepresentation.Insert(counter, song);
                        counter++;
                    }
                    if (i > Pos - 1)
                    {
                        Playlist.Items.Add(mediaPlaybackItem);
                        SongListStorage.PlaylistRepresentation.Add(song);
                    }
                }
            }
            await UpdateNowPlaying();

            //var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            //Listeneventcount = (int)localSettings.Values["Listeneventcount"];
            //SongListenInDB = (bool)localSettings.Values["nowplayingtime"];


            await SongListStorage.SaveNowPlaying();
        }

        //Appends a song to the playlist.
        public async Task AddSong(string songid, bool save)
        {
            if (SongListStorage.SongDict.ContainsKey(songid))
            {
                var song = SongListStorage.SongDict[songid];
                var file = await song.GetFile();
                if (file != null)
                {
                    var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));
                    Playlist.Items.Add(mediaPlaybackItem);
                    SongListStorage.PlaylistRepresentation.Add(SongListStorage.SongDict[songid]);
                }
                
                if (save) await SongListStorage.SaveNowPlaying();
            }
        }

        private async void SetDiscordRichPresence()
        {
            try
            {
                if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
                {
                    await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.WriteLine("Couldn't Launch Discord Rich Presence.");
            }
        }

        private async Task saveRichPresenceInfo()
        {
            //var storageFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            //var f = new StorageFolder(new Uri("ms - appx:///Assets/discordrpc"));
            //Windows.Storage.ApplicationData.Current.
            //KnownFolders.MusicLibrary.CreateFileAsync
            var localfolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await localfolder.CreateFileAsync("RichPresenceInfo.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            while (true)
            {
                await Task.Delay(500);
                //var musicLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                //musicLibrary.
                //Debug.WriteLine(musicLibrary.SaveFolder.Path);
                
                //var file = await KnownFolders.MusicLibrary.CreateFileAsync("RichPresenceInfo.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);

                long timeleft = 0;
                var song = SongListStorage.GetCurrentSong();
                if (song != null)
                {
                    timeleft = song.Duration.Subtract(mediaPlayer.PlaybackSession.Position).Ticks;
                }

                await FileIO.WriteTextAsync(file, currentartist + "\n" + currenttitle + "\n" + timeleft + "\n" + DateTime.UtcNow.Ticks + "\n" + mediaPlayer.PlaybackSession.PlaybackState);
                Debug.WriteLine("Hopefully wrote to file at " + DateTime.Now.ToString());
                
            }
            
        }
    }
}
