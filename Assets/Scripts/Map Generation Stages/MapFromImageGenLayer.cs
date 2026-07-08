using System;
using CustomInspector;
using Unity.Mathematics;
using UnityEngine;

namespace Labyrinth
{
    
    [CreateAssetMenu(fileName = "Map From Image Generation Layer", menuName = "Map Gen Layers/Map from Image Generation Layer")]
    public class MapFromImageGenLayer : MapCreationLayer
    {

        [Foldout] public ImageMap bakedImageMapData = null;


        public bool IsCellValid(int2 coord)
        {
            return bakedImageMapData.IsCellValid(coord.x, coord.y);
        }

        public override Map Execute(Map map, ref System.Random rand)
        {
            return GetMap(ref rand);
        }

        public override Map GetMap(ref System.Random random)
        {
            if (!bakedImageMapData || !bakedImageMapData.Baked)
            {
                throw new ArgumentNullException(nameof(bakedImageMapData), "Please provide the generator with a propper baked image to read from.");
            }


            return new Map(bakedImageMapData.BakedMapWidth, bakedImageMapData.BakedMapHeight, IsCellValid);
        }
    }
}
