using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse2D
{
    public class WaveFunctionCollapse2D : MonoBehaviour
    {
        [Header("Basic generation settings")]
        [SerializeField] private SOGlobalTileSettings _globalSettings;
        [SerializeField] private SOGlobalTileSettings[] _allTileSettings;
        [SerializeField] private ConfigureableTile _tileBasePrefab;
    }
}
