using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public static ObservableCollection<Song> PlaylistRepresentation { get; }
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

        public static HashSet<int> SongsInCollection = new HashSet<int>();

        private static bool loadednowplaying = false;

        public static bool FlavoursChanged = false; //Should changed this to true?

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

        public static Windows.UI.Xaml.Media.ImageSource GetCurrentSongArt()
        {
            return AlbumDict[String.Concat(PlaylistRepresentation[CurrentPlaceInPlaylist].Artist, PlaylistRepresentation[CurrentPlaceInPlaylist].Album)].albumart100; //Change back to 100
            
        }

        public static string GetCurrentSongName()
        {
            return PlaylistRepresentation[CurrentPlaceInPlaylist].Title;
        }

        public static string GetCurrentArtistName()
        {
            return PlaylistRepresentation[CurrentPlaceInPlaylist].Artist;
        }

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


        public  static void GetSongList()
        {
            StorageFolder musicFolder = KnownFolders.MusicLibrary;

            currentid = 0;

            //currentid.k = 0;

            GetFiles(musicFolder);

            //DisplayFiles();

            Windows.System.Threading.ThreadPool.RunAsync(DisplayFiles, Windows.System.Threading.WorkItemPriority.High);

            //await System.Threading.Tasks.Task.Run(() => DisplayFiles());

            /*Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => 
            {
                while (true)
                {
                    Songs.Clear(); //Need to update rather than clear (use a set of ids to compare the difference to the dict.).
                    foreach (Song song in SongDict.Values)
                    {
                        Songs.Add(song);
                    }
                    Thread.Sleep(3000);
                }
            });*/

            //FindArtists();

            //Make an async method that updates the collections every 5 seconds.
        }

    



    public static async void DisplayFiles(Windows.Foundation.IAsyncAction action)
        {
            while (true)
            {
                

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (FlavoursChanged)
                    {
                        App.GetForCurrentView().LoadPinnedFlavours();
                        FlavoursChanged = false;
                    }
                    UpdateAndOrderSongs();
                    UpdateAndOrderArtists();
                    UpdateAndOrderAlbums();

                    //Should probably move this somewhere else
                    if (!loadednowplaying)
                    {
                        try
                        {
                            var roamingSettings =
                                            Windows.Storage.ApplicationData.Current.RoamingSettings;
                            LoadNowPlaying((string)roamingSettings.Values["nowplaying"], (int)roamingSettings.Values["nowplayingplace"]);
                        }
                        catch
                        {

                        }
                    }

                    try
                    {
                        var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                        LoadFlavours((string)roamingSettings.Values["flavours"]);

                    }
                    catch
                    {
                        
                    }
                    


                });
                
                Thread.Sleep(100); //update rate of the lists used for the ui
            }
            
        }        

        private static async Task GetFiles(StorageFolder folder)
        {
            if (folder != null)
            {
                StorageFolder fold = folder;

                var items = await fold.GetItemsAsync();

                //Random rnd = new Random();

                Regex songreg = new Regex(@"^audio/");

                foreach (var item in items)
                {
                    if (item.GetType() == typeof(StorageFile) && songreg.IsMatch(((StorageFile)item).ContentType))
                    {

                        MusicProperties musicProperties = await (item as StorageFile).Properties.GetMusicPropertiesAsync();


                        //var k = await (item as StorageFile).Properties.RetrievePropertiesAsync();

                        

                        //if (musicProperties.Title != "")
                        //{
                        Song song = new Song()
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
                            //DiscNumber = resp.,
                            File = item as StorageFile
                        };

                        /*var thing2 = await (item as StorageFile).OpenAsync(FileAccessMode.Read);
                        try
                        {
                            //TagLib.File file = TagLib.File.Create(item.Path);
                            //var discNumber = file.Tag.Disc;

                            //song.DiscNumber = resp.GetFrameValue("APIC", true);
                            //string fileContent = await FileIO.ReadTextAsync((IStorageFile)item);
                            //song.DiscNumber = fileContent;
                            var thing = await (item as StorageFile).OpenAsync(FileAccessMode.Read);
                            //thing.
                        }
                        catch(Exception e)
                        {
                            //...
                            song.DiscNumber = "0";
                        }
                        if (!song.DiscNumber.Equals("0"))
                        {
                            string f = "yay";
                        }
                        // song.SetAlbumArt();*/

                        string id = "";
                        //if (song.Title == "American Darkness")
                       // {

                       // }
                        String props = song.Title + song.Album + song.AlbumArtist + song.Artist;
                        props.Replace(",", "");
                        //id = props.ToLowerInvariant().GetHashCode();
                        //int id2 = props.ToLowerInvariant().GetHashCode(); 
                        //id = Int32.Parse(props);

                        //for (int ctr = 0; ctr <= props.Length - 1; ctr++)
                        //    id += props[ctr]; //need to make this better (hashcode) as songs can have the same id.
                        id = props;
                        song.id = id;
                        SongDict.TryAdd(id, song); // should add error handling here?
                        /*
                        Boolean Added = false;
                        while (!Added)
                        {
                            id = rnd.Next(0, 214740);
                            song.id = id;
                            Added = SongDict.TryAdd(id, song);
                            //id = rnd.Next(0, 214740);
                        }*/


                        //Add artist
                        //AddArtist(id, song);
                        //Add album
                        AddAlbum(id, song);


                        //}
                        //else
                        //{
                        //this.Songs.Add(new Song() { Title = item.Path.ToString() });
                        //}
                    }
                    else
                        GetFiles(item as StorageFolder);


                }

            }
            
        }

        /*public static void AddArtist(int id, Song song)
        {
            string artist;
            if (song.AlbumArtist != "")
            {
                artist = song.AlbumArtist;
            }
            else
            {
                artist = song.Artist;
            }

            List<int> ids = new List<int>
            {
                id
            };

            ArtistDict.AddOrUpdate(artist, ids, (key, existingvalue) => ExtendIDList(existingvalue, id));
        }*/

        public static void AddAlbum(string songid, Song song)
        {
            string artistname;
            string albumname;

            if (song.AlbumArtist != "")
            {
                artistname = song.AlbumArtist;
            }
            else
            {
                artistname = song.Artist;
            }

            if (song.Album == "")
            {
                albumname = "Unknown Album";
            }
            else
            {
                albumname = song.Album;
            }

            String key = String.Concat(artistname, albumname);

            Album album = new Album()
            {
                artist = artistname,
                name = albumname,
                key = key,
                year = (int)song.Year,
                Songids = new List<string>()//Replaced with use of AddSong Songids = new List<int> { id }
            };

            album.AddSong(songid);

            //List<int> ids = new List<int>();
            //ids.Add(id);

            AlbumDict.AddOrUpdate(key, album, (key2, existingalbum) => AddSongToAlbum(existingalbum, songid));///ExtendIDList(existingvalue, id));

            //Add to the artist

            Artist artist = new Artist()
            {
                name = artistname,
                Albums = new List<String> { key }
            };

            ArtistDict.AddOrUpdate(artistname, artist, (albumname2, existingartist) => AddAlbumToArtist(existingartist, key));
        }

        public static Artist AddAlbumToArtist(Artist existingartist, string albumkey)
        {
            existingartist.AddAlbum(albumkey);
            return existingartist;
        }

        public static Album AddSongToAlbum(Album existingalbum, string songid)
        {
            existingalbum.AddSong(songid);
            return existingalbum;
        }

        public static List<int> ExtendIDList(List<int> ids, int id)
        {
            ids.Add(id);
            return ids;
        }

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

        //Returns the songs that contain the query as a substring.
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

        public static void SongsToJson()
        {
            string json = JsonConvert.SerializeObject(SongDict);
            //foreach (Song song in SongDict.Values)
            //{
             //   string json = JsonConvert.SerializeObject(song);
            //}
            
        }

        public static string NowPlayingToString()
        {
            //var list = new List<string>();
            string[] arr = new string[PlaylistRepresentation.Count()];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = PlaylistRepresentation[i].id;
            }
            var temp = String.Join(",", arr.Select(i => i.ToString()).ToArray());
            return temp;
           // return new List<string>();
        }

        public static void LoadNowPlaying(string nowplayingstring, int place)
        {
            if (PlaylistRepresentation.Count == 0) //&& loadednowplaying == true
            {
                var loadedplaylist = new ObservableCollection<Song>();
                string[] arr = nowplayingstring.Split(',').ToArray();
                foreach (string id in arr)
                {
                    loadedplaylist.Add(SongDict[id]);
                }
                Media.Instance.PlayPlaylist(loadedplaylist, place); //need to get the position too.

                Media.Instance.mediaPlayer.Pause();
            }
            loadednowplaying = true;
        }

        public static string SerializeFlavours()
        {
            var x = JsonConvert.SerializeObject(AlbumFlavourDict, Formatting.None);
            return x;
        }

        public static void LoadFlavours(String flavours)
        {
            if (AlbumFlavourDict.Values.Count == 0 && flavours != "") //May need to change this condition to a loaded bool.
            {
                var flavourdict = JsonConvert.DeserializeObject<ConcurrentDictionary<String, List<Flavour>>>(flavours);
                AlbumFlavourDict = flavourdict;
                SongListStorage.FlavoursChanged = true;
            }
            
        }

        /*private static async Task oldGetFiles(StorageFolder folder)
        {
            StorageFolder fold = folder;

            var items = await fold.GetItemsAsync();

            foreach (var item in items)
            {
                if (item.GetType() == typeof(StorageFile))
                {

                    MusicProperties musicProperties = await (item as StorageFile).Properties.GetMusicPropertiesAsync();

                    if (musicProperties.Title != "")
                    {
                            Songs.Add(new Song()
                            {
                                id = currentid,
                                Title = musicProperties.Title,
                                Album = musicProperties.Album,
                                AlbumArtist = musicProperties.AlbumArtist,
                                Artist = musicProperties.Artist,
                                Year = musicProperties.Year,
                                Duration = musicProperties.Duration,
                                File = item as StorageFile
                            });

                            currentid++;
                        
                    }
                    else
                    {
                        //this.Songs.Add(new Song() { Title = item.Path.ToString() });
                    }
                }
                else
                    GetFiles(item as StorageFolder);
            }
            FindArtists();
        }*/
    }

    //Done for locking and stuff
    //public class Myint
   // {
   //     public int k { get; set; }
    //}

    



}

