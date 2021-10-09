using Newtonsoft.Json;
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

        private static Mutex NowPlayingFileLock = new Mutex();

        public static SortDirection AlbumListSortDirection = SortDirection.Asc;
        public static SortType AlbumListSortType = SortType.Artist;

        public static string LastLoggedSong = "None";

        public enum SortDirection
        {
            Asc,
            Desc
        }

        public enum SortType
        {
            Artist,
            Duration,
            SongCount,
            AlbumName,
            Year,
            Random
        }

        public static async Task<ImageSource> GetCurrentSongArt(int size)
        {
            //return AlbumDict[String.Concat(PlaylistRepresentation[CurrentPlaceInPlaylist].Artist, PlaylistRepresentation[CurrentPlaceInPlaylist].Album)].albumart100;
            if (CurrentPlaceInPlaylist < PlaylistRepresentation.Count)
            {
                return await PlaylistRepresentation[CurrentPlaceInPlaylist].GetArt(size);
            }
            else
            {
                return null;
            }
        }

        public static async Task<StorageFile> GetCurrentSongFile()
        {
            if (CurrentPlaceInPlaylist < PlaylistRepresentation.Count)
            {
                return await PlaylistRepresentation[CurrentPlaceInPlaylist].GetFile();
            }
            else
            {
                return null;
            }
        }

        public static string GetCurrentSongName()
        {
            if (CurrentPlaceInPlaylist < PlaylistRepresentation.Count)
            {
                return PlaylistRepresentation[CurrentPlaceInPlaylist].Title;
            }
            else
            {
                return null;
            }
        }

        public static string GetCurrentArtistName()
        {
            if (CurrentPlaceInPlaylist < PlaylistRepresentation.Count)
            {
                return PlaylistRepresentation[CurrentPlaceInPlaylist].Artist;
            }
            else
            {
                return null;
            }
        }

        public static Song GetCurrentSong()
        {
            if (CurrentPlaceInPlaylist < PlaylistRepresentation.Count)
            {
                return PlaylistRepresentation[CurrentPlaceInPlaylist];
            }
            else
            {
                return null;
            }
        }

        public static async Task UpdateAndOrderMusic()
        {
            UpdateAndOrderSongs();
            UpdateAndOrderArtists();
            await UpdateAndOrderAlbums(false);
        }

        

        //Updates the list of albums from the dictionary. This is done so that the observable list of albums is smoothly updated as they are discovered asynchronously by the file finder threads. 
        public static async Task UpdateAndOrderAlbums(bool ChangedOrder)
        {
            if (Albums.Count != AlbumDict.Count || ChangedOrder)
            {
                List<string> Albumkeys = new List<string>();
                foreach (string key in AlbumDict.Keys)
                {
                    Albumkeys.Add(key);
                }

                SortAlbumKeyList(Albumkeys, AlbumListSortType, AlbumListSortDirection);

                if (Albumkeys != null)
                {
                    for (int i = 0; i < Albumkeys.Count; i++)
                    {
                        string albumkey = Albumkeys[i];
                        var album = AlbumDict[albumkey];
                        if (album.Albumart == null)
                        {
                            album.Albumart = await album.GetAlbumArt(200, SongListStorage.SongDict);
                        }
                        if (i < Albums.Count)
                        {
                            Albums[i] = album;
                        }
                        else
                        {
                            Albums.Add(album);
                        }
                    }
                }
            }
        }

        public static void SortAlbumKeyList(List<String> albumKeys, SortType sortType, SortDirection sortDirection)
        {
            if (albumKeys.Count <= 1) return;
            switch (sortType)
            {
                case SortType.AlbumName:
                    if (sortDirection == SortDirection.Asc) albumKeys.Sort((x, y) => (AlbumDict[x].Name.Replace("The ", "").CompareTo(AlbumDict[y].Name.Replace("The ", ""))));
                    else albumKeys.Sort((x, y) => (AlbumDict[y].Name.CompareTo(AlbumDict[x].Name)));
                    break;
                case SortType.Duration:
                    if (sortDirection == SortDirection.Asc) albumKeys.Sort((x, y) => (AlbumDict[x].GetTotalDuration().Ticks.CompareTo(AlbumDict[y].GetTotalDuration().Ticks)));
                    else albumKeys.Sort((x, y) => (AlbumDict[y].GetTotalDuration().Ticks.CompareTo(AlbumDict[x].GetTotalDuration().Ticks)));
                    break;
                case SortType.SongCount:
                    if (sortDirection == SortDirection.Asc) albumKeys.Sort((x, y) => (AlbumDict[x].Songids.Count.CompareTo(AlbumDict[y].Songids.Count)));
                    else albumKeys.Sort((x, y) => (AlbumDict[y].Songids.Count.CompareTo(AlbumDict[x].Songids.Count)));
                    break;
                case SortType.Year:
                    if (sortDirection == SortDirection.Asc) albumKeys.Sort((x, y) => (AlbumDict[x].Year.CompareTo(AlbumDict[y].Year)));
                    else albumKeys.Sort((x, y) => (AlbumDict[y].Year.CompareTo(AlbumDict[x].Year)));
                    break;
                case SortType.Random:
                    var c = new Random();
                    albumKeys.Sort(((x, y) => c.Next() - c.Next()));
                    break;
                default:
                case SortType.Artist:
                    if (sortDirection == SortDirection.Asc) albumKeys.Sort((x, y) => (x.CompareTo(y)));
                    else albumKeys.Sort((x, y) => (y.CompareTo(x)));
                    break;
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
                    Artistkeys.Sort((x, y) => x.Replace("The ", "").CompareTo(y.Replace("The ", "")));//((x, y) => );
                    //Artists.Clear();
                    for (int i = 0; i < Artistkeys.Count; i++)
                    {
                        String artistkey = Artistkeys[i];
                        if (i < Artists.Count)
                        {
                            Artists[i] = ArtistDict[artistkey];
                        }
                        else
                        {
                            Artists.Add(ArtistDict[artistkey]);
                        }
                    }
                }
            }
        }
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
                    Songkeys.Sort((x, y) => SongListStorage.SongDict[x].Title.Replace("The " , "").CompareTo(SongListStorage.SongDict[y].Title.Replace("The ", "")));
                    //Songs.Clear();
                    for (int i = 0; i < Songkeys.Count; i++)
                    {
                        string songkey = Songkeys[i];
                        Songs.Insert(i, SongDict[songkey]);
                        if (i < Songs.Count)
                        {
                            Songs[i] = SongDict[songkey];
                        }
                        else
                        {
                            Songs.Add(SongDict[songkey]);
                        }
                    }
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

        public static ObservableCollection<Album> SearchAlbums(String query, ObservableCollection<Album> Albums)
        {
            var Results = new ObservableCollection<Album>();
            foreach (Album album in Albums)
            {
                if (album.Name.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                {
                    Results.Add(album);
                }
            }
            return Results;
        }

        public static ObservableCollection<Artist> SearchArtists(String query)
        {
            var Results = new ObservableCollection<Artist>();
            foreach (Artist artist in Artists)
            {
                if (artist.name.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                {
                    Results.Add(artist);
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
                        //storageFolder.
                        await FileIO.WriteTextAsync(nowplayingfile, nowplayingstring);
                        SavePlace();
                    }
                    
                    return true;
                }
                catch (FileLoadException E)
                {
                    Debug.WriteLine("Couldn't save now playing.");
                    Debug.WriteLine(E.Message);
                }
                catch (IOException E)
                {
                    Debug.WriteLine("Couldn't save now playing.");
                    Debug.WriteLine(E.Message);
                }
            }
        }

        public static void SavePlace()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["nowplayingplace"] = SongListStorage.CurrentPlaceInPlaylist + 1;
            localSettings.Values["nowplayingtime"] = Media.Instance.GetSongTime();
        }

        public static void SaveDBInfo(int Listeneventcount, bool SongListenInDB)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Listeneventcount"] = Listeneventcount;
            localSettings.Values["SongListenInDB"] = SongListenInDB;
        }

        public static void SaveVolume()
        {
            //var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            //localSettings.Values["chosenvol"] = Media.Instance.chosenVol;
            //localSettings.Values["globalVol"] = Media.Instance.globalVol;
        }

        public static void LoadVolume()
        {
            /*try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                Media.Instance.chosenVol = (double)localSettings.Values["chosenvol"];
                Media.Instance.globalVol = (double)localSettings.Values["globalVol"];
            }
            catch (NullReferenceException E)
            {
                Debug.WriteLine("Couldn't load volume.");
                Debug.WriteLine(E.Message);
            }*/
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
                    if (SongListStorage.SongDict.ContainsKey(id))
                    {
                        loadedplaylist.Add(SongDict[id]);
                    }
                }
                await Media.Instance.LoadNowPlaying(loadedplaylist, place, time); //need to get the position too.

                //Media.Instance.mediaPlayer.Pause();

                //TimeSpan timeeeee = new TimeSpan()
                //Media.Instance.SetSongTime(time);

                //Media.Instance.mediaPlayer.Pause();
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
                try
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
                catch (Exception E)
                {
                    Debug.WriteLine("Couldn't load playlist '" + file.Name + "'");
                    Debug.WriteLine(E.Message);
                }
                
                
            }

            await App.GetForCurrentView().LoadPinnedFlavours();
        }
    }
}

