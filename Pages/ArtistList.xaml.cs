using Music_thing.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        public ArtistList()
        {
            this.InitializeComponent();

            //Artists = SongListStorage.Artists;

            //Artists.Add(new Artist() { name = "Steve" });

            Artists = SongListStorage.Artists;

            //string coiun = Artists.ToString();

            //foreach (String name in SongListStorage.Artists)
            //{
            //    Artists.Add(new Artist() { name = name });
            //}
        }

        private void Artistbutton_Click(object sender, RoutedEventArgs e)
        {
            string artistid = (string)((Button)sender).Tag;

            //e.Parameter = 

            this.Frame.Navigate(typeof(ArtistPage), artistid);
        }
    }
}
