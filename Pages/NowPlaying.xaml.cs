using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
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
    public sealed partial class NowPlaying : Page
    {

        public ObservableCollection<Song> Playlist { get; set; }
        = new ObservableCollection<Song>();

        private int oldmoveindex = -1;
        private bool lastremovewasdel = false;

        private bool initialised = false;


        public NowPlaying()
        {
            this.InitializeComponent();

            Playlist = SongListStorage.PlaylistRepresentation;

            Playlist.CollectionChanged += Playlist_CollectionChanged;
            Media.Instance.Playlist.CurrentItemChanged += Playlist_CurrentItemChanged;


            ShowCurrentlyPlayingSong();

            //ListViewPlayList
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ShowCurrentlyPlayingSong();
        }

        private async void Playlist_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                ShowCurrentlyPlayingSong();
            });
        }

        private void ShowCurrentlyPlayingSong()
        {
            var k = ListViewPlayList.ItemsPanelRoot;
            if (k != null)
            {
                ListViewItem currentsongitem = null;
                for (int i = 0; i < k.Children.Count; i++)
                {
                    ListViewItem listViewItem = k.Children[i] as ListViewItem;
                    if (listViewItem.Content != null && SongListStorage.PlaylistRepresentation.Count > 0 && (listViewItem.Content as Song).ID == SongListStorage.PlaylistRepresentation[SongListStorage.CurrentPlaceInPlaylist].ID)
                    {
                        if (i == SongListStorage.CurrentPlaceInPlaylist)
                        {
                            currentsongitem = listViewItem;
                        }
                        if (currentsongitem == null)
                        {
                            currentsongitem = listViewItem;
                        }
                    }
                }
                if (currentsongitem != null)
                {
                    foreach (ListViewItem listViewItem in k.Children)
                    {
                        listViewItem.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                    }
                    currentsongitem.Background = new SolidColorBrush(Color.FromArgb(100, 48, 179, 221));
                }
            }
            

            /*if (SongListStorage.PlaylistRepresentation.Count != 0 && k != null && k.Children.Count - 1 >= SongListStorage.CurrentPlaceInPlaylist)
            {
                initialised = true;
                foreach (ListViewItem listViewItem in k.Children)
                {
                    listViewItem.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                }
                //var a  = 
                var a = k.Children.Count;
                var c = (ListViewItem)k.Children[SongListStorage.CurrentPlaceInPlaylist];
                while (c.Content == null) 
                {
                    c = (ListViewItem)k.Children[SongListStorage.CurrentPlaceInPlaylist];
                }
                if (((Song)c.Content).ID.Equals(SongListStorage.PlaylistRepresentation[SongListStorage.CurrentPlaceInPlaylist].ID))
                {
                    c.Background = new SolidColorBrush(Color.FromArgb(100, 48, 179, 221));
                }
                
            }*/
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
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) //Bugged when adding songs
            {
                int newmoveindex = e.NewStartingIndex;
                if (oldmoveindex != -1)
                {
                    try
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
                    catch(InvalidOperationException E)
                    {
                        //Debug.Writeline();
                    }
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
            var button = (Button)sender;
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

        private void ListViewPlayList_Loaded(object sender, RoutedEventArgs e)
        {
            //ShowCurrentlyPlayingSong();
        }

        private void ListViewPlayList_LayoutUpdated(object sender, object e)
        {
            if (!initialised) ShowCurrentlyPlayingSong();
        }

        private void ListViewPlayList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void SongInfoGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private async void ConvertToPlaylist()
        {
            string playlistname = "New Playlist";
            TextBox textBox = new TextBox()
            {
                Text = playlistname
            };
            ContentDialog nameFlavourDialog = new ContentDialog()
            {
                Title = "Name your playlist",
                Content = textBox,
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Ok",
                DefaultButton = ContentDialogButton.Primary
            };
            ContentDialogResult result = await nameFlavourDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                playlistname = textBox.Text;
                Playlist playlist = new Playlist(playlistname, Playlist.Select(x => x.ID).ToList<string>());
                await playlist.SavePlaylistFile(true);
                SongListStorage.PlaylistDict.TryAdd(playlist.PlaylistID, playlist);
                await App.GetForCurrentView().LoadPinnedFlavours();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ConvertToPlaylist();
        }
    }
}
