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

        public bool artloaded = false;

        public AlbumList()
        {
            this.InitializeComponent();
            Albums = SongListStorage.Albums;

            ArtistSort.Tag = SongListStorage.SortType.Artist;
            DurationSort.Tag = SongListStorage.SortType.Duration;
            SongCountSort.Tag = SongListStorage.SortType.SongCount;
            AlbumNameSort.Tag = SongListStorage.SortType.AlbumName;
            RandomSort.Tag = SongListStorage.SortType.Random;

            AscSort.Tag = SongListStorage.SortDirection.Asc;
            DescSort.Tag = SongListStorage.SortDirection.Desc;

            SortTypeComboBox.SelectedItem = ArtistSort;
            SortDirectionComboBox.SelectedItem = AscSort;
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

        private void Albumbutton_Click(object sender, RoutedEventArgs e)
        {
            String albumkey = (string)(((Button)sender).Tag);

            this.Frame.Navigate(typeof(AlbumPage), albumkey);
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
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await PlayRandomAlbum();
        }

        private void RandomAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            GoToRandomAlbum();
        }

        private async void SortTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var NewSort = (SongListStorage.SortType)((((ComboBox)sender).SelectedItem) as ComboBoxItem).Tag;
            if (NewSort != SongListStorage.AlbumListSortType)
            {
                SongListStorage.AlbumListSortType = NewSort;
                await SongListStorage.UpdateAndOrderAlbums(true);
            }
        }

        private async void SortDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SongListStorage.AlbumListSortDirection = (SongListStorage.SortDirection)((((ComboBox)sender).SelectedItem) as ComboBoxItem).Tag;
            //await SongListStorage.UpdateAndOrderAlbums();

            var NewSort = (SongListStorage.SortDirection)((((ComboBox)sender).SelectedItem) as ComboBoxItem).Tag;
            if (NewSort != SongListStorage.AlbumListSortDirection)
            {
                SongListStorage.AlbumListSortDirection = NewSort;
                await SongListStorage.UpdateAndOrderAlbums(true);
            }
        }
    }
}
