using UnityEngine;

namespace CustomUtils
{
    public enum UpdateLoopType
    {
        None = 0,
        Update = 1 << 0,
        FixedUpdate = 1 << 3,

        [InspectorName("Update/2")] HalfUpdate = 1 << 1,
        [InspectorName("Update/4")] QuarterUpdate = 1 << 2,
    }
}