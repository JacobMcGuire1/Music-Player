﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace Music_thing
{
    public static class SongListStorage
    {
        public static ObservableCollection<Song> Songs { get; set; }
        = new ObservableCollection<Song>();

        public static ObservableCollection<Artist> Artists { get; set; }
        = new ObservableCollection<Artist>();

        public static ObservableCollection<Album> Albums { get; set; }
        = new ObservableCollection<Album>();

        public static int currentid;

        public static ObservableCollection<Song> PlaylistRepresentation { get; set; }
        = new ObservableCollection<Song>();

        public static int CurrentPlaceInPlaylist = new int();

        //public static Dictionary<string, List<int>> ArtistDict = new Dictionary<string, List<int>>();

        //SongID is the key
        public static ConcurrentDictionary<string, Song> SongDict = new ConcurrentDictionary<string, Song>();

        //Artist name is the key
        public static ConcurrentDictionary<String, Artist> ArtistDict = new ConcurrentDictionary<String, Artist>();

        //Key is artistname + albumname
        public static ConcurrentDictionary<String, Album> AlbumDict = new ConcurrentDictionary<String, Album>();

        public static ConcurrentDictionary<long, Playlist> PlaylistDict = new ConcurrentDictionary<long, Playlist>();
        public static ConcurrentDictionary<string, HashSet<long>> AlbumPlaylistDict = new ConcurrentDictionary<string, HashSet<long>>();

        public static bool ShowUnpinnedFlavours = false;

        //public static ConcurrentDictionary<String, List<Flavour>> AlbumFlavourDict = new ConcurrentDictionary<String, List<Flavour>>();

        public static async Task<ImageSource> GetCurrentSongArt(int size)
        {
            //return AlbumDict[String.Concat(PlaylistRepresentation[CurrentPlaceInPlaylist].Artist, PlaylistRepresentation[CurrentPlaceInPlaylist].Album)].albumart100;
            return await PlaylistRepresentation[CurrentPlaceInPlaylist].GetArt(size);
        }

        public static async Task<StorageFile> GetCurrentSongFile()
        {
            return await PlaylistRepresentation[CurrentPlaceInPlaylist].GetFile();
        }

        public static string GetCurrentSongName()
        {
            return PlaylistRepresentation[CurrentPlaceInPlaylist].Title;
        }

        public static string GetCurrentArtistName()
        {
            return PlaylistRepresentation[CurrentPlaceInPlaylist].Artist;
        }

        public static Song GetCurrentSong()
        {
            return PlaylistRepresentation[CurrentPlaceInPlaylist];
        }

        public static void UpdateAndOrderMusic()
        {
            UpdateAndOrderSongs();
            UpdateAndOrderArtists();
            UpdateAndOrderAlbums();
        }

        

        //Updates the list of albums from the dictionary. This is done so that the observable list of albums is smoothly updated as they are discovered asynchronously by the file finder threads. 
        public static void UpdateAndOrderAlbums()
        {
            if (Albums.Count != AlbumDict.Count)
            {
                List<string> Albumkeys = new List<string>(); // ArtistDict.Keys as List<string>;
                foreach (string key in AlbumDict.Keys)
                {
                    Albumkeys.Add(key);
                }
                if (Albumkeys != null)
                {
                    Albumkeys.Sort();//((x, y) => );
                    ObservableCollection<Album> Newalbums = new ObservableCollection<Album>();
                    foreach (string albumkey in Albumkeys)
                    {
                        Newalbums.Add(AlbumDict[albumkey]);
                    }
                    Albums = Newalbums;
                }
            }
        }

        //Updates the list of artists from the dictionary. This is done so that the observable list of artists is smoothly updated as they are discovered asynchronously by the file finder threads. 
        public static void UpdateAndOrderArtists()
        {
            if (Artists.Count != ArtistDict.Count) //Won't work if an artist has been removed and another has been added.
            {
                List<string> Artistkeys = new List<string>(); // ArtistDict.Keys as List<string>;
                foreach (string key in ArtistDict.Keys)
                {
                    Artistkeys.Add(key);
                }
                if (Artistkeys != null)
                {
                    Artistkeys.Sort();//((x, y) => );
                    ObservableCollection<Artist> NewArtists = new ObservableCollection<Artist>();
                    foreach (string artistkey in Artistkeys)
                    {
                        NewArtists.Add(ArtistDict[artistkey]);
                    }
                    Artists = NewArtists;

                }
            }
        }

        /*public static async Task SaveFlavours()
        {
            foreach (Playlist playlist in PlaylistDict.Values)
            {
                await playlist.SavePlaylistFile(false);
            }

        }*/


        //Updates the list of songs from the dictionary. This is done so that the observable list of songs is smoothly updated as they are discovered asynchronously by the file finder threads. 
        public static void UpdateAndOrderSongs()
        {
            if (Songs.Count != SongDict.Count)
            {
                List<string> Songkeys = new List<string>(); // ArtistDict.Keys as List<string>;
                foreach (string key in SongDict.Keys)
                {
                    Songkeys.Add(key);
                }
                if (Songkeys != null)
                {
                    Songkeys.Sort();//((x, y) => );
                    Songkeys.Sort((x, y) => SongListStorage.SongDict[x].Title.CompareTo(SongListStorage.SongDict[y].Title));
                    ObservableCollection<Song> Newsongs = new ObservableCollection<Song>();
                    foreach (string songkey in Songkeys)
                    {
                        Newsongs.Add(SongDict[songkey]);
                    }
                    Songs = Newsongs;
                }
            }
        }

        //Loads the user's songs from their files. Should only be done to initially discover their music in the future.
       /* public static void GetSongList()
        {
            Windows.System.Threading.ThreadPool.RunAsync(DisplayFiles, Windows.System.Threading.WorkItemPriority.High);
        }*/

        //Saves the position in the song every second,
       public static async void PeriodicallySave(Windows.Foundation.IAsyncAction action)
            {
                while (true)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        SavePlace();
                    });
                    Thread.Sleep(1000); //Updates every second.
                }
       }        

        //Returns the songs that contain the query as a substring to allow searching.
        public static ObservableCollection<Song> SearchSongs(String query)
        {
            var Results = new ObservableCollection<Song>();
            foreach (Song song in Songs)
            {
                if (song.Title.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                {
                    Results.Add(song);
                }
            }
            return Results;
        }

        //The following functions are for saving and loading data for when the program is ended and started.


        public static void SongsToJson()
        {
            string json = JsonConvert.SerializeObject(SongDict);            
        }

        public static string NowPlayingToString()
        {
            string[] arr = new string[PlaylistRepresentation.Count()];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = PlaylistRepresentation[i].ID;
            }
            var temp = String.Join(",", arr.Select(i => i.ToString()).ToArray());
            return temp;
        }

        public async static Task<bool> SaveNowPlaying()
        {
            while (true)
            {
                try
                {
                    if (PlaylistRepresentation.Count > 0)
                    {
                        string nowplayingstring = SongListStorage.NowPlayingToString();
                        StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                        StorageFile nowplayingfile = await storageFolder.CreateFileAsync("nowplaying.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                        await FileIO.WriteTextAsync(nowplayingfile, nowplayingstring);
                        SavePlace();
                    }
                    return true;
                }
                catch (FileLoadException E)
                {
                    Debug.WriteLine("Couldn't save now playing.");
                    Debug.WriteLine(E.Message);
                    //return false;
                }
            }
        }

        public static void SavePlace()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["nowplayingplace"] = SongListStorage.CurrentPlaceInPlaylist + 1;
            localSettings.Values["nowplayingtime"] = Media.Instance.GetSongTime();
        }

        public static void SaveVolume()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["chosenvol"] = Media.Instance.chosenVol;
            localSettings.Values["globalVol"] = Media.Instance.globalVol;
        }

        public static void LoadVolume()
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                Media.Instance.chosenVol = (int)localSettings.Values["chosenvol"];
                Media.Instance.globalVol = (int)localSettings.Values["globalVol"];
            }
            catch (NullReferenceException E)
            {
                Debug.WriteLine("Couldn't load volume.");
                Debug.WriteLine(E.Message);
            }
        }

        public static async Task GetNowPlaying()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            try
            {
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile nowplayingfile = await storageFolder.GetFileAsync("nowplaying.txt");
                string nowplayingstring = await FileIO.ReadTextAsync(nowplayingfile);

                await LoadNowPlaying(nowplayingstring, (int)localSettings.Values["nowplayingplace"], (TimeSpan)localSettings.Values["nowplayingtime"]);
                Debug.WriteLine("Loaded now playing");
            }
            catch (Exception E)
            {
                Debug.WriteLine("Couldn't load now playing yet.");
                Debug.WriteLine(E.Message);
            }
        }

        public static async Task LoadNowPlaying(string nowplayingstring, int place, TimeSpan time)
        {
            if (PlaylistRepresentation.Count == 0) //&& loadednowplaying == true
            {
                var loadedplaylist = new ObservableCollection<Song>();
                string[] arr = nowplayingstring.Split(',').ToArray();
                foreach (string id in arr)
                {
                    loadedplaylist.Add(SongDict[id]);
                }
                await Media.Instance.PlayPlaylist(loadedplaylist, place, null, false); //need to get the position too.

                Media.Instance.mediaPlayer.Pause();

                //TimeSpan timeeeee = new TimeSpan()
                Media.Instance.SetSongTime(time);

                Media.Instance.mediaPlayer.Pause();
            }
        }

        //Converts the flavours/playlists into JSON format.
        /*public static string SerializeFlavours()
        {
            var x = JsonConvert.SerializeObject(AlbumFlavourDict, Formatting.Indented);
            return x;
        }*/
        public static Album GetPinnedFlavourForAlbum(string albumid)
        {
            foreach(Playlist playlist in PlaylistDict.Values)
            {
                if (playlist.isflavour && playlist.albumkey == albumid && playlist.pinnedinalbum)
                {
                    return playlist;
                }
            }
            /*if (AlbumFlavourDict.ContainsKey(albumid))
            {
                foreach (Flavour flavour in AlbumFlavourDict[albumid])
                    if (flavour.pinnedinalbum) return flavour;
            }*/
            return AlbumDict[albumid];
        }

        public static async Task LoadFlavours()
        {
            var c = ApplicationData.Current.RoamingStorageQuota;
            //await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Roaming);

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
            QueryOptions queryOption = new QueryOptions(CommonFileQuery.DefaultQuery, new string[] { ".playlist" })
            {
                FolderDepth = FolderDepth.Deep
            };

            Queue<IStorageFolder> folders = new Queue<IStorageFolder>();

            var files = await storageFolder.CreateFileQueryWithOptions
              (queryOption).GetFilesAsync();

            foreach(StorageFile file in files)
            {
                string playliststring = await FileIO.ReadTextAsync(file);
                if (playliststring != "")
                {
                    Playlist playlist = JsonConvert.DeserializeObject<Playlist>(playliststring);
                    if (PlaylistDict.TryAdd(playlist.PlaylistID, playlist) && playlist.isflavour)
                    {
                        string albumkey = playlist.albumkey;
                        if (!AlbumPlaylistDict.ContainsKey(albumkey))
                        {
                            AlbumPlaylistDict.TryAdd(albumkey, new HashSet<long>());
                        }
                        AlbumPlaylistDict[playlist.albumkey].Add(playlist.PlaylistID);
                    }
                }
                
            }

            await App.GetForCurrentView().LoadPinnedFlavours();
        }
    }
}

