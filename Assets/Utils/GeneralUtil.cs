

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;

namespace CustomUtils
{

    public static class GeneralUtil
    {

        #region Bounds Scaling
        private static Dictionary<GameObject, Bounds?> prefabBoundsCache = new();
        /// <summary>
        /// Retrieves the bounds of a prefab, calculating them if not already cached.
        /// </summary>
        /// <param name="prefab">The prefab GameObject to calculate bounds for.</param>
        /// <returns>The bounds of the prefab, or null if no Renderer is found.</returns>
        public static Bounds? GetPrefabBounds(GameObject prefab, bool excludeInactive = false)
        {
            if (prefabBoundsCache.TryGetValue(prefab, out Bounds? cachedBounds))
            {
                return cachedBounds;
            }

            Bounds? bounds = CalculateBoundsRecursively(prefab.transform, excludeInactive);
            prefabBoundsCache.Add(prefab, bounds);
            return bounds;
        }

        static Bounds? CalculateBoundsRecursively(Transform transform, bool excludeInactive = false)
        {
            Bounds? bounds = null;
            Renderer renderer = transform.GetComponent<Renderer>();

            // Skipping the bounds from particle renderer which might create unexpectedly large prefab bounds.
            if (renderer != null && renderer.bounds.size != Vector3.zero && renderer is not ParticleSystemRenderer)
            {
                // If the current GameObject has a renderer component, include its bounds
                bounds = renderer.bounds;
            }

            // Recursively process children
            foreach (Transform child in transform.transform)
            {
                if (excludeInactive && !child.gameObject.activeSelf) break;

                Bounds? childBounds = CalculateBoundsRecursively(child, excludeInactive);
                if (childBounds != null)
                {
                    if (bounds != null)
                    {
                        var boundsValue = bounds.Value;
                        boundsValue.Encapsulate(childBounds.Value);
                        bounds = boundsValue;
                    }
                    else
                    {
                        bounds = childBounds;
                    }
                }
            }

            return bounds;
        }
        #endregion

        

        public static Ray ToRay(this Transform val)
        {
            return new(val.position, val.forward);
        }

        public static Ray FromDestination(this Ray ray, Vector3 origin, Vector3 destination)
        {
            ray.origin = origin;
            ray.direction = (destination - origin).normalized;
            return ray;
        }

        public static Ray RayFromDestination(Vector3 origin, Vector3 destination) => new Ray().FromDestination(origin, destination);

        public static Renderer SetMaterialColor(this Renderer renderer, Color color, out Color oldColor)
        {
            Material material = renderer.material;
            oldColor = material.color;
            material.color = color;
            renderer.material = material;

            return renderer;
        }

        public static Renderer SetMaterialColor(this Renderer renderer, Color color)
        {
            return renderer.SetMaterialColor(color, out _);
        }

        public static bool HasAnyFlag(this Enum rhs, Enum lhs)
        {
            return (Convert.ToInt32(rhs) & Convert.ToInt32(lhs)) != 0;
        }


        public static T[,] Transpose<T>(this T[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);

            T[,] result = new T[columns, rows];

            // Map values from [r, c] to [c, r]
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    result[c, r] = matrix[r, c];
                }
            }

            return result;
        }

        static public T[,] RotateMatrixClockwise<T>(this T[,] oldMatrix)
        {
            int rows = oldMatrix.GetLength(0);
            int columns = oldMatrix.GetLength(1);
            
            // For non-square grids, rows/cols swap.
            T[,] newMatrix = new T[columns, rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    newMatrix[c, rows - 1 - r] = oldMatrix[r, c];
                }
            }
            return newMatrix;
        }


        static public T[,] RotateMatrixCounterClockwise<T>(this T[,] oldMatrix)
        {
            int rows = oldMatrix.GetLength(0);
            int columns = oldMatrix.GetLength(1);
            
            // For non-square grids, rows/cols swap.
            T[,] newMatrix = new T[columns, rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    newMatrix[columns - 1 - c, r] = oldMatrix[r, c];
                }
            }
            return newMatrix;
        }

        static public string MatrixToString<T>(this T[,] matrix)
        {
            var full = new StringBuilder();
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                full.Insert(0, "\n");

                var line = new StringBuilder();
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    line.Append($"[{i},{j}]" + matrix[i,j] + "\t");
                }
                full.Insert(0,line.ToString());
            }

            return full.ToString();
        }

        #region Pose Helpers

        // Transform Pose Extentions
        public static Pose GetPose(this Transform transform) => new(transform.position, transform.rotation);
        public static Pose GetLocalPose(this Transform transform) => new(transform.localPosition, transform.localRotation);
        public static void SetPose(this Transform transform, Pose pose) => transform.SetPositionAndRotation(pose.position, pose.rotation);
        public static void SetLocalPose(this Transform transform, Pose pose) => transform.SetLocalPositionAndRotation(pose.position, pose.rotation);

        // Game Object Pose Extentions
        public static Pose GetPose(this GameObject gameObject) => gameObject.transform.GetPose();
        public static Pose GetLocalPose(this GameObject gameObject) => gameObject.transform.GetLocalPose();
        public static void SetPose(this GameObject gameObject, Pose pose) => gameObject.transform.SetPose(pose);
        public static void SetLocalPose(this GameObject gameObject, Pose pose) => gameObject.transform.SetLocalPose(pose);

        public static Pose AsFlat(this Pose pose) => pose.AsFlat(Vector3.up);
        public static Pose AsFlat(this Pose pose, Vector3 normal)
        {
            return new Pose(pose.position, Quaternion.LookRotation(Vector3.ProjectOnPlane(pose.forward, normal)));
        }

        #endregion

        public static Quaternion AsFlat(this Quaternion quaternion){ return Quaternion.Euler(quaternion.eulerAngles.WithX(0).WithZ(0)); }

        public static bool isApproximateRotations(Quaternion q1, Quaternion q2, float precision)
        {
            return Mathf.Abs(Quaternion.Dot(q1, q2)) >= 1 - precision;
        }
        public static bool isApproximate(this Quaternion q1, Quaternion q2, float precision)
        {
            return Mathf.Abs(Quaternion.Dot(q1, q2)) >= 1 - precision;
        }
        public static bool isApproximate(this Quaternion q1, Quaternion q2)
        {
            return isApproximate(q1,q2, 0.000004f);
        }

        public static float Remap(this float val, float startMin, float startMax, float endMin, float endMax) => 
            Mathf.Lerp(endMin, endMax, Mathf.InverseLerp(startMin, startMax, val));
        public static float Remap(this float val, float startMin, float startMax, float endMin, float endMax, AnimationCurve curved01)
        {
            var lerp = Mathf.InverseLerp(startMin, startMax, val);
            lerp = curved01.Evaluate(lerp);
            return Mathf.Lerp(endMin, endMax, lerp);
        }

        
    }
}