using Aural.Interface;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

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

        private bool _useDarkTheme = false;
        public bool UseDarkTheme
        {
            get { return _useDarkTheme; }
            set { Set("UseDarkTheme", ref _useDarkTheme, value); SetTheme(); }
        }

        public RelayCommand SetMasterFolderCommand { get; private set; }

        public SettingsViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            SetMasterFolderCommand = new RelayCommand(SetMasterFolder);
            GetTheme();
            AccessTokens = settingsService.GetAccessTokens();
            
        }

        private void GetTheme()
        {
            try
            {
                UseDarkTheme = Settings.UseDarkTheme;
            }
            catch
            {
                //there is no saved style. use light.
                Settings.UseDarkTheme = false;
                UseDarkTheme = false;
            }
        }

        private void SetTheme()
        {
            if (UseDarkTheme)
            {
                Settings.UseDarkTheme = true;
            }
            else
            {
                Settings.UseDarkTheme = false;
            }
        }

        //Set a new master folder
        private void SetMasterFolder()
        {
            settingsService.SetMasterFolder();
            AccessTokens = settingsService.GetAccessTokens();
        }

    }
}
