using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamCore.Chat;
using UnityEngine;
using AlphaTwitch.UniGif;
using AlphaTwitch.Components;
using Random = UnityEngine.Random;
using System.Collections;

namespace AlphaTwitch.EmotePopups
{
    public class EmotePopups : MonoBehaviour
    {
        private static EmotePopups _emotePopups;

        public static void Load()
        {
            if (_emotePopups == null)
                _emotePopups = new GameObject().AddComponent<EmotePopups>();
        }

        private TwitchCore _twitchCore;
        private List<Emote> emotePool = new List<Emote>();
        private BeatmapObjectSpawnController _spawnController;


        private void Awake()
        {
            _twitchCore = TwitchCore.Instance;
            _spawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().FirstOrDefault();
            _spawnController.noteWasCutEvent += NoteCut;
            AlphaTwitchManager.Instance.EmoteProcessed += EmoteProcessed;
            _twitchCore.MessageReceived += MessageReceived;
        }

        private void MessageReceived(TwitchMessage msg)
        {
            string[] msgParts = msg.message.Split(' ').Distinct().ToArray();
            int count = 0;
            foreach (var pe in msgParts)
            {
                if (count > 3)
                    break;

                if (AlphaTwitchManager.BTTVEmotePool.ContainsKey(pe))
                {
                    AlphaTwitchManager.BTTVEmotePool.TryGetValue(pe, out Sprite sprite);
                    emotePool.Add(new Emote() { EmoteData = sprite, Type = EmoteType.BTTV, Id = pe });
                    count++;
                }
                else if (AlphaTwitchManager.BTTVAnimatedEmotePool.ContainsKey(pe))
                {
                    AlphaTwitchManager.BTTVAnimatedEmotePool.TryGetValue(pe, out GIFer gif);
                    emotePool.Add(new Emote() { EmoteData = gif, Type = EmoteType.BTTVAnimated, Id = pe });
                    count++;
                }
            }
        }

        private void EmoteProcessed(string emoteid)
        {
            AlphaTwitchManager.TwitchEmotePool.TryGetValue(emoteid, out Sprite sprite);
            emotePool.Add(new Emote() { EmoteData = sprite, Type = EmoteType.Twitch, Id = emoteid });
        }

        private void NoteCut(BeatmapObjectSpawnController arg1, NoteController arg2, NoteCutInfo arg3)
        {
            if (emotePool.Count == 0)
                return;

            if (emotePool[0].Type == EmoteType.BTTVAnimated)
            {
                AlphaTwitchManager.BTTVAnimatedEmotePool.TryGetValue(emotePool[0].Id, out GIFer gifer);

                gifer.EnableGIFer(new Vector3(Random.Range(-3f, 3f), Random.Range(-.25f, 3f), Random.Range(3.3f, 7f)));
                emotePool.RemoveAt(0);
            }
            else
            {
                Imager imager = new GameObject().AddComponent<Imager>();
                imager.Create();
                imager.EnableImager((Sprite)emotePool[0].EmoteData, new Vector3(Random.Range(-3f, 3f), Random.Range(-.25f, 3f), Random.Range(3.3f, 7f)), 6.3f);
                emotePool.RemoveAt(0);
            }
        }

        private void OnDestroy()
        {
            _spawnController.noteWasCutEvent -= NoteCut;
            AlphaTwitchManager.Instance.EmoteProcessed -= EmoteProcessed;
            _twitchCore.MessageReceived -= MessageReceived;
        }

        public class Emote
        {
            public object EmoteData { get; set; }
            public EmoteType Type { get; set; }
            public string Id { get; set; }
        }

        public enum EmoteType
        {
            Twitch,
            BTTV,
            BTTVAnimated
        }
    }
}
