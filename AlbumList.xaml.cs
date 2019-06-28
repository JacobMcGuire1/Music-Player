﻿using System;
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
    public sealed partial class AlbumList : Page
    {
        public ObservableCollection<Album> Albums { get; set; }
        = new ObservableCollection<Album>();

        public Artist artist;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            String artistid = e.Parameter as string;
            //Artist artist = SongListStorage.ArtistDict[key];
            ChangeArtist(artistid);
        }

        public void ChangeArtist(string artistid)
        {
            //this.artist = artist;
            if (artistid != null)
            {
                Artist artist = SongListStorage.ArtistDict[artistid];
                this.artist = artist;
                Albums.Clear();
                foreach (string albumid in artist.Albums)
                {
                    Albums.Add(SongListStorage.AlbumDict[albumid]);
                }
            }
            else
            {
                this.artist = null;
                Albums = SongListStorage.Albums;
            }
            
        }
    }
}
