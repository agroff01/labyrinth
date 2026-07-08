using CustomInspector;
using UnityEngine;

namespace Labyrinth
{
    public class BoundsRotation : MonoBehaviour
    {
        public float r = 90;

        [Button(nameof(TestClockwise))]
        public BoundsInt bounds = new();

        [SelfFill] public GridVisualizer gridVisualizer = null;

        void OnValidate()
        {
            gridVisualizer.GridBlocks.Clear();
            gridVisualizer.GridBlocks.Add(bounds);
        }

        public void TestClockwise()
        {
            Vector3 offset = gridVisualizer.grid.cellSize/2;
            Vector3 Max = gridVisualizer.grid.CellToLocal(bounds.max), Min = gridVisualizer.grid.CellToLocal(bounds.min);
            var maxDirection = Max - offset;
            var minDirection = Min - offset;

            Vector3 rotatedMax = offset + (Quaternion.Euler(0, r, 0) * maxDirection);
            Vector3 rotatedMin = offset + (Quaternion.Euler(0, r, 0) * minDirection);

            bounds = new();

            bounds.SetMinMax(gridVisualizer.grid.LocalToCell(Vector3.Min(rotatedMax, rotatedMin)), gridVisualizer.grid.LocalToCell(Vector3.Max(rotatedMax, rotatedMin)));

        }
        public Vector3Int RotatePointY90(Vector3Int val, bool clockwise = true)
        {
            return clockwise ? new (val.z, val.y, -val.x) : new (-val.z, val.y, val.x);
        }
    }
}
