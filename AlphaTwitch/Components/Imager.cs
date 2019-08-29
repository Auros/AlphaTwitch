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
    public class Imager : MonoBehaviour
    {
        static Image _panelLeft;
        private Canvas _imageCanvas;
        public void Create()
        {

            gameObject.transform.position = new Vector3(0f, 0f, 0f);
            gameObject.transform.eulerAngles = new Vector3(0f, 0f, 0f);
            gameObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            _imageCanvas = gameObject.AddComponent<Canvas>();
            _imageCanvas.renderMode = RenderMode.WorldSpace;

            var rectTransform = _imageCanvas.transform as RectTransform;
            rectTransform.sizeDelta = new Vector2(100, 50);

            _panelLeft = new GameObject("Imager").AddComponent<Image>();
            _panelLeft.material = Utilities.Sprites.NoGlowMat;
            _panelLeft.rectTransform.SetParent(_imageCanvas.transform, false);
            _panelLeft.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _panelLeft.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _panelLeft.rectTransform.anchoredPosition = new Vector2(4.5f, 4.5f);
            _panelLeft.rectTransform.sizeDelta = new Vector2(15f, 15f);
            _panelLeft.sprite = Utilities.Sprites.BlankSprite;

            _panelLeft.material.color = new Color(1, 1, 1, .6f);
            _panelLeft.material.renderQueue = 4000;

        }

        public void ChangeOpacity(float value)
        {
            _panelLeft.material.color = new Color(1, 1, 1, value);
        }

        public void MoveUpBy(float y)
        {
            _panelLeft.rectTransform.position = new Vector3(_panelLeft.rectTransform.position.x, _panelLeft.rectTransform.position.y + y, _panelLeft.rectTransform.position.z);
        }

        int count = 0;
        private void StageOne()
        {
            if (count < 20)
            {
                MoveUpBy(.01f);
                count++;
            }
            else
            {
                StageTwo();
            }
        }

        private void StageTwo()
        {
            CancelInvoke();
            Invoke("DestroyImager", 1.7f);
        }

        public void EnableImager(Sprite sprite, Vector3 position, float scale)
        {
            _panelLeft.rectTransform.sizeDelta = new Vector2(scale, scale);
            _panelLeft.rectTransform.position = position;
            _panelLeft.sprite = sprite;
            _panelLeft.sprite.texture.wrapMode = TextureWrapMode.Clamp;
            count = 0;
            InvokeRepeating("StageOne", .01f, .05f);
        }
        public void DisableImager()
        {
            _panelLeft.sprite = Utilities.Sprites.BlankSprite;
        }

        public void DestroyImager()
        {
            DisableImager();
            Destroy(_panelLeft);
            Destroy(_imageCanvas);
            Destroy(this);
        }


    }
}
