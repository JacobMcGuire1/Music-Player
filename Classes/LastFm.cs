using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using Windows.Storage;

namespace Music_thing.Classes
{
    public class LastFm
    {
        private LastfmClient client;
        private bool initialised = false;
        private Dictionary<string, string> creds;

        private LastFm(LastfmClient client, bool initialised, Dictionary<string, string> creds) 
        {
            this.client = client;
            this.initialised = initialised;
            this.creds = creds;
        }

        public async static Task<LastFm> InitialiseLastFm()
        {
            var creds = await GetCredsData();
            var client = InitialiseClient(creds);
            var initialised = await Login(client, creds);
            return new LastFm(client, initialised, creds);
        }

        public static LastfmClient InitialiseClient(Dictionary<string, string> creds)
        {
            var client = new LastfmClient(creds["LASTFM_API_KEY"], creds["LASTFM_SHARED_SECRET"]);
            return client;
        }

        public static async Task<Dictionary<string, string>> GetCredsData()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile credsFile = await storageFolder.GetFileAsync("lastfminfo.json");
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(await FileIO.ReadTextAsync(credsFile));
        }

        public static async Task<bool> Login(LastfmClient client, Dictionary<string, string> creds)
        {
            try
            {
                var loginresponse = await client.Auth.GetSessionTokenAsync(creds["LASTFM_USERNAME"], creds["LASTFM_PASSWORD"]);
                return loginresponse.Success;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }

        private async Task<bool> CheckOrRetrySetup()
        {
            if (this.initialised) return true;

            var client = InitialiseClient(this.creds);
            var initialised = await Login(client, this.creds);

            if (initialised)
            {
                this.client = client;
                this.initialised = initialised;
                return true;
            }
            else 
            {
                return false;
            }            
        }

        public async Task<bool> ScrobbleSong(Song song)
        {
            if (!(await CheckOrRetrySetup())) return false;

            Scrobble scrobble = new Scrobble(song.AlbumArtist, song.Album, song.Title, DateTimeOffset.Now);

            var response = await client.Scrobbler.ScrobbleAsync(new List<Scrobble> { scrobble });

            return response.Success;
        }

        public async Task GetRecentSongs()
        {
            var result = await client.User.GetRecentScrobbles(creds["LASTFM_USERNAME"], null, null, true, 1, 100);
        }

        public async Task<bool> SetNowPlaying(Song song)
        {
            if (!(await CheckOrRetrySetup()) || song == null) return false;

            Scrobble scrobble = new Scrobble(song.AlbumArtist, song.Album, song.Title, DateTimeOffset.Now);            

            var response = await client.Track.UpdateNowPlayingAsync(scrobble);

            return response.Success;
        }
    }
}
