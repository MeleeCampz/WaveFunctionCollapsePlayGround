using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse2D
{
    [CreateAssetMenu(menuName = WFC2_Constants.MENU_NAME + "SOTileSettings", fileName = "SOTileSettings_")]
    public class SOTileSettings : ScriptableObject
    {
        [System.Flags]
        public enum Rotation : int
        {
            None = 0,
            D90 = 1,
            D180 = 2,
            D270 = 4,
            All = D90 | D180 | D270,
        }

        [SerializeField] private Tile _tile;
        public Tile Tile => _tile;

        public Rotation CurrentRotation { get; private set; } = Rotation.None;
        public Rotation supportedRotations = Rotation.All;

        [Header("Debug - Will be replaced later!")]
        public int[] topConnectionType;
        public int[] righConnectionType;
        public int[] bottomConnectionType;
        public int[] leftConnectionType;

        public void ApplyRotation(Rotation rotation)
        {
            if (rotation == CurrentRotation) return;

            int[][] currentSettings = new int[4][];
            currentSettings[0] = topConnectionType;
            currentSettings[1] = righConnectionType;
            currentSettings[2] = bottomConnectionType;
            currentSettings[3] = leftConnectionType;

            int offset = RotationToOffsetIndex(rotation) - RotationToOffsetIndex(CurrentRotation);
            int[][] offsettesSettings = new int[4][];
            for (int i = 0; i < 4; i++)
            {
                offsettesSettings[(i + offset) % 4] = currentSettings[i];
            }

            topConnectionType = offsettesSettings[0];
            righConnectionType = offsettesSettings[1];
            bottomConnectionType = offsettesSettings[2];
            leftConnectionType = offsettesSettings[3];

            CurrentRotation = rotation;
        }

        public static int RotationToOffsetIndex(Rotation source)
            => source switch
            {
                Rotation.None => 0,
                Rotation.D90 => 1,
                Rotation.D180 => 2,
                Rotation.D270 => 3,
                _ => -1,
            };

        public static float GetRotationAngle(Rotation rotation)
          => rotation switch
          {
              Rotation.D90 => 90f,
              Rotation.D180 => 180f,
              Rotation.D270 => 270f,
              _ => 0f,
          };
    }
}
