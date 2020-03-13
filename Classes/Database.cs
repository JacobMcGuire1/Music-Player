using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Microsoft.Data.Sqlite;
using Windows.Storage.FileProperties;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Music_thing
{
    public static class Database
    {
        //Gets a collection of song files.
        public static async void GetSongs(bool FirstTime)
        {
            try
            {
                App.GetForCurrentView().DisplayLoading(0, 0, 0, false);
            }
            catch { }

            SongListStorage.LoadingMusic = true; //mb get rid of
            var files = await GetSongList();

            var SongDict = new ConcurrentDictionary<String, Song>();
            var ArtistDict = new ConcurrentDictionary<String, Artist>();
            var AlbumDict = new ConcurrentDictionary<String, Album>();

            int songsloaded = 0;

            if (FirstTime)
            {
                SongDict = SongListStorage.SongDict;
                AlbumDict = SongListStorage.AlbumDict;
                ArtistDict = SongListStorage.ArtistDict;
            }

            try
            {
                App.GetForCurrentView().DisplayLoading(0, 0, 0, false);
            }
            catch { }


            Regex songreg = new Regex(@"^audio/");
            int filesscanned = 0;
            int songsfound = 0;
            int testets = 0;
            foreach (var file in files)
            {
                filesscanned++;
                //Checks if it's an audio file
                if (songreg.IsMatch(file.ContentType))
                {
                    songsfound++;
                    if (songsfound % 10 == 0)
                    {
                        try
                        {
                            App.GetForCurrentView().DisplayLoading(songsfound, files.Count, filesscanned, false);
                        }
                        catch { }
                    }
                    MusicProperties musicProperties = await (file as StorageFile).Properties.GetMusicPropertiesAsync();

                    Song song = new Song() //TODO: NEED TO FIND DISC NUMBER TO ORDER ALBUMS PROPERLY.
                    {
                        id = "",
                        Title = musicProperties.Title,
                        Album = musicProperties.Album,
                        AlbumArtist = musicProperties.AlbumArtist,
                        Artist = musicProperties.Artist,
                        Year = musicProperties.Year,
                        Duration = musicProperties.Duration,
                        TrackNumber = (int)musicProperties.TrackNumber,
                        isFlavour = false, //MAY NEED TO REMOVE
                        Path = ((StorageFile)file).Path
                        //Need discnumber
                    };


                    string id = "";
                    String props = song.Title + song.Album + song.AlbumArtist + song.Artist;
                    id = props.Replace(",", "");
                    song.id = id;

                    if (SongDict.TryAdd(id, song))
                    {
                        songsloaded++;
                    }
                    else
                    {
                        Debug.WriteLine("Couldn't add " + song.Title + " by " + song.Artist + " from " + song.Album + ".");
                    }                    

                    AddAlbum(id, song, SongDict, ArtistDict, AlbumDict);
                }
            }
            if (!FirstTime)
            {
                SongListStorage.SongDict = SongDict;
                SongListStorage.AlbumDict = AlbumDict;
                SongListStorage.ArtistDict = ArtistDict;
            }

            //Should also display a message saying that all files have been loaded.
            Debug.WriteLine("Loaded " + songsloaded + " songs.");
            //SongListStorage.LoadingMusic = false;
            try
            {
                App.GetForCurrentView().DisplayLoading(songsfound, files.Count, filesscanned, true);
            }
            catch { }
            

            await MusicToJSON();

        }

        public static async Task<IReadOnlyList<StorageFile>> GetSongList()
        {
            QueryOptions queryOption = new QueryOptions
                (CommonFileQuery.OrderByTitle, new string[] { "*" });

            queryOption.FolderDepth = FolderDepth.Deep;

            Queue<IStorageFolder> folders = new Queue<IStorageFolder>();

            var files = await KnownFolders.MusicLibrary.CreateFileQueryWithOptions
              (queryOption).GetFilesAsync();

            return files;
        }

        public static async Task MusicToJSON()
        {
            try
            {
                string songdictjson = JsonConvert.SerializeObject(SongListStorage.SongDict);
                string artistdictjson = JsonConvert.SerializeObject(SongListStorage.ArtistDict);
                string albumdictjson = JsonConvert.SerializeObject(SongListStorage.AlbumDict);

                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                StorageFile songdictfile = await storageFolder.CreateFileAsync("songdict.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(songdictfile, songdictjson);

                StorageFile artistdictfile = await storageFolder.CreateFileAsync("artistdict.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(artistdictfile, artistdictjson);

                StorageFile albumdictfile = await storageFolder.CreateFileAsync("albumdict.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(albumdictfile, albumdictjson);

                Debug.WriteLine("Saved songs to JSON.");
            }
            catch(Exception E)
            {
                Debug.WriteLine("Couldn't save songs to JSON.");
                Debug.WriteLine(E.Message);
            }
            

            
        }

        public static async Task LoadMusicFromJSON()
        {
            try
            {
                try
                {
                    App.GetForCurrentView().DisplayLoading(0, 0, 0, false);
                }
                catch { }
                StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                StorageFile albumdictfile = await storageFolder.CreateFileAsync("albumdict.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                string albumdictjson = await FileIO.ReadTextAsync(albumdictfile);
                var albumdict = JsonConvert.DeserializeObject<ConcurrentDictionary<String, Album>>(albumdictjson);
                SongListStorage.AlbumDict = albumdict;

                StorageFile songdictfile = await storageFolder.CreateFileAsync("songdict.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                string songdictjson = await FileIO.ReadTextAsync(songdictfile);
                var songdict = JsonConvert.DeserializeObject<ConcurrentDictionary<String, Song>>(songdictjson);
                SongListStorage.SongDict = songdict;

                StorageFile artistdictfile = await storageFolder.CreateFileAsync("artistdict.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                string artistdictjson = await FileIO.ReadTextAsync(artistdictfile);
                var artistdict = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Artist>>(artistdictjson);
                SongListStorage.ArtistDict = artistdict;

                Debug.WriteLine("Loaded music from JSON.");
                try
                {
                    App.GetForCurrentView().DisplayLoading(SongListStorage.SongDict.Count, 0 , 0, true);
                }
                catch
                {

                }
                
            }
            catch(Exception E)
            {
                Debug.WriteLine("Couldn't load music.");
                Debug.WriteLine(E.Message);
                GetSongs(true);
            }
            SongListStorage.GetSongList();


        }

        //Adds a newly discovered album to the dictionary, adding the song it found it in to the album.
        public static void AddAlbum(string songid, Song song, ConcurrentDictionary<string, Song> SongDict, ConcurrentDictionary<String, Artist> ArtistDict, ConcurrentDictionary<String, Album> AlbumDict)
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
                Songids = new List<string>()
            };

            album.AddSong(songid, SongDict);

            AlbumDict.AddOrUpdate(key, album, (key2, existingalbum) => AddSongToAlbum(existingalbum, songid, SongDict));

            //Add to the artist

            Artist artist = new Artist()
            {
                name = artistname,
                Albums = new List<String> { key }
            };

            ArtistDict.AddOrUpdate(artistname, artist, (albumname2, existingartist) => AddAlbumToArtist(existingartist, key));
        }

        //Adds an album to an artist's collection in a threadsafe way.
        public static Artist AddAlbumToArtist(Artist existingartist, string albumkey)
        {
            existingartist.AddAlbum(albumkey);
            return existingartist;
        }

        //Adds a song to an album representation in a threadsafe way.
        public static Album AddSongToAlbum(Album existingalbum, string songid, ConcurrentDictionary<string, Song> SongDict)
        {
            existingalbum.AddSong(songid, SongDict);
            return existingalbum;
        }


    }
}
