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

        private ObservableCollection<PlaylistItem> _displayedPlaylistItems = new ObservableCollection<PlaylistItem>();
        public ObservableCollection<PlaylistItem> DisplayedPlaylistItems
        {
            get { return _displayedPlaylistItems; }
            set { Set("DisplayedPlaylistItems", ref _displayedPlaylistItems, value); }
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

        public PlaylistViewModel(IFileIOService fileIOService)
        {
            this.fileIOService = fileIOService;
            OrderPlaylistCommand = new RelayCommand<string>((mode) => OrderPlaylist(mode));
            DisplayedPlaylistItems.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
            Startup();
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
                DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(unfilteredPlaylist.Where
                    (x => x.Properties.Title.ToLower().Contains(SearchParameter.ToLower())
                    || x.Properties.Artist.ToLower().Contains(SearchParameter.ToLower())
                    || x.Properties.Album.ToLower().Contains(SearchParameter.ToLower())
                    || x.Properties.AlbumArtist.ToLower().Contains(SearchParameter.ToLower())
                    ));

            }
            else
            {
                DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(unfilteredPlaylist);
            }
        }

        private void TransferPlaylist()
        {
            LabelPlaylistNumbers();
            if (DisplayedPlaylistItems != null && DisplayedPlaylistItems.Count > 0)
            {
                DisplayedPlaylistHasItems = true;
                Messenger.Default.Send(new NotificationMessage<ObservableCollection<PlaylistItem>>(DisplayedPlaylistItems, "DisplayToCurrent"));
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
                    DisplayedPlaylistItems.Add(item);
                }
                SelectedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems);
                Messenger.Default.Send(new NotificationMessage<ObservableCollection<PlaylistItem>>(DisplayedPlaylistItems, "DisplayToDisplay"));
                IsPlaylistLoadInProgress = false;
            }
        }

        //clear the displayed playlist
        private void ClearDisplayedPlaylist()
        {
            DisplayedPlaylistItems.Clear();
        }

        //highlight the current item
        private void HighlightCurrentlyPlayingItem()
        {
            if (NowPlayingItem != null)
            {
                SelectedDisplayedItem = DisplayedPlaylistItems.Where(x => x.Id == NowPlayingItem.Id).FirstOrDefault();
            }
        }

        private void NowPlayingItem_Changed()
        {

        }

        //Handle the order event, that is order the playlist items by the given parameter
        private void OrderPlaylist(string mode)
        {
            if (DisplayedPlaylistItems != null && DisplayedPlaylistItems.Count > 0)
            {
                DisplayedPlaylistItems.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
                switch (mode)
                {
                    case ("artist"):
                        if (DisplayedPlaylistItems.SequenceEqual(DisplayedPlaylistItems.OrderBy(x => x.Properties.Artist)))
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderByDescending(x => x.Properties.Artist));
                        else
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderBy(x => x.Properties.Artist));
                        break;
                    case ("album"):
                        if (DisplayedPlaylistItems.SequenceEqual(DisplayedPlaylistItems.OrderBy(x => x.Properties.Album)))
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderByDescending(x => x.Properties.Album));
                        else
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderBy(x => x.Properties.Album));
                        break;
                    case ("title"):
                        if (DisplayedPlaylistItems.SequenceEqual(DisplayedPlaylistItems.OrderBy(x => x.Properties.Title)))
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderByDescending(x => x.Properties.Title));
                        else
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderBy(x => x.Properties.Title));
                        break;
                    case ("year"):
                        if (DisplayedPlaylistItems.SequenceEqual(DisplayedPlaylistItems.OrderBy(x => x.Properties.Year)))
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderByDescending(x => x.Properties.Year));
                        else
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderBy(x => x.Properties.Year));
                        break;
                    case ("albumartist"):
                        if (DisplayedPlaylistItems.SequenceEqual(DisplayedPlaylistItems.OrderBy(x => x.Properties.AlbumArtist)))
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderByDescending(x => x.Properties.AlbumArtist));
                        else
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderBy(x => x.Properties.AlbumArtist));
                        break;
                    case ("genre"):
                        if (DisplayedPlaylistItems.SequenceEqual(DisplayedPlaylistItems.OrderBy(x => x.Properties.Genre)))
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderByDescending(x => x.Properties.Genre));
                        else
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderBy(x => x.Properties.Genre));
                        break;
                    case ("tracknumber"):
                        if (DisplayedPlaylistItems.SequenceEqual(DisplayedPlaylistItems.OrderBy(x => x.Properties.TrackNumber)))
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderByDescending(x => x.Properties.TrackNumber));
                        else
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderBy(x => x.Properties.TrackNumber));
                        break;
                    case ("rating"):
                        if (DisplayedPlaylistItems.SequenceEqual(DisplayedPlaylistItems.OrderBy(x => x.Properties.Rating)))
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderByDescending(x => x.Properties.Rating));
                        else
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderBy(x => x.Properties.Rating));
                        break;
                    case ("duration"):
                        if (DisplayedPlaylistItems.SequenceEqual(DisplayedPlaylistItems.OrderBy(x => x.Properties.Duration)))
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderByDescending(x => x.Properties.Duration));
                        else
                            DisplayedPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems.OrderBy(x => x.Properties.Duration));
                        break;
                    default:
                        break;
                }
                DisplayedPlaylistItems.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
                if (SelectedPlaylist != null)
                {
                    LabelPlaylistNumbers();
                    HighlightCurrentlyPlayingItem();
                    SelectedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems);
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
            foreach (var item in DisplayedPlaylistItems)
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
