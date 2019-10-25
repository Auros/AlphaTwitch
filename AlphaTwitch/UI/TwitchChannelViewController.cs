using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRUI;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using StreamCore.Config;
using UnityEngine.Networking;
using System.Collections;
using StreamCore.SimpleJSON;

namespace AlphaTwitch.UI
{
    public class TwitchChannelViewController : BSMLResourceViewController
    {
        public override string ResourceName => "AlphaTwitch.Views.twitchchannelinfo.bsml";

        [UIComponent("channelpic")]
        private RawImage rawImage;

        [UIComponent("channelname")]
        private TextMeshProUGUI channelname;

        [UIComponent("status")]
        private TextMeshProUGUI statusText;

        [UIComponent("title")]
        private TextMeshProUGUI titleText;

        [UIComponent("viewcount")]
        private TextMeshProUGUI vc;

        [UIAction("press")]
        private void ButtonPress()
        {
            StartCoroutine(UpdateViewCountInternally());
        }

        public Action yee;

        [UIAction("yee")]
        private void Yee()
        {
            yee.Invoke();
        }

        public void GenerateImage(string url)
        {
            SharedCoroutineStarter.instance.StartCoroutine(Utilities.LoadScripts.LoadSpriteCoroutine(url, (image) =>
            {
                rawImage.texture = image.texture;
            }));
        }

        private string userName = "";
        private int viewCount = -1;
        private Type status = Type.Offline;
        private string titLE = "";


        private IEnumerator UpdateViewCountInternally()
        {
            yield return new WaitForSecondsRealtime(.1f);
            using (UnityWebRequest www = UnityWebRequest.Get($"https://api.twitch.tv/helix/streams?user_login={userName}"))
            {
                www.SetRequestHeader("Client-ID", "gbrndadd5hgnm9inrdsw8xpx5w9xa9");
                yield return www.SendWebRequest();

                JSONNode response = JSON.Parse(www.downloadHandler.text);
                if (response["data"].AsArray.Count > 0)
                {
                    var userdata = response["data"][0];
                    var viewcount = userdata["viewer_count"].AsInt;
                    UpdateViewCount(viewcount);
                }
                else
                {
                    UpdateViewCount(-1);
                }
            }
        }

        public void ApplyData(string username, string statusvalue, string title = "", int viewcount = -1)
        {
            if (viewcount == -1)
                viewCount = viewcount;

            if (statusvalue.Contains("live"))
            {
                status = Type.Live;
                statusText.color = Color.green;
                statusText.text = "Online";
                viewCount = viewcount;
            }   
            else
            {
                status = Type.Offline;
                statusText.color = Color.red;
                statusText.text = "Offline";
            }

            titLE = title;
            userName = username;

            SetBoard();
        }

        private void SetBoard()
        {
            if (status == Type.Live)
            {
                statusText.color = Color.green;
                statusText.text = "Online";
            }
            else
            {
                statusText.color = Color.red;
                statusText.text = "Offline";
            }

            if (viewCount >= 0)
                vc.text = "View Count: " + viewCount.ToString();
            else
                vc.text = "";

            titleText.text = titLE;

            channelname.text = userName;
        }

        public void SetName(string name)
        {
            userName = name;
            SetBoard();
        }
        
        public void SetTitle(string title)
        {
            titLE = title;
            SetBoard();
        }

        public void UpdateStatus(string statusvalue)
        {
            if (statusvalue.Contains("live"))
                status = Type.Live;
            else
                status = Type.Offline;
            SetBoard();
        }

        public void UpdateViewCount(int viewcount)
        {
            viewCount = viewcount;
            SetBoard();
        }

        public enum Type
        {
            Live,
            Offline
        }
    }
}
