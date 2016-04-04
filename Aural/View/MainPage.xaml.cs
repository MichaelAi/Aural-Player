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
            rootSplitView.IsPaneOpen = !rootSplitView.IsPaneOpen;
        }

        private void playlistControlsGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var uiSender = sender as UIElement;
            var flyout = (FlyoutBase)uiSender.GetValue(FlyoutBase.AttachedFlyoutProperty);
            flyout.Placement = FlyoutPlacementMode.Bottom;
            flyout.ShowAt(uiSender as FrameworkElement);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            NavBarSplitView.IsPaneOpen = !NavBarSplitView.IsPaneOpen;
        }
    }
}
