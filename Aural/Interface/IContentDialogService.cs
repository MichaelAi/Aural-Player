using Aural.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aural.Interface
{
    public interface IContentDialogService
    {
        Task<Playlist> CreateNewPlaylist();
        Task<bool> ShowPlaylistDeletionConfirmation(Playlist play);
        Task<Playlist> ShowEditPlaylistDialog(Playlist oldPlay);
    }
}
