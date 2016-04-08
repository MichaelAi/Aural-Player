using Aural.Converters;
using Aural.Model;
using Aural.ViewModel;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        ObservableCollection<PlaylistItem> selectedItems = new ObservableCollection<PlaylistItem>();
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
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    List<StorageFile> files = new List<StorageFile>();
                    foreach (var item in items.OfType<StorageFile>().Select(storageFile => new AppFile { Name = storageFile.Name, File = storageFile }))
                    {
                        files.Add(item.File);
                    }
                    Messenger.Default.Send<NotificationMessage<IReadOnlyList<StorageFile>>>(new NotificationMessage<IReadOnlyList<StorageFile>>(files, "fromDragDrop"));
                }
            }
        }

        private void StackPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var uiSender = sender as UIElement;
            var flyout = (FlyoutBase)uiSender.GetValue(FlyoutBase.AttachedFlyoutProperty);
            flyout.Placement = FlyoutPlacementMode.Bottom;
            flyout.ShowAt(uiSender as FrameworkElement);
        }


        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItems.Count == 0)
            {
                selectedItems = new ObservableCollection<PlaylistItem>();
                var x = ((sender as UIElement) as MenuFlyoutItem);
                selectedItems.Add(x.CommandParameter as PlaylistItem);
            }
            var viewModel = SimpleIoc.Default.GetInstance<PlaylistViewModel>();
            if (viewModel.RemoveItemsFromPlaylistCommand.CanExecute(null))
                viewModel.RemoveItemsFromPlaylistCommand.Execute(selectedItems);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //iterating through the list is the only way to retrieve selected items
            //i've tried a lot of things and this is the only thing that worked
            selectedItems.Clear();
            foreach (var item in PlaylistListView.SelectedItems)
            {
                selectedItems.Add(item as PlaylistItem);
            }         
        }

        private void MenuItemPlay_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItems.Count == 0)
            {
                selectedItems = new ObservableCollection<PlaylistItem>();
                var x = ((sender as UIElement) as MenuFlyoutItem);
                selectedItems.Add(x.CommandParameter as PlaylistItem);
            }
            var viewModel = SimpleIoc.Default.GetInstance<PlayerViewModel>();
            if (viewModel.MediaPlaySelectionCommand.CanExecute(null))
                viewModel.MediaPlaySelectionCommand.Execute(selectedItems);
        }

        private void MenuItemQueueNext_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItems.Count == 0)
            {
                selectedItems = new ObservableCollection<PlaylistItem>();
                var x = ((sender as UIElement) as MenuFlyoutItem);
                selectedItems.Add(x.CommandParameter as PlaylistItem);
            }
            var viewModel = SimpleIoc.Default.GetInstance<PlayerViewModel>();
            if (viewModel.AddToQueueNextCommand.CanExecute(null))
                viewModel.AddToQueueNextCommand.Execute(selectedItems);
        }

        private void MenuItemQueueLast_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItems.Count == 0)
            {
                selectedItems = new ObservableCollection<PlaylistItem>();
                var x = ((sender as UIElement) as MenuFlyoutItem);
                selectedItems.Add(x.CommandParameter as PlaylistItem);
            }
            var viewModel = SimpleIoc.Default.GetInstance<PlayerViewModel>();
            if (viewModel.AddToQueueLastCommand.CanExecute(null))
                viewModel.AddToQueueLastCommand.Execute(selectedItems);
        }

        private void MenuItemClearSelection_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
