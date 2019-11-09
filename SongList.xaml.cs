using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
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
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.FileProperties;
//using Windows.Media.Playback;
//using Windows.Media.Core;
using System.Diagnostics;
using Windows.Media.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Music_thing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SongList : Page
    {
        //private MediaPlayer mediaPlayer; //Get rud of this

        public ObservableCollection<Song> Songs { get; }
        = new ObservableCollection<Song>();

        public SongList()
        {
            this.InitializeComponent();

            Songs = SongListStorage.Songs;

        }


        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(Windows.UI.Colors.Blue);

            string song = (string)((Button)sender).Tag;

            Media.Instance.playSong(song);
        }


        /*private async void GetFiles(StorageFolder folder)
        {
            StorageFolder fold = folder;

            var items = await fold.GetItemsAsync();

            foreach (var item in items)
            {
                if (item.GetType() == typeof(StorageFile))
                {
                    //StorageFile Music = item as StorageFile;

                    MusicProperties musicProperties = await (item as StorageFile).Properties.GetMusicPropertiesAsync();

                    if (musicProperties.Title != "")
                    {
                        this.Songs.Add(new Song() {
                            Title = musicProperties.Title,
                            Album = musicProperties.Album,
                            AlbumArtist = musicProperties.AlbumArtist,
                            Artist = musicProperties.Artist,
                            Year = musicProperties.Year,
                            Duration = musicProperties.Duration,
                            File = item as StorageFile
                        });
                    }
                    else
                    {
                        //this.Songs.Add(new Song() { Title = item.Path.ToString() });
                    }
                    
                    
                }
                else
                    GetFiles(item as StorageFolder);
            }

            //listView.ItemsSource = files;
        }*/

        private void addToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            //Song song = ((Button)sender).Tag as Song;

            string song = (string)(((Button)sender).Tag);

            Media.Instance.addSong(song);
        }
    }
}
