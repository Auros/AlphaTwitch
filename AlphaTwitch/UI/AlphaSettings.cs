using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace AlphaTwitch.UI
{
   public class AlphaSettings : PersistentSingleton<AlphaSettings>
    {
        [UIValue("ep")]
        private bool emotepopups = AlphaTwitchManager.Settings.EmotePopups.Enable;

        [UIAction("eef")]
        private void ApplyImmediately(bool value)
        {
            AlphaTwitchManager.Settings.EmotePopups.Enable = value;
            
        }

    }
}
