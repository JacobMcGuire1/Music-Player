using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Music_thing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumList : Page
    {
        public ObservableCollection<Album> Albums { get; set; }
        = new ObservableCollection<Album>();

        public Artist artist;

        public bool artloaded = false;

        public AlbumList()
        {
            this.InitializeComponent();

            if (artist != null)
            {
                Albums = SongListStorage.Albums;
            }
            
        }

        private void Albumbutton_Click(object sender, RoutedEventArgs e)
        {
            //Button b = (Button)sender;
            //b.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            //Song song = ((Button)sender).Tag as Song;

            String albumkey = (string)(((Button)sender).Tag);

            this.Frame.Navigate(typeof(AlbumPage), albumkey);

            //Media.Instance.addSong(song);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            String artistid = e.Parameter as string;
            //Artist artist = SongListStorage.ArtistDict[key];
            ChangeArtist(artistid);

            if (!artloaded)
            {
                foreach (Album album in Albums)
                {
                    if (album.Albumart == null)
                    {
                        try
                        {
                            album.Albumart = await album.GetAlbumArt(200);
                            //Bindings.Update();
                        }
                        catch
                        {

                        }
                        /*catch
                        {
                            //BitmapImage bitmapImage = new BitmapImage();
                            //bitmapImage.UriSource = new Uri("Assets/Album.png");
                            //BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///[Music_thing]/Assets/Album.png"));
                            BitmapImage bitmapImage = new BitmapImage(new Uri(this.BaseUri, "/Assets/Album.png"));
                            //BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///[Music_Thing]/Assets/Album.png"));
                            bitmapImage.DecodePixelHeight = 200;
                            bitmapImage.DecodePixelWidth = 200;
                            album.Albumart = bitmapImage;
                        }*/
                    }
                }
                artloaded = true;
            }
        }

        protected new void Unloaded(NavigationEventArgs e)
        {
            foreach (Album album in Albums)
            {
                album.albumart = null;
            }
            artloaded = false;
        }

        public void ChangeArtist(string artistid)
        {
            //this.artist = artist;
            if (artistid != null)
            {
                Artist artist = SongListStorage.ArtistDict[artistid];
                this.artist = artist;
                Albums.Clear();
                artist.Albums.Sort((x, y) => SongListStorage.AlbumDict[y].year.CompareTo(SongListStorage.AlbumDict[x].year)); //Sorts the albums by year. Should change this to allow choice of sorting method?
                foreach (string albumid in artist.Albums)
                {
                    Albums.Add(SongListStorage.AlbumDict[albumid]);
                }
                //Albums.Sort((x, y) => SongListStorage.SongDict[x].Title.CompareTo(SongListStorage.SongDict[y].Title));
            }
            else
            {
                this.artist = null;
                Albums = SongListStorage.Albums;
            }
            
        }

        private void playalbumButton_Click(object sender, RoutedEventArgs e)
        {
            string albumid = (string)((Button)sender).Tag;
            Album album = SongListStorage.AlbumDict[albumid];
            Media.Instance.PlayPlaylist(album.ObserveSongs(), 1); //mb 1
        }
    }
}
