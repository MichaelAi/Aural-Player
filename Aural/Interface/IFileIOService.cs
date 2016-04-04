using Aural.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aural.Interface
{
    public interface IFileIOService
    {
        Task<ObservableCollection<Playlist>> ReadPlaylistsFromFolder();
        Task<List<StorageFile>> ReadPlaylistFile(Playlist play);
        Task<bool> WritePlaylistFile(Playlist play);
        Task<bool> DeletePlaylistFile(Playlist play);
        Task<ObservableCollection<PlaylistItem>> LoadFiles(IReadOnlyList<StorageFile> files);
    }
}
