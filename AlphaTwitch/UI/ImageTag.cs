//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using BeatSaberMarkupLanguage.Tags;
//using UnityEngine.UI;
//using BeatSaberMarkupLanguage;

//namespace AlphaTwitch.UI
//{
//    public class ImageTag : BSMLTag
//    {
//        public override string[] Aliases => new[] { "image" };

//        public override GameObject CreateObject(Transform parent)
//        {
//            GameObject gameObject = new GameObject("ATImage");

//            RawImage image = gameObject.AddComponent<RawImage>();
//            image.material = Utilities.Sprites.NoGlowMat;
//            image.rectTransform.sizeDelta = new Vector2(20f, 20f);
//            image.rectTransform.SetParent(parent, false);
//            image.texture = Utilities.Sprites.BlankSprite.texture;

//            return gameObject;
//        }
//    }
//}
