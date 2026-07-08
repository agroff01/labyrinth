using System;
using UnityEngine;

namespace Labyrinth
{
    [Serializable, Flags]
    public enum GridSpace
    {
        None = 0,
        Blocked = 1 << 0,
        Valid = 1 << 1,
        Room = 1 << 2,
        Doorway = 1 << 3,
        Path = 1 << 4,

        All = ~None
    }
    
}
