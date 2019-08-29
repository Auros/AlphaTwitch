using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlphaTwitch.Utilities;
using StreamCore.Chat;
using StreamCore.Config;
using StreamCore.SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using AlphaTwitch.UniGif;
using AlphaTwitch.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage;
using AlphaTwitch.UI;
using CustomUI.MenuButton;

namespace AlphaTwitch
{
    public class AlphaTwitchManager : MonoBehaviour
    {
        public static Dictionary<string, Sprite> TwitchEmotePool = new Dictionary<string, Sprite>();
        public static Dictionary<string, Sprite> BTTVEmotePool = new Dictionary<string, Sprite>();
        public static Dictionary<string, GIFer> BTTVAnimatedEmotePool = new Dictionary<string, GIFer>();

        private static AlphaTwitchManager _instance;
        public static AlphaTwitchManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject managerGO = new GameObject("AlphaTwitch Manager");
                    _instance = managerGO.AddComponent<AlphaTwitchManager>();
                    _instance.Init();
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        public Action<string> EmoteProcessed;

        private TwitchCore _twitchCore;

        internal void Init()
        {
            _twitchCore = TwitchCore.Instance;

            SceneManager.activeSceneChanged += OnSceneChange;
            SceneManager.sceneLoaded += OnSceneLoad;

            _twitchCore.MessageReceived += MessageReceived;
            EmoteProcessed += T_EmoteProcessed;
            ImageTag img = new ImageTag();
            BSMLParser.instance.RegisterTag(img);
        }


        private void MessageReceived(TwitchMessage msg)
        {

            //Emote Cache
            if (msg.emotes != "")
            {
                string[] emotes = msg.emotes.Split('/');
                foreach (var emote in emotes)
                {
                    string invemote = emote;
                    int index = invemote.IndexOf(":");
                    if (index > 0)
                        invemote = invemote.Substring(0, index);

                    if (!TwitchEmotePool.ContainsKey(invemote))
                    {
                        SharedCoroutineStarter.instance.StartCoroutine(LoadScripts.LoadSpriteCoroutine($"https://static-cdn.jtvnw.net/emoticons/v1/{invemote}/3.0", (image) =>
                        {
                            if (!TwitchEmotePool.ContainsKey(invemote))
                            {
                                TwitchEmotePool.Add(invemote, image);
                                EmoteProcessed.Invoke(invemote);
                            }
                        }));
                    }
                    else
                    {
                        EmoteProcessed.Invoke(invemote);
                    }
                }
            }
        }

        private void OnSceneChange(Scene arg0, Scene arg1)
        {
            if (arg1.name == "GameCore")
            {
                EmotePopups.EmotePopups.Load();
            }
        }

        private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name == "MenuCore")
            {
                if (BTTVAnimatedEmotePool.Count > 0)
                {
                    foreach (var ele in BTTVAnimatedEmotePool)
                    {
                        ele.Value.DestroyGIFer();
                    }
                    BTTVAnimatedEmotePool.Clear();
                }
                if (BTTVEmotePool.Count > 0)
                {
                    BTTVEmotePool.Clear();
                }

                GetBTTVEmotes();
                
                PluginUI.Instance.Init();
            }
        }

        private void GetBTTVEmotes()
        {
            StartCoroutine(GetBTTVChannelEmotes());
        }

        private IEnumerator GetBTTVChannelEmotes()
        {
            UnityWebRequest www = UnityWebRequest.Get($"https://api.betterttv.net/2/channels/{TwitchLoginConfig.Instance.TwitchChannelName}");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                
            }
            else
            {
                JSONNode json = JSON.Parse(www.downloadHandler.text);
                if (json["status"].AsInt == 200)
                {

                    JSONArray emotes = json["emotes"].AsArray;
                    foreach (JSONObject o in emotes)
                    {
                        if (o["imageType"] != "gif")
                        {
                            var id = (string)o["id"];
                            SharedCoroutineStarter.instance.StartCoroutine(LoadScripts.LoadSpriteCoroutine($"https://cdn.betterttv.net/emote/{id}/3x", (image) =>
                            {
                                BTTVEmotePool.Add(o["code"], image);
                            }));
                        }
                        else
                        {
                            var id = (string)o["id"];
                            var gif = new GameObject().AddComponent<GIFer>();
                            gif.Create($"https://cdn.betterttv.net/emote/{id}/3x");
                            DontDestroyOnLoad(gif); //Im sorry
                            BTTVAnimatedEmotePool.Add(o["code"], gif);
                        }
                    }
                }
                
            }
        }


        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChange;
            SceneManager.sceneLoaded -= OnSceneLoad;

            _twitchCore.MessageReceived -= MessageReceived;
            EmoteProcessed -= T_EmoteProcessed;
        }

        private void T_EmoteProcessed(string dat) { }
    }
}
