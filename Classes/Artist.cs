using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Music_thing
{
    public class Artist
    {
        public string name { get; set; }
        public int year { get; set; }
        public List<String> Albums = new List<String>();

        public bool AddAlbum(string albumkey)
        {
            if (!Albums.Contains(albumkey)){
                Albums.Add(albumkey);
                return true;
            }
            return false;
        }

        public string GetStringAlbumCount()
        {
            return (" " + Albums.Count.ToString() + " album(s)");
        }

        //Add functions to order albums by year etc.

    }


}
