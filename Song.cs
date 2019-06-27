using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Music_thing
{
    public class Song
    {
        public int id { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string AlbumArtist { get; set; }
        public string Album { get; set; }
        public TimeSpan Duration { get; set; }
        public uint Year { get; set; }
        public int TrackNumber { get; set; }
        public uint Bitrate { get; set; }
        public StorageFile File { get; set; }

        public override bool Equals(object obj)
        {
            if (this.id == (obj as Song).id) return true;
            return false;
        }

    }

    
}
