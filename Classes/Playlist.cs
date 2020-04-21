using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Music_thing
{
    public class Playlist : Album
    {
        public long PlaylistID;
        public string albumname;
        public string albumkey;

        public bool pinned = false;
        public bool pinnedinalbum = false;
        public bool isflavour;

        //[JsonIgnore]
        //public ImageSource thumbnail;

        public Playlist() { }
        public Playlist(String name)
        {
            Name = name;
            pinned = true;
            isflavour = false;
            albumname = "";
            PlaylistID = GenerateID();
        }

        public Playlist(string flavourname, string albumid)
        {
            Album album = SongListStorage.AlbumDict[albumid];
            Name = flavourname;
            albumname = album.Name;
            albumkey = album.Key;
            Artist = album.Artist;
            Songids = new List<string>(album.Songids);
            pinned = true;
            isflavour = true;
            PlaylistID = GenerateID();
            SongListStorage.AlbumDict[albumid].AddFlavour(PlaylistID);
        }

        /*private async Task<bool> TryCreatePlaylistFile()
        {
            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
            try
            {
                StorageFile file = await storageFolder.CreateFileAsync(PlaylistID.ToString() + ".playlist", Windows.Storage.CreationCollisionOption.FailIfExists);
                await FileIO.WriteTextAsync(file, ConvertToJSON());
                return true;
            }
            catch(Exception E)
            {
                Debug.WriteLine("Couldn't create flavour file.");
                return false;
            }
            
        }*/

        public async Task<string> Rename()
        {
            TextBox textBox = new TextBox()
            {
                Text = Name
            };
            ContentDialog nameFlavourDialog = new ContentDialog()
            {
                Title = "Name your flavour",
                Content = textBox,
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Ok",
                DefaultButton = ContentDialogButton.Primary
            };
            //nameFlavourDialog.
            ContentDialogResult result = await nameFlavourDialog.ShowAsync();
            /*string flavourname = "";
            bool err = false;
            while (flavourname == "")
            {
                flavourname = Name;
                ContentDialog nameFlavourDialog = new ContentDialog()
                {
                    Title = "Name your flavour",
                    CloseButtonText = "Ok"
                };
                TextBox textBox = new TextBox()
                {
                    Text = Name
                };
                if (!err)
                {
                    nameFlavourDialog.Content = textBox;
                }
                else
                {
                    var stackpanel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical
                    };
                    TextBlock errormsgtext = new TextBlock()
                    {
                        Text = "Error: Please try another name."
                    };
                    stackpanel.Children.Add(textBox);
                    stackpanel.Children.Add(errormsgtext);
                    nameFlavourDialog.Content = stackpanel;
                }
                await nameFlavourDialog.ShowAsync();

                flavourname = textBox.Text;
                err = true;
            }*/
            if (result == ContentDialogResult.Primary)
            {
                Name = textBox.Text;
                App.GetForCurrentView().UpdatePlaylistName(PlaylistID);
                await SavePlaylistFile(false);
                return Name;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> SavePlaylistFile(bool fail)
        {
            while (true)
            {
                try
                {
                    StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
                    //StorageFile file = await storageFolder.CreateFileAsync(PlaylistID.ToString() + ".playlist", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                    StorageFile file = fail ? await storageFolder.CreateFileAsync(PlaylistID.ToString() + ".playlist", Windows.Storage.CreationCollisionOption.FailIfExists) : await storageFolder.CreateFileAsync(PlaylistID.ToString() + ".playlist", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, ConvertToJSON());
                    return true;
                    //failed = false;
                }
                catch { }
            }

        }

        public async Task DeleteFile()
        {
            while (true)
            {
                try
                {
                    StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
                    StorageFile file = await storageFolder.GetFileAsync(PlaylistID.ToString() + ".playlist");
                    await file.DeleteAsync();
                    return;
                }
                catch
                {
                }
            }
        }

        private long GenerateID()
        {
            DateTime st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan t = (DateTime.Now.ToUniversalTime() - st);
            return t.Ticks;
        }

         //Should check if a playlist with this ID already exists.
       /* private async Task CreateIDAndSave()
        {
            bool done = false;
            while (!done)
            {
                
                await TryCreatePlaylistFile();
            }
        }*/

        public string ConvertToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public ObservableCollection<Song> ObserveSongs()
        {
            ObservableCollection<Song> Songs = new ObservableCollection<Song>();

            for (int i = 0; i < Songids.Count; i++)
            {
                if (SongListStorage.SongDict.ContainsKey(Songids[i]))
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
