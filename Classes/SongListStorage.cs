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
using TagLib;
using Windows.Storage;
using Windows.Storage.FileProperties;
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
        
        public static ConcurrentDictionary<String, List<Flavour>> AlbumFlavourDict = new ConcurrentDictionary<String, List<Flavour>>();

        //public static HashSet<int> SongsInCollection = new HashSet<int>();

        private static bool loadednowplaying = false;
        public static bool FlavoursLoaded = false;
        public static bool LoadingMusic = true;

        public static bool FlavoursChanged = false; //Should changed this to true?

        public static async Task<ImageSource> GetCurrentSongArt(int size)
        {
            //return AlbumDict[String.Concat(PlaylistRepresentation[CurrentPlaceInPlaylist].Artist, PlaylistRepresentation[CurrentPlaceInPlaylist].Album)].albumart100;
            return await PlaylistRepresentation[CurrentPlaceInPlaylist].GetArt(size);
        }

        public static string GetCurrentSongName()
        {
            return PlaylistRepresentation[CurrentPlaceInPlaylist].Title;
        }

        public static string GetCurrentArtistName()
        {
            return PlaylistRepresentation[CurrentPlaceInPlaylist].Artist;
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
        public static void GetSongList()
        {
            Windows.System.Threading.ThreadPool.RunAsync(DisplayFiles, Windows.System.Threading.WorkItemPriority.High);
        }

        //This is the UI thread. It updates and orders the visible lists of songs and loads and newly created flavours/playlists.
        public static async void DisplayFiles(Windows.Foundation.IAsyncAction action)
            {
                var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

                while (true)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        if (FlavoursChanged)
                        {
                            App.GetForCurrentView().LoadPinnedFlavours();
                            FlavoursChanged = false;
                        }

                        

                        //Should do something similar to the flavourschanged bool here.
                        UpdateAndOrderSongs();
                        UpdateAndOrderArtists();
                        UpdateAndOrderAlbums();

                        //Should probably move this somewhere else
                        if (!loadednowplaying)
                        {
                            
                        }

                        if (!FlavoursLoaded)
                        {
                            try
                            {
                                int flavourcount = (int)roamingSettings.Values["flavourcount"];
                                string flavourstr = "";
                                for (int i = 0; i <= flavourcount; i++)
                                {
                                    flavourstr = flavourstr + roamingSettings.Values["flavourstr" + i];
                                }
                                LoadFlavours(flavourstr);

                            }
                            catch (Exception E)
                            {
                                Debug.WriteLine("Couldn't load flavours.");
                                Debug.WriteLine(E.Message);
                            }
                        }
                        
                    


                    });
                
                    Thread.Sleep(100); //update rate of the lists used for the ui
                }
            
            }        

            //Asynchronously gets the songs and organises them in the concurrentdictionary.
            //It iterates through the folders recursively, with a separate thread for each one.
            //Replaced by database.
            /*private static async Task GetFiles(StorageFolder folder)
            {
                if (folder != null)
                {
                    StorageFolder fold = folder;

                    var items = await fold.GetItemsAsync();

                    Regex songreg = new Regex(@"^audio/");

                    foreach (var item in items)
                    {
                        if (item.GetType() == typeof(StorageFile) && songreg.IsMatch(((StorageFile)item).ContentType))
                        {

                            MusicProperties musicProperties = await (item as StorageFile).Properties.GetMusicPropertiesAsync();

                            Song song = new Song() //TODO: NEED TO FIND DISC NUMBER TO ORDER ALBUMS PROPERLY.
                            {
                                id = "", //don't Remove this id
                                Title = musicProperties.Title,
                                Album = musicProperties.Album,
                                AlbumArtist = musicProperties.AlbumArtist,
                                Artist = musicProperties.Artist,
                                Year = musicProperties.Year,
                                Duration = musicProperties.Duration,
                                TrackNumber = (int)musicProperties.TrackNumber,
                                isFlavour = false, //MAY NEED TO REMOVE
                                Path = ((StorageFile)item).Path
                                //DiscNumber = resp.,
                                //File = item as StorageFile
                            };

                            
                            string id = "";
                            String props = song.Title + song.Album + song.AlbumArtist + song.Artist;
                            props.Replace(",", "");
                            id = props;
                            song.id = id;

                            SongDict.TryAdd(id, song); // should add error handling here?

                            AddAlbum(id, song);
                        }
                        else
                            GetFiles(item as StorageFolder);

                    }

                }
            
            }*/

            

            //Returns a flavour/playlist based on the album it is sourced from and its name.
            public static Flavour GetFlavourByName(String albumkey, String flavourname)
            {
                List<Flavour> flavours = AlbumFlavourDict[albumkey];
                foreach (Flavour flavour in flavours)
                {
                    if (flavour.name == flavourname) // TODO: Must make flavour names unique
                    {
                        return flavour;
                    }
                }
                return null;
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
                    arr[i] = PlaylistRepresentation[i].id;
                }
                var temp = String.Join(",", arr.Select(i => i.ToString()).ToArray());
                return temp;
            }

            public static void SaveNowPlaying()
            {
                if (PlaylistRepresentation.Count > 0)
                {
                    var roamingSettings =
                        Windows.Storage.ApplicationData.Current.RoamingSettings;
                    roamingSettings.Values["nowplaying"] = SongListStorage.NowPlayingToString();
                    roamingSettings.Values["nowplayingplace"] = SongListStorage.CurrentPlaceInPlaylist + 1;
                    roamingSettings.Values["nowplayingtime"] = Media.Instance.GetSongTime();
                }
            }

            public static async Task GetNowPlaying()
            {
                var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                try
                {
                    await LoadNowPlaying((string)roamingSettings.Values["nowplaying"], (int)roamingSettings.Values["nowplayingplace"], (TimeSpan)roamingSettings.Values["nowplayingtime"]);
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
                    await Media.Instance.PlayPlaylist(loadedplaylist, place, false); //need to get the position too.

                    Media.Instance.mediaPlayer.Pause();

                    //TimeSpan timeeeee = new TimeSpan()
                    Media.Instance.SetSongTime(time);
                }
                loadednowplaying = true;
            }

            //Converts the flavours/playlists into JSON format.
            public static string SerializeFlavours()
            {
                var x = JsonConvert.SerializeObject(AlbumFlavourDict, Formatting.Indented);
                return x;
            }

            public static void LoadFlavours(String flavours)
            {
                if (AlbumFlavourDict.Values.Count == 0 && flavours != "") //May need to change this condition to a loaded bool.
                {
                    try
                    {
                        var flavourdict = JsonConvert.DeserializeObject<ConcurrentDictionary<String, List<Flavour>>>(flavours);
                        AlbumFlavourDict = flavourdict;
                        SongListStorage.FlavoursChanged = true;
                        FlavoursLoaded = true;
                    }
                    catch (Exception E)
                    {
                        

                    }
                    
                }
            
            }


    }

    



}

