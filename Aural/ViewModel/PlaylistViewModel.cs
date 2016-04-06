using Aural.Interface;
using Aural.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aural.ViewModel
{
    public class PlaylistViewModel : ViewModelBase
    {
        IFileIOService fileIOService;

        private string _searchParameter = "";
        public string SearchParameter
        {
            get { return _searchParameter; }
            set { Set("SearchParameter", ref _searchParameter, value); SearchPlaylist(); }
        }

        private bool _displayedPlaylistHasItems = false;
        public bool DisplayedPlaylistHasItems
        {
            get { return _displayedPlaylistHasItems; }
            set { Set("DisplayedPlaylistHasItems", ref _displayedPlaylistHasItems, value);
                //SavePlaylistCommand.RaiseCanExecuteChanged();
            }
        }

        private PlaylistItem _nowPlayingItem;
        public PlaylistItem NowPlayingItem
        {
            get { return _nowPlayingItem; }
            set { Set("NowPlayingItem", ref _nowPlayingItem, value); NowPlayingItem_Changed(); }
        }

        private Playlist _displayedPlaylist = new Playlist { PlaylistId = Guid.Empty, PlaylistName = "", Items = new ObservableCollection<PlaylistItem>() };
        public Playlist DisplayedPlaylist
        {
            get { return _displayedPlaylist; }
            set { Set("DisplayedPlaylist", ref _displayedPlaylist, value); }
        }

        private Playlist _selectedPlaylist = new Playlist();
        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set { Set("SelectedPlaylist", ref _selectedPlaylist, value); }
        }

        private PlaylistItem _selectedDisplayedItem = new PlaylistItem();
        public PlaylistItem SelectedDisplayedItem
        {
            get { return _selectedDisplayedItem; }
            set { Set("SelectedDisplayedItem", ref _selectedDisplayedItem, value); }
        }

        private bool _isPlaylistLoadInProgress = true;
        public bool IsPlaylistLoadInProgress
        {
            get { return _isPlaylistLoadInProgress; }
            set { Set("IsPlaylistLoadInProgress", ref _isPlaylistLoadInProgress, value); }
        }

        public RelayCommand<string> OrderPlaylistCommand { get; private set; }
        public RelayCommand<PlaylistItem> RemoveItemFromPlaylistCommand { get; private set; }

        public PlaylistViewModel(IFileIOService fileIOService)
        {
            this.fileIOService = fileIOService;
            OrderPlaylistCommand = new RelayCommand<string>((mode) => OrderPlaylist(mode));
            DisplayedPlaylist.Items.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
            RemoveItemFromPlaylistCommand = new RelayCommand<PlaylistItem>((playItem) => RemoveItemFromPlaylist(playItem));
            Startup();
        }

        private void RemoveItemFromPlaylist(PlaylistItem playItem)
        {
            DisplayedPlaylist.Items.Remove(playItem);
            SelectedPlaylist.Items.Remove(playItem);
            Messenger.Default.Send(new NotificationMessage<Playlist>(SelectedPlaylist, "ItemRemovedFromPlaylist"));
            SavePlaylist(SelectedPlaylist);
        }

        //do stuff at app launch
        private void Startup()
        {
            RegisterMessaging();
        }

        private void RegisterMessaging()
        {
            Messenger.Default.Register <NotificationMessage<Playlist>>(this,
                nm =>
                {
                    if (nm.Notification != null)
                    {
                        if (nm.Notification == "SelectedPlaylistChanged")
                        {
                            ClearDisplayedPlaylist();
                            SelectedPlaylist = nm.Content;
                            PopulatePlaylist(nm.Content.Items);
                        }
                    }
                });
        Messenger.Default.Register<NotificationMessage<ObservableCollection<PlaylistItem>>>(this,
                nm =>
                {
                    if (nm.Notification != null)
                    {                       
                        if(nm.Notification == "fromDragDrop")
                        {
                            PopulatePlaylist(nm.Content);
                            SavePlaylist(SelectedPlaylist);
                        }
                        else if (nm.Notification == "fromFileOpen")
                        {
                            ClearDisplayedPlaylist();
                            PopulatePlaylist(nm.Content);
                            PlayItems(nm.Content);
                        }
                    }
                }
                );
            Messenger.Default.Register<NotificationMessage<string>>(this,
        nm =>
        {
            if (nm.Notification != null)
            {
                if (nm.Notification == "RequestDisplayed")
                {
                    TransferPlaylist();
                }
                if (nm.Notification == "ShowPlaylistLoadingBar")
                {
                    IsPlaylistLoadInProgress = true;
                }
            }
        }
        );
            Messenger.Default.Register<NotificationMessage<PlaylistItem>>(this, nm =>
            {
                if (nm.Notification == "NowPlayingItemChanged")
                {
                    NowPlayingItem = nm.Content;
                }
            });

        }

        private void PlayItems(ObservableCollection<PlaylistItem> play)
        {
            Messenger.Default.Send(new NotificationMessage<ObservableCollection<PlaylistItem>>(play, "PlayFirst"));
        }

        private bool DisplayedPlaylistHasItemsCheck()
        {
            return DisplayedPlaylistHasItems;
        }

        //save a playlist in thebackground
        private async void SavePlaylist(Playlist play)
        {
            bool success = await fileIOService.WritePlaylistFile(play);
        }

        //filter the playlist using the given term
        private ObservableCollection<PlaylistItem> unfilteredPlaylist = new ObservableCollection<PlaylistItem>();
        private void SearchPlaylist()
        {
            if (SearchParameter != null && SearchParameter.Length != 0)
            {
                DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(unfilteredPlaylist.Where
                    (x => x.Properties.Title.ToLower().Contains(SearchParameter.ToLower())
                    || x.Properties.Artist.ToLower().Contains(SearchParameter.ToLower())
                    || x.Properties.Album.ToLower().Contains(SearchParameter.ToLower())
                    || x.Properties.AlbumArtist.ToLower().Contains(SearchParameter.ToLower())
                    ));

            }
            else
            {
                DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(unfilteredPlaylist);
            }
        }

        private void TransferPlaylist()
        {
            LabelPlaylistNumbers();
            if (SelectedPlaylist.Items != null && SelectedPlaylist.Items.Count > 0)
            {
                Messenger.Default.Send(new NotificationMessage<Playlist>(SelectedPlaylist, "DisplayToCurrent"));
            }
            else
            {
                DisplayedPlaylistHasItems = false;
            }

        }

        public async Task<ObservableCollection<PlaylistItem>> AcceptFiles(IReadOnlyList<StorageFile> files)
        {
            return await fileIOService.LoadFiles(files);
        }

        //fill the currently selected playlist
        public void PopulatePlaylist(ObservableCollection<PlaylistItem> items)
        {
            if (SelectedPlaylist != null)
            {
                IsPlaylistLoadInProgress = true;
                foreach (var item in items)
                {
                    DisplayedPlaylist.Items.Add(item);
                }
                SelectedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items);
                Messenger.Default.Send(new NotificationMessage<Playlist>(SelectedPlaylist, "DisplayToDisplay"));
                IsPlaylistLoadInProgress = false;
            }
        }

        //clear the displayed playlist
        private void ClearDisplayedPlaylist()
        {
            DisplayedPlaylist.Items.Clear();
        }

        //highlight the current item
        private void HighlightCurrentlyPlayingItem()
        {
            if (NowPlayingItem != null)
            {
                SelectedDisplayedItem = DisplayedPlaylist.Items.Where(x => x.Id == NowPlayingItem.Id).FirstOrDefault();
            }
        }

        private void NowPlayingItem_Changed()
        {

        }

        //Handle the order event, that is order the playlist items by the given parameter
        private void OrderPlaylist(string mode)
        {
            if (DisplayedPlaylist.Items != null && DisplayedPlaylist.Items.Count > 0)
            {
                DisplayedPlaylist.Items.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
                switch (mode)
                {
                    case ("artist"):
                        if (DisplayedPlaylist.Items.SequenceEqual(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Artist)))
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderByDescending(x => x.Properties.Artist));
                        else
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Artist));
                        break;
                    case ("album"):
                        if (DisplayedPlaylist.Items.SequenceEqual(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Album)))
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderByDescending(x => x.Properties.Album));
                        else
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Album));
                        break;
                    case ("title"):
                        if (DisplayedPlaylist.Items.SequenceEqual(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Title)))
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderByDescending(x => x.Properties.Title));
                        else
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Title));
                        break;
                    case ("year"):
                        if (DisplayedPlaylist.Items.SequenceEqual(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Year)))
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderByDescending(x => x.Properties.Year));
                        else
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Year));
                        break;
                    case ("albumartist"):
                        if (DisplayedPlaylist.Items.SequenceEqual(DisplayedPlaylist.Items.OrderBy(x => x.Properties.AlbumArtist)))
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderByDescending(x => x.Properties.AlbumArtist));
                        else
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderBy(x => x.Properties.AlbumArtist));
                        break;
                    case ("genre"):
                        if (DisplayedPlaylist.Items.SequenceEqual(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Genre)))
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderByDescending(x => x.Properties.Genre));
                        else
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Genre));
                        break;
                    case ("tracknumber"):
                        if (DisplayedPlaylist.Items.SequenceEqual(DisplayedPlaylist.Items.OrderBy(x => x.Properties.TrackNumber)))
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderByDescending(x => x.Properties.TrackNumber));
                        else
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderBy(x => x.Properties.TrackNumber));
                        break;
                    case ("rating"):
                        if (DisplayedPlaylist.Items.SequenceEqual(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Rating)))
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderByDescending(x => x.Properties.Rating));
                        else
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Rating));
                        break;
                    case ("duration"):
                        if (DisplayedPlaylist.Items.SequenceEqual(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Duration)))
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderByDescending(x => x.Properties.Duration));
                        else
                            DisplayedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items.OrderBy(x => x.Properties.Duration));
                        break;
                    default:
                        break;
                }
                DisplayedPlaylist.Items.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
                if (SelectedPlaylist != null)
                {
                    LabelPlaylistNumbers();
                    HighlightCurrentlyPlayingItem();
                    SelectedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Items);
                    SavePlaylist(SelectedPlaylist);

                }
                //TransferPlaylist(SelectedPlaylist);
            }
        }

        //public void TransferPlaylist(Playlist play)
        //{

        //}

        //put a numeric label on each playlist item in the playlist
        private void LabelPlaylistNumbers()
        {
            int playlistItemCounter = 1;
            foreach (var item in DisplayedPlaylist.Items)
            {
                item.PlaylistTrackNo = playlistItemCounter;
                playlistItemCounter += 1;
            }
        }

        //save the playlist on reorder and relabel the items
        object m_ReorderItem;
        int m_ReorderIndexFrom;
        private void HandleReorder(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    m_ReorderItem = e.OldItems[0];
                    m_ReorderIndexFrom = e.OldStartingIndex;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                case NotifyCollectionChangedAction.Add:
                    var _ReorderIndexTo = e.NewStartingIndex;
                    LabelPlaylistNumbers();
                    m_ReorderItem = null;
                    break;
            }
           
        }
    }
}
