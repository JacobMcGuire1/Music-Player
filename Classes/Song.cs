using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Music_thing
{
    public class Song
    {
        public string ID { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string AlbumArtist { get; set; }
        public string Album { get; set; }
        public TimeSpan Duration { get; set; }
        public uint Year { get; set; }
        public int TrackNumber { get; set; }
        public string DiscNumber { get; set; }
        public string Path { get; set; }
        public string AlbumKey { get; set; }
        public string ArtistKey { get; set; }


        public bool IsFlavour { get; set; }

        public uint Bitrate { get; set; }
        //public StorageFile File { get; set; }

        //public BitmapImage AlbumArt { get; set; }

        public async Task<StorageFile> GetFile()
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(Path);
            return file;
        }

        public override bool Equals(object obj)
        {
            if (this.ID == (obj as Song).ID) return true;
            return false;
        }

        public String SongToJSON()
        {
            String JSON = "{";
            return JSON;
        }

        public async Task Play()
        {
            await Media.Instance.PlaySong(ID);
        }

        public async Task AddToPlaylist()
        {
            await Media.Instance.AddSong(ID, true);
            App.GetForCurrentView().NotificationMessage("Added " + Artist + " - " + Title + " to now playing.");
        }

        // //public string GetPath()
        // {
        //   return File.Path;
        //}

        public string GetDuration()
        {
            //Double dur = Duration.TotalSeconds / 100;
            //dur.ToString("N2");
            //return dur.ToString("N2").Replace('.', ':');
            return Duration.ToString(@"mm\:ss");
        }

        public string GetStringYear()
        {
            if (Year == 0)
            {
                return "Unknown Year";
            }
            return Year.ToString();
        }

        public Visibility CheckIfFlavour()
        {
            if (IsFlavour)
            {
                return 0;
            }
            else
            {
                return (Visibility)1;
            }
        }

        public async Task<ImageSource> GetArt(int size)
        {
            var file = await GetFile();
            var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, (uint)size);
            var bitmapImage = new BitmapImage();
            if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
            {
                bitmapImage.SetSource(thumbnail);
            }
            else
            {
                 bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/DefaultAlbumArt.png"));
            }
            bitmapImage.DecodePixelHeight = size;
            bitmapImage.DecodePixelWidth = size;
            return bitmapImage;
        }

        public override int GetHashCode()
        {
            return 1877310944 + EqualityComparer<string>.Default.GetHashCode(ID);
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
                    AlbumArt =  bitmapImage;
                }
                else
                {
                    AlbumArt =  null;
                }
              
            //}
        }*/

    }

    
}
