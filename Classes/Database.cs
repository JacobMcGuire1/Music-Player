using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.FileProperties;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;

namespace Music_thing
{
    public static class Database
    {
        //Gets a collection of song files.
        public static async Task GetSongs(bool FirstTime)
        {
            try
            {
                App.GetForCurrentView().DisplayLoading(0, 0, 0, false);
            }
            catch (Exception E)
            {
                Debug.WriteLine("Couldn't display loading bar yet.");
                Debug.WriteLine(E.Message);
            }
            var files = await GetSongList();

            ConcurrentDictionary<String, Song> SongDict;
            ConcurrentDictionary<String, Artist> ArtistDict;
            ConcurrentDictionary<String, Album> AlbumDict;

            int songsloaded = 0;

            if (FirstTime)
            {
                SongDict = SongListStorage.SongDict;
                AlbumDict = SongListStorage.AlbumDict;
                ArtistDict = SongListStorage.ArtistDict;
            }
            else
            {
                SongDict = new ConcurrentDictionary<String, Song>();
                ArtistDict = new ConcurrentDictionary<String, Artist>();
                AlbumDict = new ConcurrentDictionary<String, Album>();
            }

            try
            {
                App.GetForCurrentView().DisplayLoading(0, 0, 0, false);
            }
            catch (Exception E)
            {
                Debug.WriteLine("Couldn't display loading bar yet.");
                Debug.WriteLine(E.Message);
            }


            Regex songreg = new Regex(@"^audio/");
            int filesscanned = 0;
            int songsfound = 0;
            //foreach (var file in files)
            for (int i = 0; i<files.Count; i++)
            {
                var file = files[i];
                filesscanned++;
                //Checks if it's an audio file
                if (songreg.IsMatch(file.ContentType))
                {
                    songsfound++;
                    if (songsfound % 10 == 0 || songsfound <= 1)
                    {
                        try
                        {
                            App.GetForCurrentView().DisplayLoading(songsfound, files.Count, filesscanned, false);
                        }
                        catch { }
                    }
                    if (songsfound % 50 == 0)
                    {
                        if (FirstTime) await SongListStorage.UpdateAndOrderMusic();
                    }
                    MusicProperties musicProperties = await (file as StorageFile).Properties.GetMusicPropertiesAsync();

                    IDictionary<string, object> returnedProps = await file.Properties.RetrievePropertiesAsync(new string[] { "System.Music.PartOfSet" } );
                    string discnumber = (string)returnedProps["System.Music.PartOfSet"];
                    if (discnumber == null) discnumber = "1";
                    
                    Song song = new Song()
                    {
                        ID = "",
                        Title = musicProperties.Title,
                        Album = musicProperties.Album,
                        AlbumArtist = musicProperties.AlbumArtist,
                        Artist = musicProperties.Artist,
                        Year = musicProperties.Year,
                        Duration = musicProperties.Duration,
                        TrackNumber = (int)musicProperties.TrackNumber,
                        IsFlavour = false, //MAY NEED TO REMOVE
                        Path = ((StorageFile)file).Path,
                        DiscNumber = discnumber
                    };


                    string id = "";
                    String props = song.Title + song.Album + song.AlbumArtist + song.Artist;
                    id = props.Replace(",", "");
                    song.ID = id;

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
            else
            {
                await SongListStorage.LoadFlavours();
            }
            await App.GetForCurrentView().ResetFlavours();

            //Should also display a message saying that all files have been loaded.
            Debug.WriteLine("Loaded " + songsloaded + " songs.");
            try
            {
                App.GetForCurrentView().DisplayLoading(songsfound, files.Count, filesscanned, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            await SongListStorage.UpdateAndOrderMusic();

            await MusicToJSON();

        }

        public static async Task<IReadOnlyList<StorageFile>> GetSongList()
        {
            QueryOptions queryOption = new QueryOptions(CommonFileQuery.DefaultQuery, new string[] { "*" })
            {
                FolderDepth = FolderDepth.Deep
            };

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

                //StorageFile albumdictfile = await storageFolder.CreateFileAsync("albumdict.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
                StorageFile albumdictfile = await storageFolder.GetFileAsync("albumdict.txt");
                string albumdictjson = await FileIO.ReadTextAsync(albumdictfile);
                var albumdict = JsonConvert.DeserializeObject<ConcurrentDictionary<String, Album>>(albumdictjson);
                SongListStorage.AlbumDict = albumdict;

                StorageFile songdictfile = await storageFolder.GetFileAsync("songdict.txt");
                string songdictjson = await FileIO.ReadTextAsync(songdictfile);
                var songdict = JsonConvert.DeserializeObject<ConcurrentDictionary<String, Song>>(songdictjson);
                SongListStorage.SongDict = songdict;

                StorageFile artistdictfile = await storageFolder.GetFileAsync("artistdict.txt");
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
            catch(FileNotFoundException E)
            {
                Debug.WriteLine("Couldn't load music.");
                Debug.WriteLine(E.Message);
                await GetSongs(true);
            }
            catch(Newtonsoft.Json.JsonSerializationException E)
            {
                Debug.WriteLine("Couldn't load music.");
                Debug.WriteLine(E.Message);
                await GetSongs(true);
            }
            catch(Exception E)
            {
                Debug.WriteLine("Couldn't load music.");
                Debug.WriteLine(E.Message);
                await GetSongs(true);
            }
            if (!Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("ShowUnpinnedFlavours"))
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["ShowUnpinnedFlavours"] = true;
            }
            await SongListStorage.GetNowPlaying();
            await SongListStorage.GetPlaylistFolder();
            await SongListStorage.LoadFlavours();
            SongListStorage.LoadVolume();
            Media.Instance.VolChanged();
            await SongListStorage.UpdateAndOrderMusic();
            await Windows.System.Threading.ThreadPool.RunAsync(SongListStorage.PeriodicallySave, Windows.System.Threading.WorkItemPriority.High);
            
            
            //SongListStorage.GetSongList();



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
                Artist = artistname,
                Name = albumname,
                Key = key,
                Year = (int)song.Year,
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

            song.AlbumKey = key;
            song.ArtistKey = artistname;
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
