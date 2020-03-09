using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Microsoft.Data.Sqlite;

namespace Music_thing
{
    public static class Database
    {
        //Gets a collection of song files.
        public static async void GetSongs()
        {
            QueryOptions queryOption = new QueryOptions
                (CommonFileQuery.OrderByTitle, new string[] { "*" });

            queryOption.FolderDepth = FolderDepth.Deep;

            Queue<IStorageFolder> folders = new Queue<IStorageFolder>();

            var files = await KnownFolders.MusicLibrary.CreateFileQueryWithOptions
              (queryOption).GetFilesAsync();

            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile songsfile = await storageFolder.CreateFileAsync("test.txt",
                    Windows.Storage.CreationCollisionOption.OpenIfExists);
            var listOfStrings = new List<string> { "Songs: " };

            Regex songreg = new Regex(@"^audio/");
            foreach (var file in files)
            {
                //Checks if it's an audio file
                if (songreg.IsMatch(file.ContentType))
                {
                    //MusicProperties musicProperties = await (item as StorageFile).Properties.GetMusicPropertiesAsync();
                    listOfStrings.Add(file.Name);
                }
            }
            await Windows.Storage.FileIO.AppendLinesAsync(songsfile, listOfStrings); // each entry in the list is written to the file on its own line.
        }
    }
}
