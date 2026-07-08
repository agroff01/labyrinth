using System;
using UnityEngine;

namespace Labyrinth
{
    [CreateAssetMenu(fileName = "Plain Map Creator", menuName = "Map Gen Layers/Plain Map Creator")]
    public class SquareMapCreator : MapCreationLayer
    {
        public Vector2Int size = new(25,25);
        public override Map Execute(Map map, ref System.Random random)
        {
            return GetMap(ref random);
        }

        public override Map GetMap(ref System.Random random)
        {
            return new(size.x, size.y, c=> true);
        }
    }
}
