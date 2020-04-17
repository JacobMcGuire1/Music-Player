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

        public bool artloaded = false;

        public AlbumList()
        {
            this.InitializeComponent();
            Albums = SongListStorage.Albums;
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
            /*if (true)
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
            }*/
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
    }
}
