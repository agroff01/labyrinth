using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CSharp;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace Labyrinth
{
    public class MazeRoom : MonoBehaviour
    {
        [Serializable]
        public class RoomCell
        {
            [Serializable]
            public enum RoomCellDesignation
            {
                Normal = 0,
                Doorway = 1
            }
            [LayoutStart("Hori", ELayout.Horizontal)]
            public int x = 0;
            public int y = 0;
            public RoomCellDesignation cellDesignation = RoomCellDesignation.Normal;
        }


        public List<RoomCell> uniqueCells = new();
        public List<BoundsInt> roomBlocks = new();


        // [ShowInInspector]
        public BoundsInt Bounds
        {
            get
            {
                
                var min = Vector3Int.one * int.MaxValue;
                var max = Vector3Int.one * int.MinValue;

                foreach (var block in roomBlocks)
                {
                    min = Vector3Int.Min(min, block.min);
                    max = Vector3Int.Max(max, block.max);
                }
                foreach (var cell in uniqueCells)
                {
                    min = Vector3Int.Min(min, new(cell.x, 0, cell.y));
                    max = Vector3Int.Max(max, new Vector3Int(cell.x, 0, cell.y) + Vector3Int.one);
                }

                var _internalBounds = new BoundsInt();
                _internalBounds.SetMinMax(min, max);
                
                return _internalBounds;
            }
        }

        public Bounds GridSpaceBounds(Grid g, Vector3 gridOffset = default)
        {
            var realBounds = g.GetBoundsLocal(Bounds.max - Vector3Int.one);
            realBounds.Encapsulate(g.GetBoundsLocal(Bounds.min));
            realBounds.center = g.LocalToWorld(realBounds.center + gridOffset);
            return realBounds;
        }


        [Header("Runtime")]
        public GridVisualizer visualizer;
        void OnValidate()
        {
            if (visualizer)
            {
                visualizer.GridBlocks = roomBlocks;
                visualizer.SingleGridPoints = new(uniqueCells.Select(c => new Vector3Int(c.x, 0, c.y)));
            }
        }

        public bool ready = false;
        public Grid grid = null;
        void OnDrawGizmosSelected()
        {   
            if (ready && grid)
            {
                var color = Gizmos.color;
                Gizmos.color = Color.black;

                var gridBounds = GridSpaceBounds(grid);
                Gizmos.DrawWireCube((gridBounds.center), gridBounds.size);

                Gizmos.color = color;
            }
        }
    }
}
