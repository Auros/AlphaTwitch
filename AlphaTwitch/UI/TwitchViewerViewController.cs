using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AlphaTwitch.UI
{
    public class TwitchViewerViewController : BSMLResourceViewController
    {
        public override string ResourceName => "AlphaTwitch.Views.twitchviewer.bsml";

        [UIComponent("viewerpicture")]
        private RawImage image;

        [UIComponent("viewername")]
        private TextMeshProUGUI viewerName;

        private Viewer currentViewer;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);

                if (image.gameObject.GetComponent<AspectRatioFitter>() == null)
                {
                    var element = image.gameObject.AddComponent<AspectRatioFitter>();
                    element.aspectRatio = 1f;
                    element.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                }
            }

            if (type == ActivationType.AddedToHierarchy)
            {
                if (StreamCore.Config.TwitchLoginConfig.Instance.TwitchOAuthToken == "")
                {
                    oauthText.text = "StreamCore OAuth is not setup";
                    oauthText.color = Color.red;

                    banButton.interactable = false;
                    timeoutButton.interactable = false;
                    modButton.interactable = false;
                    vipButton.interactable = false;
                }
            }
        }

        private bool oauthDetected = true;

        public void SetData(Viewer viewer)
        {
            currentViewer = viewer;

            modButton.interactable = true;
            vipButton.interactable = true;

            if (viewer.Role == Role.Mod) 
                modButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unmod User";
            else
                modButton.GetComponentInChildren<TextMeshProUGUI>().text = "Give Mod";

            if (viewer.Role == Role.Vip)
                vipButton.GetComponentInChildren<TextMeshProUGUI>().text = "UnVIP User";
            else
                vipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Give VIP";

            if (oauthDetected)
            {
                if (viewer.Role == Role.Mod)
                    vipButton.interactable = false;
                else if (viewer.Role == Role.Vip)
                    modButton.interactable = false;
            }

            


            viewerName.text = viewer.Name;
            image.texture = viewer.Profile;
            image.texture.wrapMode = TextureWrapMode.Clamp;
        }

        [UIComponent("oauth")]
        TextMeshProUGUI oauthText;

        [UIComponent("ban")]
        Button banButton;

        [UIComponent("timeout")]
        Button timeoutButton;

        [UIComponent("mod")]
        Button modButton;

        [UIComponent("vip")]
        Button vipButton;


        [UIAction("ban")]
        private void BanUser()
        {
            StreamCore.Chat.TwitchWebSocketClient.SendCommand($"/ban {currentViewer.Name}");
        }

        [UIAction("timeout")]
        private void TimeoutUser()
        {
            StreamCore.Chat.TwitchWebSocketClient.SendCommand($"/timeout {currentViewer.Name} 120");
        }

        [UIAction("mod")]
        private void ModOrUnmodUser()
        {
            if (currentViewer.Role == Role.Mod)
            {
                StreamCore.Chat.TwitchWebSocketClient.SendCommand($"/unmod {currentViewer.Name}");
                modButton.GetComponentInChildren<TextMeshProUGUI>().text = "Give Mod";
                if (oauthDetected)
                    vipButton.interactable = true;
                currentViewer.Role = Role.None;
            }
            else
            {
                StreamCore.Chat.TwitchWebSocketClient.SendCommand($"/mod {currentViewer.Name}");
                modButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unmod User";
                if (oauthDetected)
                    vipButton.interactable = false;
                currentViewer.Role = Role.Mod;
            }
                
        }

        [UIAction("vip")]
        private void VIPOrUnVIPUser()
        {
            if (currentViewer.Role == Role.Vip)
            {
                StreamCore.Chat.TwitchWebSocketClient.SendCommand($"/unvip {currentViewer.Name}");
                vipButton.GetComponentInChildren<TextMeshProUGUI>().text = "Give VIP";
                if (oauthDetected)
                    modButton.interactable = true;
                currentViewer.Role = Role.None;
            }
            else
            {
                StreamCore.Chat.TwitchWebSocketClient.SendCommand($"/vip {currentViewer.Name}");
                vipButton.GetComponentInChildren<TextMeshProUGUI>().text = "UnVIP User";
                if (oauthDetected)
                    modButton.interactable = false;
                currentViewer.Role = Role.Vip;
            }
        }
    }
}
