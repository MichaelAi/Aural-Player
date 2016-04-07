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


            this.PlaylistList.SettingsButtonClicked += new EventHandler(PlaylistList_SettingsButtonClicked);
            this.PlaylistList.NowPlayingButtonClicked += new EventHandler(PlaylistList_NowPlayingButtonClicked);
        }

        public void PlaylistList_SettingsButtonClicked(object sender, EventArgs e)
        {
            rootSplitView.IsPaneOpen = !rootSplitView.IsPaneOpen;
        }

        public void PlaylistList_NowPlayingButtonClicked(object sender, EventArgs e)
        {
            NavBarSplitView.IsPaneOpen = !NavBarSplitView.IsPaneOpen;
        }
    }
}
