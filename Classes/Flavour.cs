using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Music_thing
{
    public class Flavour : Album
    {
        public string albumname;
        public string albumkey;

        public bool pinned = false;
        public bool pinnedinalbum = false;

        //[JsonIgnore]
        //public ImageSource thumbnail;

        public ObservableCollection<Song> ObserveSongs()
        {
            ObservableCollection<Song> Songs = new ObservableCollection<Song>();

            for (int i = 0; i < Songids.Count; i++)
            {
                try
                {
                    Song song = SongListStorage.SongDict[Songids[i]];
                    //song.isFlavour = true;
                    //song.TrackNumber = i;

                    Song newSong = new Song()
                    {
                        ID = song.ID, //dont Remove this id
                        Title = song.Title,
                        Album = song.Album,
                        AlbumArtist = song.AlbumArtist,
                        Artist = song.Artist,
                        Year = song.Year,
                        Duration = song.Duration,
                        TrackNumber = i + 1,
                        IsFlavour = true, //MAY NEED TO REMOVE
                        Path = song.Path
                    };

                    Songs.Add(newSong);
                }
                catch(KeyNotFoundException E)
                {
                    //Debug.WriteLine("Couldn't fine key " + Songids[i] + " in the collection.");
                    Debug.WriteLine(E.Message);
                }
                
            }

            //OrderByTrack(); //Should get rid of this

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

        public List<string> AddSong(string songid)
        {
            Songids.Add(songid);
            return Songids;
        }

        public void AddSongList(List<String> songidlist)
        {
            foreach(String songid in songidlist)
            {
                Songids.Add(songid);
            }
        }

        public void ReorderSongs(ObservableCollection<Song> Songs)
        {
            for (int i = 0; i < Songs.Count; i++)
            {
                Songids[i] = Songs[i].ID;
            }
        }

    }
}
