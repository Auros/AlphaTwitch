using BeatSaberMarkupLanguage;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRUI;

namespace AlphaTwitch.UI
{
    class AlphaTwitchFlowCoordinator : FlowCoordinator
    {
        public MainFlowCoordinator mainFlowCoordinator;
        private TwitchChannelViewController _twitchChannelVC;
        protected DismissableNavigationController _dismissableNavController;

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            if (firstActivation && activationType == ActivationType.AddedToHierarchy)
            {
                title = "Alpha Twitch";

                _twitchChannelVC = BeatSaberUI.CreateViewController<TwitchChannelViewController>();

                _dismissableNavController = Instantiate(Resources.FindObjectsOfTypeAll<DismissableNavigationController>().First());
                _dismissableNavController.didFinishEvent += Dismiss;

                SetViewControllersToNavigationConctroller(_dismissableNavController, _twitchChannelVC);
                ProvideInitialViewControllers(_dismissableNavController);
                _twitchChannelVC.GenerateTempImage();
            }
        }

        private void Dismiss(DismissableNavigationController navController)
        {
            (mainFlowCoordinator as FlowCoordinator).InvokePrivateMethod("DismissFlowCoordinator", new object[] { this, null, false });
        }
    }
}
