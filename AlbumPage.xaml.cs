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
    public sealed partial class AlbumPage : Page
    {
        public ObservableCollection<Song> Songs { get; set; }
        = new ObservableCollection<Song>();

        public AlbumPage()
        {
            this.InitializeComponent();
        }

        public void ChangeAlbum(Album album)
        {
            Songs = album.ObserveSongs();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            String key = e.Parameter as string;
            Album album = SongListStorage.AlbumDict[key];
            ChangeAlbum(album);
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(Windows.UI.Colors.Blue);

            int song = (int)((Button)sender).Tag;

            Media.Instance.playSong(song);
        }

        private void addToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            //Song song = ((Button)sender).Tag as Song;

            int song = (int)(((Button)sender).Tag);

            Media.Instance.addSong(song);
        }
    }
}
