using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse2D
{
    [CreateAssetMenu(menuName = WFC2_Constants.MENU_NAME +  "SOGloablTileSettings", fileName = "SOGlobalTileSettings")]
    public class SOGlobalTileSettings : ScriptableObject
    {
        [SerializeField] private Vector3Int _tileSize;
        public Vector3Int TileSize => _tileSize;

        [SerializeField] private Vector2Int _mapSize;
        public Vector2Int MapSize => _mapSize;

        [SerializeField] private List<SOTileSettings> _allTileSettings;
        public IReadOnlyList<SOTileSettings> AllTileSettings => _allTileSettings;

        [SerializeField] private Tile _notSolvedTile;
        public Tile NotSolvedTile => _notSolvedTile;

        [SerializeField] private Tile _errorTile;
        public Tile ErrorTile => _errorTile;

        [System.Serializable]
        public class IntList
        {
            public string DisplayName;
            public List<int> data;
        }

        //TODO Make this into bitmask instead of this mess!!
        public List<IntList> AllowedConnection; 
    }
}
