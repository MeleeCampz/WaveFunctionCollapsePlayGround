using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WaveFunctionCollapse2D
{
    public class ConfigureableTile : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private RectTransform _rectTransform;

        public SOGlobalTileSettings GlobalSettings { get; private set; }
        public SOSTileSettings Settings { get; private set; }

        public void Init(SOGlobalTileSettings globalSettings, SOSTileSettings settings)
        {
            GlobalSettings = globalSettings;
            Settings = settings;

            if(!TryGetComponent(out _rectTransform))
            {
                Debug.LogError("Missing Rect Transform Component!", this);
                return;
            }

            _image.sprite = settings.Sprite;
            _rectTransform.sizeDelta = GlobalSettings.TileSize;
        }
    }
}
