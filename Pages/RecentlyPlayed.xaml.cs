using Music_thing.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class RecentlyPlayed : Page
    {
        public static int Count = 100;
        public ObservableCollection<Song> Songs { get; set; }
        = new ObservableCollection<Song>();


        public RecentlyPlayed()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var songData = SongLog.GetRecentListens(Count);

            Songs = new ObservableCollection<Song>(songData.Select(x => SongListStorage.SongDict[x.Item1]));
        }

        private async void playButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            await SongListStorage.SongDict[button.Tag as string].Play();
        }

        private async void addToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            await SongListStorage.SongDict[button.Tag as string].AddToPlaylist();
        }

    }
}
