using System;
using UnityEngine;

namespace Labyrinth
{
    
    public class MapDisplayLayer : MapGenerationLayer
    {
        
        public int yOffset = 5;

        public GridVisualizer gridVisualizer;
        public bool Display = false;
        
        // If this changes to Ienumerable then have it stall here until told to skip
        public override Map Execute(Map map, ref System.Random random)
        {
            Display = true;

            gridVisualizer.SingleGridPoints.Add(new(0, yOffset, 0));
            
            return map;
        }



        
        
    }
}