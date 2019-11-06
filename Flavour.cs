using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_thing
{
    public class Flavour : Album
    {
        public string albumname;

        public bool pinned = false;
        
        public new ObservableCollection<Song> ObserveSongs()
        {
            ObservableCollection<Song> Songs = new ObservableCollection<Song>();


            /*foreach (int songid in Songids)
            {
                Song song = SongListStorage.SongDict[songid];
                song.isFlavour = true;
                song.TrackNumber = i;
                Songs.Add(song);
            }*/

            for (int i = 0; i < Songids.Count; i++)
            {
                Song song = SongListStorage.SongDict[Songids[i]];
                //song.isFlavour = true;
                //song.TrackNumber = i;

                Song newSong = new Song()
                {
                    id = song.id, //Remove this id
                    Title = song.Title,
                    Album = song.Album,
                    AlbumArtist = song.AlbumArtist,
                    Artist = song.Artist,
                    Year = song.Year,
                    Duration = song.Duration,
                    TrackNumber = i + 1,
                    isFlavour = true, //MAY NEED TO REMOVE
                    File = song.File
                };

                Songs.Add(newSong);
            }

            OrderByTrack(); //Should get rid of this

            return Songs;
        }

        public ObservableCollection<Song> RemoveSong(int TrackNumber)
        {
            if (TrackNumber < Songids.Count)
            {
                Songids.RemoveAt(TrackNumber);
            }
            else
            {

            }
            
            return ObserveSongs();
        }

        public new List<int> AddSong(int songid)
        {
            Songids.Add(songid);
            return Songids;
        }

        public void ReorderSongs(ObservableCollection<Song> Songs)
        {
            for (int i = 0; i < Songs.Count; i++)
            {
                Songids[i] = Songs[i].id;
            }
        }

    }
}
