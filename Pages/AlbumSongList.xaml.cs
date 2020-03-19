using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace Music_thing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumSongList : Page
    {
        public ObservableCollection<Song> Songs { get; set; }
        = new ObservableCollection<Song>();

        public Album album;

        public Flavour flavour;

        public AlbumSongList()
        {
            this.InitializeComponent();
        }

        private void Songs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            flavour.ReorderSongs(Songs);
            SongListStorage.SaveFlavours();
        }

        //arg is a struct containing album id and an int representing which flavour of the album this is.
        //Original album is -1.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!(e.Parameter is Flavour))
            {
                album = (Album)e.Parameter;
                flavour = null;
                Songs = album.ObserveSongs(SongListStorage.SongDict);
            }
            else
            {
                flavour = (Flavour)e.Parameter;
                //album = SongListStorage.AlbumDict[flavour.alb]
                album = null;
                ListViewSongs.CanReorderItems = true;
                ListViewSongs.AllowDrop = true;
                ListViewSongs.Drop += ListViewSongs_Drop;
                Songs = flavour.ObserveSongs();
                Songs.CollectionChanged += Songs_CollectionChanged;
                addSongButton.Visibility = (Visibility)0;
                addSongText.Visibility = (Visibility)0;
            }

            


        }

        private void ListViewSongs_Drop(object sender, DragEventArgs e)
        {
            flavour.ReorderSongs(Songs);
            SongListStorage.SaveFlavours();
        }

        

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            int songid = (int)((Button)sender).Tag;

            await Media.Instance.PlayPlaylist(Songs, songid, true);
        }

        private async void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            //Song song = ((Button)sender).Tag as Song;

            string songid = (string)(((Button)sender).Tag);
            Song song = SongListStorage.SongDict[songid];
            App.GetForCurrentView().NotificationMessage("Added '" + song.Artist + " - " + song.Title + "' to now playing.");

            await Media.Instance.AddSong(songid);
        }

        private void RemoveFromFlavourButton_Click(object sender, RoutedEventArgs e)
        {
            int TrackNumber = (int)((Button)sender).Tag - 1;
            ObservableCollection<Song> NewSongs = flavour.RemoveSong(TrackNumber);
            Songs.RemoveAt(Songs.Count - 1);

            for(int i = 0; i < NewSongs.Count; i++)
            {
                Songs[i] = NewSongs[i];
            }

            SongListStorage.SaveFlavours();


        }

        private async void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            //Put the flavour in the panel on the left to allow songs to be dragged onto it.
            flavour.pinned = true;
            await App.GetForCurrentView().LoadPinnedFlavours();
        }

        private void StackPanel_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            StackPanel send = (StackPanel)sender;
            var songid = send.Tag.ToString();
            args.Data.SetText(songid);
            args.Data.RequestedOperation = DataPackageOperation.Copy;
        }

        private async void PlayAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            if (flavour == null)
            {
                await album.Play();
            }
            else
            {
                await flavour.Play();
            }
                
        }


        private async void AddAlbumToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (flavour == null)
            {
                await album.AddToPlaylist();
            }
            else
            {
                await flavour.AddToPlaylist();
            }
        }
    }
}
