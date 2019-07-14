using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
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

            //SongListStorage.FindArtists();

            mediaPlayerElement.SetMediaPlayer(Media.Instance.mediaPlayer);

            




        }

        /*public async void testSong()
        {

            Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets");
            Windows.Storage.StorageFile file = await folder.GetFileAsync("Antibiotics.mp3");

            mediaPlayerElement.Source = MediaSource.CreateFromStorageFile(file);
            //mediaPlayerElement.Play();
        }*/

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;

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

        public enum MediaState
        {
            Stopped, Playing, Paused
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {

        }

        //MAKE THIS ONLY HAPPEN WHEN THE SELECTION DOESN'T CHANGE
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
    }
}
