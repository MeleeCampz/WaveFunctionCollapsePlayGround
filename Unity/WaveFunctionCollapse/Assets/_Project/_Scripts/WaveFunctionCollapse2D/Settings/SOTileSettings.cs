using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace WaveFunctionCollapse2D
{
    [CreateAssetMenu(menuName = WFC2_Constants.MENU_NAME + "SOTileSettings", fileName = "SOTileSettings_")]

    public class SOTileSettings : ScriptableObject
    {
        public TileSettings Settingss;
    }

    [System.Serializable]
    public class TileSettings
    {
        [Flags]
        public enum Rotation : int
        {
            None = 0,
            D90 = 1,
            D180 = 2,
            D270 = 4,
            All = D90 | D180 | D270,
        }

        [Flags]
        public enum Reflection : int
        {
            None = 0,
            X = 1,
            Y = 2,
            //Lower left to upper right axis
            D1 = 4,
            //Lower right to upper left axis
            D2 = 8,
            All = X | Y | D1 | D2,
        }

        public enum Direction : int
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3,
        }

        public Tile Tile;

        public Rotation CurrentRotation { get; private set; } = Rotation.None;
        public Reflection CurrentReflection { get; private set; } = Reflection.None;
        public Matrix4x4 Matrix { get; private set; } = Matrix4x4.identity;

        public Rotation supportedRotations = Rotation.None;
        public Reflection supportedReflections = Reflection.None;

        [Header("Order: Up, Right, Down, Left")]
        public int[] connections;

        public TileSettings Clone()
        {
            var ret = new TileSettings();
            ret.Tile = Tile;
            ret.supportedRotations = supportedRotations;
            ret.supportedReflections = supportedReflections;
            ret.connections = new int[4];
            connections.CopyTo(ret.connections, 0);
            return ret;
        }

        public TileSettings GetRotatedVersion(Rotation rotation)
        {
            var ret = Clone();
            ret.ApplyRotation(rotation);
            return ret;
        }

        public TileSettings GetReflectedVersion(Reflection reflection)
        {
            var ret = Clone();
            ret.ApplyReflection(reflection);
            return ret;
        }

        private readonly int[] offsettesSettings = new int[4];
        public void ApplyRotation(Rotation rotation)
        {
            if (rotation == CurrentRotation) return;

            int offset = RotationToOffsetIndex(rotation) - RotationToOffsetIndex(CurrentRotation);
            for (int i = 0; i < 4; i++)
            {
                offsettesSettings[i] = connections[(i + offset) % 4];
            }

            offsettesSettings.CopyTo(connections, 0);
            connections = offsettesSettings;

            CurrentRotation = rotation;
            Matrix = GetRotationMatrix(CurrentRotation);
        }

        public void ApplyReflection(Reflection reflection)
        {
            if (reflection == CurrentReflection) return;

            int temp;
            switch (reflection)
            {
                case Reflection.X:
                    temp = connections[0];
                    connections[0] = connections[2];
                    connections[2] = temp;
                    break;
                case Reflection.Y:
                    temp = connections[1];
                    connections[1] = connections[3];
                    connections[3] = temp;
                    break;
                case Reflection.D1:
                    temp = connections[0];
                    connections[0] = connections[1];
                    connections[1] = temp;
                    temp = connections[2];
                    connections[2] = connections[3];
                    connections[3] = temp;
                    break;
                case Reflection.D2:
                    temp = connections[0];
                    connections[0] = connections[3];
                    connections[3] = temp;
                    temp = connections[1];
                    connections[1] = connections[2];
                    connections[2] = temp;
                    break;
            }

            CurrentReflection = reflection;
            Matrix = GetReflectionMatrix(CurrentReflection);
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

        public static Matrix4x4 MatD90 = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90));
        public static Matrix4x4 MatD180 = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180));
        public static Matrix4x4 MatD270 = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270));

        public static Matrix4x4 GetRotationMatrix(Rotation rotation)
          => rotation switch
          {
              Rotation.D90 => MatD90,
              Rotation.D180 => MatD180,
              Rotation.D270 => MatD270,
              _ => Matrix4x4.identity,
          };

        public static readonly Matrix4x4 XReflect = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
        public static readonly Matrix4x4 YReflect = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
        public static readonly Matrix4x4 D1Reflect = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, -1, 1));
        public static readonly Matrix4x4 D2Reflect = Matrix4x4.Transpose(D1Reflect);

        public static Matrix4x4 GetReflectionMatrix(Reflection reflection)
         => reflection switch
         {
             Reflection.X => XReflect,
             Reflection.Y => YReflect,
             Reflection.D1 => D1Reflect,
             Reflection.D2 => D2Reflect,
             _ => Matrix4x4.identity,
         };
    }
}
