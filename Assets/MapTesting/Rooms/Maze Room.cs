using System;
using System.Collections.Generic;
using System.Linq;
using CustomInspector;
using CustomUtils;
using Unity.Mathematics;
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
                Doorway = 1,
                BufferBlock = 1 << 1,
            }
            public int x = 0;
            public int y = 0;
            public int2 Coord => new(x,y);
            public RoomCellDesignation cellDesignation = RoomCellDesignation.Normal;
        }

        [ReadOnly] public string roomID = null;

        public Grid grid = null;
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


        IEnumerable<Vector3Int> uniqueCellSpaces => uniqueCells.Select(c => new Vector3Int(c.x, 0, c.y)).Distinct();
        IEnumerable<Vector3Int> roomBlockAsSpaces => roomBlocks.SelectMany(b => b.allPositionsWithin.AsEnumerable()).Distinct();
        public IEnumerable<Vector3Int> AllRoomBlockedSpaces => roomBlockAsSpaces.Concat(uniqueCells.Where(c => c.cellDesignation == RoomCell.RoomCellDesignation.Normal).Select(c => new Vector3Int(c.x, 0, c.y))).Distinct();
        public IEnumerable<Vector3Int> AllDoorwaySpaces => uniqueCells.Where(c => c.cellDesignation == RoomCell.RoomCellDesignation.Doorway).Select(c => new Vector3Int(c.x, 0, c.y)).Distinct();
        public IEnumerable<Vector3Int> AllSpaces => roomBlockAsSpaces.Concat(uniqueCellSpaces).Distinct();

        public Bounds GridSpaceBounds(Grid g, Vector3 gridOffset = default)
        {
            var realBounds = g.GetBoundsLocal(Bounds.max - Vector3Int.one);
            realBounds.Encapsulate(g.GetBoundsLocal(Bounds.min));
            realBounds.center = g.LocalToWorld(realBounds.center + gridOffset);
            return realBounds;
        }


        #if UNITY_EDITOR
        [Header("Runtime")]
        [Button(nameof(GenNewID))]
        public GridVisualizer visualizer;

        void OnValidate()
        {
            if (visualizer)
            {
                visualizer.GridBlocks = roomBlocks;
                visualizer.SingleGridPoints = new(uniqueCells.Select(c => new Vector3Int(c.x, 0, c.y)));
            }

            if (string.IsNullOrEmpty(roomID))
            {
                GenNewID();
            }
        }

        void GenNewID()
        {
            roomID = Guid.NewGuid().ToString();
        }

        public bool ForceShowDebugBounds = false;
        void OnDrawGizmosSelected()
        {   
            if (grid && (ForceShowDebugBounds || (DebugGridVisualizerCore.Instance && DebugGridVisualizerCore.Instance.ShowRoomVisualizers)))
            {
                var color = Gizmos.color;
                Gizmos.color = Color.black;

                var gridBounds = GridSpaceBounds(grid);
                Gizmos.DrawWireCube((gridBounds.center), gridBounds.size);

                Gizmos.color = color;
            }
        }
        #endif
    }
}
