using Aural.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Aural.View
{
    public sealed partial class PlaylistControl : UserControl
    {
        public PlaylistControl()
        {
            this.InitializeComponent();
        }

        private void OnFileDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void OnFileDrop(object sender, DragEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    List<StorageFile> files = new List<StorageFile>();
                    foreach (var item in items.OfType<StorageFile>().Select(storageFile => new AppFile { Name = storageFile.Name, File = storageFile}))
                    {
                        files.Add(item.File);
                    }
                    Messenger.Default.Send<NotificationMessage<IReadOnlyList<StorageFile>>>(new NotificationMessage<IReadOnlyList<StorageFile>>(files,"fromDragDrop"));
                }
            }
        }

        private void StackPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

    }

    public class AppFile
    {
        public string Name { get; set; }
        public StorageFile File { get; set; }
    }
}
