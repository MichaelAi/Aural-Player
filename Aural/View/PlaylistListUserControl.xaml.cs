using System;
using System.Collections.Generic;
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
    public sealed partial class PlaylistListUserControl : UserControl
    {

        public event EventHandler SettingsButtonClicked;

        public PlaylistListUserControl()
        {
            this.InitializeComponent();
        }

        private void playlistControlsGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var uiSender = sender as UIElement;
            var flyout = (FlyoutBase)uiSender.GetValue(FlyoutBase.AttachedFlyoutProperty);
            flyout.Placement = FlyoutPlacementMode.Bottom;
            flyout.ShowAt(uiSender as FrameworkElement);
        }

        private void commandBarSettings_Click(object sender, RoutedEventArgs e)
        {
            if (this.SettingsButtonClicked != null)
                this.SettingsButtonClicked(new object(), new EventArgs());
        }
    }
}
