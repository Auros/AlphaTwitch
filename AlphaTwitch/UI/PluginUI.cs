using CustomUI.MenuButton;
using CustomUI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlphaTwitch.UI
{
    public class PluginUI : MonoBehaviour
    {
        private static PluginUI _instance;
        public static PluginUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("PluginUI");
                    _instance = obj.AddComponent<PluginUI>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        private AlphaTwitchFlowCoordinator _alphaTwitchFlowCoordinator;

        public void Init()
        {
            MenuButtonUI.AddButton("Alpha Twitch", AlphaTwitchButtonPressed);

            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.AddSettingsMenu("Alpha Twitch", "AlphaTwitch.Views.alphatwitchsettings.bsml", PersistentSingleton<AlphaSettings>.instance);
        }

        private void AlphaTwitchButtonPressed()
        {
            _alphaTwitchFlowCoordinator = new GameObject("AlphaTwitchFlowCoordinator").AddComponent<AlphaTwitchFlowCoordinator>();

            MainFlowCoordinator mainFlow = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();
            _alphaTwitchFlowCoordinator.mainFlowCoordinator = mainFlow;
            mainFlow.InvokeMethod("PresentFlowCoordinator", _alphaTwitchFlowCoordinator, null, false, false);
        }
    }
}