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

        public TileSettings Solution { get; private set; }

        public Tilemap Map { get; private set; }

        private readonly List<TileSettings> _validTiles = new List<TileSettings>();
        private bool _dirtyDisplayData = false;

        public void Init(SOGlobalTileSettings globalSettings, Vector3Int coordinates, IEnumerable<TileSettings> allSolutions, Tilemap map)
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

            //Left Neightbor
            if (neighbor.Coordinates.x < Coordinates.x)         RemoveInvalidConnnection(3, neighbor.Solution.connections[1]);
            //Right Neightbor
            else if (neighbor.Coordinates.x > Coordinates.x)    RemoveInvalidConnnection(1, neighbor.Solution.connections[3]);
            //Bottom Neightbor
            else if (neighbor.Coordinates.y < Coordinates.y)    RemoveInvalidConnnection(2, neighbor.Solution.connections[0]);
            //Top Neightbor
            else                                                RemoveInvalidConnnection(0, neighbor.Solution.connections[2]);

            CheckForSolution();
            return IsCompleted;
        }

        private void RemoveInvalidConnnection(int directionOfNeighbour, int solution)
        {
            List<int> allowedConnections = GlobalSettings.AllowedConnection[solution].data;
            _validTiles.RemoveAll(x => !allowedConnections.Contains(x.connections[directionOfNeighbour]));
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
                Map.SetTransformMatrix(location, Solution.Matrix);
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
