using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
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

            if (true)
            {
                foreach (Album album in Albums)
                {
                    if (album.Albumart == null)
                    {
                        try
                        {
                            album.Albumart = await album.GetAlbumArt(200, SongListStorage.SongDict);
                            //Bindings.Update();
                        }
                        catch
                        {

                        }
                    }
                }
                artloaded = true;
            }
        }

        public void ChangeArtist(string artistid)
        {
            //this.artist = artist;
            if (artistid != null)
            {
                Artist artist = SongListStorage.ArtistDict[artistid];
                this.artist = artist;
                if (TheBigGrid.RowDefinitions.Count == 1)
                {
                    var height = new GridLength(50.0);
                    var rowdef = new RowDefinition() { Height = height };
                    TheBigGrid.RowDefinitions.Insert(0, rowdef);
                }
                
                ArtistInfoStack.Visibility = Visibility.Visible;
                Albums.Clear();
                artist.Albums.Sort((x, y) => SongListStorage.AlbumDict[y].Year.CompareTo(SongListStorage.AlbumDict[x].Year)); //Sorts the albums by year. Should change this to allow choice of sorting method?
                foreach (string albumid in artist.Albums)
                {
                    Albums.Add(SongListStorage.AlbumDict[albumid]);
                }
                //Albums.Sort((x, y) => SongListStorage.SongDict[x].Title.CompareTo(SongListStorage.SongDict[y].Title));
            }
            else
            {
                ArtistInfoStack.Visibility = Visibility.Collapsed;
                if (TheBigGrid.RowDefinitions.Count > 1)
                {
                    TheBigGrid.RowDefinitions.RemoveAt(0);
                }
                this.artist = null;
                Albums = SongListStorage.Albums;
            }
            Bindings.Update();
            
        }

        private async void playalbumButton_Click(object sender, RoutedEventArgs e)
        {
            string albumid = (string)((Button)sender).Tag;
            Album album = SongListStorage.GetPinnedFlavourForAlbum(albumid); //SongListStorage.AlbumDict[albumid];
            await album.Play(); //mb 1
        }

        private async void AddAlbumToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string albumid = (string)((Button)sender).Tag;
            Album album = SongListStorage.GetPinnedFlavourForAlbum(albumid);
            await album.AddToPlaylist();
        }

        private void Jess_ItemClick(object sender, ItemClickEventArgs e)
        {
            Album album = (Album)e.ClickedItem;
            this.Frame.Navigate(typeof(AlbumPage), album.Key);
        }

        private void Jess_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var album = (Album)e.Items[0];

            var songids = album.Songids;

            var items = new StringBuilder();
            foreach (String songid in songids)
            {
                if (items.Length > 0) items.AppendLine();
                items.Append(songid);
            }
            var t = items.ToString();
            e.Data.SetText(items.ToString());

            //args.Data.SetData("", songids);
            e.Data.RequestedOperation = DataPackageOperation.Copy;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            /*foreach (Album album in Albums)
            {
                album.albumart = null;
            }
            artloaded = false;*/
        }
    }
}
