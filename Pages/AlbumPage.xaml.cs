using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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

        public ImageSource albumart;


        public AlbumPage()
        {
            this.InitializeComponent();
            SongVersionTabs.ItemsSource = TabItems;

            //TabViewItem test = new TabViewItem();
            //test.Header = "TEST TAB";
            //TabItems.Add(test);
        }

        public void ChangeAlbum(Album album)
        {
            Songs = album.ObserveSongs(SongListStorage.SongDict);
            CurrentAlbum = album.key;
        }

        public async Task SetAlbumArt()
        {
            //return SongListStorage.AlbumDict[CurrentAlbum].albumart250;
            albumart = await SongListStorage.AlbumDict[CurrentAlbum].GetAlbumArt(250, SongListStorage.SongDict);
            Bindings.Update();
        }

        public string GetAlbumName()
        {
            return SongListStorage.AlbumDict[CurrentAlbum].name;
        }

        public string GetArtistName()
        {
            return SongListStorage.AlbumDict[CurrentAlbum].artist;
        }

        public string GetYear()
        {
            return SongListStorage.AlbumDict[CurrentAlbum].GetStringYear();
            
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            try
            {
                String key = e.Parameter as string;
                Album album = SongListStorage.AlbumDict[key];
                ChangeAlbum(album);
                AddExistingFlavourTabs();
            }
            catch(Exception f)
            {
                //Album page is clicked from a flavour, so the flavour is navigated to.
                var dict = e.Parameter as Dictionary<String, String>;
                Album album = SongListStorage.AlbumDict[dict["albumkey"]];
                ChangeAlbum(album);
                AddExistingFlavourTabs();


                /*foreach(TabViewItem tab in TabItems)
                {
                    if ((String)tab.Header == dict["flavourname"])
                    {
                        TabItems.
                    }
                }*/
                for (int i = 0; i < TabItems.Count(); i++)
                {
                    if ((String)TabItems[i].Header == dict["flavourname"])
                    {
                        //TabView.SelectedIndex = i;
                        SongVersionTabs.SelectedIndex = i;
                        //TabView.SelectedIndexProperty = i;
                    }
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
            
            /*
            if (e.GetType() == typeof(Dictionary<String, String>))
            {
                var dict = e.Parameter as Dictionary<String, String>;
                Album album = SongListStorage.AlbumDict[dict["albumkey"]];
                ChangeAlbum(album);
                AddExistingFlavourTabs();


                foreach(TabViewItem tab in TabItems)
                {
                    if ((String)tab.Header == dict["flavourname"])
                    {
                        TabItems.
                    }
                }
                for (int i = 0; i < TabItems.Count(); i++)
                {
                    if ((String)TabItems[i].Header == dict["flavourname"])
                    {
                        //TabView.SelectedIndex = i;
                        SongVersionTabs.SelectedIndex = i;
                        //TabView.SelectedIndexProperty = i;
                    }
                }
            }
            else
            {
                String key = e.Parameter as string;
                Album album = SongListStorage.AlbumDict[key];
                ChangeAlbum(album);
                AddExistingFlavourTabs();
            }*/

        }

        private void AddExistingFlavourTabs()
        {
            //Add original album tab
            Frame frameO = new Frame();
            Args argsO = new Args() { id = CurrentAlbum, flavourid = -1 };
            
            frameO.Navigate(typeof(AlbumSongList), argsO);
            TabViewItem newtabO = new TabViewItem();
            newtabO.Header = "Original Album"; //Need to add support for flavour names
            newtabO.Content = frameO;
            newtabO.IsClosable = false;

            TabItems.Add(newtabO);

            if (SongListStorage.AlbumFlavourDict.ContainsKey(CurrentAlbum))
            {
                for (int i = 0; i < SongListStorage.AlbumFlavourDict[CurrentAlbum].Count; i++)
                {
                    Frame frame = new Frame();
                    //frame.SetValue(FrameworkElement.NameProperty, "needtoputalbumversionidhere"); //probably not needed
                    Flavour flavourobj = SongListStorage.AlbumFlavourDict[CurrentAlbum][i];
                    Args args = new Args() { id = CurrentAlbum, flavourid = i };
                    frame.Navigate(typeof(AlbumSongList), args);

                    TabViewItem newtab = new TabViewItem
                    {
                        Header = flavourobj.name,
                        Content = frame
                    };

                    TabItems.Add(newtab);
                }
            }
        }

        private async void playButton_Click(object sender, RoutedEventArgs e)
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

            await Media.Instance.PlayPlaylist(Songs, songid, true);
        }

        private void addToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            //Song song = ((Button)sender).Tag as Song;

            string song = (string)(((Button)sender).Tag);

            Media.Instance.addSong(song);
        }

        public struct Args
        {
            public String id;
            public int flavourid;
        };

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            //TabViewItem newtab = new TabViewItem();// = OriginalAlbumTab.;


            //newtab = OriginalAlbumTab;
            //newtab.
            //newtab.IsClosable = true;
            //SongVersionTabs.Items.Add(newtab);


            //newtab.Header = "New Album Version";
            //TabItems.Add(newtab);
            CreateAlbumVersion();
            
        }

        private async void CreateAlbumVersion()
        {

            TabViewItem newtab = new TabViewItem();
            newtab.Header = "New Album Version";

            //Generate a list of the current flavour names to ensure this one is unique.
            List<String> currentflavournames = new List<string>();
            if (SongListStorage.AlbumFlavourDict.ContainsKey(CurrentAlbum))
            {
                for (int i = 0; i < SongListStorage.AlbumFlavourDict[CurrentAlbum].Count; i++)
                {
                    currentflavournames.Add(SongListStorage.AlbumFlavourDict[CurrentAlbum][i].name);
                }
            }
            string flavourname = "";
            bool err = false;
            while (flavourname == "" || currentflavournames.Contains(flavourname))
            {
                ContentDialog nameFlavourDialog = new ContentDialog()
                {
                    Title = "Name your flavour",
                    CloseButtonText = "Ok"
                };
                TextBox textBox = new TextBox()
                {

                };
                if (!err)
                {
                    nameFlavourDialog.Content = textBox;
                }
                else
                {
                    var stackpanel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical
                    };
                    TextBlock errormsgtext = new TextBlock()
                    {
                        Text = "Error: Please try another name."
                    };
                    stackpanel.Children.Add(textBox);
                    stackpanel.Children.Add(errormsgtext);
                    nameFlavourDialog.Content = stackpanel;
                }
                

                


                await nameFlavourDialog.ShowAsync();

                flavourname = textBox.Text;
                err = true;
            }
            

            newtab.Header = flavourname;
            Frame frame = new Frame();
            frame.SetValue(FrameworkElement.NameProperty, "needtoputalbumversionidhere");

            Flavour flavour = new Flavour()
            {
                name = flavourname,
                albumname = SongListStorage.AlbumDict[CurrentAlbum].name,
                artist = SongListStorage.AlbumDict[CurrentAlbum].artist,
                Songids = new List<string>(SongListStorage.AlbumDict[CurrentAlbum].Songids),
                pinned = true
            };

            //List<List<int>> flavours = SongListStorage.AlbumFlavours[CurrentAlbum];
            List<Flavour> flavours = new List<Flavour>();
            flavours.Add(flavour);
            //flavours.Add(songids);
            
            SongListStorage.AlbumFlavourDict.AddOrUpdate(CurrentAlbum, flavours, (CurrentAlbum2, existingflavours) => AddNewFlavour(existingflavours, flavour));

            Args args = new Args() { id = CurrentAlbum, flavourid = SongListStorage.AlbumFlavourDict[CurrentAlbum].Count - 1 };

            frame.Navigate(typeof(AlbumSongList), args);
            newtab.Content = frame;
            TabItems.Add(newtab);

            App.GetForCurrentView().LoadPinnedFlavours(); //Because the flavour is pinned by default the list is updated in the UI.
            SongListStorage.SaveFlavours();

        }

        private List<Flavour> AddNewFlavour(List<Flavour> existingflavours, Flavour newflavour)
        {
            existingflavours.Add(newflavour);
            return existingflavours;
        }

        private void OpenArtistPageButton_Click(object sender, RoutedEventArgs e)
        {
            string artistid = (string)((HyperlinkButton)sender).Tag;
            this.Frame.Navigate(typeof(AlbumList), artistid);
        }

        private void SongVersionTabs_TabClosing(object sender, TabClosingEventArgs e)
        {
            var tab = (TabViewItem)e.Tab;
            String flavourname = (String)tab.Header;
            List<Flavour> flavours = SongListStorage.AlbumFlavourDict[CurrentAlbum];
            int index = -1;
            for(int i = 0; i < flavours.Count; i++)
            {
                if (flavours[i].name == flavourname)
                {
                    index = i;
                }
            }
            flavours.RemoveAt(index);
            TabItems.Remove(tab);
            App.GetForCurrentView().LoadPinnedFlavours();
            SongListStorage.SaveFlavours();
        }
    }
}
