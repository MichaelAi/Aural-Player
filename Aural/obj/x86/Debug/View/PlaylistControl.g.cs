﻿#pragma checksum "C:\Users\micha\Documents\Visual Studio 2015\Projects\Aural\Aural\View\PlaylistControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "D271FF8F60573E30BC8E7577DD3B02FC"
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
    partial class PlaylistControl : 
        global::Windows.UI.Xaml.Controls.UserControl, 
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
                    this.PlaylistControlPage = (global::Windows.UI.Xaml.Controls.UserControl)(target);
                }
                break;
            case 2:
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyoutItem element2 = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    #line 42 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)element2).Click += this.MenuFlyoutItem_Click;
                    #line default
                }
                break;
            case 3:
                {
                    this.MenuItemClearSelection = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    #line 43 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)this.MenuItemClearSelection).Click += this.MenuItemClearSelection_Click;
                    #line default
                }
                break;
            case 4:
                {
                    this.MenuItemQueueNext = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    #line 20 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)this.MenuItemQueueNext).Click += this.MenuItemQueueNext_Click;
                    #line default
                }
                break;
            case 5:
                {
                    this.MenuItemQueueLast = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    #line 21 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)this.MenuItemQueueLast).Click += this.MenuItemQueueLast_Click;
                    #line default
                }
                break;
            case 6:
                {
                    this.MenuItemPlay = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    #line 22 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)this.MenuItemPlay).Click += this.MenuItemPlay_Click;
                    #line default
                }
                break;
            case 7:
                {
                    this.PlaylistRoot = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 8:
                {
                    this.PlaylistListView = (global::Windows.UI.Xaml.Controls.ListView)(target);
                    #line 82 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.ListView)this.PlaylistListView).DragOver += this.OnFileDragOver;
                    #line 82 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.ListView)this.PlaylistListView).Drop += this.OnFileDrop;
                    #line default
                }
                break;
            case 9:
                {
                    global::Windows.UI.Xaml.Controls.Grid element9 = (global::Windows.UI.Xaml.Controls.Grid)(target);
                    #line 93 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.Grid)element9).Tapped += this.Grid_Tapped;
                    #line 93 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.Grid)element9).RightTapped += this.StackPanel_RightTapped;
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

