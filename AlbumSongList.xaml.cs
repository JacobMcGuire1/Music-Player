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

        public string albumid;

        public int flavourid;

        public AlbumSongList()
        {
            this.InitializeComponent();
        }

        private void Songs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Flavour flavour = SongListStorage.AlbumFlavourDict[albumid][flavourid];
            flavour.ReorderSongs(Songs);
            //Songs.Clear();
            //Songs = flavour.ObserveSongs();
            //Songs.CollectionChanged += Songs_CollectionChanged;
        }

        //arg is a struct containing album id and an int representing which flavour of the album this is.
        //Original album is -1.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AlbumPage.Args args = (AlbumPage.Args)e.Parameter;
            albumid =  args.id;
            flavourid = args.flavourid;
            
            if (args.flavourid == -1)
            {
                Songs = SongListStorage.AlbumDict[albumid].ObserveSongs();
            }
            else
            {
                ListViewSongs.CanReorderItems = true;
                ListViewSongs.AllowDrop = true;
                ListViewSongs.Drop += ListViewSongs_Drop;
                Songs = SongListStorage.AlbumFlavourDict[albumid][flavourid].ObserveSongs();
                Songs.CollectionChanged += Songs_CollectionChanged;
                addSongButton.Visibility = (Visibility)0;
                addSongText.Visibility = (Visibility)0;
            }

            


        }

        private void ListViewSongs_Drop(object sender, DragEventArgs e)
        {
            Flavour flavour = SongListStorage.AlbumFlavourDict[albumid][flavourid];
            flavour.ReorderSongs(Songs);
        }

        

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            //Button b = (Button)sender;
            //b.Foreground = new SolidColorBrush(Windows.UI.Colors.Blue);

            int songid = (int)((Button)sender).Tag;

            /*foreach (Song song in Songs)
            {
                Media.Instance.
                Media.Instance.addSong(song.id);
            }

            Media.Instance.playSong(song);*/

            Media.Instance.PlayPlaylist(Songs, songid);
        }

        private void addToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            //Song song = ((Button)sender).Tag as Song;

            string song = (string)(((Button)sender).Tag);

            Media.Instance.addSong(song);
        }

        private void RemoveFromFlavourButton_Click(object sender, RoutedEventArgs e)
        {
            int TrackNumber = (int)((Button)sender).Tag;

            //Can maybe do this in a way that doesn't clear the list.

            Songs.Clear();

            ObservableCollection<Song> NewSongs = SongListStorage.AlbumFlavourDict[albumid][flavourid].RemoveSong(TrackNumber);

            foreach (Song song in NewSongs)
            {
                Songs.Add(song);
            }


        }

        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            //Put the flavour in the panel on the left to allow songs to be dragged onto it.
            SongListStorage.AlbumFlavourDict[albumid][flavourid].pinned = true;
        }

        private void StackPanel_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            StackPanel send = (StackPanel)sender;
            args.Data.SetText((String)send.Tag.ToString());
            args.Data.RequestedOperation = DataPackageOperation.Copy;
        }

    }
}
