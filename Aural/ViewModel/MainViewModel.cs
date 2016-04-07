using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.Storage;
using System;
using Aural.Model;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Aural;
using Aural.Interface;
using Windows.UI.Xaml;

namespace Aural.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        ISettingsService settingsService;
        IFileIOService fileIOService;
        IContentDialogService contentDialogService;

        public RelayCommand OpenFilesCommand { get; private set; }
        public static ElementTheme RequestedAppStyle = ElementTheme.Default;

        public MainViewModel(ISettingsService settingsService, IFileIOService fileIOService, IContentDialogService contentDialogService)
        {
            this.settingsService = settingsService;
            this.fileIOService = fileIOService;
            this.contentDialogService = contentDialogService;
            OpenFilesCommand = new RelayCommand(OpenFiles);
            Startup();
        }

        //do stuff at app launch
        private void Startup()
        {
            RegisterMessaging();
      
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