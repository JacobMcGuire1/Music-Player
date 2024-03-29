﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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

        public Playlist flavour;

        private bool initialised = false;

        public AlbumSongList()
        {
            this.InitializeComponent();
            Media.Instance.Playlist.CurrentItemChanged += Playlist_CurrentItemChanged;
            ShowCurrentlyPlayingSong();
        }

        private async void Playlist_CurrentItemChanged(Windows.Media.Playback.MediaPlaybackList sender, Windows.Media.Playback.CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                ShowCurrentlyPlayingSong();
            });
        }

        private void ShowCurrentlyPlayingSong()
        {
            var k = ListViewSongs.ItemsPanelRoot;
            if (SongListStorage.PlaylistRepresentation.Count != 0 && k != null && k.Children.Count - 1 >= SongListStorage.CurrentPlaceInPlaylist)
            {
                initialised = true;
                foreach (ListViewItem listViewItem in k.Children)
                {
                    if (((Song)listViewItem.Content).ID.Equals(SongListStorage.PlaylistRepresentation[SongListStorage.CurrentPlaceInPlaylist].ID))
                    {
                        listViewItem.Background = new SolidColorBrush(Color.FromArgb(100, 48, 179, 221));
                    }
                    else
                    {
                        listViewItem.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                    }
                }
                /*var c = (ListViewItem)k.Children[SongListStorage.CurrentPlaceInPlaylist];
                if (c.Content != null && ((Song)c.Content).ID.Equals())
                {
                    c.Background = new SolidColorBrush(Color.FromArgb(100, 48, 179, 221));
                }*/

            }
        }

        private async void Songs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            flavour.ReorderSongs(Songs);
            await flavour.SavePlaylistFile(false);
        }

        public void ReloadSongs()
        {
            Songs = flavour.ObserveSongs();
            Songs.CollectionChanged += Songs_CollectionChanged;
            Bindings.Update();
        }

        //arg is a struct containing album id and an int representing which flavour of the album this is.
        //Original album is -1.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!(e.Parameter is Playlist))
            {
                album = (Album)e.Parameter;
                flavour = null;
                Songs = album.ObserveSongs(SongListStorage.SongDict);
            }
            else
            {
                flavour = (Playlist)e.Parameter;
                //album = SongListStorage.AlbumDict[flavour.alb]
                album = null;
                ListViewSongs.CanReorderItems = true;
                ListViewSongs.AllowDrop = true;
                ListViewSongs.Drop += ListViewSongs_Drop;
                Songs = flavour.ObserveSongs();
                Songs.CollectionChanged += Songs_CollectionChanged;
                addSongButton.Visibility = Visibility.Visible;
                addSongText.Visibility = Visibility.Visible;
                if (flavour.isflavour)
                {
                    PinButtonPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    PinButtonPanel.Visibility = Visibility.Collapsed;
                    
                }

                if (flavour.pinned)
                {
                    addSongText.Text = "Unpin this flavour from the side panel";
                }
                else
                {
                    addSongText.Text = "Pin your flavour to the side panel";
                }
            }
        }

        private async void ListViewSongs_Drop(object sender, DragEventArgs e)
        {
            flavour.ReorderSongs(Songs);
            await flavour.SavePlaylistFile(false);
        }

        

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            int tracknumber = (int)((Button)sender).Tag;

            var parent = VisualTreeHelper.GetParent((Button)sender) as UIElement;
            string songid = (string)(parent as Grid).Tag;

            await Media.Instance.PlayPlaylist(Songs, tracknumber, songid, true);
        }

        private async void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string songid = (string)(((Button)sender).Tag);
            Song song = SongListStorage.SongDict[songid];
            App.GetForCurrentView().NotificationMessage("Added '" + song.Artist + " - " + song.Title + "' to now playing.");

            await Media.Instance.AddSong(songid, true);
        }

        private async void RemoveFromFlavourButton_Click(object sender, RoutedEventArgs e)
        {
            int TrackNumber = (int)((Button)sender).Tag - 1;
            ObservableCollection<Song> NewSongs = flavour.RemoveSong(TrackNumber);
            Songs.RemoveAt(Songs.Count - 1);

            for(int i = 0; i < NewSongs.Count; i++)
            {
                Songs[i] = NewSongs[i];
            }

            //await SongListStorage.SaveFlavours();
            await flavour.SavePlaylistFile(false);


        }

        private async void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            //Put the flavour in the panel on the left to allow songs to be dragged onto it.
            if (flavour.isflavour)
            {
                flavour.pinned = !flavour.pinned;
            }
            if (flavour.pinned)
            {
                addSongText.Text = "Unpin this flavour from the side panel";
            }
            else
            {
                addSongText.Text = "Pin your flavour to the side panel";
            }
            await flavour.SavePlaylistFile(false);
            await App.GetForCurrentView().LoadPinnedFlavours();
        }

        private void StackPanel_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            Grid send = (Grid)sender;
            var songid = send.Tag.ToString();
            args.Data.SetText(songid);
            args.Data.RequestedOperation = DataPackageOperation.Copy;
        }

        private async void PlayAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            if (Songs.Count > 0)
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
                
        }


        private async void AddAlbumToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (Songs.Count > 0)
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

        internal void ToggledPinned()
        {
            if (flavour.pinned)
            {
                addSongText.Text = "Unpin this flavour from the side panel";
            }
            else
            {
                addSongText.Text = "Pin your flavour to the side panel";
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid grid = (Grid)sender;
            var artistname = (TextBlock)grid.FindName("SongArtistNameTextBlock");
            if (flavour == null)
            {
                grid.ColumnDefinitions[7] = new ColumnDefinition() { Width = new GridLength(0.0, GridUnitType.Star) };
            }
            if (e.NewSize.Width <= 800)
            {
                artistname.Visibility = Visibility.Collapsed;
                grid.ColumnDefinitions[4] = new ColumnDefinition() { Width = new GridLength(0.0, GridUnitType.Star) };
            }
            else
            {
                artistname.Visibility = Visibility.Visible;
                grid.ColumnDefinitions[4] = new ColumnDefinition() { Width = new GridLength(1.5, GridUnitType.Star) };
            }
        }

        private void ListViewSongs_LayoutUpdated(object sender, object e)
        {
            if (!initialised) ShowCurrentlyPlayingSong();
        }
    }
}
