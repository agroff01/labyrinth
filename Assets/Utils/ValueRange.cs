using System;
using UnityEngine;

namespace CustomUtils
{
    [Serializable]
    public struct ValueRange3D
    {
        public Vector3 Min;
        public Vector3 Max;

        public ValueRange3D (Vector3 minimum = default, Vector3 maximum = default)
        {
            Min = minimum;
            Max = maximum;
        }

        public ValueRange3D (float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            Min = new(minX, minY, minZ);
            Max = new(maxX, maxY, maxZ);
        }
    }

    [Serializable]
    public struct ValueRange2D
    {
        public Vector2 Min;
        public Vector2 Max;

        public ValueRange2D (Vector2 minimum = default, Vector2 maximum = default)
        {
            Min = minimum;
            Max = maximum;
        }

        public ValueRange2D (float minX, float minY, float maxX, float maxY)
        {
            Min = new(minX, minY);
            Max = new(maxX, maxY);
        }
    }

    [Serializable]
    public struct ValueRange2DInt
    {
        public Vector2Int Min;
        public Vector2Int Max;

        public ValueRange2DInt (Vector2Int minimum = default, Vector2Int maximum = default)
        {
            Min = minimum;
            Max = maximum;
        }

        public ValueRange2DInt (int minX, int minY, int maxX, int maxY)
        {
            Min = new(minX, minY);
            Max = new(maxX, maxY);
        }


        public static ValueRange2DInt InverseMaxed => new(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);


        public override readonly string ToString()
        {
            return $"Min: [{Min}] - Max: [{Max}]";
        }
    }
    
}

