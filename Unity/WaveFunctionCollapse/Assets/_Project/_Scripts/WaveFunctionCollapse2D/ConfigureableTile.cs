using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Security;
using System.Linq;

namespace WaveFunctionCollapse2D
{
    public class ConfigureableTile : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _debugText;
        [SerializeField] private RectTransform _rectTransform;

        public SOGlobalTileSettings GlobalSettings { get; private set; }
        //public SOSTileSettings Settings { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2Int Coordinates { get; private set; }

        public int Entropy => _validTiles.Count;

        public bool IsSolved => Solution != null;
        public bool IsError { get; private set; }

        public bool IsCompleted => IsSolved || IsError;

        public SOTileSettings Solution { get; private set; }

        private readonly List<SOTileSettings> _validTiles = new List<SOTileSettings>();

        public void Init(SOGlobalTileSettings globalSettings, Vector2Int coordinates, Vector2 position, IEnumerable<SOTileSettings> allSolutions)
        {
            Debug.Assert(_image, "Missing image assignement!", this);
            Debug.Assert(_debugText, "Missing Debug Test assignement!", this);
            Debug.Assert(_rectTransform, "Missing Rect Transform assignement!", this);

            GlobalSettings = globalSettings;

            Coordinates = coordinates;
            Position = position;

            _rectTransform.sizeDelta = GlobalSettings.TileSize;
            _rectTransform.anchoredPosition = position;

            _validTiles.AddRange(allSolutions);

            CheckForSolution();
            UpdateDisplay();
        }

        public void ForceSolution()
        {
            Solution = _validTiles[Random.Range(0, _validTiles.Count)];
            _validTiles.RemoveAll(x => x != Solution);

            UpdateDisplay();
        }

        /// <summary>
        /// Updates possible solutions basesd on neigbour information
        /// </summary>
        /// <param name="neighbor"></param>
        /// <returns>True if the set of solutions has changed</returns>
        public bool Propagate(ConfigureableTile neighbor)
        {
            if (IsCompleted) return false;

            Debug.Assert((neighbor.Coordinates - Coordinates).sqrMagnitude ==  1, "Argument is not a neighbour!", this);
            Debug.Assert(neighbor.IsSolved, "Neighbour is not solved!", this);

            //Remove all tiles that are no longer allowed
            //Left Neightbor
            if      (neighbor.Coordinates.x < Coordinates.x) _validTiles.RemoveAll(x => x.leftConnectionType != neighbor.Solution.righConnectionType);
            //Right Neightbor
            else if (neighbor.Coordinates.x > Coordinates.x) _validTiles.RemoveAll(x => x.righConnectionType != neighbor.Solution.leftConnectionType);
            //Bottom Neightbor
            else if (neighbor.Coordinates.y < Coordinates.y) _validTiles.RemoveAll(x => x.bottomConnectionType != neighbor.Solution.topConnectionType);
            //Top Neightbor
            else                                             _validTiles.RemoveAll(x => x.topConnectionType != neighbor.Solution.bottomConnectionType);

            CheckForSolution();
            UpdateDisplay();
            return IsSolved;
        }

        private bool CheckForSolution()
        {
            Solution = null;
            IsError = false;
            if (_validTiles.Count == 0)
            {
                IsError = true;
                return false;
            }
            else if (_validTiles.Count > 1)
            {
                return false;
            }
            Solution = _validTiles.First();
            return true;
        }

        private void UpdateDisplay()
        {
            _debugText.enabled = false;
            if (IsError)
            {
                _image.sprite = GlobalSettings.ErrorSprite;
            }
            else if (Solution == null)
            {
                _image.sprite = GlobalSettings.NotSolvedSprite;
                _debugText.text = _validTiles.Count.ToString();
                _debugText.enabled = true;
            }
            //Solved
            else
            {
                _image.sprite = Solution.Sprite;
                switch (Solution.CurrentRotation)
                {
                    case SOTileSettings.Rotation.D90:
                        _rectTransform.localEulerAngles = Vector3.back * 90.0f;
                        break;
                    case SOTileSettings.Rotation.D180:
                        _rectTransform.localEulerAngles = Vector3.back * 180;
                        break;
                    case SOTileSettings.Rotation.D270:
                        _rectTransform.localEulerAngles = Vector3.back * 270;
                        break;
                }

            }
        }
    }
}
