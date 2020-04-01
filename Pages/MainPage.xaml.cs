using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.System;
using Windows.UI.Composition;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Music_thing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        

        public int currentid;

        String currentpage;

        public MainPage()
        {
            this.InitializeComponent();

            PageGateway.MainPage = this;

            currentid = 0;

            //BAck stuff
            //KeyboardAccelerator GoBack = new KeyboardAccelerator();
            //GoBack.Key = VirtualKey.GoBack;
            //GoBack.Invoked += BackInvoked;
            //KeyboardAccelerator AltLeft = new KeyboardAccelerator();
            //AltLeft.Key = VirtualKey.Left;
            // AltLeft.Invoked += BackInvoked;
            ///    //  KeyboardAccelerator t = new KeyboardAccelerator();
            //   t.Key = VirtualKey.Back;
            //  t.Invoked += BackInvoked;
            //   this.KeyboardAccelerators.Add(GoBack);
            //  this.KeyboardAccelerators.Add(AltLeft);
            //  this.KeyboardAccelerators.Add(t);
            // ALT routes here
            //   AltLeft.Modifiers = VirtualKeyModifiers.Menu;



            //SongListStorage.GetSongList(); //Should change/remove this.
            //Database.GetSongs(); //Temp
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);

            

            //SongListStorage.FindArtists();

            mediaPlayerElement.SetMediaPlayer(Media.Instance.mediaPlayer);

            

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await LoadPinnedFlavours();
        }

        //Returns the instance of the media instance to allow information from it to be accessed.
        public Media MediaProxy { get { return Media.Instance; } }

        public void NotificationMessage(String Message)
        {
            progressbar.Visibility = Visibility.Collapsed;
            progressbartextblock.Visibility = Visibility.Visible;
            progressbartextblock.Text = Message;
        }

        public void DisplayLoading(int songsloaded, int totalfiles, int filesscanned, bool complete)
        {
            progressbartextblock.Visibility = Visibility.Visible;
            if (!complete)
            {
                progressbartextblock.Text = "Scanning for music. Found " + songsloaded.ToString() + " songs so far.";
                progressbar.Visibility = Visibility.Visible;
                if (totalfiles == 0)
                {
                    progressbar.IsIndeterminate = true;
                }
                else
                {
                    progressbar.IsIndeterminate = false;
                    progressbar.Value = filesscanned;
                    progressbar.Maximum = totalfiles;
                }
                
            }
            else
            {
                progressbartextblock.Text = "Loaded " + songsloaded + " songs.";
                progressbar.Visibility = Visibility.Collapsed;
            }
        }

        //Loads the pinned flavours and playlists into the list on the left of the screen.

        public async Task LoadPinnedFlavours()
        {
            List<long> inlist = new List<long>();
            foreach (NavigationViewItemBase menuitem in NavView.MenuItems)
            {

                if (menuitem.Name.Equals("Flavour"))
                {
                    var tag = (long)menuitem.Tag;
                    inlist.Add(tag);
                }
            }
            var playlists = SongListStorage.PlaylistDict.Values;
            //Loops through the playlists to load the necessary ones.
            foreach(Playlist playlist in playlists)
            {
                if (playlist.pinned && !inlist.Contains(playlist.PlaylistID))
                {
                    await LoadFlavour(playlist);
                }
            }

            foreach (NavigationViewItemBase menuitem in NavView.MenuItems)
            {
                if (menuitem.Name.Equals("Flavour"))
                {
                    var key = (long)menuitem.Tag;
                    if (!SongListStorage.PlaylistDict.ContainsKey(key) || !SongListStorage.PlaylistDict[key].pinned)
                    {
                        menuitem.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private async Task LoadFlavour(Playlist playlist)
        {
            NavigationViewItem navigationViewItem = new NavigationViewItem();
            var stackpanelouter = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var stackpanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            TextBlock nametb = new TextBlock
            {
                Text = playlist.Name
            };
            //nametb.FontWeight = FontWeights.Bold; 

            TextBlock artisttb = new TextBlock
            {
                Text = playlist.Artist + " - " + playlist.albumname,
                FontWeight = FontWeights.ExtraLight,
                FontSize = 12
            };

            stackpanel.Children.Add(nametb);
            stackpanel.Children.Add(artisttb);

            //IMAGESTUFF

            Image icon = new Image();
            try
            {
                icon.Source = await SongListStorage.AlbumDict[playlist.albumkey].GetAlbumArt(15, SongListStorage.SongDict);
            }
            catch { }
            icon.Width = 20;
            icon.Height = 20;
            icon.Margin = new Thickness(0, 0, 14, 0);

            stackpanelouter.Children.Add(icon);
            stackpanelouter.Children.Add(stackpanel);

            navigationViewItem.Content = stackpanelouter;
            navigationViewItem.Name = "Flavour";
            navigationViewItem.AllowDrop = true;
            navigationViewItem.Drop += NavigationViewItem_Drop;
            navigationViewItem.DragOver += NavigationViewItem_DragOver;

            navigationViewItem.Tag = playlist.PlaylistID;

            NavView.MenuItems.Add(navigationViewItem);
        }



        private void NavigationViewItem_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = (e.DataView.Contains(StandardDataFormats.Text)) ? DataPackageOperation.Copy : DataPackageOperation.None;
        }

        //Adds a song to a flavour/playlist if it is dragged onto it.
        private async void NavigationViewItem_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                var def = e.GetDeferral();

                var send = (NavigationViewItem)sender;
                var playlistid = (long)send.Tag;
                Playlist flavour = SongListStorage.PlaylistDict[playlistid];

                var s = await e.DataView.GetTextAsync();
                var ids = s.Split(Environment.NewLine);
                if (ids.Length > 0)
                {
                    foreach (string id in ids)
                    {
                        flavour.AddSong(id);
                    }
                }
                e.AcceptedOperation = DataPackageOperation.Copy;
                //await SongListStorage.SaveFlavours();
                await flavour.SavePlaylistFile(false);
                App.GetForCurrentView().NotificationMessage("Added song(s) to flavour '" + SongListStorage.PlaylistDict[playlistid].Name + "'.");
                def.Complete();
            }
        }

        //Loads a page when it is navigated to.
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

                if (item.Name.Equals("Flavour"))
                {
                    HandleFlavourClick(item);
                }
                else
                {
                    currentpage = item.Tag.ToString(); //Probs remove

                    switch (item.Tag.ToString())
                    {
                        case "songs":
                            ContentFrame.Navigate(typeof(SongList));
                            //NavView.Header = "Songs";
                            break;
                        case "artists":
                            ContentFrame.Navigate(typeof(ArtistList));
                            //NavView.Header = "Artists";
                            break;
                        case "albums":
                            ContentFrame.Navigate(typeof(AlbumList));
                            //NavView.Header = "Albums";
                            break;
                        case "nowPlaying":
                            ContentFrame.Navigate(typeof(NowPlaying));
                            //NavView.Header = "Now Playing";
                            break;
                        case "recentlyPlayed":
                            ContentFrame.Navigate(typeof(RecentlyPlayed));
                            //NavView.Header = "Recently Played";
                            break;
                    }
                }

                
            }
        }

        //Loads the flavour/playlist when it's clicked on.
        private void HandleFlavourClick(NavigationViewItem item)
        {
            var id = (long)item.Tag;
            //ContentFrame.Navigate(typeof(AlbumPage), dict["albumkey"]);
            ContentFrame.Navigate(typeof(AlbumPage), id);
        }

        public enum MediaState
        {
            Stopped, Playing, Paused
        }

        //MAKE THIS ONLY HAPPEN WHEN THE SELECTION DOESN'T CHANGE
        //Reloads the current page when it is clicked on.
        private void NavView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            switch (currentpage)
            {
                case "songs":
                    ContentFrame.Navigate(typeof(SongList));
                    //NavView.Header = "Songs";
                    break;
                case "artists":
                    ContentFrame.Navigate(typeof(ArtistList));
                    //NavView.Header = "Artists";
                    break;
                case "albums":
                    ContentFrame.Navigate(typeof(AlbumList));
                    //NavView.Header = "Albums";
                    break;
                case "nowPlaying":
                    ContentFrame.Navigate(typeof(NowPlaying));
                    //NavView.Header = "Now Playing";
                    break;
                case "recentlyPlayed":
                    ContentFrame.Navigate(typeof(RecentlyPlayed));
                    //NavView.Header = "Recently Played";
                    break;
            }
        }

        //Shows song suggestions in the searchbox based on what has been typed.
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = SongListStorage.SearchSongs(SearchBox.Text);
                var textresults = new List<String>();
                foreach(Song song in results)
                {
                    textresults.Add(song.Title);
                }
                SearchBox.ItemsSource = textresults;
            }
        }
        
        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
        }

        private void BackInvoked(KeyboardAccelerator sender,
                         KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            //if (NavView.IsPaneOpen &&
            //    (NavView.DisplayMode == muxc.NavigationViewDisplayMode.Compact ||
            //     NavView.DisplayMode == muxc.NavigationViewDisplayMode.Minimal))
            //    return false;

            ContentFrame.GoBack();
            return true;
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            progressbar.Visibility = Visibility.Collapsed;
            progressbartextblock.Visibility = Visibility.Collapsed;
        }

        /*public async Task LoadPinnedFlavours() //Could probably be done more efficiently
        {

            //navigationViewItem.Content = "TEst";

            List<String> inlist = new List<string>();
            foreach (NavigationViewItemBase menuitem in NavView.MenuItems)
            {
                
                if (menuitem.Name.Equals("Flavour"))
                {
                    var tag = (Dictionary<String, string>)menuitem.Tag;
                    inlist.Add(tag["albumkey"] + tag["flavourname"]);
                    //menuitem.Visibility = Visibility.Collapsed;
                }
            }

            var flavourlists = SongListStorage.AlbumFlavourDict.Values;
            foreach (List<Flavour> flavourlist in flavourlists)
            {
                foreach (Flavour flavour in flavourlist)
                {
                    if (flavour.pinned)
                    {
                        NavigationViewItem navigationViewItem = new NavigationViewItem();
                        var stackpanelouter = new StackPanel
                        {
                            Orientation = Orientation.Horizontal
                        };

                        var stackpanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical
                        };

                        TextBlock nametb = new TextBlock
                        {
                            Text = flavour.Name
                        };
                        //nametb.FontWeight = FontWeights.Bold; 

                        TextBlock artisttb = new TextBlock
                        {
                            Text = flavour.Artist + " - " + flavour.albumname,
                            FontWeight = FontWeights.ExtraLight,
                            FontSize = 12
                        };

                        stackpanel.Children.Add(nametb);
                        stackpanel.Children.Add(artisttb);

                        //IMAGESTUFF

                        Image icon = new Image();
                        try
                        {
                            icon.Source = await SongListStorage.AlbumDict[flavour.albumkey].GetAlbumArt(15, SongListStorage.SongDict);
                        }
                        catch { }
                        icon.Width = 20;
                        icon.Height = 20;
                        icon.Margin = new Thickness(0,0,14,0);
                        //icon.Opacity = 0.5;
                        //icon.ren
                        //icon.

                        //IMAGESTUFF

                        stackpanelouter.Children.Add(icon);
                        stackpanelouter.Children.Add(stackpanel);
                        //stackpanel.Children.Add(albumtb);


                        // stackpanelhor.Children.Add(stackpanel);

                        //navigationViewItem.Content = flavour.artist + " - " + flavour.albumname + ": " + flavour.name;
                        navigationViewItem.Content = stackpanelouter;
                        navigationViewItem.Name = "Flavour";
                        navigationViewItem.AllowDrop = true;
                        navigationViewItem.Drop += NavigationViewItem_Drop;
                        navigationViewItem.DragOver += NavigationViewItem_DragOver;

                        //The details needed to reference the corresponding flavour.
                        var dict = new Dictionary<String, String>
                        {
                            { "albumkey", flavour.Artist + flavour.albumname },
                            { "flavourname", flavour.Name }
                        };
                        navigationViewItem.Tag = playlist.PlaylistID;

                        //navigationViewItem.Tapped = 
                        if (!inlist.Contains(dict["albumkey"] + dict["flavourname"]))
                        {
                            NavView.MenuItems.Add(navigationViewItem);
                        }
                        
                    }
                }
                
            }
            foreach (NavigationViewItemBase menuitem in NavView.MenuItems)
            {
                if (menuitem.Name.Equals("Flavour"))
                {
                    var tag = (Dictionary<String, string>)menuitem.Tag;
                    if (SongListStorage.GetFlavourByName(tag["albumkey"], tag["flavourname"]) == null || !SongListStorage.GetFlavourByName(tag["albumkey"], tag["flavourname"]).pinned)
                    {
                        menuitem.Visibility = Visibility.Collapsed;
                    }
                }

                //if (menuitem.Name.Equals("Flavour"))
                //{
                    //inlist.Add(tag["albumkey"] + tag["flavourname"]);
                  //  menuitem.Visibility = Visibility.Collapsed;
               // }
            }
        }*/
    }
}
