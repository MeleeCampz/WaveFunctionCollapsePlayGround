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

        [SerializeField] private Vector2Int _mapSize;
        public Vector2Int MapSize => _mapSize;

        [SerializeField] private List<SOTileSettings> _allTileSettings;
        public IReadOnlyList<SOTileSettings> AllTileSettings => _allTileSettings;

        [SerializeField] private Sprite _notSolvedSprite;
        public Sprite NotSolvedSprite => _notSolvedSprite;

        [SerializeField] private Sprite _errorSprite;
        public Sprite ErrorSprite => _errorSprite;
    }
}
