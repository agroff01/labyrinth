using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUtils
{
    [Serializable]
    public class QP_RaycastConstraints
    {
        public Vector3? intendedRaycastHeading;
        public Vector2 allowedAngleFromIndendedHeading;



        public bool CheckAllActiveFilters(Vector3 direction)
        {
            if (intendedRaycastHeading.HasValue && !CheckHeadingConstraintFilter(direction)) return false;

            return true;
        }

        public bool CheckHeadingConstraintFilter(Vector3 direction)
        {
            if (!intendedRaycastHeading.HasValue)
            {
                Debug.LogError($"Tried to check the raycast constraint of {direction} when intended heading has not been initialized");
                return false;
            }


            var dot = Vector3.Dot(intendedRaycastHeading.Value, direction);
            var angle = Mathf.LerpAngle(0, 180, Mathf.InverseLerp(1, -1, dot));

            return allowedAngleFromIndendedHeading.CheckAsRange(angle);
        }

        public bool GetValidConstrainedVector(out Vector3? validVector)
        {
            validVector = null;

            if (!intendedRaycastHeading.HasValue)
            {
                return false;
            }

            var rotationAxis = Vector3.Cross(intendedRaycastHeading.Value, UnityEngine.Random.onUnitSphere);
            var rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(allowedAngleFromIndendedHeading.x, allowedAngleFromIndendedHeading.y), rotationAxis);
            validVector = rotation * intendedRaycastHeading.Value;
            return true;
        }
    }
}
