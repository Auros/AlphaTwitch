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

namespace AlphaTwitch.UI
{
    public class TwitchChannelViewController : BSMLResourceViewController
    {
        public override string ResourceName => "AlphaTwitch.Views.twitchchannelinfo.bsml";

        [UIComponent("channelpic")]
        private RawImage rawImage;

        [UIAction("press")]
        private void ButtonPress()
        {
            GenerateTempImage();
        }


        public void GenerateTempImage()
        {
            SharedCoroutineStarter.instance.StartCoroutine(Utilities.LoadScripts.LoadSpriteCoroutine("https://static-cdn.jtvnw.net/jtv_user_pictures/3aecb867-eec6-4a9e-94aa-04a9da9b1992-profile_image-300x300.png", (image) =>
            {
                rawImage.texture = image.texture;
            }));
        }
    }
}
