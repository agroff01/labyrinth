using System;
using System.Collections.Generic;
using UnityEngine;

namespace Labyrinth
{
    [RequireComponent(typeof(Grid))]
    public class GridVisualizer : MonoBehaviour
    {
        public Grid grid = null;

        public List<Vector3Int> SingleGridPoints;
        public List<Vector3> FloatingGridPoints;
        public List<BoundsInt> GridBlocks;

        public bool useRandomColors = true;

        void OnDrawGizmos()
        {
            // if no grid is provided
            if (!grid || !isActiveAndEnabled) return;

            // Singles
            foreach (var point in SingleGridPoints)
            {
                if (useRandomColors) RandomGizmoColor(point.GetHashCode());
                Gizmos.DrawCube(grid.CellToWorld(point), grid.cellSize);
            }

            // Floats
            foreach (var point in FloatingGridPoints)
            {
                if (useRandomColors) RandomGizmoColor(point.GetHashCode());
                Gizmos.DrawSphere(grid.LocalToWorld(grid.CellToLocalInterpolated(point)), grid.cellSize.magnitude/4);
            }

            // Blocks
            foreach (var box in GridBlocks)
            {
                if (useRandomColors) RandomGizmoColor(box.GetHashCode());

                foreach (var cell in box.allPositionsWithin)
                {
                    Gizmos.DrawWireCube(grid.CellToWorld(cell), grid.cellSize);
                }
            }


        }

        void RandomGizmoColor(int hash)
        {
            float hue = (hash & int.MaxValue) % 360 / 360f;
            
            float saturation = 0.8f; 
            float value = 0.9f;


            Gizmos.color = Color.HSVToRGB(hue,saturation, value);

        }
    }
}
