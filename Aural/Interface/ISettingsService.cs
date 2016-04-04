using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aural.Interface
{
    public interface ISettingsService
    {
        void SetMasterFolder();
        ObservableCollection<string> GetAccessTokens();
    }
}
