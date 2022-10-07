using System.Collections;
using System.Collections.Generic;
using Util;
using UnityEngine;
using System.Linq;

namespace WaveFunctionCollapse2D
{
    public class WaveFunctionCollapse2D : MonoBehaviour
    {
        private struct NeighbourData
        {
            public ConfigureableTile source;
            public ConfigureableTile neighbour;

            public NeighbourData(ConfigureableTile source, ConfigureableTile neighbour)
            {
                this.source = source;
                this.neighbour = neighbour;
            }
        }
        private class PropagationStack : Stack<NeighbourData> { }

        [Header("Basic generation settings")]
        [SerializeField] private SOGlobalTileSettings _globalSettings;
        [SerializeField] private ConfigureableTile _tileBasePrefab;
        [SerializeField] private RectTransform _mapRoot;

        [SerializeField] private int seed;

        public float AutoIterationDelay { get; set; } = 0.5f;
        public bool IsReadyToIterate { get; private set; } = false;
        public bool IsCompleted { get; private set; } = false;

        private readonly HashSet<ConfigureableTile> _allTiles = new HashSet<ConfigureableTile>();
        //Includes all supported rotations as well
        private List<SOTileSettings> _allSettings = new List<SOTileSettings>();

        private ConfigureableTile[][] _map;

        private Coroutine _simulationRoutine;

        private void Awake()
        {
            this.DisableIfNull(_globalSettings, "Missing Global Settings assignment!");
            this.DisableIfNull(_tileBasePrefab, "Missing Tile Base Prefab!");
        }

        //Destroy all instantiated settings, so we do not leek them
        private void OnDestroy()
        {
            CleanUp();
        }

        public void Generate()
        {
            CleanUp();
            SetUp();
        }

        public void Iterate()
        {
            StopAutoIterate();
            DoIterate();
        }

        public void StartAutoIterate()
        {
            if (_simulationRoutine != null) return;
            _simulationRoutine = StartCoroutine(Simulation_Routine());
        }

        public void StopAutoIterate()
        {
            if(_simulationRoutine != null)
            {
                StopCoroutine(_simulationRoutine);
                _simulationRoutine = null;
            }
        }

        private void DoIterate()
        {
            var tile = GetLowestEnropyTile();
            if (!IsCompleted && IsReadyToIterate)
            {
                Iterate(tile.Coordinates);
                if(GetLowestEnropyTile() == null)
                {
                    IsCompleted = true;
                    IsReadyToIterate = false;
                }
            }
        }

        private IEnumerator Simulation_Routine()
        {
            while(IsReadyToIterate)
            {
                DoIterate();
                yield return new WaitForSeconds(AutoIterationDelay);
                if(IsCompleted)
                {
                    _simulationRoutine = null;
                    yield break;
                }
            }
        }


        /// <summary>
        /// Destory all instatiated tiles and reset the internal state
        /// </summary>
        public void CleanUp()
        {
            foreach (var one in _allSettings)
            {
                Destroy(one);
            }
            _allSettings.Clear();

            //TODO: Object pooling
            foreach (var one in _allTiles)
            {
                Destroy(one.gameObject);
            }
            _allTiles.Clear();

            IsReadyToIterate = false;
            IsCompleted = false;
        }

        /// <summary>
        /// Setup the initial set of tile in their super position
        /// </summary>
        private void SetUp()
        {
            Random.InitState(seed);

            foreach (var one in _globalSettings.AllTileSettings)
            {
                AddTileWithRotations(one);                
            }
            

            _map = new ConfigureableTile[_globalSettings.MapSize.x][];
            for (int x = 0; x < _globalSettings.MapSize.x; x++)
            {
                _map[x] = new ConfigureableTile[_globalSettings.MapSize.y];
                for (int y = 0; y < _globalSettings.MapSize.x; y++)
                {
                    var newTile = Instantiate(_tileBasePrefab, _mapRoot);
                    newTile.Init(_globalSettings, new Vector2Int(x, y), new Vector2(x * _globalSettings.TileSize.x, y * _globalSettings.TileSize.y), _allSettings);
                    _allTiles.Add(newTile);
                    _map[x][y] = newTile;
                }
            }

            _mapRoot.anchoredPosition = -(_globalSettings.MapSize * _globalSettings.TileSize) * 0.5f * _mapRoot.localScale;

            IsReadyToIterate = true;
            IsCompleted = false;
        }

        private void AddTileWithRotations(SOTileSettings setting)
        {
            _allSettings.Add(Instantiate(setting));
            AddTileRotationIfSupported(setting, SOTileSettings.Rotation.D90);
            AddTileRotationIfSupported(setting, SOTileSettings.Rotation.D180);
            AddTileRotationIfSupported(setting, SOTileSettings.Rotation.D270);
        }

        private void AddTileRotationIfSupported(SOTileSettings setting, SOTileSettings.Rotation rotation)
        {
            if(setting.supportedRotations.HasFlag(rotation))
            {
                var instantiated = Instantiate(setting);
                instantiated.ApplyRotation(rotation);
                _allSettings.Add(instantiated);
            }
        }

        public void Iterate(Vector2Int position)
        {
            var tile = _map[position.x][position.y];
            tile.ForceSolution();
            PropagateTile(tile);
        }

        private void PropagateTile(ConfigureableTile tile)
        {
            PropagationStack stack = new PropagationStack();
            AddNeighbours(stack, tile);


            while (stack.Count > 0)
            {
                var neighbourData = stack.Pop();
                if(neighbourData.neighbour.Propagate(neighbourData.source))
                {
                    AddNeighbours(stack, neighbourData.neighbour);
                }
            }
        }

        private void AddNeighbours(PropagationStack stack, ConfigureableTile updatedTile)
        {
            Vector2Int left = updatedTile.Coordinates + Vector2Int.left;
            Vector2Int right = updatedTile.Coordinates + Vector2Int.right;
            Vector2Int bottom = updatedTile.Coordinates + Vector2Int.down;
            Vector2Int top = updatedTile.Coordinates + Vector2Int.up;

            TryAddNeighbour(stack, updatedTile, left);
            TryAddNeighbour(stack, updatedTile, right);
            TryAddNeighbour(stack, updatedTile, bottom);
            TryAddNeighbour(stack, updatedTile, top);
        }

        private bool TryAddNeighbour(PropagationStack stack, ConfigureableTile updatedTile, Vector2Int coordinates)
        {
            if (IsValidCoordinate(coordinates))
            {
                var neighbour = _map[coordinates.x][coordinates.y];
                if (neighbour.IsCompleted)
                {
                    return false;
                }
                stack.Push(new NeighbourData(updatedTile, neighbour));
                return true;
            }
            return false;
        }

        private bool IsValidCoordinate(Vector2Int coordinate)
        {
            if (coordinate.x < 0 || coordinate.x >= _map.Length) return false;
            if (coordinate.y < 0 || coordinate.y >= _map[coordinate.x].Length) return false;
            return true;
        }

        private ConfigureableTile GetLowestEnropyTile()
        {
            List<ConfigureableTile> possibleTiles = new List<ConfigureableTile>();


            int entropy = -1;
            foreach (var tile in _allTiles.OrderBy(x => x.Entropy))
            {
                if (tile.IsCompleted) continue;
                if (entropy == -1) entropy = tile.Entropy;
                if (tile.Entropy > entropy) break;
                possibleTiles.Add(tile);
            }

            if (possibleTiles.Count > 0)
            {
                return possibleTiles[Random.Range(0, possibleTiles.Count)];
            }
            return null;
        }
    }
}
