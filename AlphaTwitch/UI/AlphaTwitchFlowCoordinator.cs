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
        protected DismissableNavigationController _dismissableNavController;

        public string TwitchId = "";

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            if (firstActivation && activationType == ActivationType.AddedToHierarchy)
            {
                title = "Alpha Twitch";

                _twitchChannelVC = BeatSaberUI.CreateViewController<TwitchChannelViewController>();

                _dismissableNavController = Instantiate(Resources.FindObjectsOfTypeAll<DismissableNavigationController>().First());
                _dismissableNavController.didFinishEvent += Dismiss;

                SetViewControllersToNavigationConctroller(_dismissableNavController, _twitchChannelVC);
                ProvideInitialViewControllers(_dismissableNavController); //TwitchLoginConfig.Instance.TwitchChannelName

                SharedCoroutineStarter.instance.StartCoroutine(GetThenSetTwitchData(TwitchLoginConfig.Instance.TwitchChannelName));
            }
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
            (mainFlowCoordinator as FlowCoordinator).InvokePrivateMethod("DismissFlowCoordinator", new object[] { this, null, false });
        }
    }
}
