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
        public static async void GetSongs()
        {
            var files = await GetSongList();

            //Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            //Windows.Storage.StorageFile songsfile = await storageFolder.CreateFileAsync("test.txt",
            //        Windows.Storage.CreationCollisionOption.OpenIfExists);
            //var listOfStrings = new List<string> { "Songs: " };
            SongListStorage.SongDict = new ConcurrentDictionary<string, Song>();
            SongListStorage.ArtistDict = new ConcurrentDictionary<String, Artist>();
            SongListStorage.AlbumDict = new ConcurrentDictionary<String, Album>();

            SongListStorage.Songs = new ObservableCollection<Song>();
            SongListStorage.Artists = new ObservableCollection<Artist>();
            SongListStorage.Albums = new ObservableCollection<Album>();

            SongListStorage.PlaylistRepresentation = new ObservableCollection<Song>();

            Regex songreg = new Regex(@"^audio/");
            foreach (var file in files)
            {
                //Checks if it's an audio file
                if (songreg.IsMatch(file.ContentType))
                {
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
                    props.Replace(",", "");
                    id = props;
                    song.id = id;

                    SongListStorage.SongDict.TryAdd(id, song); // should add error handling here?

                    SongListStorage.AddAlbum(id, song);
                }
            }

            await MusicToJSON();
           
            //await Windows.Storage.FileIO.AppendLinesAsync(songsfile, listOfStrings); // each entry in the list is written to the file on its own line.

            

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
            //StoreDict("songdict.txt", SongListStorage.SongDict);

            string songdictjson = JsonConvert.SerializeObject(SongListStorage.SongDict);
            string artistdictjson = JsonConvert.SerializeObject(SongListStorage.ArtistDict);
            string albumdictjson = JsonConvert.SerializeObject(SongListStorage.AlbumDict);
            //string albumflavourdict = JsonConvert.SerializeObject(SongListStorage.AlbumFlavourDict);

            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            StorageFile songdictfile = await storageFolder.CreateFileAsync("songdict.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(songdictfile, songdictjson);

            StorageFile artistdictfile = await storageFolder.CreateFileAsync("artistdict.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(artistdictfile, artistdictjson);

            StorageFile albumdictfile = await storageFolder.CreateFileAsync("albumdict.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(albumdictfile, albumdictjson);
        }

        public static async Task LoadMusicFromJSON()
        {
            try
            {
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
            }
            catch(Exception E)
            {
                Debug.WriteLine("Couldn't load music.");
                Debug.WriteLine(E.Message);
                GetSongs();
            }
            SongListStorage.GetSongList();


        }


            /*public static async Task StoreDict<T>(string filename, IDictionary dict)
            {
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                string json = JsonConvert.SerializeObject(dict);

                Windows.Storage.StorageFile file = await storageFolder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.ReplaceExisting);

                await Windows.Storage.FileIO.WriteTextAsync(file, json);
            }*/
        }
}
