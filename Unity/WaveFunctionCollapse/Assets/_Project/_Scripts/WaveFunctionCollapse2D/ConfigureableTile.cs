//#define DASSERT

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse2D
{
    public class ConfigureableTile
    {
        public SOGlobalTileSettings GlobalSettings { get; private set; }

        public Vector3Int Coordinates { get; private set; }

        public int Entropy { get; private set; } = -1;

        public bool IsSolved { get; private set; }
        public bool IsError { get; private set; }

        public bool IsCompleted { get; private set; }

        public SOTileSettings Solution { get; private set; }

        public Tilemap Map { get; private set; }

        private readonly List<SOTileSettings> _validTiles = new List<SOTileSettings>();
        private bool _dirtyDisplayData = false;

        public void Init(SOGlobalTileSettings globalSettings, Vector3Int coordinates, IEnumerable<SOTileSettings> allSolutions, Tilemap map)
        {
            GlobalSettings = globalSettings;
            Coordinates = coordinates;
            Map = map;

            _validTiles.Clear();
            _validTiles.AddRange(allSolutions);

            CheckForSolution();
            UpdateDisplay();
        }

        public void ForceSolution()
        {
            Solution = _validTiles[Random.Range(0, Entropy)];
            _validTiles.RemoveAll(x => x != Solution);
            CheckForSolution();
        }

        /// <summary>
        /// Updates possible solutions basesd on neigbour information
        /// </summary>
        /// <param name="neighbor"></param>
        /// <returns>True if this tile is now solved or if it is in an unsolveable error state </returns>
        public bool Propagate(ConfigureableTile neighbor)
        {
            if (IsCompleted) return false;

#if DASSERT
            Debug.AssertFormat((neighbor.Coordinates - Coordinates).sqrMagnitude ==  1, "Argument is not a neighbour!", this);
            Debug.AssertFormat(neighbor.IsSolved, "Neighbour is not solved!", this);
            Debug.AssertFormat(!neighbor.IsError, "Neighbour is in error state!", this);
#endif

            int beforeCount = _validTiles.Count;

            //Left Neightbor
            if      (neighbor.Coordinates.x < Coordinates.x) _validTiles.RemoveAll(x => x.leftConnectionType != neighbor.Solution.righConnectionType);
            //Right Neightbor
            else if (neighbor.Coordinates.x > Coordinates.x) _validTiles.RemoveAll(x => x.righConnectionType != neighbor.Solution.leftConnectionType);
            //Bottom Neightbor
            else if (neighbor.Coordinates.y < Coordinates.y) _validTiles.RemoveAll(x => x.bottomConnectionType != neighbor.Solution.topConnectionType);
            //Top Neightbor
            else                                             _validTiles.RemoveAll(x => x.topConnectionType != neighbor.Solution.bottomConnectionType);

            CheckForSolution();
            return IsCompleted;
        }

        public void UpdateDisplay()
        {
            if (!_dirtyDisplayData || Map ==  null) return;

            if (IsError)
            {
                Map.SetTile(Coordinates * GlobalSettings.TileSize, GlobalSettings.ErrorTile);
            }
            else if (Solution == null)
            {
                Map.SetTile(Coordinates * GlobalSettings.TileSize,  GlobalSettings.NotSolvedTile);
            }
            //Solved
            else
            {

                Vector3Int location = Coordinates * GlobalSettings.TileSize;
                Map.SetTile(location, Solution.Tile);

                float rotation = SOTileSettings.GetRotationAngle(Solution.CurrentRotation);
                Map.SetTransformMatrix(location, Matrix4x4.Rotate(Quaternion.Euler(Vector3.back * rotation)));
            }

            _dirtyDisplayData = false;
        }

        private void CheckForSolution()
        {
            int beforeCount = Entropy;
            Entropy = _validTiles.Count;

            //Skip if nothing changed
            if (beforeCount == Entropy)  return;

            _dirtyDisplayData = true;
            if (Entropy == 0)
            {
                IsError = true;
                IsCompleted = true;
                Solution = null;
                IsSolved = false;
            }
            else if (Entropy > 1)
            {
                IsError = false;
                IsCompleted = false;
                Solution = null;
                IsSolved = false;
            }
            else
            {
                IsError = false;
                IsCompleted = true;
                Solution = _validTiles[0];
                IsSolved = true;
            }

            if(IsCompleted)
            {
                UpdateDisplay();
            }
        }
    }
}
