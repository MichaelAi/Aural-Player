using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aural.Model
{
    public class Playlist : ObservableObject
    {
        private ObservableCollection<PlaylistItem> _items = new ObservableCollection<PlaylistItem>();
        public ObservableCollection<PlaylistItem> Items
        {
            get { return _items; }
            set { Set("Items", ref _items, value); }
        }

        private string _playlistName;
        public string PlaylistName
        {
            get { return _playlistName; }
            set { Set("PlaylistName", ref _playlistName, value); }
        }

        private Guid _playlistId;
        public Guid PlaylistId
        {
            get { return _playlistId; }
            set { Set("PlaylistId", ref _playlistId, value); }
        }
    }
}
