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
using Windows.Storage;

namespace Music_thing.Classes
{
    public class LastFm
    {
        private LastfmClient client;
        private bool initialised = false;

        private LastFm(LastfmClient client, bool initialised) 
        {
            this.client = client;
            this.initialised = initialised;
        }

        public async static Task<LastFm> InitialiseLastFm()
        {
            var client = await InitialiseClient();
            var initialised = await Login(client);
            return new LastFm(client, initialised);
        }

        public static async Task<LastfmClient> InitialiseClient()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile credsFile = await storageFolder.GetFileAsync("lastfminfo.json");
            var lastfminfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(await FileIO.ReadTextAsync(credsFile));

            var client = new LastfmClient(lastfminfo["LASTFM_API_KEY"], lastfminfo["LASTFM_SHARED_SECRET"]);

            return client;
        }

        public static async Task<bool> Login(LastfmClient client)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile credsFile = await storageFolder.GetFileAsync("lastfminfo.json");
            var lastfminfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(await FileIO.ReadTextAsync(credsFile));

            var loginresponse = await client.Auth.GetSessionTokenAsync(lastfminfo["LASTFM_USERNAME"], lastfminfo["LASTFM_PASSWORD"]);

            return loginresponse.Success;
        }

        private async Task<bool> CheckOrRetrySetup()
        {
            if (this.initialised) return true;

            var client = await InitialiseClient();
            var initialised = await Login(client);

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

            Scrobble scrobble = new Scrobble(song.Artist, song.Album, song.Title, DateTimeOffset.Now);

            var response = await client.Scrobbler.ScrobbleAsync(new List<Scrobble> { scrobble });

            return response.Success;
        }
    }
}
