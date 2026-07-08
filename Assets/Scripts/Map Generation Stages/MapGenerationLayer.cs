using System;
using CustomInspector;
using UnityEngine;

namespace Labyrinth
{
    [Serializable]
    public abstract class MapGenerationLayer : ScriptableObject
    {
        // public abstract void Awake();
        public abstract Map Execute(Map map, ref System.Random random);
    }


    public abstract class MapCreationLayer : MapGenerationLayer
    {
        public abstract Map GetMap(ref System.Random random);
    }
    public abstract class ObjectPlacementLayer : MapGenerationLayer{}
    public abstract class PathCarvingLayer : MapGenerationLayer{}
}
