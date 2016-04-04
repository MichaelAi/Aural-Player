using GalaSoft.MvvmLight.Command;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using System;
using Windows.UI.Xaml;
using Aural.Model;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Aural.Helpers;
using System.Threading;
using Windows.ApplicationModel.Background;
using System.Collections.Specialized;
using Aural;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using Windows.ApplicationModel.Activation;
using System.Text.RegularExpressions;
using Aural.Interface;

namespace Aural.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        ISettingsService settingsService;
        IFileIOService fileIOService;
        IContentDialogService contentDialogService;
        #region Private Fields and Properties
        private AutoResetEvent SererInitialized;
        private bool isMyBackgroundTaskRunning = false;
        private bool DisableDeletionConfirmations = false;

        /// <summary>
        /// Gets the information about background task is running or not by reading the setting saved by background task
        /// </summary>
        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                if (isMyBackgroundTaskRunning)
                    return true;

                object value = ApplicationSettingsHelper.ReadResetSettingsValue(Constants.BackgroundTaskState);
                if (value == null)
                {
                    return false;
                }
                else
                {
                    isMyBackgroundTaskRunning = ((String)value).Equals(Constants.BackgroundTaskRunning);
                    return isMyBackgroundTaskRunning;
                }
            }
        }

        /// <summary>
        /// Read current track information from application settings
        /// </summary>
        private string CurrentTrack
        {
            get
            {
                object value = ApplicationSettingsHelper.ReadResetSettingsValue(Constants.CurrentTrack);
                if (value != null)
                {
                    return (String)value;
                }
                else
                    return String.Empty;
            }
        }
        #endregion

        SystemMediaTransportControls systemMediaControls = null;

        private StorageFile _nowPlayingFile;
        public StorageFile NowPlayingFile
        {
            get { return _nowPlayingFile; }
            set { Set("NowPlayingFile", ref _nowPlayingFile, value); }
        }

        private PlaylistItem _nowPlayingItem;
        public PlaylistItem NowPlayingItem
        {
            get { return _nowPlayingItem; }
            set { Set("NowPlayingItem", ref _nowPlayingItem, value); NowPlayingItem_Changed(); }
        }

        private double _nowPlayingMaxDuration;
        public double NowPlayingMaxDuration
        {
            get { return _nowPlayingMaxDuration; }
            set { Set("NowPlayingMaxDuration", ref _nowPlayingMaxDuration, value); }
        }

        private TimeSpan _totalTime;
        public TimeSpan TotalTime
        {
            get { return _totalTime; }
            set { Set("TotalTime", ref _totalTime, value); }
        }

        private double _currentPosition;
        public double CurrentPosition
        {
            get { return _currentPosition; }
            set { Set("CurrentPosition", ref _currentPosition, value); }
        }

        private MediaElement _mediaElementObject;
        public MediaElement MediaElementObject
        {
            get { return _mediaElementObject; }
            set { Set("MediaElementObject", ref _mediaElementObject, value); }
        }

        private IRandomAccessStream _nowPlayingStream;
        public IRandomAccessStream NowPlayingStream
        {
            get { return _nowPlayingStream; }
            set { Set("NowPlayingStream", ref _nowPlayingStream, value); }
        }

        private ObservableCollection<PlaylistItem> _displayedPlaylistItems = new ObservableCollection<PlaylistItem>();
        public ObservableCollection<PlaylistItem> DisplayedPlaylistItems
        {
            get { return _displayedPlaylistItems; }
            set { Set("DisplayedPlaylistItems", ref _displayedPlaylistItems, value); }
        }

        private ObservableCollection<PlaylistItem> _currentPlaylistItems = new ObservableCollection<PlaylistItem>();
        public ObservableCollection<PlaylistItem> CurrentPlaylistItems
        {
            get { return _currentPlaylistItems; }
            set { Set("CurrentPlaylistItems", ref _currentPlaylistItems, value); }
        }

        private string _searchParameter = "";
        public string SearchParameter
        {
            get { return _searchParameter; }
            set { Set("SearchParameter", ref _searchParameter, value); SearchPlaylist(); }
        }

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

        private bool _displayedPlaylistHasItems = false;
        public bool DisplayedPlaylistHasItems
        {
            get { return _displayedPlaylistHasItems; }
            set { Set("DisplayedPlaylistHasItems", ref _displayedPlaylistHasItems, value); SavePlaylistCommand.RaiseCanExecuteChanged(); }
        }

        private Playlist _selectedPlaylist = new Playlist();
        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set { Set("SelectedPlaylist", ref _selectedPlaylist, value); SelectedPlaylistChanged(); }
        }

        private ObservableCollection<string> _accessTokens = new ObservableCollection<string>();
        public ObservableCollection<string> AccessTokens
        {
            get { return _accessTokens; }
            set { Set("AccessTokens", ref _accessTokens, value); }
        }

        private PlaylistItem _selectedDisplayedItem = new PlaylistItem();
        public PlaylistItem SelectedDisplayedItem
        {
            get { return _selectedDisplayedItem; }
            set { Set("SelectedDisplayedItem", ref _selectedDisplayedItem, value); }
        }

        CancellationTokenSource cts;
        private bool cancelInProgress = false;
        private MusicProperties properties;
        private ObservableCollection<PlaylistItem> unfilteredPlaylist = new ObservableCollection<PlaylistItem>();

        public RelayCommand<int> MediaPlayCommand { get; private set; }
        public RelayCommand MediaPauseCommand { get; private set; }
        public RelayCommand MediaStopCommand { get; private set; }
        public RelayCommand MediaPreviousCommand { get; private set; }
        public RelayCommand MediaNextCommand { get; private set; }
        public RelayCommand OpenFilesCommand { get; private set; }
        public RelayCommand<string> OrderPlaylistCommand { get; private set; }
        public RelayCommand SavePlaylistCommand { get; private set; }
        public RelayCommand SetMasterFolderCommand { get; private set; }
        public RelayCommand<string> DeletePlaylistCommand { get; private set; }
        public RelayCommand<string> EditPlaylistCommand { get; private set; }
        public RelayCommand MediaStopAfterCurrentCommand { get; private set; }

        public MainViewModel(ISettingsService settingsService, IFileIOService fileIOService, IContentDialogService contentDialogService)
        {
            this.settingsService = settingsService;
            this.fileIOService = fileIOService;
            this.contentDialogService = contentDialogService;
            Startup();
            // SererInitialized = new AutoResetEvent(false);
            // ApplicationSettingsHelper.SaveSettingsValue(Constants.AppState, Constants.ForegroundAppActive);
        }

        //create the mediaelementobject
        private void InitializeMediaObject()
        {
            if (MediaElementObject == null)
            {
                MediaElementObject = new MediaElement() { AutoPlay = true, IsLooping = false, AudioCategory = Windows.UI.Xaml.Media.AudioCategory.BackgroundCapableMedia, AreTransportControlsEnabled = true };
            }
            MediaElementObject.TransportControls.IsCompact = true;
            MediaElementObject.TransportControls.IsZoomButtonVisible = false;
            MediaElementObject.TransportControls.IsZoomEnabled = false;
            MediaElementObject.TransportControls.IsPlaybackRateButtonVisible = true;
            MediaElementObject.TransportControls.IsPlaybackRateEnabled = true;
            MediaElementObject.MediaEnded += MediaElement_MediaEnded;
            systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
        }

        //initialize the commands
        private void RegisterCommands()
        {
            MediaPlayCommand = new RelayCommand<int>((id) => MediaPlay(id));
            MediaPauseCommand = new RelayCommand(MediaPause);
            MediaStopCommand = new RelayCommand(MediaStop);
            MediaPreviousCommand = new RelayCommand(MediaPrevious);
            MediaNextCommand = new RelayCommand(MediaNext);
            OpenFilesCommand = new RelayCommand(OpenFiles);
            OrderPlaylistCommand = new RelayCommand<string>((mode) => OrderPlaylist(mode));
            SavePlaylistCommand = new RelayCommand(CreatePlaylist);
            SetMasterFolderCommand = new RelayCommand(SetMasterFolder);
            DeletePlaylistCommand = new RelayCommand<string>((id) => PrepareDeleteFile(id));
            EditPlaylistCommand = new RelayCommand<string>((id) => PrepareEditFile(id));
            MediaStopAfterCurrentCommand = new RelayCommand(MediaStopAfterCurrent);
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

        //set up the mvvmlight messaging service
        private void RegisterMessaging()
        {
            Messenger.Default.Register<NotificationMessage<IReadOnlyList<StorageFile>>>(this,
                async nm =>
                {
                    if (nm.Notification != null)
                    {
                        if ((string)nm.Notification == "fromDragDrop")
                        {
                            var play = await AcceptFiles(nm.Content);
                            PopulatePlaylist(play);
                        }
                        else if ((string)nm.Notification == "fromFileOpen")
                        {
                            var play = await AcceptFiles(nm.Content);
                            PopulatePlaylist(play);
                        }
                    }
                }
                );
        }

        //Do app launch operations
        private async void Startup()
        {
            RegisterMessaging();
            RegisterCommands();
            InitializeMediaObject();
            InitializeSystemMediaControls();     
            AccessTokens = settingsService.GetAccessTokens();
            await LoadPlaylists();

            DisplayedPlaylistItems.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
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

        //Set a new master folder
        //TODO: move to settings viewmodel
        private void SetMasterFolder()
        {
            settingsService.SetMasterFolder();
            AccessTokens = settingsService.GetAccessTokens();
        }


        private bool DisplayedPlaylistHasItemsCheck()
        {
            return DisplayedPlaylistHasItems;
        }

        //Create a new playlist button handler
        private async void CreatePlaylist()
        {
            Playlist play = await contentDialogService.CreateNewPlaylist();
            if(play != null)
                Playlists.Add(play);
        }

        //save a playlist in thebackground
        private async void SavePlaylist(Playlist play)
        {
            bool success = await fileIOService.WritePlaylistFile(play);
        }

        private void TransferPlaylist()
        {
            LabelPlaylistNumbers();
            if (DisplayedPlaylistItems != null && DisplayedPlaylistItems.Count > 0)
            {
                DisplayedPlaylistHasItems = true;
                CurrentPlaylistItems = new ObservableCollection<PlaylistItem>(DisplayedPlaylistItems);
            }
            else
            {
                DisplayedPlaylistHasItems = false;
            }

        }

        //Set a timer to stop the playback after this track has ended.
        //the timer changes on seek
        private async void MediaStopAfterCurrent()
        {
            try
            {
                if (cancelInProgress == true)
                    CancelStopAfterCurrent();
                else
                    cancelInProgress = true;
                cts = new CancellationTokenSource();
                Debug.WriteLine(properties.Duration - MediaElementObject.Position);
                await Task.Delay(properties.Duration - MediaElementObject.Position - TimeSpan.FromMilliseconds(100), cts.Token);
                MediaStop();
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Stop canceled");
            }

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
                    TransferPlaylist();
            }
        }

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

        //link the player to the system media controls
        private void InitializeSystemMediaControls()
        {
            systemMediaControls.ButtonPressed += SystemMediaControls_ButtonPressed;
            systemMediaControls.IsPlayEnabled = true;
            systemMediaControls.IsPauseEnabled = true;
            systemMediaControls.IsStopEnabled = true;
            systemMediaControls.IsNextEnabled = true;
            systemMediaControls.IsPreviousEnabled = true;
        }

        //handle button presses of the system media controls
        private async void SystemMediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            switch (e.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (MediaElementObject.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Paused || MediaElementObject.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Stopped)
                        {
                            MediaPlay(0);
                        }
                        else
                        {
                            MediaPause();
                        }
                    });
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (MediaElementObject.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Paused || MediaElementObject.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Stopped)
                        {
                            MediaPlay(0);
                        }
                        else
                        {
                            MediaPause();
                        }
                    });
                    break;

                case SystemMediaTransportControlsButton.Stop:
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        MediaStop();
                    });
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        MediaPrevious();
                    });
                    break;
                case SystemMediaTransportControlsButton.Next:
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        MediaNext();
                    });
                    break;
                default:
                    break;
            }
        }

        //select files to play through a play picker
        private async void OpenFiles()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            // Filter to include a sample subset of file types
            fileOpenPicker.FileTypeFilter.Add(".mp3");
            fileOpenPicker.FileTypeFilter.Add(".wma");
            fileOpenPicker.FileTypeFilter.Add(".flac");
            fileOpenPicker.FileTypeFilter.Add(".m4a");
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            IReadOnlyList<StorageFile> files = await fileOpenPicker.PickMultipleFilesAsync();
            //add files to playlist
            if (files.Count != 0)
                    await AcceptFiles(files);
        }

        //fill the currently selected playlist
        public void PopulatePlaylist(ObservableCollection<PlaylistItem> items)
        {
            if(SelectedPlaylist != null)
            {
                foreach(var item in items)
                {
                    DisplayedPlaylistItems.Add(item);
                }
                SavePlaylist(SelectedPlaylist);
            }
        }


        //accept the files from the various sources and read them
        public async Task<ObservableCollection<PlaylistItem>> AcceptFiles(IReadOnlyList<StorageFile> files)
        {
            return await fileIOService.LoadFiles(files);
        }
        
        //play
        private async void MediaPlay(int id)
        {
            if(id == 0)
            {
                MediaElementObject.Play();
            }
            else
            {
                NowPlayingItem = DisplayedPlaylistItems.Where(x => x.PlaylistTrackNo == id).FirstOrDefault();
            }
            properties = await TryGetProperties(NowPlayingFile);
            MediaElementObject.SeekCompleted += MediaElementObject_SeekCompleted;
        }

        //attempt to get the properties of the current track
        private async Task<MusicProperties> TryGetProperties(StorageFile file)
        {
            MusicProperties mproperties;
            try
            {
                mproperties = await NowPlayingFile.Properties.GetMusicPropertiesAsync();
            }
            catch
            {
                mproperties = null;
            }
            return mproperties;
        }

        //reset the timer on cancel after current operation
        private void MediaElementObject_SeekCompleted(object sender, RoutedEventArgs e)
        {
            if (cancelInProgress)
            {
                MediaStopAfterCurrent();
            }
        }

        //pause
        private void MediaPause()
        {
            MediaElementObject.Pause();
        }

        //stop
        private void MediaStop()
        {
            MediaElementObject.Stop();
            cancelInProgress = false;
        }

        //previous
        private void MediaPrevious()
        {
            int index = CurrentPlaylistItems.IndexOf(NowPlayingItem);
            if (index > CurrentPlaylistItems.Count - 1 && index > -1)
            {
                NowPlayingItem = CurrentPlaylistItems.ElementAt(index - 1);
            }
        }

        //next
        private void MediaNext()
        {
            int index = CurrentPlaylistItems.IndexOf(NowPlayingItem);
            if (index < CurrentPlaylistItems.Count - 1 && index > -1)
            {
                NowPlayingItem = CurrentPlaylistItems.ElementAt(index + 1);
            }
        }

        //play the next item in the playlist after a song was ended
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaNext();
        }

        //in case the user changes song or manually cancels  the stop, cancel the stop
        private void CancelStopAfterCurrent()
        {
            if(cts != null)
            {
                cts.Cancel();
            }            
        }

        private async void NowPlayingItem_Changed()
        {
            if (NowPlayingItem != null)
            {
                CancelStopAfterCurrent();
                cancelInProgress = false;
                // Open the selected file and set it as the MediaElement's source
                NowPlayingFile = NowPlayingItem.PlaylistFile;
                MediaPlaybackType mediaPlaybackType = MediaPlaybackType.Music;

                // Inform the system transport controls of the media information
                if (!(await systemMediaControls.DisplayUpdater.CopyFromFileAsync(mediaPlaybackType, NowPlayingFile)))
                {
                    //  Problem extracting metadata- just clear everything
                    systemMediaControls.DisplayUpdater.ClearAll();
                }
                systemMediaControls.DisplayUpdater.Update();
                NowPlayingStream = await NowPlayingFile.OpenAsync(FileAccessMode.Read);
                MediaStop();
                MediaElementObject.SetSource(NowPlayingStream, NowPlayingFile.FileType);
                MediaPlay(0);
                GetAlbumArt();
                TransferPlaylist();
            }
        }

        //Get album art
        //TODO: get album art from files in the folder
        private async void GetAlbumArt()
        {
            using (StorageItemThumbnail thumbnail = await NowPlayingItem.PlaylistFile.GetThumbnailAsync(ThumbnailMode.MusicView, 60))
            {
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                    if (bitmapImage.PixelHeight != 0)
                    {
                        NowPlayingItem.AlbumArt.Source = bitmapImage;
                    }
                }
                else
                {
                    Debug.WriteLine("Could not open thumbnail");
                }
            }
        }

        //filter the playlist using the given term
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
      
        //remove the playlist from the playlists list
        private void DeletePlaylistFromList(string playlistName)
        {
            var tempPlaylist = Playlists.Where(x => x.PlaylistName == playlistName).FirstOrDefault();
            if (tempPlaylist.Items == DisplayedPlaylistItems)
                DisplayedPlaylistItems.Clear();
            Playlists.Remove(tempPlaylist);
            SelectedPlaylist = null;
        }


        private async void SelectedPlaylistChanged()
        {
            if(SelectedPlaylist.PlaylistId == Guid.Empty)
            {
                SelectedPlaylist.PlaylistId = Guid.NewGuid();
                SelectedPlaylist.Items = new ObservableCollection<PlaylistItem>(await AcceptFiles(await fileIOService.ReadPlaylistFile(SelectedPlaylist)));
            }
            ClearDisplayedPlaylist();
            PopulatePlaylist(SelectedPlaylist.Items);
        }

        private void ClearDisplayedPlaylist()
        {
            DisplayedPlaylistItems.Clear();
        }


        //highlight the current item
        private void HighlightCurrentlyPlayingItem()
        {
            if(NowPlayingItem != null)
            {
                SelectedDisplayedItem = DisplayedPlaylistItems.Where(x => x.Id == NowPlayingItem.Id).FirstOrDefault();
            }
        }

      
     
    }
}
