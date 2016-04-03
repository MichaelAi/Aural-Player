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

namespace Aural.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

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

        private PlaylistItem _previousPlayingItem;
        public PlaylistItem PreviousPlayingItem
        {
            get { return _previousPlayingItem; }
            set { Set("PreviousPlayingItem", ref _previousPlayingItem, value); PreviousPlayingItem_Changed(); }
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

        private ObservableCollection<PlaylistItem> _displayedPlaylist = new ObservableCollection<PlaylistItem>();
        public ObservableCollection<PlaylistItem> DisplayedPlaylist
        {
            get { return _displayedPlaylist; }
            set { Set("DisplayedPlaylist", ref _displayedPlaylist, value); }
        }

        private ObservableCollection<PlaylistItem> _currentPlaylist = new ObservableCollection<PlaylistItem>();
        public ObservableCollection<PlaylistItem> CurrentPlaylist
        {
            get { return _currentPlaylist; }
            set { Set("CurrentPlaylist", ref _currentPlaylist, value); }
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
        public MainViewModel()
        {

            if (MediaElementObject == null)
            {
                MediaElementObject = new MediaElement() { AutoPlay = true, IsLooping = false, AudioCategory = Windows.UI.Xaml.Media.AudioCategory.BackgroundCapableMedia, AreTransportControlsEnabled = true };
            }
            MediaPlayCommand = new RelayCommand<int>((id)=>MediaPlay(id));
            MediaPauseCommand = new RelayCommand(MediaPause);
            MediaStopCommand = new RelayCommand(MediaStop);
            MediaPreviousCommand = new RelayCommand(MediaPrevious);
            MediaNextCommand = new RelayCommand(MediaNext);
            OpenFilesCommand = new RelayCommand(OpenFiles);
            OrderPlaylistCommand = new RelayCommand<string>((mode) => OrderPlaylist(mode));
            SavePlaylistCommand = new RelayCommand(SavePlaylist);
            SetMasterFolderCommand = new RelayCommand(SetMasterFolder);
            DeletePlaylistCommand = new RelayCommand<string>((id) => ShowPlaylistDeletionConfirmation(id));
            EditPlaylistCommand = new RelayCommand<string>((id) => ShowEditPlaylistDialog(id));
            MediaStopAfterCurrentCommand = new RelayCommand(MediaStopAfterCurrent);

            MediaElementObject.TransportControls.IsCompact = true;
            MediaElementObject.TransportControls.IsZoomButtonVisible = false;
            MediaElementObject.TransportControls.IsZoomEnabled = false;
            MediaElementObject.TransportControls.IsPlaybackRateButtonVisible = true;
            MediaElementObject.TransportControls.IsPlaybackRateEnabled = true;

            MediaElementObject.MediaEnded += MediaElement_MediaEnded;
            systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
            InitializeSystemMediaControls();
            SererInitialized = new AutoResetEvent(false);
            ApplicationSettingsHelper.SaveSettingsValue(Constants.AppState, Constants.ForegroundAppActive);
            DisplayedPlaylist.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
            ReadPlaylistsFromFolder();

            Messenger.Default.Register<NotificationMessage<IReadOnlyList<StorageFile>>>(this,
                nm =>
                {
                    if (nm.Notification != null)
                    {
                        if ((string)nm.Notification == "fromDragDrop")
                            PopulatePlayist(nm.Content, false, true,false);
                        else if ((string)nm.Notification == "fromFileOpen")
                            PopulatePlayist(nm.Content, true, false,false);
                    }
                }
                );

            GetAccessTokens();

            SelectedPlaylist = Playlists.FirstOrDefault();
        }

        private bool DisplayedPlaylistHasItemsCheck()
        {
            return DisplayedPlaylistHasItems;
        }
        private void SavePlaylist()
        {
                ButtonShowContentDialog();
        }
        private void TransferPlaylist()
        {
            LabelPlaylistNumbers();
            if (DisplayedPlaylist != null && DisplayedPlaylist.Count > 0)
            {
                DisplayedPlaylistHasItems = true;
                CurrentPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist);
            }
            else
            {
                DisplayedPlaylistHasItems = false;
            }

        }

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
            }

        }
        private async void OrderPlaylist(string mode)
        {
            if (DisplayedPlaylist != null && DisplayedPlaylist.Count > 0)
            {
                DisplayedPlaylist.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
                switch (mode)
                {
                    case ("artist"):
                        if (DisplayedPlaylist.SequenceEqual(DisplayedPlaylist.OrderBy(x => x.Properties.Artist)))
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderByDescending(x => x.Properties.Artist));
                        else
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.Properties.Artist));
                        break;
                    case ("album"):
                        if (DisplayedPlaylist.SequenceEqual(DisplayedPlaylist.OrderBy(x => x.Properties.Album)))
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderByDescending(x => x.Properties.Album));
                        else
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.Properties.Album));
                        break;
                    case ("title"):
                        if (DisplayedPlaylist.SequenceEqual(DisplayedPlaylist.OrderBy(x => x.Properties.Title)))
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderByDescending(x => x.Properties.Title));
                        else
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.Properties.Title));
                        break;
                    case ("year"):
                        if (DisplayedPlaylist.SequenceEqual(DisplayedPlaylist.OrderBy(x => x.Properties.Year)))
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderByDescending(x => x.Properties.Year));
                        else
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.Properties.Year));
                        break;
                    case ("albumartist"):
                        if (DisplayedPlaylist.SequenceEqual(DisplayedPlaylist.OrderBy(x => x.Properties.AlbumArtist)))
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderByDescending(x => x.Properties.AlbumArtist));
                        else
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.Properties.AlbumArtist));
                        break;
                    case ("genre"):
                        if (DisplayedPlaylist.SequenceEqual(DisplayedPlaylist.OrderBy(x => x.Properties.Genre)))
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderByDescending(x => x.Properties.Genre));
                        else
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.Properties.Genre));
                        break;
                    case ("tracknumber"):
                        if (DisplayedPlaylist.SequenceEqual(DisplayedPlaylist.OrderBy(x => x.Properties.TrackNumber)))
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderByDescending(x => x.Properties.TrackNumber));
                        else
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.Properties.TrackNumber));
                        break;
                    case ("rating"):
                        if (DisplayedPlaylist.SequenceEqual(DisplayedPlaylist.OrderBy(x => x.Properties.Rating)))
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderByDescending(x => x.Properties.Rating));
                        else
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.Properties.Rating));
                        break;
                    case ("duration"):
                        if (DisplayedPlaylist.SequenceEqual(DisplayedPlaylist.OrderBy(x => x.Properties.Duration)))
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderByDescending(x => x.Properties.Duration));
                        else
                            DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.Properties.Duration));
                        break;
                    default:
                        break;
                }
                DisplayedPlaylist.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
                if (SelectedPlaylist != null)
                {
                    LabelPlaylistNumbers();
                    HighlightCurrentlyPlayingItem();
                    SelectedPlaylist.Items = new ObservableCollection<PlaylistItem>(DisplayedPlaylist);
                    await WriteToPlaylistFile(SelectedPlaylist.PlaylistName, await PrepareM3uFile(SelectedPlaylist));
                    
                }
                    TransferPlaylist();
            }
        }

        private void LabelPlaylistNumbers()
        {
            int playlistItemCounter = 1;
            foreach (var item in DisplayedPlaylist)
            {
                item.PlaylistTrackNo = playlistItemCounter;
                playlistItemCounter += 1;
            }
        }

        object m_ReorderItem;
        int m_ReorderIndexFrom;

        private async void HandleReorder(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    m_ReorderItem = e.OldItems[0];
                    m_ReorderIndexFrom = e.OldStartingIndex;
                    break;
                case NotifyCollectionChangedAction.Add:
                    if (m_ReorderItem == null)
                        return;
                    var _ReorderIndexTo = e.NewStartingIndex;
                    LabelPlaylistNumbers();
                    await WriteToPlaylistFile(SelectedPlaylist.PlaylistName, await PrepareM3uFile(SelectedPlaylist));
                    
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


        private async void OpenFiles()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();

            // Filter to include a sample subset of file types
            fileOpenPicker.FileTypeFilter.Add(".mp3");
            fileOpenPicker.FileTypeFilter.Add(".wma");
            fileOpenPicker.FileTypeFilter.Add(".flac");
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;

            IReadOnlyList<StorageFile> files = await fileOpenPicker.PickMultipleFilesAsync();

            //add files to playlist
            if (files.Count != 0)
                PopulatePlayist(files, false, false, false);
        }

        public async void PopulatePlayist(IReadOnlyList<StorageFile> files, bool fromOpenedFile, bool fromDragOver, bool fromPlaylist)
        {
            ObservableCollection<PlaylistItem> tempPlaylist = new ObservableCollection<PlaylistItem>();
            DisplayedPlaylist.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
            if (!fromPlaylist)
            {
                 tempPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x=>x.PlaylistTrackNo));
            }

            // Ensure files were selected

            if (files != null)
            {
                foreach (var file in files)
                {
                    Windows.Storage.FileProperties.MusicProperties x = await file.Properties.GetMusicPropertiesAsync();
                    tempPlaylist.Add(new PlaylistItem { Id = Guid.NewGuid(), Properties = x, PlaylistFile = file });
                }

                if (fromOpenedFile)
                {
                    CreateExplorerQueuePlaylist(tempPlaylist);
                    TransferPlaylist();
                    var item = tempPlaylist.LastOrDefault();
                    NowPlayingItem = item;
                }
                else if (fromDragOver)
                {
                    DisplayedPlaylist = new ObservableCollection<PlaylistItem>(tempPlaylist);
                    TransferPlaylist();
                }
                else if (fromPlaylist)
                {
                    int playIndex = Playlists.IndexOf(SelectedPlaylist);
                    if (playIndex > -1)
                    {
                        Playlists[playIndex] = new Playlist { PlaylistId = Guid.NewGuid(), PlaylistName = SelectedPlaylist.PlaylistName, Items = tempPlaylist };
                        SelectedPlaylist = Playlists[playIndex];
                    }
                }
                if (MediaElementObject.CurrentState != Windows.UI.Xaml.Media.MediaElementState.Playing && !fromOpenedFile)
                {
                    
                }
                DisplayedPlaylist = new ObservableCollection<PlaylistItem>(tempPlaylist.OrderBy(x=>x.PlaylistTrackNo));
                LabelPlaylistNumbers();
                HighlightCurrentlyPlayingItem();
                SelectedPlaylist.Items = new ObservableCollection<PlaylistItem>(tempPlaylist);
                await WriteToPlaylistFile(SelectedPlaylist.PlaylistName, await PrepareM3uFile(SelectedPlaylist));
                DisplayedPlaylist.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
            }
        }

        private void CreateExplorerQueuePlaylist(ObservableCollection<PlaylistItem> play)
        {
            if (Playlists.Where(x => x.PlaylistName == "Explorer Queue").Count() == 0)
            {
                var explorerQueue = new Playlist { PlaylistName = "Explorer Queue", PlaylistId = Guid.NewGuid(), Items = play };
                Playlists.Insert(0, explorerQueue);
            }
            SelectedPlaylist = Playlists.Where(x => x.PlaylistName == "Explorer Queue").FirstOrDefault();
        }

        private async void DeleteExplorerQueuePlaylist()
        {
            if (Playlists.Where(x => x.PlaylistName == "Explorer Queue").Count() != 0)
            {
                await DeletePlaylistFile("Explorer Queue");
                DeletePlaylistFromList("Explorer Queue");
            }
        }
        private async void MediaPlay(int id)
        {
            if(id == 0)
            {
                MediaElementObject.Play();

            }
            else
            {
                NowPlayingItem = DisplayedPlaylist.Where(x => x.PlaylistTrackNo == id).FirstOrDefault();
            }
            properties = await TryGetProperties(NowPlayingFile);
            MediaElementObject.SeekCompleted += MediaElementObject_SeekCompleted;
        }

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
        private void MediaElementObject_SeekCompleted(object sender, RoutedEventArgs e)
        {
            if (cancelInProgress)
            {
                MediaStopAfterCurrent();
            }
        }

        private void MediaPause()
        {
            MediaElementObject.Pause();
        }

        private void MediaStop()
        {
            MediaElementObject.Stop();
            cancelInProgress = false;
        }

        private void MediaPrevious()
        {
            int index = CurrentPlaylist.IndexOf(NowPlayingItem);
            if (index > 0 && index > -1)
            {
                NowPlayingItem = CurrentPlaylist.ElementAt(index - 1);
            }
        }

        private void MediaNext()
        {
            int index = CurrentPlaylist.IndexOf(NowPlayingItem);
            if (index < CurrentPlaylist.Count - 1 && index > -1)
            {
                NowPlayingItem = CurrentPlaylist.ElementAt(index + 1);
            }
        }

        //play the next item in the playlist after a song was ended
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaNext();
        }

        private void PreviousPlayingItem_Changed()
        {
            if (PreviousPlayingItem != null)
            {
               // NowPlayingItem = PreviousPlayingItem;
            }
        }

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

        private void SearchPlaylist()
        {
            if (SearchParameter != null && SearchParameter.Length != 0)
            {
                DisplayedPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.Where
                    (x => x.Properties.Title.ToLower().Contains(SearchParameter.ToLower())
                    || x.Properties.Title.ToLower().Contains(SearchParameter.ToLower())
                    || x.Properties.Title.ToLower().Contains(SearchParameter.ToLower())
                    ));

            }
            else
            {
                DisplayedPlaylist = CurrentPlaylist;
            }
        }

        private async void ButtonShowContentDialog()
        {
            var dialog = new ContentDialog()
            {
                Title = "New Playlist",
            };

            var panel = new StackPanel();
            var tb = new TextBox
            {
                PlaceholderText = "Enter the new playlist's name",
                Margin = new Windows.UI.Xaml.Thickness { Top = 10 }
            };
            panel.Children.Add(tb);
            dialog.Content = panel;

            dialog.PrimaryButtonText = "Save";
            var cmd = new RelayCommand(async () =>
            {
                Playlists.Add(new Playlist { Items = CurrentPlaylist, PlaylistId = Guid.NewGuid(), PlaylistName = tb.Text });
                var TempSelectedPlaylist = Playlists.Where(x => x.PlaylistName == tb.Text).FirstOrDefault();
                HasNoPlaylists = false;
                string contents = await PrepareM3uFile(new Playlist());
                await WriteToPlaylistFile(TempSelectedPlaylist.PlaylistName, contents);
                SelectedPlaylist = TempSelectedPlaylist;
            }, () => CanSave(tb.Text));

            dialog.IsPrimaryButtonEnabled = false;
            dialog.PrimaryButtonCommand = cmd;

            dialog.SecondaryButtonText = "Cancel";
            dialog.SecondaryButtonCommand = new RelayCommand(() =>
            {
                //nothing to do here
            });

            tb.TextChanged += delegate
            {
                cmd.RaiseCanExecuteChanged();
                if (tb.Text.Trim().Length > 0)
                {
                    dialog.IsPrimaryButtonEnabled = true;
                }
                else
                {
                    dialog.IsPrimaryButtonEnabled = false;
                }

            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.None)
            {
                //sdfdfs
            }
        }

        private bool CanSave(string parameter)
        {
            if (parameter != "")
            {
                return true;
            }
            return false;
        }

        private async void ShowEditPlaylistDialog(string oldPlaylistName)
        {
            var dialog = new ContentDialog()
            {
                Title = "Edit Playlist",
            };

            var panel = new StackPanel();
            var tb = new TextBox
            {
                PlaceholderText = "Enter the playlist's new name",
                Margin = new Windows.UI.Xaml.Thickness { Top = 10 }
            };
            panel.Children.Add(tb);
            dialog.Content = panel;

            dialog.PrimaryButtonText = "Save";
            var cmd = new RelayCommand(async () =>
            {
                //add the new playlist
                int index = Playlists.IndexOf(Playlists.Where(x => x.PlaylistName == oldPlaylistName).FirstOrDefault());
                var TempSelectedPlaylist = Playlists.Where(x => x.PlaylistName == oldPlaylistName).FirstOrDefault();



                Playlists.Insert(index, new Playlist { Items = new ObservableCollection<PlaylistItem>(TempSelectedPlaylist.Items), PlaylistId = Guid.NewGuid(), PlaylistName = tb.Text });
                var y = new ObservableCollection<PlaylistItem>(await ReturnPlaylistInBackgorund(TempSelectedPlaylist));
                HasNoPlaylists = false;
                string contents = await PrepareM3uFile(new Playlist { Items = y });
                await WriteToPlaylistFile(tb.Text, contents);

                //delete the old
                await DeletePlaylistFile(oldPlaylistName);
                DeletePlaylistFromList(oldPlaylistName);
            }, () => CanSave(tb.Text));

            dialog.IsPrimaryButtonEnabled = false;
            dialog.PrimaryButtonCommand = cmd;

            dialog.SecondaryButtonText = "Cancel";
            dialog.SecondaryButtonCommand = new RelayCommand(() =>
            {
                //nothing to do here
            });

            tb.TextChanged += delegate
            {
                cmd.RaiseCanExecuteChanged();
                if (tb.Text.Trim().Length > 0)
                {
                    dialog.IsPrimaryButtonEnabled = true;
                }
                else
                {
                    dialog.IsPrimaryButtonEnabled = false;
                }

            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.None)
            {
                //sdfdfs
            }
        }

        private async Task<string> PrepareM3uFile(Playlist playlistToSave)
        {
            string contents = "";
            contents += "#EXTM3U";
            contents += "\r\n";
            foreach (var item in playlistToSave.Items.OrderBy(x => x.PlaylistTrackNo))
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

        private async Task<bool> WriteToPlaylistFile(string playlistName, string contents)
        {
            try
            {

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFolder playlistsFolder = await storageFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);
                StorageFile playlist = await playlistsFolder.CreateFileAsync(playlistName + ".m3u",
                        CreationCollisionOption.ReplaceExisting);


                var stream = await playlist.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                using (var outputStream = stream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                    {
                        dataWriter.WriteString(contents);
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

        private async void ReadPlaylistsFromFolder()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFolder playlistsFolder = await storageFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);
            foreach (var item in await playlistsFolder.GetItemsAsync())
            {
                Playlists.Add(new Playlist { PlaylistName = item.Name.Substring(0, item.Name.LastIndexOf(".")) });
            }

            if (Playlists.Count > 0)
            {
                HasNoPlaylists = false;
            }

            DeleteExplorerQueuePlaylist();
        }

        private async Task<ObservableCollection<PlaylistItem>> ReturnPlaylistInBackgorund(Playlist play)
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
            var tempPlaylist = new ObservableCollection<PlaylistItem>(DisplayedPlaylist.OrderBy(x => x.PlaylistTrackNo));
            return tempPlaylist;
        }
        private async Task ReadPlaylistFromFile(Playlist play)
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
            PopulatePlayist(items, false, false, true);
        }

        private async void ShowPlaylistDeletionConfirmation(string playlistName)
        {
            if (!DisableDeletionConfirmations)
            {
                var dialog = new ContentDialog()
                {
                    Title = "Are you sure?",
                };

                var panel = new StackPanel();
                var tb = new TextBlock
                {
                    Text = "Deleting a playlist cannot be undone. Are you sure you want to proceed?",
                };

                var cb = new CheckBox
                {
                    Content = "Disable confirmations for this session",
                    Margin = new Windows.UI.Xaml.Thickness { Top = 10 }
                };

                panel.Children.Add(tb);
                panel.Children.Add(cb);
                dialog.Content = panel;

                dialog.PrimaryButtonText = "Delete";
                var cmd = new RelayCommand(async () =>
                {
                    await DeletePlaylistFile(playlistName);
                    DeletePlaylistFromList(playlistName);
                    var disable = cb.IsChecked ?? false;
                    if (disable)
                        DisableDeletionConfirmations = true;
                });

                dialog.PrimaryButtonCommand = cmd;

                dialog.SecondaryButtonText = "Cancel";
                dialog.SecondaryButtonCommand = new RelayCommand(() =>
                {
                //nothing to do here
            });


                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.None)
                {
                    //nothing to do here
                }
            }
            else
            {
                await DeletePlaylistFile(playlistName);
                DeletePlaylistFromList(playlistName);
            }
        }

        private async Task DeletePlaylistFile(string playlistName)
        {
            try {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFolder playlistsFolder = await storageFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);
                StorageFile playlistFile = await playlistsFolder.GetFileAsync(playlistName + ".m3u");
                await playlistFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch
            {

            }
        }

        private void DeletePlaylistFromList(string playlistName)
        {
            var tempPlaylist = Playlists.Where(x => x.PlaylistName == playlistName).FirstOrDefault();
            if (tempPlaylist.Items == DisplayedPlaylist)
                DisplayedPlaylist.Clear();
            Playlists.Remove(tempPlaylist);
            SelectedPlaylist = null;
        }


        private async void SelectedPlaylistChanged()
        {
            if (SelectedPlaylist != null)
            {
                if (SelectedPlaylist.PlaylistId != Guid.Empty || SelectedPlaylist.Items.Count > 0)
                {
                    DisplayedPlaylist = SelectedPlaylist.Items;
                    
                    HighlightCurrentlyPlayingItem();
                }
                else
                    await ReadPlaylistFromFile(SelectedPlaylist);
                DisplayedPlaylist.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleReorder);
            }

        }

        private void HighlightCurrentlyPlayingItem()
        {
            if(NowPlayingItem != null)
            {
                SelectedDisplayedItem = DisplayedPlaylist.Where(x => x.Id == NowPlayingItem.Id).FirstOrDefault();
            }
        }

        private void GetAccessTokens()
        {
            //Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Clear();
            var tokens = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Entries;
            AccessTokens.Clear();
            foreach (var token in tokens)
            {
                AccessTokens.Add(token.Metadata);
            }
        }
        private async void SetMasterFolder()
        {
            FolderPicker picker = new FolderPicker();
            //a filter is needed despite being a folder picker
            picker.FileTypeFilter.Add(".mp3");
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            StorageFolder masterFolder = await picker.PickSingleFolderAsync();
            // Store the folder to access again later
            if (masterFolder != null)
            {
                var listToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(masterFolder,masterFolder.Path);
                GetAccessTokens();
            }
        }
    }
}
