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

        public RelayCommand OpenFilesCommand { get; private set; }      

        public MainViewModel(ISettingsService settingsService, IFileIOService fileIOService, IContentDialogService contentDialogService)
        {
            this.settingsService = settingsService;
            this.fileIOService = fileIOService;
            this.contentDialogService = contentDialogService;
            //Do app launch operations
            RegisterMessaging();
            RegisterCommands();

        }

        //initialize the commands
        private void RegisterCommands()
        {
            OpenFilesCommand = new RelayCommand(OpenFiles);

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
                            PopulatePlaylist(play, "fromDragDrop");
                        }
                        else if ((string)nm.Notification == "fromFileOpen")
                        {
                            var play = await AcceptFiles(nm.Content);
                            PopulatePlaylist(play, "fromFileOpen");
                        }
                    }
                }
                );
        }

        private void PopulatePlaylist(ObservableCollection<PlaylistItem> play, string msg)
        {
            Messenger.Default.Send(new NotificationMessage<ObservableCollection<PlaylistItem>>(play, msg));
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

        //accept the files from the various sources and read them
        public async Task<ObservableCollection<PlaylistItem>> AcceptFiles(IReadOnlyList<StorageFile> files)
        {
            return await fileIOService.LoadFiles(files);
        }
    }
}