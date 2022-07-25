using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System; //Launcher
using Windows.Storage; //Files File.AccessMode
using Microsoft.OneDrive.Sdk;
using System.Collections.Concurrent;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Music_thing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

        private string LastLoggedSong;

        

        public SettingsPage()
        {
            this.InitializeComponent();
            globalvol.Value = Media.Instance.globalVol * 100;
            AudioBalanceSlider.Value = Media.Instance.mediaPlayer.AudioBalance / 100;
            LastLoggedSong = SongListStorage.LastLoggedSong;
            ChosenFolderText.Text = "Picked folder: " + SongListStorage.PlayListFolder.Name;
        }

        private void Globalvol_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Media.Instance.globalVol = e.NewValue / 100;
            Media.Instance.ChangeVolume();
            Windows.Storage.ApplicationData.Current.LocalSettings.Values["globalvol"] = Media.Instance.globalVol;
        }

        private async void Updatemusicbutton_Click(object sender, RoutedEventArgs e)
        {
            await Database.GetSongs(false);
            await App.GetForCurrentView().LoadPinnedFlavours();
        }

        private void AudioBalanceSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Media.Instance.mediaPlayer.AudioBalance = (sender as Slider).Value / 100;
        }

        private async void OpenfolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            await Launcher.LaunchFolderAsync(folder);

        }

        private async void OneDriveButton_Click(object sender, RoutedEventArgs e)
        {
            var scopes = new[]
            {
              "onedrive.readwrite",
              "onedrive.appfolder",
              "wl.signin"
            };
            SongListStorage._client = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(scopes);
            var session = await SongListStorage._client.AuthenticateAsync();
            
            //Debug.WriteLine($"Token: {session.AccessToken}");
        }

        private async void PickPlayListFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PlaylistFolder", folder);
                ChosenFolderText.Text = "Picked folder: " + folder.Name;
                SongListStorage.PlaylistDict = new ConcurrentDictionary<long, Playlist>();
                await SongListStorage.LoadFlavours();
                SongListStorage.PlayListFolder = folder;
            }
            else
            {
                //ChosenFolderText.Text = "No Folder Found.";
            }
        }
    }
}
