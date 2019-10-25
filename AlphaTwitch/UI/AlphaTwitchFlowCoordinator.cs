using BeatSaberMarkupLanguage;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using StreamCore.Config;
using UnityEngine.Networking;
using VRUI;
using StreamCore.SimpleJSON;

namespace AlphaTwitch.UI
{
    class AlphaTwitchFlowCoordinator : FlowCoordinator
    {
        public MainFlowCoordinator mainFlowCoordinator;
        private TwitchChannelViewController _twitchChannelVC;
        private TwitchViewersListViewController _twitchViewListVC;
        private TwitchViewerViewController _twitchViewerVC;
        protected DismissableNavigationController _dismissableNavController;

        public string TwitchId = "";

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            if (firstActivation && activationType == ActivationType.AddedToHierarchy)
            {
                title = "Alpha Twitch";

                _twitchChannelVC = BeatSaberUI.CreateViewController<TwitchChannelViewController>();
                _twitchViewListVC = BeatSaberUI.CreateViewController<TwitchViewersListViewController>();
                _twitchViewListVC.viewerClicked += ViewerClicked;
                _twitchViewerVC = BeatSaberUI.CreateViewController<TwitchViewerViewController>();

                _dismissableNavController = Instantiate(Resources.FindObjectsOfTypeAll<DismissableNavigationController>().First());
                _dismissableNavController.didFinishEvent += Dismiss;

                SetViewControllersToNavigationConctroller(_dismissableNavController, _twitchViewListVC);
                ProvideInitialViewControllers(_dismissableNavController, _twitchChannelVC);
                
                SharedCoroutineStarter.instance.StartCoroutine(GetThenSetTwitchData(TwitchLoginConfig.Instance.TwitchChannelName));
            }
        }

        private void ViewerClicked(Viewer obj)
        {
            if (!_twitchViewerVC.isInViewControllerHierarchy)
            {
                PushViewControllerToNavigationController(_dismissableNavController, _twitchViewerVC);
            }

            if (obj.Role == Role.Other)
                UpdateLights(new Color(0, 1, 1, 1));
            else if (obj.Type == Type.Admin)
                UpdateLights(Color.red);
            else if (obj.Type == Type.Staff)
                UpdateLights(Color.white);
            else if (obj.Type == Type.Partner)
                UpdateLights(new Color(119f / 255, 50f / 255, 209f / 255, 1));
            else if (obj.Role == Role.Mod)
                UpdateLights(new Color(96f / 255, 245f / 255, 66f / 255, 1));
            else if (obj.Role == Role.Vip)
                UpdateLights(new Color(218f / 255, 66f / 255, 245f / 255, 1));
            else
                MenuLightsSO().SetColorsFromPreset(defaultPreset);

            _twitchViewerVC.SetData(obj);
        }

        private void UpdateLights(Color color)
        {
            var x = Resources.FindObjectsOfTypeAll<MenuLightsPresetSO>().First();
            if (x != null)
            {
                var ids = x.lightIdColorPairs;
                foreach (var light in ids)
                {
                    MenuLightsSO().SetColor(light.lightId, color);
                }
            }
            else
                Logger.log.Info("bruh");
        }

        private MenuLightsPresetSO defaultPreset;

        private MenuLightsManager _mLSO;
        public MenuLightsManager MenuLightsSO()
        {
            if (_mLSO == null)
            {
                _mLSO = Resources.FindObjectsOfTypeAll<MenuLightsManager>().First();
                defaultPreset = Resources.FindObjectsOfTypeAll<MenuLightsPresetSO>().First();
                return _mLSO;
            }
            else
                return _mLSO;
        }

        private void Bruh()
        {
            
        }

        private IEnumerator GetThenSetTwitchData(string channelName)
        {
            yield return new WaitForSecondsRealtime(.1f);
            using (UnityWebRequest www = UnityWebRequest.Get($"https://api.twitch.tv/helix/streams?user_login={channelName}"))
            {
                www.SetRequestHeader("Client-ID", "gbrndadd5hgnm9inrdsw8xpx5w9xa9");
                yield return www.SendWebRequest();

                JSONNode response = JSON.Parse(www.downloadHandler.text);
                if (response["data"].AsArray.Count > 0)
                {
                    var userdata = response["data"][0];
                    var username = (string)userdata["user_name"];
                    var type = (string)userdata["type"];
                    var title = (string)userdata["title"];
                    var viewcount = userdata["viewer_count"].AsInt;
                    _twitchChannelVC.ApplyData(username, type, title, viewcount);
                }
                else
                {
                    _twitchChannelVC.UpdateViewCount(-1);
                    _twitchChannelVC.SetTitle("");
                    _twitchChannelVC.UpdateStatus("no");
                    _twitchChannelVC.SetName(TwitchLoginConfig.Instance.TwitchChannelName);
                }

                using (UnityWebRequest www2 = UnityWebRequest.Get($"https://api.twitch.tv/helix/users?login={channelName}"))
                {
                    www2.SetRequestHeader("Client-ID", "gbrndadd5hgnm9inrdsw8xpx5w9xa9");
                    yield return www2.SendWebRequest();

                    JSONNode userres = JSON.Parse(www2.downloadHandler.text);
                    if (userres["data"].AsArray.Count > 0)
                    {
                        var userdata = userres["data"][0];
                        _twitchChannelVC.GenerateImage((string)userdata["profile_image_url"]);
                    }
                }
            }
        }

        private void Dismiss(DismissableNavigationController navController)
        {
            MenuLightsSO().SetColorsFromPreset(defaultPreset);
            (mainFlowCoordinator as FlowCoordinator).InvokePrivateMethod("DismissFlowCoordinator", new object[] { this, null, false });
        }
    }
}
