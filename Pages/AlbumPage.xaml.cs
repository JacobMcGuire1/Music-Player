﻿using Microsoft.Toolkit.Uwp.UI.Controls;
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
using Windows.UI;
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
                var dict = e.Parameter as Dictionary<String, String>;
                Album album = SongListStorage.AlbumDict[dict["albumkey"]];
                ChangeAlbum(album);
                AddExistingFlavourTabs();

                for (int i = 0; i < TabItems.Count(); i++)
                {
                    var headerstackpanel = (StackPanel)TabItems[i].Header;
                    var nametextblock = (TextBlock)headerstackpanel.Children[1];
                    if (nametextblock.Text == dict["flavourname"])
                    {
                        SongVersionTabs.SelectedIndex = i;
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
        }

        private void AddExistingFlavourTabs()
        {
            OrderTabs();
        }

        private async void playButton_Click(object sender, RoutedEventArgs e)
        {
            int songid = (int)((Button)sender).Tag;
            await Media.Instance.PlayPlaylist(Songs, songid, true);
        }

        private async void addToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

            //Song song = ((Button)sender).Tag as Song;

            string song = (string)(((Button)sender).Tag);

            await Media.Instance.addSong(song);
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            CreateAlbumVersion();
        }

        private async void CreateAlbumVersion()
        {

            //TabViewItem newtab = new TabViewItem();
            //newtab.Header = "New Album Version";

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
            while (flavourname == "" || currentflavournames.Contains(flavourname) || flavourname == "Original Album")
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
            

            //newtab.Header = flavourname;
            //Frame frame = new Frame();
            //frame.SetValue(FrameworkElement.NameProperty, "needtoputalbumversionidhere");

            Flavour flavour = new Flavour()
            {
                name = flavourname,
                albumname = SongListStorage.AlbumDict[CurrentAlbum].name,
                albumkey = SongListStorage.AlbumDict[CurrentAlbum].key,
                artist = SongListStorage.AlbumDict[CurrentAlbum].artist,
                Songids = new List<string>(SongListStorage.AlbumDict[CurrentAlbum].Songids),
                pinned = true,
            };

            //List<List<int>> flavours = SongListStorage.AlbumFlavours[CurrentAlbum];
            List<Flavour> flavours = new List<Flavour>();
            flavours.Add(flavour);
            //flavours.Add(songids);
            
            SongListStorage.AlbumFlavourDict.AddOrUpdate(CurrentAlbum, flavours, (CurrentAlbum2, existingflavours) => AddNewFlavour(existingflavours, flavour));

            //frame.Navigate(typeof(AlbumSongList), flavour);
            //newtab.Content = frame;
            //TabItems.Add(newtab);
            CreateTab(flavour);

             await App.GetForCurrentView().LoadPinnedFlavours(); //Because the flavour is pinned by default the list is updated in the UI.
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

        private async void SongVersionTabs_TabClosing(object sender, TabClosingEventArgs e)
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
            await App.GetForCurrentView().LoadPinnedFlavours();
            SongListStorage.SaveFlavours();
        }

        /*private void SongVersionTabs_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            var album = SongListStorage.AlbumDict[CurrentAlbum];
            album.flavourorder = new List<string>();
            for (int i = 0; i < TabItems.Count; i++)
            {
                var Tab = (TabViewItem)TabItems[i];
                String header = (String)Tab.Header;
                if (header != "Original Album")
                {
                    album.flavourorder.Add(header);
                    //var flavour = SongListStorage.GetFlavourByName(CurrentAlbum, header);
                    //flavour.place = i;
                }
                else
                {
                    album.flavourorder.Add("");
                }
            }
        }*/

        private void OrderTabs()
        {
            TabItems.Clear();
            if (SongListStorage.AlbumFlavourDict.ContainsKey(CurrentAlbum))
            {
                Flavour PinnedFlavour = null;
                foreach (Flavour flavour in SongListStorage.AlbumFlavourDict[CurrentAlbum])
                {
                    if (flavour.pinnedinalbum)
                    {
                        PinnedFlavour = flavour;
                        CreateTab(flavour);
                    }
                }
                CreateTab(null);
                foreach (Flavour flavour in SongListStorage.AlbumFlavourDict[CurrentAlbum])
                    if (flavour != PinnedFlavour) CreateTab(flavour);
            }
            else
            {
                CreateTab(null);
            }
        }

        private void CreateTab(Flavour flavour)
        {
            Frame frame = new Frame();
            var headerstackpanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            var pinbutton = new Button()
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Content = "\xE718",
                Background = new SolidColorBrush(Colors.Transparent)
        };
            pinbutton.Click += Pinbutton_Click;
            var nametextblock = new TextBlock();
            headerstackpanel.Children.Add(pinbutton);
            headerstackpanel.Children.Add(nametextblock);
            TabViewItem tab = new TabViewItem
            {
                Header = headerstackpanel,
                Content = frame
            };
            TabItems.Add(tab);
            if (flavour == null)
            {
                frame.Navigate(typeof(AlbumSongList), SongListStorage.AlbumDict[CurrentAlbum]);
                //tab.Header = "Original Album"; //Need to add support for flavour names
                //tab.Content = frame;
                tab.IsClosable = false;
                pinbutton.Tag = "";
                nametextblock.Text = "Original Album";
                // TabItems.Add(tab);
            }
            else
            {
                frame.Navigate(typeof(AlbumSongList), flavour);
                pinbutton.Tag = flavour.name;
                nametextblock.Text = flavour.name;
            }
        }

        private void Pinbutton_Click(object sender, RoutedEventArgs e)
        {
            var flavourname = (String)((Button)sender).Tag;
            if (SongListStorage.AlbumFlavourDict.ContainsKey(CurrentAlbum))
            {
                var oldval = false;
                if (flavourname != "")
                    oldval = SongListStorage.GetFlavourByName(CurrentAlbum, flavourname).pinnedinalbum;
                var flavourlist = SongListStorage.AlbumFlavourDict[CurrentAlbum];
                foreach (Flavour flavour in flavourlist)
                    flavour.pinnedinalbum = false;

                if (flavourname != "")
                {
                    SongListStorage.GetFlavourByName(CurrentAlbum, flavourname).pinnedinalbum = !oldval;
                }
                OrderTabs();
                SongListStorage.SaveFlavours();
            }

            //neeed to order tabs then save flavours.

        }
    }
}
