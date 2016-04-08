using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aural
{
    public static class Settings
    {
        private const string USE_DARK_THEME = "use_dark_theme";
        private const bool USE_DARK_THEME_DEFAULT = false;

        public static bool UseDarkTheme
        {
            get { return bool.Parse(Helpers.ApplicationSettingsHelper.ReadSettingsValue(USE_DARK_THEME).ToString()); }
            set { Helpers.ApplicationSettingsHelper.SaveSettingsValue(USE_DARK_THEME, value); }
        }
    }
}
