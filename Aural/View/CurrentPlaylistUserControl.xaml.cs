using Aural.Model;
using Aural.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class CurrentPlaylistUserControl : UserControl
    {
        ObservableCollection<PlaylistItem> selectedItems = new ObservableCollection<PlaylistItem>();
        public CurrentPlaylistUserControl()
        {
            this.InitializeComponent();
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
            var viewModel = (PlaylistViewModel)DataContext;
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
    }
}
