using AlphaTwitch.UniGif;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AlphaTwitch.Components
{
    public class GIFer : MonoBehaviour
    {
        private UniGifImage gifimage;
        private RawImage _panelLeftG;
        private Canvas _imageCanvas;

        public void Create(string url)
        {
            gameObject.transform.position = new Vector3(0f, 0f, 0f);
            gameObject.transform.eulerAngles = new Vector3(0f, 0f, 0f);
            gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            _imageCanvas = gameObject.AddComponent<Canvas>();
            _imageCanvas.renderMode = RenderMode.WorldSpace;

            _panelLeftG = new GameObject("GIFPanelImageLeft").AddComponent<RawImage>();
            _panelLeftG.material = Utilities.Sprites.NoGlowMat;
            _panelLeftG.rectTransform.SetParent(_imageCanvas.transform, false);
            _panelLeftG.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _panelLeftG.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _panelLeftG.rectTransform.anchoredPosition = new Vector2(4.5f, 4.5f);
            _panelLeftG.rectTransform.sizeDelta = new Vector2(7f, 7f);

            gifimage = _panelLeftG.gameObject.AddComponent<UniGifImage>();
            UniGifImageAspectController ugiac = _panelLeftG.gameObject.AddComponent<UniGifImageAspectController>();
            gifimage.SetPrivateField("m_imgAspectCtrl", ugiac);
            _panelLeftG.color = new Color(1, 1, 1, .6f);
            _panelLeftG.enabled = false;

            StartCoroutine(SetGIF(url));
        }

        private IEnumerator SetGIF(string url)
        {
            if (gifimage != null)
            {
                Coroutine gifC = null;
                _panelLeftG.enabled = false;
                try
                {
                    gifC = StartCoroutine(gifimage.SetGifFromUrlCoroutine(url, false));
                }
                catch
                {
                }
                yield return gifC;
            }
        }

        public void ChangeOpacity(float value)
        {
            _panelLeftG.material.color = new Color(1, 1, 1, value);
        }

        public void MoveUpBy(float y)
        {
            _panelLeftG.rectTransform.position = new Vector3(_panelLeftG.rectTransform.position.x, _panelLeftG.rectTransform.position.y + y, _panelLeftG.rectTransform.position.z);
        }

        private IEnumerator HideGifAfterTime()
        {
            int counter = 20;
            while (counter > 0)
            {
                counter--;
                MoveUpBy(.01f);
                yield return new WaitForSecondsRealtime(.05f);
            }

            yield return new WaitForSecondsRealtime(1.3f);

            HideGIFer();
        }

        public void EnableGIFer(Vector3 pos)
        {
            _panelLeftG.rectTransform.position = pos;
            _panelLeftG.enabled = true;
            gifimage.Play();
            StartCoroutine(HideGifAfterTime());
        }

        public void HideGIFer()
        {
            _panelLeftG.enabled = false;
            gifimage.Stop();
        }

        /// <summary>
        /// EMERGENCY USE ONLY
        /// </summary>
        public void DestroyGIFer()
        {
            _panelLeftG.enabled = false;
            Destroy(gifimage);
            Destroy(_panelLeftG);
            Destroy(_imageCanvas);
            Destroy(this);
        }
    }
}
