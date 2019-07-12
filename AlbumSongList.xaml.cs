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

        public AlbumSongList()
        {
            this.InitializeComponent();
        }

        //arg is a struct containing album id and an int representing which flavour of the album this is.
        //Original album is -1.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AlbumPage.Args args = (AlbumPage.Args)e.Parameter;
            String albumid =  args.id;
            //Artist artist = SongListStorage.ArtistDict[key];
            if (args.flavour == -1)
            {
                Songs = SongListStorage.AlbumDict[albumid].ObserveSongs();
            }
            else
            {
                Album album = new Album()
                {
                    Songids = SongListStorage.AlbumFlavours[albumid][args.flavour]
                };
                Songs = album.ObserveSongs();
            }
            

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

            int song = (int)(((Button)sender).Tag);

            Media.Instance.addSong(song);
        }

        private void RemoveFromFlavourButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
