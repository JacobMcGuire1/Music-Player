using Microsoft.Toolkit.Uwp.UI.Controls;
using Music_thing.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
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

        public ObservableCollection<ObservableCollection<Song>> AlbumTypes { get; set; }
        = new ObservableCollection<ObservableCollection<Song>>();

        ObservableCollection<TabViewItem> TabItems = new ObservableCollection<TabViewItem>();

        public string CurrentAlbum;

        public string PlaylistName;

        public ImageSource albumart;


        public AlbumPage()
        {
            this.InitializeComponent();
            SongVersionTabs.ItemsSource = TabItems;
        }

        public void ChangeAlbum(Album album)
        {
            Songs = album.ObserveSongs(SongListStorage.SongDict);
            CurrentAlbum = album.Key;
        }

        public async Task SetAlbumArt()
        {
            if (CurrentAlbum != null)
            {
                albumart = await SongListStorage.AlbumDict[CurrentAlbum].GetAlbumArt(250, SongListStorage.SongDict);
            }
            else
            {
                try
                {
                    var art = new BitmapImage(new Uri("ms-appx:///Assets/DefaultAlbumArt.png"))
                    {
                        DecodePixelHeight = 250,
                        DecodePixelWidth = 250
                    };
                    albumart = art;
                }
                catch { }
            }
            Bindings.Update();
        }

        public string GetAlbumName()
        {
            if (CurrentAlbum != null)
            {
                if (SongListStorage.AlbumDict.ContainsKey(CurrentAlbum))
                {
                    return SongListStorage.AlbumDict[CurrentAlbum].Name;
                }
                return "Album not yet loaded.";
            }
            return PlaylistName;
        }

        public string GetArtistName()
        {
            if (CurrentAlbum != null)
            {
                if (SongListStorage.AlbumDict.ContainsKey(CurrentAlbum))
                {
                    return SongListStorage.AlbumDict[CurrentAlbum].Artist;
                }
                return "";
            }
            return "";
        }

        public string GetYear()
        {
            if (CurrentAlbum != null)
            {
                if (SongListStorage.AlbumDict.ContainsKey(CurrentAlbum))
                {
                    return SongListStorage.AlbumDict[CurrentAlbum].GetStringYear();
                }
                return "";
            }
            return "";
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string)
            {
                String key = e.Parameter as string;
                Album album = SongListStorage.AlbumDict[key];
                ChangeAlbum(album);
                AddExistingFlavourTabs();
            }
            else
            {
                //Album page is clicked from a flavour, so the flavour is navigated to.
                var playlistid = (long)e.Parameter;
                var playlist = SongListStorage.PlaylistDict[playlistid];
                if (playlist.isflavour)
                {
                    if (SongListStorage.AlbumDict.ContainsKey(SongListStorage.PlaylistDict[playlistid].albumkey))
                    {
                        NewTabButton.Visibility = Visibility.Visible;
                        Album album = SongListStorage.AlbumDict[SongListStorage.PlaylistDict[playlistid].albumkey];
                        ChangeAlbum(album);
                        AddExistingFlavourTabs();

                        for (int i = 0; i < TabItems.Count(); i++)
                        {
                            var tab = (TabViewItem)TabItems[i];
                            //var nametextblock = (TextBlock)headerstackpanel.Children[1];
                            if (tab.Tag is long id && id == playlistid)
                            {
                                SongVersionTabs.SelectedIndex = i;
                            }
                        }
                    }
                    else
                    {
                        CurrentAlbum = "Album not yet loaded.";
                        NewTabButton.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    CurrentAlbum = null;
                    CreateTab(playlist);
                    PlaylistName = playlist.Name;
                    var PinnedTabPinButton = ((TabItems[0].Header as StackPanel).Children[0] as Button);
                    PinnedTabPinButton.Visibility = Visibility.Collapsed;
                    NewTabButton.Visibility = Visibility.Collapsed;
                }
                
            }
            try
            {
                await SetAlbumArt();
                
            }
            catch(Exception E)
            {
                Debug.WriteLine("Couldn't set album art.");
                Debug.WriteLine(E.Message);
            }
        }

        private void AddExistingFlavourTabs()
        {
            OrderTabs();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            int songid = (int)((Button)sender).Tag;
            await Media.Instance.PlayPlaylist(Songs, songid, null, true);
        }

        private async void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            //Song song = ((Button)sender).Tag as Song;

            string song = (string)(((Button)sender).Tag);

            await Media.Instance.AddSong(song, true);
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            CreateAlbumVersion();
        }

        private async void CreateAlbumVersion()
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
                Playlist playlist = new Playlist(playlistname, CurrentAlbum);
                await playlist.SavePlaylistFile(true);
                SongListStorage.PlaylistDict.TryAdd(playlist.PlaylistID, playlist);
                CreateTab(playlist);
                await App.GetForCurrentView().LoadPinnedFlavours();
            }
            
        }

        private void OpenArtistPageButton_Click(object sender, RoutedEventArgs e)
        {
            string artistid = (string)((HyperlinkButton)sender).Tag;
            this.Frame.Navigate(typeof(ArtistPage), artistid);
        }

        private async void SongVersionTabs_TabClosing(object sender, TabClosingEventArgs e)
        {
            e.Cancel = true;
            var tab = (TabViewItem)e.Tab;

            MessageDialog dialog = new MessageDialog("Are you sure you want to delete this playlist?");
            dialog.Commands.Add(new UICommand("Yes", null));
            dialog.Commands.Add(new UICommand("No", null));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            var cmd = await dialog.ShowAsync();

            if (cmd.Label == "Yes")
            {
                if (tab.Tag is long flavourid)
                {
                    e.Cancel = false;
                    try
                    {
                        var flavour = SongListStorage.PlaylistDict[flavourid];
                        if (flavour.isflavour) SongListStorage.AlbumDict[CurrentAlbum].RemoveFlavour(flavourid);
                        await flavour.DeleteFile();
                        TabItems.Remove(tab);
                        SongListStorage.PlaylistDict.Remove(flavourid, out flavour);

                        await App.GetForCurrentView().LoadPinnedFlavours();
                        if (!flavour.isflavour) this.Frame.Navigate(typeof(AlbumList));
                    }
                    catch { }

                }
            }
        }

        private void OrderTabs()
        {
            TabItems.Clear();
            var flavourlist = SongListStorage.AlbumDict[CurrentAlbum].GetFlavourList();
            if (flavourlist.Count > 0)
            {
                Playlist PinnedFlavour = null;
                foreach (Playlist flavour in flavourlist)
                {
                    if (flavour.pinnedinalbum)
                    {
                        PinnedFlavour = flavour;
                        CreateTab(flavour);
                    }
                }
                CreateTab(null);
                foreach (Playlist flavour in flavourlist)
                    if (flavour != PinnedFlavour) CreateTab(flavour);
            }
            else
            {
                CreateTab(null);
            }
            var PinnedTabPinButton = ((TabItems[0].Header as StackPanel).Children[0] as Button);
            if (CurrentAlbum == null)
            {
                PinnedTabPinButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                PinnedTabPinButton.Visibility = Visibility.Visible;
                PinnedTabPinButton.Content = "\xE735";
            }
            
        }

        private void CreateTab(Playlist playlist)
        {
            Frame frame = new Frame();
            var headerstackpanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            
            var pinbutton = new Button()
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Content = "\xE734",
                Background = new SolidColorBrush(Colors.Transparent)
            };
            
            pinbutton.Click += Pinbutton_Click;
            var nametextblock = new TextBlock();
            nametextblock.Margin = new Thickness(5);
            headerstackpanel.Children.Add(pinbutton);
            headerstackpanel.Children.Add(nametextblock);
            TabViewItem tab = new TabViewItem
            {
                Header = headerstackpanel,
                Content = frame
            };
            TabItems.Add(tab);
            if (playlist == null)
            {
                headerstackpanel.IsRightTapEnabled = false;
                frame.Navigate(typeof(AlbumSongList), SongListStorage.AlbumDict[CurrentAlbum]);
                //tab.Header = "Original Album"; //Need to add support for flavour names
                //tab.Content = frame;
                tab.IsClosable = false;
                //pinbutton.Tag = "";
                nametextblock.Text = "Original Album";
                frame.Tag = CurrentAlbum;
                // TabItems.Add(tab);
            }
            else
            {

                //headerstackpanel.ContextFlyout = new MenuFlyout();
                //Button RenameButton = new Button() { Content = "Rename", Tag = playlist.PlaylistID, Name = "RenameButton" };
                //RenameButton.Click += RenameButton_Click;
                frame.Tag = playlist.PlaylistID;
                var flyout = new MenuFlyout();
                var flyoutitem = new MenuFlyoutItem() { Text = "Rename", Tag = playlist.PlaylistID };
                flyoutitem.Click += RenameButton_Click;
                flyout.Items.Add(flyoutitem);
                //flyout.
                headerstackpanel.Tag = playlist.PlaylistID;
                headerstackpanel.ContextFlyout = flyout;
                //headerstackpanel.ContextFlyout.
                //headerstackpanel.RightTapped += Headerstackpanel_RightTapped;
                //headerstackpanel.IsRightTapEnabled = true;
                //headerstackpanel.Tag = playlist.PlaylistID;
                frame.Navigate(typeof(AlbumSongList), playlist);
                pinbutton.Tag = playlist.PlaylistID;
                nametextblock.Text = playlist.Name;
                tab.Tag = playlist.PlaylistID;
            }
        }

        internal void DeletedPlaylist(long playlistid)
        {
            /*foreach (TabViewItem tab in TabItems)
            {
                if (((tab.Header as StackPanel).Children[0] as Button).Tag is long pid && pid == playlistid)
                {
                    TabItems.Remove(tab);
                    break;
                }
            }*/
            if (CurrentAlbum != null)
            {
                OrderTabs();
            }
            else
            {
                if ((long)((Frame)TabItems[0].Content).Tag == playlistid)
                {
                    this.Frame.Navigate(typeof(AlbumList));
                }
            }
        }

        public void RenamedPlaylist(long playlistid)
        {
            var playlist = SongListStorage.PlaylistDict[playlistid];
            foreach (TabViewItem tab in TabItems)
            {
                if (((tab.Header as StackPanel).Children[0] as Button).Tag is long pid && pid == playlistid)
                {
                    ((TextBlock)((StackPanel)tab.Header).Children[1]).Text = playlist.Name;
                }
            }
            PlaylistName = playlist.Name;
            Bindings.Update();
        }

        public void ToggledPinned(long playlistid)
        {
            foreach (TabViewItem tab in TabItems)
            {
                if (((tab.Header as StackPanel).Children[0] as Button).Tag is long pid && pid == playlistid)
                {
                    ((tab.Content as Frame).Content as AlbumSongList).ToggledPinned();
                }
            }
        }

        private async void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var menuflyoutitem = (MenuFlyoutItem)sender;
            
            var playlist = SongListStorage.PlaylistDict[(long)menuflyoutitem.Tag];
            //playlist.Name = 
            string newname = await playlist.Rename();
            PlaylistName = newname;
            foreach (TabViewItem tab in TabItems)
            {
                if (((StackPanel)tab.Header).Tag is long playlistid && playlistid == playlist.PlaylistID)
                {
                    ((TextBlock)((StackPanel)tab.Header).Children[1]).Text = newname;
                }
            }
            Bindings.Update();
        }

        /*private void Headerstackpanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var stack = sender as StackPanel;
            var playlistid = stack.Tag;
        }*/

        private async void Pinbutton_Click(object sender, RoutedEventArgs e)
        {
            var flavourlist = SongListStorage.AlbumDict[CurrentAlbum].GetFlavourList();
            if (((Button)sender).Tag is long flavourid)//if its a flavour and not the album
            {
                if (flavourlist.Count > 0)
                {
                    var oldval = SongListStorage.PlaylistDict[flavourid].pinnedinalbum;
                    foreach (Playlist flavour in flavourlist)
                    {
                        flavour.pinnedinalbum = false;
                        await flavour.SavePlaylistFile(false); //maybe shouldn't await this?
                    }
                        
                    SongListStorage.PlaylistDict[flavourid].pinnedinalbum = !oldval;
                    await SongListStorage.PlaylistDict[flavourid].SavePlaylistFile(false); //maybe shouldn't await this?
                }
            }
            else
            {
                foreach (Playlist flavour in flavourlist)
                {
                    flavour.pinnedinalbum = false;
                    await flavour.SavePlaylistFile(false); //maybe shouldn't await this?
                }
                    
            }
            OrderTabs();



            //neeed to order tabs then save flavours.

        }

        private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentAlbum != null)
            {
                if (BigArt.Height == 250)
                {
                    BigArt.Height = 700;
                    BigArt.Width = 700;
                    albumart = await SongListStorage.AlbumDict[CurrentAlbum].GetAlbumArt(700, SongListStorage.SongDict);
                    Bindings.Update();
                }
                else
                {
                    BigArt.Height = 250;
                    BigArt.Width = 250;
                    albumart = await SongListStorage.AlbumDict[CurrentAlbum].GetAlbumArt(250, SongListStorage.SongDict);
                    Bindings.Update();
                }
            }
        }

        public void PlaylistChanged(long playlistid)
        {
            foreach(TabViewItem tab in TabItems)
            {
                if (((tab.Header as StackPanel).Children[0] as Button).Tag is long pid && pid == playlistid)
                {
                    ((tab.Content as Frame).Content as AlbumSongList).ReloadSongs();
                }
            }
        }

        private void SongVersionTabs_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var c = (Frame)e.Items[0];
            Album album = new Album();
            if (c.Tag is long playlistid)
            {
                album = SongListStorage.PlaylistDict[playlistid];
            }
            else if (c.Tag is string albumid)
            {
                album = SongListStorage.AlbumDict[albumid];
            }
            var songids = album.Songids;

            var items = new StringBuilder();
            foreach (String songid in songids)
            {
                if (items.Length > 0) items.AppendLine();
                items.Append(songid);
            }
            var t = items.ToString();
            e.Data.SetText(items.ToString());

            e.Data.RequestedOperation = DataPackageOperation.Copy;
        }
    }
}
