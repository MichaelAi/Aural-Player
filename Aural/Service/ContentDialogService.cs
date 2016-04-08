using Aural.Helpers;
using Aural.Interface;
using Aural.Model;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Aural.Service
{
    public class ContentDialogService : IContentDialogService
    {
        //create a dialog that accepts a playlist name and returns the new playlist
        public async Task<Playlist> CreateNewPlaylist()
        {
            Playlist play = new Playlist { PlaylistId = Guid.NewGuid() };
            var dialog = new ContentDialog()
            {
                Title = "New Playlist",
            };
            var panel = new StackPanel();
            var tb = new TextBox
            {
                PlaceholderText = "Enter the new playlist's name",
                Margin = new Windows.UI.Xaml.Thickness { Top = 10 }
            };
            panel.Children.Add(tb);
            dialog.Content = panel;

            dialog.PrimaryButtonText = "Save";
            var cmd = new RelayCommand(() =>
            {
                play.PlaylistName = tb.Text;
                
            }, () => CanSave(tb.Text));

            dialog.IsPrimaryButtonEnabled = false;
            dialog.PrimaryButtonCommand = cmd;
            dialog.SecondaryButtonText = "Cancel";

            tb.TextChanged += delegate
            {
                cmd.RaiseCanExecuteChanged();
                if (tb.Text.Trim().Length > 0)
                {
                    dialog.IsPrimaryButtonEnabled = true;
                }
                else
                {
                    dialog.IsPrimaryButtonEnabled = false;
                }

            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                return play;
            }
            return null;
        }

        //check whether the new name is not empty
        private bool CanSave(string parameter)
        {
            if (parameter != "")
            {
                return true;
            }
            return false;
        }

        //make sure the user wants to delete the playlist
        public async Task<bool> ShowPlaylistDeletionConfirmation(Playlist play)
        {
            bool disable = false;

            var dialog = new ContentDialog()
            {
                Title = "Confirm Deletion",
            };

            var panel = new StackPanel();
            var tb = new TextBlock
            {
                Text = "Deleting a playlist cannot be undone. Are you sure you want to proceed?",
            };

            var cb = new CheckBox
            {
                Content = "Disable confirmations for this session",
                Margin = new Windows.UI.Xaml.Thickness { Top = 10 }
            };

            panel.Children.Add(tb);
            panel.Children.Add(cb);
            dialog.Content = panel;

            dialog.PrimaryButtonText = "Delete";
            var cmd = new RelayCommand(() =>
            {
                disable = cb.IsChecked ?? false;
            });

            dialog.PrimaryButtonCommand = cmd;
            dialog.SecondaryButtonText = "Cancel";
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (disable)
                    ApplicationSettingsHelper.SaveSettingsValue("disableConfirmSession", true);
                return true;
            }
            return false;
        }
  
        //show a dialog, similar to new playlist, accept the old and return a new playlist with a new name.
        public async Task<Playlist> ShowEditPlaylistDialog(Playlist oldPlay)
        {
            Playlist newPlay = new Playlist { PlaylistId = Guid.NewGuid() };
            var dialog = new ContentDialog()
            {
                Title = "Edit Playlist",
            };

            var panel = new StackPanel();
            var tb = new TextBox
            {
                PlaceholderText = "Enter the playlist's new name",
                Margin = new Windows.UI.Xaml.Thickness { Top = 10 }
            };
            panel.Children.Add(tb);
            dialog.Content = panel;

            dialog.PrimaryButtonText = "Save";
            var cmd = new RelayCommand(() =>
            {
                newPlay.PlaylistName = tb.Text;
            }, () => CanSave(tb.Text));

            dialog.IsPrimaryButtonEnabled = false;
            dialog.PrimaryButtonCommand = cmd;

            dialog.SecondaryButtonText = "Cancel";

            tb.TextChanged += delegate
            {
                cmd.RaiseCanExecuteChanged();
                if (tb.Text.Trim().Length > 0)
                {
                    dialog.IsPrimaryButtonEnabled = true;
                }
                else
                {
                    dialog.IsPrimaryButtonEnabled = false;
                }

            };

            dialog.RequestedTheme = Windows.UI.Xaml.ElementTheme.Dark;
            var result = await dialog.ShowAsync();
            if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                newPlay.Items = oldPlay.Items;
                newPlay.PlaylistId = oldPlay.PlaylistId;
                return newPlay;
            }
            return null;
        }
    }
}
