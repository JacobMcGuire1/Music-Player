using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Music_thing
{
    public class Album
    {
        public string name { get; set; }

        public string artist { get; set; }

        public string key { get; set; }

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
            return Songids;
        }

        //Function to sort songs by track number.

    }

}
