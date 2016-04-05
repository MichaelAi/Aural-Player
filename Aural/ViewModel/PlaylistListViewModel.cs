using Aural.Interface;
using Aural.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Aural.ViewModel
{
    public class PlaylistListViewModel : ViewModelBase
    {
        IFileIOService fileIOService;
        IContentDialogService contentDialogService;
        CancellationTokenSource cts = new CancellationTokenSource();
        bool isPlaylistLoadInProgress = false;

        private ObservableCollection<Playlist> _playlists = new ObservableCollection<Playlist>();
        public ObservableCollection<Playlist> Playlists
        {
            get { return _playlists; }
            set { Set("Playlists", ref _playlists, value); }
        }

        private bool _hasNoPlaylists = true;
        public bool HasNoPlaylists
        {
            get { return _hasNoPlaylists; }
            set { Set("HasNoPlaylists", ref _hasNoPlaylists, value); }
        }

        private Playlist _selectedPlaylist = new Playlist();
        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set { Set("SelectedPlaylist", ref _selectedPlaylist, value); SelectedPlaylistChanged(); }
        }

        public RelayCommand SavePlaylistCommand { get; private set; }
        public RelayCommand<string> DeletePlaylistCommand { get; private set; }
        public RelayCommand<string> EditPlaylistCommand { get; private set; }

        public PlaylistListViewModel(IFileIOService fileIOService, IContentDialogService contentDialogService)
        {
            this.fileIOService = fileIOService;
            this.contentDialogService = contentDialogService;
            SavePlaylistCommand = new RelayCommand(CreatePlaylist);
            Startup();
        }

        //do stuff at app launch
        private async void Startup()
        {
            await LoadPlaylists();
        }

        //Create a new playlist button handler
        private async void CreatePlaylist()
        {
            Playlist play = await contentDialogService.CreateNewPlaylist();
            if (play != null)
                Playlists.Add(play);
        }

        //listen when the selected playist changes and change the displayed playlist
        //only read the playlist when it has been clicked for the first time, otherwise reference it
        //consider cleaning up the playlists when they have not been used for a while.
        private async void SelectedPlaylistChanged()
        { 
            Messenger.Default.Send(new NotificationMessage<string>("ShowPlaylistLoadingBar", "ShowPlaylistLoadingBar"));
            await Task.Delay(5);     
            if (SelectedPlaylist.PlaylistName != "")
            {
                if (SelectedPlaylist.PlaylistId == Guid.Empty)
                {
                    try
                    {
                        SelectedPlaylist.PlaylistId = Guid.NewGuid();
                        var files = await fileIOService.ReadPlaylistFile(SelectedPlaylist);
                        SelectedPlaylist.Items = new ObservableCollection<PlaylistItem>(await AcceptFiles(files));
                    }
                    catch
                    {
                        Debug.WriteLine("oops!");
                    }
                }
            }
            Messenger.Default.Send(new NotificationMessage<Playlist>(SelectedPlaylist, "SelectedPlaylistChanged"));
        }

        public async Task<ObservableCollection<PlaylistItem>> AcceptFiles(IReadOnlyList<StorageFile> files)
        {
            return await fileIOService.LoadFiles(files);
        }

        //Load the playlists at startup
        private async Task LoadPlaylists()
        {
            Playlists = await fileIOService.ReadPlaylistsFromFolder();
            if (Playlists != null)
                if (Playlists.Count > 0)
                    SelectedPlaylist = Playlists.First();
 
            Playlists.CollectionChanged += new NotifyCollectionChangedEventHandler(PlaylistsChanged);
        }

        //check whether there are no playlists anymore
        private void PlaylistsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Playlists.Count > 0)
                HasNoPlaylists = false;
            else
                HasNoPlaylists = true;
        }

        //show edit dialog and save the new playlist
        private async void PrepareEditFile(string id)
        {
            var result = Playlists.Where(x => x.PlaylistName == id).FirstOrDefault();
            if (result != null)
            {
                var newPlay = await contentDialogService.ShowEditPlaylistDialog(result);
                await fileIOService.WritePlaylistFile(newPlay);
            }
        }

        //show delete dialog to confirm deletion
        private async void PrepareDeleteFile(string id)
        {
            var result = Playlists.Where(x => x.PlaylistName == id).FirstOrDefault();
            bool confirm = false;
            if (result != null)
            {
                confirm = await contentDialogService.ShowPlaylistDeletionConfirmation(result);
            }
            if (confirm)
            {
                await fileIOService.DeletePlaylistFile(result);
                //TODO: delete playlist from list
            }
        }
    }
}
