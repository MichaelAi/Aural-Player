﻿#pragma checksum "C:\Users\micha\Documents\Visual Studio 2015\Projects\Aural\Aural\View\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "3C00BCD322EE01387D964B45705A4135"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Aural.View
{
    partial class MainPage : 
        global::Aural.CustomControls.TitleBarPage, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    this.rootSplitView = (global::Windows.UI.Xaml.Controls.SplitView)(target);
                }
                break;
            case 2:
                {
                    this.playlistSplitView = (global::Windows.UI.Xaml.Controls.SplitView)(target);
                }
                break;
            case 3:
                {
                    this.commandBarSettings = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    #line 157 "..\..\..\View\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.commandBarSettings).Click += this.commandBarSettings_Click;
                    #line default
                }
                break;
            case 4:
                {
                    this.playlistsListView = (global::Windows.UI.Xaml.Controls.ListView)(target);
                }
                break;
            case 5:
                {
                    global::Windows.UI.Xaml.Controls.Grid element5 = (global::Windows.UI.Xaml.Controls.Grid)(target);
                    #line 126 "..\..\..\View\MainPage.xaml"
                    ((global::Windows.UI.Xaml.Controls.Grid)element5).RightTapped += this.playlistControlsGrid_RightTapped;
                    #line default
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

