using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
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
    public sealed partial class NowPlaying : Page
    {

        public ObservableCollection<Song> Playlist { get; set; }
        = new ObservableCollection<Song>();

        private int oldmoveindex = -1;
        private bool lastremovewasdel = false;


        public NowPlaying()
        {
            this.InitializeComponent();

            Playlist = SongListStorage.PlaylistRepresentation;

            Playlist.CollectionChanged += Playlist_CollectionChanged;

            //ListViewPlayList
        }

        private async void Playlist_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (!lastremovewasdel)
                {
                    oldmoveindex = e.OldStartingIndex;
                }
                lastremovewasdel = false;
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                int newmoveindex = e.NewStartingIndex;
                if (oldmoveindex != -1)
                {
                    var currenttime = Media.Instance.GetSongTime();
                    int place = SongListStorage.CurrentPlaceInPlaylist;
                    if (oldmoveindex == place)
                    {
                        place = newmoveindex;
                    }
                    else
                    {
                        if (oldmoveindex < place && newmoveindex >= place)
                        {
                            place--;
                        }
                        if (oldmoveindex > place && newmoveindex <= place)
                        {
                            place++;
                        }
                    }
                    var newplaylist = new MediaPlaybackList();
                    foreach (Song song in Playlist)
                    {
                        var mediaPlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(await song.GetFile()));
                        newplaylist.Items.Add(mediaPlaybackItem);
                    }
                    newplaylist.CurrentItemChanged += Media.Instance.Playlist_CurrentItemChanged;
                    currenttime = Media.Instance.GetSongTime();
                    Media.Instance.mediaPlayer.Source = newplaylist;
                    Media.Instance.Playlist = newplaylist;
                    newplaylist.MoveTo((uint)place);
                    SongListStorage.CurrentPlaceInPlaylist = place;
                    Media.Instance.SetSongTime(currenttime);
                    await SongListStorage.SaveNowPlaying();
                }
                oldmoveindex = -1;
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int index = GetIndexFromButton(button);
            Media.Instance.MoveTo(index);
        }

        private async void addToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var songid = (string)button.Tag;
            var song = SongListStorage.SongDict[songid];
            await song.AddToPlaylist();
        }

        private async void Removebutton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int index = GetIndexFromButton(button);
            lastremovewasdel = true;
            await Media.Instance.RemoveSong(index);
        }

        private int GetIndexFromButton(DependencyObject obj)
        {
            int index = -1;
            while (index == -1)
            {
                index = ListViewPlayList.IndexFromContainer(obj);
                obj = VisualTreeHelper.GetParent(obj);
            }
            return index;
        }

    }
}
