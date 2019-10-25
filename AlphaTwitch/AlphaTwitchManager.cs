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
        public class Settings
        {
            public class EmotePopups
            {
                public static bool Enable = true;
            }

            public static void Load()
            {
                EmotePopups.Enable = ModuleEnabled("EmotePopups");
            }

            public static void Save()
            {
                Logger.log.Info("Saving: " + EmotePopups.Enable.ToString());
                Config.SetBool(BuildUIString("EmotePopups"), "Enable", EmotePopups.Enable);
            }

            private static string BuildUIString(string name)
            {
                return $"AlphaTwitch - {name}";
            }

            private static bool ModuleEnabled(string name)
            {
                return Config.GetBool(BuildUIString(name), "Enable", true, true);
            }
        }

        internal static BS_Utils.Utilities.Config Config = new BS_Utils.Utilities.Config("[LM] - AlphaTwitch");
        public static Dictionary<string, Sprite> TwitchEmotePool = new Dictionary<string, Sprite>();
        public static Dictionary<string, Sprite> BTTVEmotePool = new Dictionary<string, Sprite>();
        public static Dictionary<string, GIFer> BTTVAnimatedEmotePool = new Dictionary<string, GIFer>();


        public static ConcurrentDictionary<string, Viewer> Viewers = new ConcurrentDictionary<string, Viewer>();



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

            Settings.Load();
        }

        private IEnumerator GetTwitchViewerInfo(TwitchMessage msg)
        {
            yield return new WaitForSecondsRealtime(.025f);
            using (UnityWebRequest www = UnityWebRequest.Get($"https://api.twitch.tv/helix/users?login={msg.user.displayName}"))
            {
                www.SetRequestHeader("Client-ID", "gbrndadd5hgnm9inrdsw8xpx5w9xa9");
                yield return www.SendWebRequest();

                JSONNode response = JSON.Parse(www.downloadHandler.text);
                if (response["data"].AsArray.Count > 0)
                {
                    var userdata = response["data"][0];
                    var profile = (string)userdata["profile_image_url"];
                    var type = (string)userdata["type"];
                    var btype = (string)userdata["broadcaster_type"];

                    bool userCached = Viewers.TryGetValue(msg.user.displayName, out Viewer vi);

                    if (!userCached)
                    {
                        Viewer newViewer = new Viewer
                        {
                            Name = msg.user.displayName,
                            ID = msg.user.id,
                        };

                        Logger.log.Info("Color: " + msg.user.color);
                        if (msg.user.color != "")
                            newViewer.Color = msg.user.color;
                        else
                            newViewer.Color = "#ffffff";

                        if (type == "")
                        {
                            if (btype == "partner")
                                newViewer.Type = Type.Partner;
                            else if (btype == "affiliate")
                                newViewer.Type = Type.Affiliate;
                            else
                                newViewer.Type = Type.None;
                        }
                        else if (type == "staff")
                            newViewer.Type = Type.Staff;
                        else if (type == "admin")
                            newViewer.Type = Type.Admin;
                        else
                            newViewer.Type = Type.None;

                        if (msg.user.id == "152734662")
                            newViewer.Role = Role.Other;
                        else if (msg.user.isBroadcaster)
                            newViewer.Role = Role.Broadcaster;
                        else if (msg.user.isMod)
                            newViewer.Role = Role.Mod;
                        else if (msg.user.isVip)
                            newViewer.Role = Role.Vip;
                        else if (msg.user.isSub)
                            newViewer.Role = Role.Sub;
                        else
                            newViewer.Role = Role.None;

                        SharedCoroutineStarter.instance.StartCoroutine(LoadScripts.LoadSpriteCoroutine(profile, (image) => {
                            newViewer.Profile = image.texture;
                            Viewers.TryAdd(msg.user.displayName.ToLower(), newViewer);
                        }));
                        
                    }
                }
            }
        }


        private void MessageReceived(TwitchMessage msg)
        {
            //User Fetch and Cache
            PersistentSingleton<HMMainThreadDispatcher>.instance.Enqueue(GetTwitchViewerInfo(msg));


            //Emote Cache
            if (msg.emotes != "")
            {
                string[] emotes = msg.emotes.Split('/');

                if (!Settings.EmotePopups.Enable)
                    return;

                int count = 0;
                foreach (var emote in emotes)
                {
                    if (count > 3)
                        break;

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
                    count++;
                }
            }
        }

        private void OnSceneChange(Scene arg0, Scene arg1)
        {
            if (arg1.name == "GameCore")
            {
                if (Settings.EmotePopups.Enable) //MOD DONE?! NEEDS TESTING @ FEFE
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
            StartCoroutine(GetBTTVGlobalEmotes());
            StartCoroutine(GetFFZChannelEmotes());
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

        public IEnumerator GetBTTVGlobalEmotes()
        {

            UnityWebRequest www = UnityWebRequest.Get($"https://api.betterttv.net/2/emotes");
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

        public IEnumerator GetFFZChannelEmotes() //{TwitchLoginConfig.Instance.TwitchChannelName}
        {

            UnityWebRequest www = UnityWebRequest.Get($"https://api.frankerfacez.com/v1/room/{TwitchLoginConfig.Instance.TwitchChannelName}");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {

            }
            else
            {

                JSONNode json = JSON.Parse(www.downloadHandler.text);
                if (json["sets"].IsObject)
                {
                    JSONArray emotes = json["sets"][json["room"]["set"].ToString()]["emoticons"].AsArray;
                    foreach (JSONObject o in emotes)
                    {
                        JSONObject urls = o["urls"].AsObject;
                        string url = urls[urls.Count - 1];
                        string index = url.Substring(url.IndexOf(".com/") + 5);
                        SharedCoroutineStarter.instance.StartCoroutine(LoadScripts.LoadSpriteCoroutine(url, (image) =>
                        {
                            BTTVEmotePool.Add(o["name"], image); //i think this works? lmao
                        }));

                        
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
