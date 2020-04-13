//using Music_thing.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Music_thing
{
    public class Album : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public string Artist { get; set; }

        public string Key { get; set; }

        public int Year { get; set; }

        //public int priorityflavour = -1;

        //Should only be in memory for the album list.
        [JsonIgnore]
        public ImageSource albumart;

        public event PropertyChangedEventHandler PropertyChanged;

        public string AlbumArtSongId { get; set; }

        public List<string> Songids { get; set; }
        = new List<string>();

       // public HashSet<long> Flavours { get; set; }
      //  = new HashSet<long>();

        public String GetSongCount()
        {
            int count = Songids.Count;
            return count.ToString();
            /*switch (count)
            {
                case 0:
                    return "0 songs";
                case 1:
                    return "1 song";
                default:
                    return count + " songs";
            }*/
        }

        public List<Playlist> GetFlavourList()
        {
            HashSet<long> Flavours;
            if (SongListStorage.AlbumPlaylistDict.ContainsKey(Key))
            {
                Flavours = SongListStorage.AlbumPlaylistDict[Key];
            }
            else
            {
                return new List<Playlist>();
            }

            var ids = Flavours.ToList<long>();
            var flavours = new List<Playlist>();
            foreach (long id in ids)
            {
                flavours.Add(SongListStorage.PlaylistDict[id]);
            }
            return flavours;
        }

        public bool AddFlavour(long flavourid)
        {
            if (!SongListStorage.AlbumPlaylistDict.ContainsKey(Key))
            {
                SongListStorage.AlbumPlaylistDict.TryAdd(Key, new HashSet<long>());
            }
            return SongListStorage.AlbumPlaylistDict[Key].Add(flavourid);
        }

        public bool RemoveFlavour(long flavourid)
        {
            if (!SongListStorage.AlbumPlaylistDict.ContainsKey(Key))
            {
                return false;
            }
            return SongListStorage.AlbumPlaylistDict[Key].Remove(flavourid);
        }

        public ObservableCollection<Song> ObserveSongs(ConcurrentDictionary<string, Song> SongDict)
        {
            ObservableCollection<Song> Songs = new ObservableCollection<Song>();

            foreach (string songid in Songids)
            {
                Song song = SongDict[songid];
                song.IsFlavour = false; //MB REMOVE
                Songs.Add(song);
            }

            return Songs;
        }

        public async Task Play()
        {
            await Media.Instance.PlayPlaylist(ObserveSongs(SongListStorage.SongDict), 1, null, true); //mb 1?
        }

        public async Task AddToPlaylist()
        {
            foreach (String songid in Songids)
            {
                await Media.Instance.AddSong(songid, false);
            }
            App.GetForCurrentView().NotificationMessage("Added " + Artist + " - " + Name + " to now playing.");
            await SongListStorage.SaveNowPlaying();
        }


        /*public ObservableCollection<Song> ObserveSongsForFlavour()
        {
            ObservableCollection<Song> Songs = new ObservableCollection<Song>();

            foreach (int songid in Songids)
            {
                Song song = SongListStorage.SongDict[songid];
                song.isFlavour = true;
                Songs.Add(song);
            }

            return Songs;
        }*/

        public List<string> AddSong(string songid, ConcurrentDictionary<string, Song> SongDict)
        {
           // if (Songids.Count != 0) //fix
           // {
                SetAlbumArt(songid, SongDict);
           // }
            Songids.Add(songid);
            OrderByTrack(SongDict);
            return Songids;
        }

        public string GetStringYear()
        {
            if (Year == 0)
            {
                return "N/A";
            }
            return Year.ToString();
        }

        //Function to sort songs by track number.
        public void OrderByTrack(ConcurrentDictionary<string, Song> SongDict)
        {
            //Need to add disk number support
            //Songids = Songids.OrderBy(x => SongListStorage.SongDict[x].TrackNumber) as List<int>;

            Songids.Sort((x, y) => SongDict[x].TrackNumber - SongDict[y].TrackNumber);
        }

        public async void SetAlbumArt(string songid, ConcurrentDictionary<string, Song> SongDict)
        {
            Song song = SongDict[songid];
            if (AlbumArtSongId == null || SongDict[songid].TrackNumber == 1)
            {
                var file = await song.GetFile();
                var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView);
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    AlbumArtSongId = songid;
                }
            }
        }

        public async Task<ImageSource> GetAlbumArt(int size, ConcurrentDictionary<string, Song> SongDict)
        {
            if (AlbumArtSongId != null)
            {
                return await SongDict[AlbumArtSongId].GetArt(size);
            }
            BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/DefaultAlbumArt.png"))
            {
                DecodePixelHeight = size,
                DecodePixelWidth = size
            };
            return bitmapImage;
        }

        [JsonIgnore]
        public ImageSource Albumart
        {
            get
            {
                return this.albumart;
            }
            set
            {
                NotifyPropertyChanged();
                if (value != this.albumart)
                {
                    this.albumart = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public async void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {

            // Your UI update code goes here!
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        /* public async void SetAlbumArt(string songid)
            {
                Song song = SongListStorage.SongDict[songid];
                if (albumart200 == null || SongListStorage.SongDict[songid].TrackNumber == 1)
                {
                    var file = await song.GetFile();
                    var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 200);
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        //ImageControl.Source = bitmapImage;

                        bitmapImage.DecodePixelHeight = 200;
                        bitmapImage.DecodePixelWidth = 200;
                        albumart200 = bitmapImage;
                    }

                    thumbnail = await (await song.GetFile()).GetThumbnailAsync(ThumbnailMode.MusicView, 250);
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        //ImageControl.Source = bitmapImage;

                        bitmapImage.DecodePixelHeight = 250;
                        bitmapImage.DecodePixelWidth = 250;
                        albumart250 = bitmapImage;
                    }

                    thumbnail = await (await song.GetFile()).GetThumbnailAsync(ThumbnailMode.MusicView, 100);
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        //ImageControl.Source = bitmapImage;

                        bitmapImage.DecodePixelHeight = 100;
                        bitmapImage.DecodePixelWidth = 100;
                        albumart100 = bitmapImage;
                    }


                    //using (var )
                }
            }*/









        /*public async void SetAlbumArt()
        {
            var thumbnail = await File.GetThumbnailAsync(ThumbnailMode.MusicView, 300);
            //var result = task.WaitAndUnwrapException();
            //using (StorageItemThumbnail thumbnail = File.GetThumbnailAsync(ThumbnailMode.MusicView, 300).Wait() )
            //{
            if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(thumbnail);
                //ImageControl.Source = bitmapImage;
                AlbumArt = bitmapImage;
            }
            else
            {
                AlbumArt = null;
            }

            //}
        }*/

    }

}
