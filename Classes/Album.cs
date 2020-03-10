using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Music_thing
{
    public class Album
    {
        public string name { get; set; }

        public string artist { get; set; }

        public string key { get; set; }

        public int year { get; set; }

        //public BitmapImage albumart;
        public Windows.UI.Xaml.Media.ImageSource albumart200;

        public Windows.UI.Xaml.Media.ImageSource albumart250;

        public Windows.UI.Xaml.Media.ImageSource albumart100;

       // public BitmapIcon icon;

        public List<string> Songids { get; set; }
        = new List<string>();

        public ObservableCollection<Song> ObserveSongs()
        {
            ObservableCollection<Song> Songs = new ObservableCollection<Song>();

            foreach (string songid in Songids)
            {
                Song song = SongListStorage.SongDict[songid];
                song.isFlavour = false; //MB REMOVE
                Songs.Add(song);
            }

            return Songs;
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

        public List<string> AddSong(string songid)
        {
           // if (Songids.Count != 0) //fix
           // {
                SetAlbumArt(songid);
           // }
            Songids.Add(songid);
            OrderByTrack();
            return Songids;
        }

        //Function to sort songs by track number.
        public void OrderByTrack()
        {
            //Need to add disk number support
            //Songids = Songids.OrderBy(x => SongListStorage.SongDict[x].TrackNumber) as List<int>;

            Songids.Sort((x, y) => SongListStorage.SongDict[x].TrackNumber - SongListStorage.SongDict[y].TrackNumber);
        }


        public async void SetAlbumArt(string songid)
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

                /*thumbnail = await song.File.GetThumbnailAsync(ThumbnailMode.MusicView, 20);
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                    //ImageControl.Source = bitmapImage;

                    bitmapImage.DecodePixelHeight = 20;
                    bitmapImage.DecodePixelWidth = 20;
                }*/


                //using (var )
            }
        }

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
