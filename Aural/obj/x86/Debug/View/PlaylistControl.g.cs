﻿#pragma checksum "C:\Users\micha\Documents\Visual Studio 2015\Projects\Aural\Aural\View\PlaylistControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "12D52FBF9BD4C48A5B45D59BCF4A5D91"
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
                    this.PlaylistListView = (global::Windows.UI.Xaml.Controls.ListView)(target);
                    #line 41 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.ListView)this.PlaylistListView).DragOver += this.OnFileDragOver;
                    #line 41 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.ListView)this.PlaylistListView).Drop += this.OnFileDrop;
                    #line default
                }
                break;
            case 2:
                {
                    global::Windows.UI.Xaml.Controls.Button element2 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    #line 57 "..\..\..\View\PlaylistControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.Button)element2).RightTapped += this.StackPanel_RightTapped;
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

