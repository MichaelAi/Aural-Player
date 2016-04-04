using Aural.Interface;
using Aural.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aural.Service
{
    public class FileIOService : IFileIOService
    {
        public async Task<ObservableCollection<Playlist>> ReadPlaylistsFromFolder()
        {
            ObservableCollection<Playlist> playlists = new ObservableCollection<Playlist>();
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFolder playlistsFolder = await storageFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);
            foreach (var item in await playlistsFolder.GetItemsAsync())
            {
                playlists.Add(new Playlist { PlaylistName = item.Name.Substring(0, item.Name.LastIndexOf(".")) });
            }
            return playlists;
        }

        public async Task<List<StorageFile>> ReadPlaylistFile(Playlist play)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFolder playlistsFolder = await storageFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);
            StorageFile playlistFile = await playlistsFolder.GetFileAsync(play.PlaylistName + ".m3u");
            string contents = await Windows.Storage.FileIO.ReadTextAsync(playlistFile);
            string[] paths = Regex.Split(contents, "\r\n|\r|\n");
            List<StorageFile> items = new List<StorageFile>();

            foreach (var path in paths)
            {
                if (path.Contains("#") || path.Length == 0)
                {
                    //do nothing
                }
                else
                {
                    items.Add(await StorageFile.GetFileFromPathAsync(path));
                }
            }

            return items;
          //  PopulatePlayist(items, false, false, true);
        }

        public async Task<bool> WritePlaylistFile(Playlist play)
        {
            try
            {

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFolder playlistsFolder = await storageFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);
                StorageFile playlist = await playlistsFolder.CreateFileAsync(play.PlaylistName + ".m3u",
                        CreationCollisionOption.ReplaceExisting);


                var stream = await playlist.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                    {
                        dataWriter.WriteString(await PrepareM3uFile(play));
                        await dataWriter.StoreAsync();
                        await outputStream.FlushAsync();
                    }
                }
                stream.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePlaylistFile(Playlist play)
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFolder playlistsFolder = await storageFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);
                StorageFile playlistFile = await playlistsFolder.GetFileAsync(play.PlaylistName + ".m3u");
                await playlistFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return true;
            }
            catch
            {

            }
            return false;
        }

        private async Task<string> PrepareM3uFile(Playlist play)
        {
            string contents = "";
            contents += "#EXTM3U";
            contents += "\r\n";
            foreach (var item in play.Items.OrderBy(x => x.PlaylistTrackNo))
            {
                Windows.Storage.FileProperties.BasicProperties basicProperties =
                     await item.PlaylistFile.GetBasicPropertiesAsync();
                string filePath = item.PlaylistFile.Path;
                contents += "#EXTINF:" + item.Properties.Duration.TotalSeconds + ", "
                    + item.Properties.Artist + " - " + item.Properties.Title;
                contents += "\r\n";
                contents += filePath;
                contents += "\r\n";
            }
            return contents;
        }

        public async Task<ObservableCollection<PlaylistItem>> LoadFiles(IReadOnlyList<StorageFile> files)
        {
            ObservableCollection<PlaylistItem> tempPlaylist = new ObservableCollection<PlaylistItem>();
            // Ensure files were selected
            if (files != null)
            {
                foreach (var file in files)
                {
                    Windows.Storage.FileProperties.MusicProperties x = await file.Properties.GetMusicPropertiesAsync();
                    tempPlaylist.Add(new PlaylistItem { Id = Guid.NewGuid(), Properties = x, PlaylistFile = file });
                }
            }
            return tempPlaylist;
        }
    }
}
