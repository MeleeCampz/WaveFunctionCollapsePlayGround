using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse2D
{
    [CreateAssetMenu(menuName = WFC2_Constants.MENU_NAME + "SOTileSettings", fileName = "SOTileSettings_")]
    public class SOSTileSettings : ScriptableObject
    {
        [SerializeField] private Sprite _sprite;
        public Sprite Sprite => Sprite;
    }
}
