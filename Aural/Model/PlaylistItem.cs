using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Aural.Model
{
    public class PlaylistItem : ObservableObject
    {
        private Guid _id;
        public Guid Id
        {
            get { return _id; }
            set { Set("Id", ref _id, value); }
        }

        private MusicProperties _properties;
        public MusicProperties Properties
        {
            get { return _properties; }
            set { Set("Properties", ref _properties, value); }
        }

        private StorageFile _playlistFile;
        public StorageFile PlaylistFile
        {
            get { return _playlistFile; }
            set { Set("PlaylistFile", ref _playlistFile, value); }
        }

        private Image _albumArt = new Image();
        public Image AlbumArt
        {
            get { return _albumArt; }
            set { Set("AlbumArt", ref _albumArt, value); }
        }

        private int _playlistTrackNo;
        public int PlaylistTrackNo
        {
            get { return _playlistTrackNo; }
            set { Set("PlaylistTrackNo", ref _playlistTrackNo, value); }
        }
    }
}
