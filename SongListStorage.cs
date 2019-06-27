using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;

namespace Music_thing
{
    public static class SongListStorage
    {
        public static ObservableCollection<Song> Songs { get; }
        = new ObservableCollection<Song>();

        public static ObservableCollection<Artist> Artists { get; }
        = new ObservableCollection<Artist>();

        public static ObservableCollection<Album> Albums { get; }
        = new ObservableCollection<Album>();

        public static int currentid;

        public static ObservableCollection<Song> PlaylistRepresentation { get; }
        = new ObservableCollection<Song>();

        public static int CurrentPlaceInPlaylist = new int();

        //public static Dictionary<string, List<int>> ArtistDict = new Dictionary<string, List<int>>();

        //SongID is the key
        public static ConcurrentDictionary<int, Song> SongDict = new ConcurrentDictionary<int, Song>();

        //Artist name is the key
        public static ConcurrentDictionary<String, List<int>> ArtistDict = new ConcurrentDictionary<String, List<int>>();

        //Key is artistname + albumname
        public static ConcurrentDictionary<String, Album> AlbumDict = new ConcurrentDictionary<String, Album>();

        public static HashSet<int> SongsInCollection = new HashSet<int>();


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

                //Songs.Clear(); //Need to update rather than clear (use a set of ids to compare the difference to the dict.). DONE?!
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (KeyValuePair<int, Song> item in SongDict)
                    {
                        if (!SongsInCollection.Contains(item.Key))
                        {
                            Songs.Add(item.Value);
                            SongsInCollection.Add(item.Key);
                                //Songs.Add(item.Value);
                        }
                    }

                
                    Artists.Clear(); //need ti fux clearfing
                    foreach (String artist in ArtistDict.Keys)
                    {
                        Artists.Add(new Artist() { name = artist });
                    }

                    Albums.Clear();
                    foreach (Album album in AlbumDict.Values)
                    {
                        Albums.Add(album);
                    }


                });
                
                Thread.Sleep(10000); //update rate of the lists used for the ui
            }
            
        }

        /*public static void FindArtists()
        {
            foreach(Song song in Songs)
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

                

                if (!ArtistDict.ContainsKey(artist))
                {
                    List<int> ids = new List<int>();
                    ids.Add(song.id);
                    ArtistDict.TryAdd(artist, ids);

                    //Add to observable collection of artists //Do this in the other thread now
                    //Artists.Add(artist);
                }
                else
                {
                    List<int> ids = ArtistDict[artist];
                    ids.Add(song.id);
                    //ArtistDict.Remove(artist);
                    ArtistDict.TryUpdate(artist, ids);
                }
                //ArtistDict.Add(artist, song.id);
            }

            foreach(String artist in ArtistDict.Keys)
            {

            }
        }*/

        

        private static async Task GetFiles(StorageFolder folder)
        {
            StorageFolder fold = folder;

            var items = await fold.GetItemsAsync();

            Random rnd = new Random();

            Regex songreg = new Regex(@"^audio/");

            foreach (var item in items)
            {
                if (item.GetType() == typeof(StorageFile) && songreg.IsMatch(((StorageFile)item).ContentType))
                {

                    MusicProperties musicProperties = await (item as StorageFile).Properties.GetMusicPropertiesAsync();

                    //if (musicProperties.Title != "")
                    //{
                    Song song = new Song()
                        {
                            id = 0, //Remove this id
                            Title = musicProperties.Title,
                            Album = musicProperties.Album,
                            AlbumArtist = musicProperties.AlbumArtist,
                            Artist = musicProperties.Artist,
                            Year = musicProperties.Year,
                            Duration = musicProperties.Duration,
                            TrackNumber = musicProperties.TrackNumber,
                            File = item as StorageFile
                        };

                    int id = 0;                  
                    Boolean Added = false;
                    while (!Added)
                    {
                        id = rnd.Next(0, 214740);
                        song.id = id;
                        Added = SongDict.TryAdd(id, song);
                        //id = rnd.Next(0, 214740);
                    }
                   

                    //Add artist
                    AddArtist(id, song);
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

        public static void AddArtist(int id, Song song)
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
        }

        public static void AddAlbum(int id, Song song)
        {
            string artist;
            string albumname;

            if (song.AlbumArtist != "")
            {
                artist = song.AlbumArtist;
            }
            else
            {
                artist = song.Artist;
            }

            if (song.Album == "")
            {
                albumname = "Unknown Album";
            }
            else
            {
                albumname = song.Album;
            }

            String key = String.Concat(artist, albumname);

            Album album = new Album()
            {
                artist = artist,
                name = albumname,
                key = key,
                Songids = new List<int> { id }
            };

            //List<int> ids = new List<int>();
            //ids.Add(id);

            AlbumDict.AddOrUpdate(key, album, (key2, existingalbum) => AddSongToAlbum(existingalbum, id));///ExtendIDList(existingvalue, id));
        }

        public static Album AddSongToAlbum(Album existingalbum, int songid)
        {
            existingalbum.AddSong(songid);
            return existingalbum;
        }

        public static List<int> ExtendIDList(List<int> ids, int id)
        {
            ids.Add(id);
            return ids;
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

