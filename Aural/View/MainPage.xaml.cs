using Aural.CustomControls;
using Aural.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Aural.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : TitleBarPage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void commandBarSettings_Click(object sender, RoutedEventArgs e)
        {
            rootSplitView.IsPaneOpen = true;
        }

        public void TestFirstName()
        {
            foreach (var item in playlistsListView.Items)
            {
                var _Container = playlistsListView.ItemContainerGenerator
                    .ContainerFromItem(item);
                var _Children = AllChildren(_Container);

                var _FirstName = _Children
                    // only interested in TextBoxes
                    .OfType<TextBox>()
                    // only interested in FirstName
                    .First(x => x.Name.Equals("FirstName"));

                // test & set color
                _FirstName.Background =
                    (string.IsNullOrWhiteSpace(_FirstName.Text))
                    ? new SolidColorBrush(Colors.Red)
                    : new SolidColorBrush(Colors.White);
            }
        }

        public List<Control> AllChildren(DependencyObject parent)
        {
            var _List = new List<Control>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is Control)
                    _List.Add(_Child as Control);
                _List.AddRange(AllChildren(_Child));
            }
            return _List;
        }

        private void playlistControlsGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var uiSender = sender as UIElement;
            var flyout = (FlyoutBase)uiSender.GetValue(FlyoutBase.AttachedFlyoutProperty);
            flyout.Placement = FlyoutPlacementMode.Bottom;
            flyout.ShowAt(uiSender as FrameworkElement);
        }
    }
}
