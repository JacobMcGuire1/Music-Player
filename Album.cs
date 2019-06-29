using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Music_thing
{
    public class Album
    {
        public string name { get; set; }

        public string artist { get; set; }

        public string key { get; set; }

        public int year { get; set; }

        public BitmapImage albumart;

        public List<int> Songids { get; set; }
        = new List<int>();

        public ObservableCollection<Song> ObserveSongs()
        {
            ObservableCollection<Song> Songs = new ObservableCollection<Song>();

            foreach (int songid in Songids)
            {
                Song song = SongListStorage.SongDict[songid];
                Songs.Add(song);
            }

            return Songs;
        }

        public List<int> AddSong(int songid)
        {
            Songids.Add(songid);
            SetAlbumArt(songid);
            OrderByTrack();
            return Songids;
        }

        //Function to sort songs by track number.
        public void OrderByTrack()
        {
            //Need to add disk number support
            //Songids = Songids.OrderBy(x => SongListStorage.SongDict[x].TrackNumber) as List<int>;

            Songids.Sort((x, y) => SongListStorage.SongDict[x].TrackNumber - SongListStorage.SongDict[y].TrackNumber);
        }

        public void SetAlbumArt(int songid)
        {
            Song song = SongListStorage.SongDict[songid];
            if (albumart != null)
            {
                albumart = song.AlbumArt;
            }
        }

    }

}
