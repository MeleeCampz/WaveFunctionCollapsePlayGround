using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse2D
{
    [CreateAssetMenu(menuName = WFC2_Constants.MENU_NAME +  "SOGloablTileSettings", fileName = "SOGlobalTileSettings")]
    public class SOGlobalTileSettings : ScriptableObject
    {
        [SerializeField] private Vector2 _tileSize;
        public Vector2 TileSize => _tileSize;
    }
}
