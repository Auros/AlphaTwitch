using IPA;
using IPA.Config;
using IPA.Utilities;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using StreamCore.Chat;
using System.Reflection;
using Harmony;

namespace AlphaTwitch
{
    public class Plugin : IBeatSaberPlugin
    {
        public void Init(IPALogger logger)
        {
            Logger.log = logger;
        }

        public void OnApplicationStart()
        {
            TwitchWebSocketClient.Initialize();
            _ = TwitchCore.Instance;
            _ = AlphaTwitchManager.Instance;

            HarmonyInstance.Create("com.auros.BeatSaber.AlphaTwitch").PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnApplicationQuit()
        {
            
        }

        public void OnFixedUpdate()
        {

        }

        public void OnUpdate()
        {

        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {

        }

        public void OnSceneUnloaded(Scene scene)
        {

        }
    }
}
