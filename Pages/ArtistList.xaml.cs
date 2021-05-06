using Music_thing.Pages;
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
    public sealed partial class ArtistList : Page
    {

        public ObservableCollection<Artist> Artists { get; }
        = new ObservableCollection<Artist>();

        private string searchterm = "";


        public ArtistList()
        {
            this.InitializeComponent();


            Artists = SongListStorage.Artists;


        }

        private void ListViewArtists_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(ArtistPage), ((Artist)e.ClickedItem).name);
        }

        //Doesn't work for numbers.
        private async void ListViewArtists_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.ToString() == "Back")
            {
                if (searchterm.Length > 0)
                {
                    searchterm = searchterm.Remove(searchterm.Length - 1);
                    SearchTextBlock.Text = searchterm;
                }
                return;
            }
            if (e.Key.ToString().Length == 1)
            {
                SearchPopup.IsOpen = true;
                var listview = ListViewArtists;
                searchterm = searchterm + e.Key.ToString().ToUpperInvariant();
                Artist result = null;
                foreach (Artist artist in Artists)
                {
                    if (artist.name.Length >= searchterm.Length && artist.name.Substring(0, searchterm.Length).ToLowerInvariant() == searchterm.ToLowerInvariant())
                    {
                        result = artist;
                        break;
                    }
                }
                if (result == null)
                {
                    var r = SongListStorage.SearchArtists(searchterm);
                    if (r.Count > 0) result = r[0];
                }
                if (result != null)
                {
                    listview.ScrollIntoView(result);
                    listview.SelectedItem = result;
                }
                var oldsearchterm = searchterm;
                SearchTextBlock.Text = searchterm;
                await Task.Delay(1000);
                if (searchterm.Equals(oldsearchterm))
                {
                    SearchPopup.IsOpen = false;
                    searchterm = "";
                    SearchTextBlock.Text = "";
                }
            }
        }

        private void centrePopup()
        {
            var h = ((Frame)Window.Current.Content).ActualHeight;
            var w = ((Frame)Window.Current.Content).ActualWidth;
            SearchPopup.VerticalOffset = h / 2;
            SearchPopup.HorizontalOffset = w / 2;
        }


        /*private void placeTextInCentre()
        {
            double primScreenHeight = System.Windows.SystemParameters.FullPrimaryScreenHeight;
            double primScreenWidth = System.Windows.SystemParameters.FullPrimaryScreenWidth;
            SearchTextBlock. = (primScreenHeight - SearchTextBlock.Height) / 2;
            SearchTextBlock.Left = (primScreenWidth - SearchTextBlock.Width) / 2;
        }*/

        private void ListViewArtists_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as ListView).Focus(FocusState.Programmatic);
        }
    }
}
