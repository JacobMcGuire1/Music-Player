using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Music_thing
{
    public static class SongListStorage
    {
        public static ObservableCollection<Song> Songs { get; }
        = new ObservableCollection<Song>();

        public static int currentid;

        public static ObservableCollection<Song> PlaylistRepresentation { get; }
        = new ObservableCollection<Song>();

        public static int CurrentPlaceInPlaylist = new int();


        public static void GetSongList()
        {
            StorageFolder musicFolder = KnownFolders.MusicLibrary;

            GetFiles(musicFolder);
        }

        private static async void GetFiles(StorageFolder folder)
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
        }
    }
}

