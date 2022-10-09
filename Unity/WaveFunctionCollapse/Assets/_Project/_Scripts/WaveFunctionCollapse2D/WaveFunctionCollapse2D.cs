using System.Collections;
using System.Collections.Generic;
using Util;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using static WaveFunctionCollapse2D.TileSettings;

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
        [SerializeField] private Tilemap _mapRoot;


        [SerializeField] private int seed;

        public float AutoIterationDelay { get; set; } = 0.5f;
        public bool IsReadyToIterate { get; private set; } = false;
        public bool IsCompleted { get; private set; } = false;

        private readonly HashSet<ConfigureableTile> _completedTiles = new();
        private readonly HashSet<ConfigureableTile> _inprogressTiles = new();
        private readonly HashSet<ConfigureableTile> _sleeptingTiles = new();



        //Includes all supported rotations as well
        private List<TileSettings> _allSettings = new();


        private ConfigureableTile[][] _map = new ConfigureableTile[0][];

        private Coroutine _simulationRoutine;


        private void Awake()
        {
            this.DisableIfNull(_globalSettings, "Missing Global Settings assignment!");
        }

        //Destroy all instantiated settings, so we do not leek them
        private void OnDestroy()
        {
            CleanUp();
        }

        private void Start()
        {
            Generate();
            Solve();
        }

        public void Update()
        {
            foreach (var one in _inprogressTiles)
            {
                one.UpdateDisplay();
            }
        }

        public void Generate()
        {
            seed++;
            CleanUp();
            SetUp();
        }

        public void Iterate()
        {
            StopAutoIterate();
            DoIterate();
        }

        public void Solve()
        {
            if (IsCompleted)
            {
                Generate();
            }

            int i = 0;
            while (!IsCompleted)
            {
                Iterate();
                //Add saftey condition
                if (i++ > 10000000)  break;
            }
        }

        public void StartAutoIterate()
        {
            if (_simulationRoutine != null) return;
            _simulationRoutine = StartCoroutine(Simulation_Routine());
        }

        public void StopAutoIterate()
        {
            if (_simulationRoutine != null)
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
                if (_sleeptingTiles.Count == 0 && _inprogressTiles.Count == 0)
                {
                    IsReadyToIterate = false;
                    IsCompleted = true;
                }
            }
        }

        private IEnumerator Simulation_Routine()
        {
            while (IsReadyToIterate)
            {
                DoIterate();
                yield return new WaitForSeconds(AutoIterationDelay);
                if (IsCompleted)
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
            _allSettings.Clear();

            for (int x = 0; x < _map.Length; x++)
            {
                for (int y = 0; y < _map[x].Length; y++)
                {
                    _map[x][y] = null;
                }
            }
            _sleeptingTiles.Clear();
            _completedTiles.Clear();
            _inprogressTiles.Clear();

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
                AddTileWithRotationsAndReflections(one.Settingss);
            }


            if (_map.Length != _globalSettings.MapSize.x)
            {
                _map = new ConfigureableTile[_globalSettings.MapSize.x][];
            }
            for (int x = 0; x < _globalSettings.MapSize.x; x++)
            {
                if (_map[x] == null || _map[x].Length != _globalSettings.MapSize.y)
                {
                    _map[x] = new ConfigureableTile[_globalSettings.MapSize.y];
                }
                for (int y = 0; y < _globalSettings.MapSize.y; y++)
                {
                    ConfigureableTile newTile = new();
                    newTile.Init(_globalSettings, new Vector3Int(x, y, 0), _allSettings, _mapRoot);
                    _map[x][y] = newTile;
                    _sleeptingTiles.Add(newTile);
                }
            }

            IsReadyToIterate = true;
            IsCompleted = false;
        }

        private void AddTileWithRotationsAndReflections(TileSettings setting)
        {
            _allSettings.Add(setting.Clone());
            AddTileRotationIfSupported(setting, Rotation.D90);
            AddTileRotationIfSupported(setting, Rotation.D180);
            AddTileRotationIfSupported(setting, Rotation.D270);
            AddTileReflectionIfSupported(setting, Reflection.X);
            AddTileReflectionIfSupported(setting, Reflection.Y);
            AddTileReflectionIfSupported(setting, Reflection.D1);
            AddTileReflectionIfSupported(setting, Reflection.D2);
        }

        private void AddTileRotationIfSupported(TileSettings setting, Rotation rotation)
        {
            if (setting.supportedRotations.HasFlag(rotation))
            {
                _allSettings.Add(setting.GetRotatedVersion(rotation));
            }
        }

        private void AddTileReflectionIfSupported(TileSettings settings, Reflection reflection)
        {
            if(settings.supportedReflections.HasFlag(reflection))
            {
                _allSettings.Add(settings.GetReflectedVersion(reflection));
            }
        }

        public void Iterate(Vector3Int position)
        {
            var tile = _map[position.x][position.y];
            tile.ForceSolution();
            _inprogressTiles.Remove(tile);
            _sleeptingTiles.Remove(tile);
            _completedTiles.Add(tile);
            PropagateTile(tile);
        }

        private readonly PropagationStack _stack = new();
        private void PropagateTile(ConfigureableTile tile)
        {
            _stack.Clear();
            AddNeighbours(tile, _stack);

            while (_stack.Count > 0)
            {
                var neighbourData = _stack.Pop();
                //We may have solved this tile already, so we do not want to add it multiple times to the stack
                if (neighbourData.neighbour.IsCompleted) continue;
                _sleeptingTiles.Remove(neighbourData.neighbour);
                //Only continue propagation if we found a solution
                if (neighbourData.neighbour.Propagate(neighbourData.source))
                {
                    _inprogressTiles.Remove(neighbourData.neighbour);
                    _completedTiles.Add(neighbourData.neighbour);
                    //Add its non solved neighbours to the stack as well, if not in error state
                    if (neighbourData.neighbour.IsSolved)
                    {
                        AddNeighbours(neighbourData.neighbour, _stack);
                    }
                }
                else
                {
                    //Entropy should changed so we add it to in progress list
                    _inprogressTiles.Add(neighbourData.neighbour);
                }
            }
        }

        private void AddNeighbours(ConfigureableTile updatedTile, PropagationStack stack)
        {
            Vector3Int left = updatedTile.Coordinates + Vector3Int.left;
            Vector3Int right = updatedTile.Coordinates + Vector3Int.right;
            Vector3Int bottom = updatedTile.Coordinates + Vector3Int.down;
            Vector3Int top = updatedTile.Coordinates + Vector3Int.up;

            TryAddNeighbour(stack, updatedTile, left);
            TryAddNeighbour(stack, updatedTile, right);
            TryAddNeighbour(stack, updatedTile, bottom);
            TryAddNeighbour(stack, updatedTile, top);
        }

        private bool TryAddNeighbour(PropagationStack stack, ConfigureableTile updatedTile, Vector3Int coordinates)
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

        private bool IsValidCoordinate(Vector3Int coordinate)
        {
            if (coordinate.x < 0 || coordinate.x >= _map.Length) return false;
            if (coordinate.y < 0 || coordinate.y >= _map[coordinate.x].Length) return false;
            return true;
        }


        private readonly List<ConfigureableTile> _possibleTiles = new List<ConfigureableTile>();
        private ConfigureableTile GetLowestEnropyTile()
        {
            _possibleTiles.Clear();
            int entropy = int.MaxValue;

            //In progress tiles will always have lower entropy than sleeping
            if (_inprogressTiles.Count > 0)
            {
                foreach (var tile in _inprogressTiles)
                {
                    Debug.Assert(!tile.IsCompleted, "Completed tile found in inprogress list!", this);
                    if (tile.Entropy < entropy)
                    {
                        entropy = tile.Entropy;
                        _possibleTiles.Clear();
                        _possibleTiles.Add(tile);
                    }
                    else if (tile.Entropy == entropy)
                    {
                        _possibleTiles.Add(tile);
                    }
                }

                if (_possibleTiles.Count > 0)
                {
                    return _possibleTiles[Random.Range(0, _possibleTiles.Count)];
                }
            }
            else if (_sleeptingTiles.Count > 0)
            {
                //TODO: return random elemtnt with somewhat decent speed
                return _sleeptingTiles.First();
            }
            return null;
        }
    }
}
