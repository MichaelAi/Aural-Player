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

namespace Aural.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region Private Fields and Properties
        private AutoResetEvent SererInitialized;
        private bool isMyBackgroundTaskRunning = false;

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

        public RelayCommand MediaPlayCommand { get; private set; }
        public RelayCommand MediaPauseCommand { get; private set; }
        public RelayCommand MediaStopCommand { get; private set; }
        public RelayCommand MediaPreviousCommand { get; private set; }
        public RelayCommand MediaNextCommand { get; private set; }
        public RelayCommand OpenFilesCommand { get; private set; }
        public RelayCommand<string> OrderPlaylistCommand { get; private set; }

        public MainViewModel()
        {

            if (MediaElementObject == null)
            {
                MediaElementObject = new MediaElement() { AutoPlay = true, IsLooping = false, AudioCategory = Windows.UI.Xaml.Media.AudioCategory.BackgroundCapableMedia, AreTransportControlsEnabled = true };
            }
            MediaPlayCommand = new RelayCommand(MediaPlay);
            MediaPauseCommand = new RelayCommand(MediaPause);
            MediaStopCommand = new RelayCommand(MediaStop);
            MediaPreviousCommand = new RelayCommand(MediaPrevious);
            MediaNextCommand = new RelayCommand(MediaNext);
            OpenFilesCommand = new RelayCommand(OpenFiles);
            OrderPlaylistCommand = new RelayCommand<string>((mode) => OrderPlaylist(mode));

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
            Messenger.Default.Register<IReadOnlyList<StorageFile>>(this, (x) => PopulatePlayist(x, true));
        }

        private void TransferPlaylist()
        {
            CurrentPlaylist = DisplayedPlaylist;
        }

        private void OrderPlaylist(string mode)
        {
            if (DisplayedPlaylist != null && DisplayedPlaylist.Count > 0)
            {
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
                TransferPlaylist();
            }
        }

        private void HandleReorder(object sender, NotifyCollectionChangedEventArgs e)
        {

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
                            MediaPlay();
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
                            MediaPlay();
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
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;

            IReadOnlyList<StorageFile> files = await fileOpenPicker.PickMultipleFilesAsync();

            //add files to playlist
            if (files.Count != 0)
                PopulatePlayist(files, false);
        }

        public async void PopulatePlayist(IReadOnlyList<StorageFile> files, bool fromOpenedFile)
        {
            // Ensure files were selected
            if (files != null)
            {
                foreach (var file in files)
                {
                    Windows.Storage.FileProperties.MusicProperties x = await file.Properties.GetMusicPropertiesAsync();
                    DisplayedPlaylist.Add(new PlaylistItem { Id = Guid.NewGuid(), Properties = x, PlaylistFile = file });
                }
                TransferPlaylist();
                if (fromOpenedFile)
                {
                    NowPlayingItem = CurrentPlaylist.Last();
                }
                if (MediaElementObject.CurrentState != Windows.UI.Xaml.Media.MediaElementState.Playing && !fromOpenedFile)
                {
                    NowPlayingItem = CurrentPlaylist.First();
                }
            }
        }


        private void MediaPlay()
        {
            MediaElementObject.Play();
        }

        private void MediaPause()
        {
            MediaElementObject.Pause();
        }

        private void MediaStop()
        {
            MediaElementObject.Stop();
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
                NowPlayingItem = PreviousPlayingItem;
            }
        }

        private async void NowPlayingItem_Changed()
        {
            if (NowPlayingItem != null)
            {
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
                MediaPlay();
                GetAlbumArt();

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

            }else
            {
                DisplayedPlaylist = CurrentPlaylist;
            }
        }

    }
}
