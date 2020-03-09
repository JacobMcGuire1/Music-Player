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
using Windows.Storage;
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

            

            SongListStorage.GetSongList();
            Database.GetSongs(); //Temp

            //SongListStorage.FindArtists();

            mediaPlayerElement.SetMediaPlayer(Media.Instance.mediaPlayer);

            LoadPinnedFlavours();

        }

        //Returns the instance of the media instance to allow information from it to be accessed.
        public Media MediaProxy { get { return Media.Instance; } }

        //Loads the pinned flavours and playlists into the list on the left of the screen.
        public void LoadPinnedFlavours() //Could probably be done more efficiently
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

                        /*Image icon stuff
                        var stackpanelhor = new StackPanel
                        {
                            Orientation = Orientation.Horizontal
                        };

                        var icon = new Image();
                        icon.Source = flavour.albumart100;
                        icon.Height = 20;
                        icon.Width = 20;

                        stackpanelhor.Children.Add(icon);
                        */

                        var stackpanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical
                        };

                        TextBlock nametb = new TextBlock
                        {
                            Text = flavour.name
                        };
                        //nametb.FontWeight = FontWeights.Bold; 

                        TextBlock artisttb = new TextBlock
                        {
                            Text = flavour.artist + " - " + flavour.albumname,
                            FontWeight = FontWeights.ExtraLight,
                            FontSize = 12
                        };

                        TextBlock albumtb = new TextBlock
                        {
                            Text = flavour.albumname,
                            FontWeight = FontWeights.ExtraLight,
                            FontSize = 10
                        };


                        stackpanel.Children.Add(nametb);
                        stackpanel.Children.Add(artisttb);
                        stackpanel.Children.Add(albumtb);


                       // stackpanelhor.Children.Add(stackpanel);

                        //navigationViewItem.Content = flavour.artist + " - " + flavour.albumname + ": " + flavour.name;
                        navigationViewItem.Content = stackpanel;
                        navigationViewItem.Name = "Flavour";
                        navigationViewItem.AllowDrop = true;
                        navigationViewItem.Drop += NavigationViewItem_Drop;
                        navigationViewItem.DragOver += NavigationViewItem_DragOver;

                        //The details needed to reference the corresponding flavour.
                        var dict = new Dictionary<String, String>
                        {
                            { "albumkey", flavour.artist + flavour.albumname },
                            { "flavourname", flavour.name }
                        };
                        navigationViewItem.Tag = dict;

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
                    if (!SongListStorage.GetFlavourByName(tag["albumkey"], tag["flavourname"]).pinned)
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
        }

        
        private void NavigationViewItem_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = (e.DataView.Contains(StandardDataFormats.Text)) ? DataPackageOperation.Copy : DataPackageOperation.None;
        }

        //Adds a song to a flavour/playlist if it is dragged onto it.
        private async void NavigationViewItem_Drop(object sender, DragEventArgs e)
        {
            var task = e.DataView.GetTextAsync();
            String songid = await task;
            var send = (NavigationViewItem)sender;
            var dict = (Dictionary<String, String>)send.Tag;
            String albumkey = dict["albumkey"];
            String flavourname = dict["flavourname"];

            Flavour flavour = SongListStorage.GetFlavourByName(albumkey, flavourname);
            flavour.AddSong(songid);
            //LoadPinnedFlavours();
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
            var dict = (Dictionary <String, String>)item.Tag;
            //ContentFrame.Navigate(typeof(AlbumPage), dict["albumkey"]);
            ContentFrame.Navigate(typeof(AlbumPage), dict);
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
    }
}
