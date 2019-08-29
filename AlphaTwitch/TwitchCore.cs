using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using StreamCore.Chat;
using StreamCore.Config;

namespace AlphaTwitch
{
    public class TwitchCore : MonoBehaviour
    {
        private static TwitchCore _instance;
        public static TwitchCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject();
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<TwitchCore>();
                    Instance.Init();
                }
                return _instance;
            }
        }

        //Events
        public Action<TwitchMessage> MessageReceived;

        //Internal Methods
        internal void Init()
        {
            try
            {
                TwitchMessageHandlers.PRIVMSG += (chatmessage) =>
                {
                    //If the message doesn't come from the channel specified, ignore it.
                    if (chatmessage.channelName != TwitchLoginConfig.Instance.TwitchChannelName)
                        return;
                    
                    MessageReceived.Invoke(chatmessage);
                };
            }
            catch
            {

            }
        }
    }
}
