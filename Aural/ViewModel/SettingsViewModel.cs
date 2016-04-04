using Aural.Interface;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aural.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        ISettingsService settingsService;

        private ObservableCollection<string> _accessTokens = new ObservableCollection<string>();
        public ObservableCollection<string> AccessTokens
        {
            get { return _accessTokens; }
            set { Set("AccessTokens", ref _accessTokens, value); }
        }

        public RelayCommand SetMasterFolderCommand { get; private set; }

        public SettingsViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            SetMasterFolderCommand = new RelayCommand(SetMasterFolder);
            AccessTokens = settingsService.GetAccessTokens();
        }

        //Set a new master folder
        private void SetMasterFolder()
        {
            settingsService.SetMasterFolder();
            AccessTokens = settingsService.GetAccessTokens();
        }

    }
}
