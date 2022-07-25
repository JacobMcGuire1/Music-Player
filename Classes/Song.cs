using Music_thing.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI;
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
            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(Path);
                return file;
            }
            catch (System.IO.FileNotFoundException e)
            {
                Debug.WriteLine("Couldn't get file for song " + ID);
                Debug.WriteLine(e.Message);
                return null;
            }
            catch(UnauthorizedAccessException B)
            {
                Debug.WriteLine("Couldn't get file for song " + ID);
                Debug.WriteLine(B.Message);
                return null;
            }

        }

        public int GetDiscInt()
        {
            try
            {
                int k = int.Parse(DiscNumber[0].ToString());
                return k;
            }
            catch
            {
                return 1;
            }
            
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

        public string GetListenCount()
        {
            return SongLog.GetSongListenCount(ID);
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
            var bitmapImage = new BitmapImage();
            if (file != null)
            {
                var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, (uint)size);
                
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    bitmapImage.SetSource(thumbnail);
                }
                else
                {
                    try
                    {
                        bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/DefaultAlbumArt.png"));
                    }
                    catch { }
                }
                bitmapImage.DecodePixelHeight = size;
                bitmapImage.DecodePixelWidth = size;
            }
            else
            {
                bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/DefaultAlbumArt.png"));
            }
            
            return bitmapImage;
        }

        public override int GetHashCode()
        {
            return 1877310944 + EqualityComparer<string>.Default.GetHashCode(ID);
        }

        public Brush GetBackgroundColour()
        {
            if (SongListStorage.PlaylistRepresentation[SongListStorage.CurrentPlaceInPlaylist].ID == ID)
            {
                return new SolidColorBrush(Color.FromArgb(100, 48, 179, 221));
            }
            return new SolidColorBrush(Color.FromArgb(0,0,0,0));
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
