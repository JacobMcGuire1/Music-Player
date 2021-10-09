using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Music_thing.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ArtistPage : Page
    {
        public ObservableCollection<Album> Albums { get; set; }
        = new ObservableCollection<Album>();

        public Artist artist;

        private SongListStorage.SortDirection SortDirection = SongListStorage.SortDirection.Desc;
        private SongListStorage.SortType SortType = SongListStorage.SortType.Year;

        private string searchterm = "";

        public ArtistPage()
        {
            this.InitializeComponent();

            DurationSort.Tag = SongListStorage.SortType.Duration;
            SongCountSort.Tag = SongListStorage.SortType.SongCount;
            AlbumNameSort.Tag = SongListStorage.SortType.AlbumName;
            YearSort.Tag = SongListStorage.SortType.Year;
            RandomSort.Tag = SongListStorage.SortType.Random;

            AscSort.Tag = SongListStorage.SortDirection.Asc;
            DescSort.Tag = SongListStorage.SortDirection.Desc;

            SortTypeComboBox.SelectedItem = YearSort;
            SortDirectionComboBox.SelectedItem = DescSort;
        }

        private async void jess_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.ToString() == "Back")
            {
                if (searchterm.Length > 0)
                {
                    searchterm = searchterm.Remove(searchterm.Length - 1);
                    SearchTextBlock.Text = searchterm;
                }
                return;
            }
            if (e.Key.ToString().Length == 1)
            {
                SearchPopup.IsOpen = true;
                var listview = jess;
                searchterm = searchterm + e.Key.ToString().ToUpperInvariant();
                Album result = null;
                foreach (Album album in Albums)
                {
                    if (album.Name.Length >= searchterm.Length && album.Name.Substring(0, searchterm.Length).ToLowerInvariant() == searchterm.ToLowerInvariant())
                    {
                        result = album;
                        break;
                    }
                }
                if (result == null)
                {
                    //Need to change this to albums
                    var r = SongListStorage.SearchAlbums(searchterm, Albums);
                    if (r.Count > 0) result = r[0];
                }
                if (result != null)
                {
                    listview.ScrollIntoView(result);
                    listview.SelectedItem = result;
                }
                var oldsearchterm = searchterm;
                SearchTextBlock.Text = searchterm;
                await Task.Delay(1000);
                if (searchterm.Equals(oldsearchterm))
                {
                    SearchPopup.IsOpen = false;
                    searchterm = "";
                    SearchTextBlock.Text = "";
                }
            }
        }

        private void Albumbutton_Click(object sender, RoutedEventArgs e)
        { 
            String albumkey = (string)(((Button)sender).Tag);
            this.Frame.Navigate(typeof(AlbumPage), albumkey);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            String artistid = e.Parameter as string;
            ChangeArtist(artistid);
            jess.Focus(FocusState.Programmatic);
            if (true)
            {
                foreach (Album album in Albums)
                {
                    if (album.Albumart == null)
                    {
                        try
                        {
                            album.Albumart = await album.GetAlbumArt(200, SongListStorage.SongDict);
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        public void ChangeArtist(string artistid)
        {
            Artist artist = SongListStorage.ArtistDict[artistid];
            this.artist = artist;
            Albums.Clear();
            SortAlbums();
            //artist.Albums.Sort((x, y) => SongListStorage.AlbumDict[y].Year.CompareTo(SongListStorage.AlbumDict[x].Year)); //Sorts the albums by year. Should change this to allow choice of sorting method?
            Bindings.Update();

        }

        private void SortAlbums()
        {
            if (artist != null)
            {
                SongListStorage.SortAlbumKeyList(artist.Albums, SortType, SortDirection);
                Albums.Clear();
                foreach (string albumid in artist.Albums)
                {
                    Albums.Add(SongListStorage.AlbumDict[albumid]);
                }
            }
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
            e.Data.RequestedOperation = DataPackageOperation.Copy;
        }

        private async Task PlayRandomAlbum()
        {
            if (Albums.Count > 0)
            {
                var rand = new Random();
                int chosen = rand.Next(Albums.Count);
                var albumkey = Albums[chosen].Key;
                Album album = SongListStorage.GetPinnedFlavourForAlbum(albumkey);
                await album.Play();
            }
        }

        private void GoToRandomAlbum()
        {
            if (Albums.Count > 0)
            {
                var rand = new Random();
                int chosen = rand.Next(Albums.Count);
                Album album = Albums[chosen];
                this.Frame.Navigate(typeof(AlbumPage), album.Key);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        { 
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await PlayRandomAlbum();
        }

        private void RandomAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            GoToRandomAlbum();
        }

        private void SortTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var NewSort = (SongListStorage.SortType)((((ComboBox)sender).SelectedItem) as ComboBoxItem).Tag;
            if (NewSort != SortType)
            {
                SortType = NewSort;
                SortAlbums();
            }
        }

        private void SortDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var NewSort = (SongListStorage.SortDirection)((((ComboBox)sender).SelectedItem) as ComboBoxItem).Tag;
            if (NewSort != SortDirection)
            {
                SortDirection = NewSort;
                SortAlbums();
            }
        }

        private void jess_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as GridView).Focus(FocusState.Programmatic);
        }


    }
}

