using Aural.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Aural.Service
{

    /// <summary>
    /// SettingService performs the operations done in the settings pane.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        // Store the folder to access again later
        public async void SetMasterFolder()
        {
            FolderPicker picker = new FolderPicker();
            //a filter is needed despite being a folder picker
            picker.FileTypeFilter.Add(".mp3");
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            StorageFolder masterFolder = await picker.PickSingleFolderAsync();
            if (masterFolder != null)
            {
                var listToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(masterFolder, masterFolder.Path);
            }
        }

        //retrieve the current tokens
        public ObservableCollection<string> GetAccessTokens()
        {
            //Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Clear();
            var tokens = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Entries;
            ObservableCollection<string> stringTokens = new ObservableCollection<string>();
            foreach (var token in tokens)
            {
                stringTokens.Add(token.Metadata);
            }
            return stringTokens;
        }

    }
}
