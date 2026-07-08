using CustomInspector;
using CustomUtils;
using Unity.VisualScripting;
using UnityEngine;

namespace Labyrinth
{

    public class MapVisualizer : MonoBehaviour
    {
        
        public MapManager Manager = null;

        [Dictionary] public ReorderableDictionary<GridSpace, Color> colorKeys = new();
        public Color fallBackColor = Color.blue;
        public Color WallColor = Color.black;
        public float WallThickness = .1f;

        [Space(10)]
        public Vector3Int cursor = Vector3Int.zero;
        public Color cursorColor = Color.floralWhite;


#if UNITY_EDITOR

        [Button(nameof(ClearMap)), SerializeField, HideField] bool _;
        Map savedMap = null;
        void OnEnable()
        {
            if (Manager) Manager.OnGenerationFinished.AddListener(StoreMap);
        }
        void OnDisable()
        {
            if (Manager) Manager.OnGenerationFinished.RemoveListener(StoreMap);
        }

        void StoreMap(Map map) {
            savedMap = map;
        }
        void ClearMap() => savedMap = null;

        void OnDrawGizmos()
        {
            if (isActiveAndEnabled && Manager && savedMap != null)
            {

                // each grid cell
                foreach (var cell in savedMap.layout)
                {
                    if (!colorKeys.TryGetValue(cell.gridSpaceType, out var color)) color = fallBackColor;

                    Gizmos.color = color;
                    Gizmos.DrawWireCube(Manager.mazeGrid.CellToWorld(cell.Coords3D), Manager.mazeGrid.cellSize);
                }


                Gizmos.color = WallColor;
                var openWalls = savedMap.Connections;

                for (int x = 0; x < savedMap.Width; x++)
                {
                    for (int z = 0; z < savedMap.Height; z++)
                    {
                        // if the right wall is not in the connections list then draw it
                        if (x+1 >= savedMap.Width || !openWalls.Contains(new(savedMap.layout[x,z], savedMap.layout[x+1,z])))
                        {
                            Gizmos.DrawCube(Manager.mazeGrid.LocalToWorld(Manager.mazeGrid.CellToLocalInterpolated(new(x + .5f, 0, z))), 
                                            new Vector3(WallThickness, 1f, 1f).AsScaled(Manager.mazeGrid.cellSize));
                        }
                        // if the upper wall is not in the connections list then draw it
                        if (z+1 >= savedMap.Height || !openWalls.Contains(new(savedMap.layout[x,z], savedMap.layout[x,z+1])))
                        {
                            Gizmos.DrawCube(Manager.mazeGrid.LocalToWorld(Manager.mazeGrid.CellToLocalInterpolated(new(x, 0, z + .5f))), 
                                            new Vector3(1f, 1f, WallThickness).AsScaled(Manager.mazeGrid.cellSize));
                        }
                    }
                }

                //x-axis edge
                Gizmos.DrawCube(Manager.mazeGrid.LocalToWorld(Manager.mazeGrid.CellToLocalInterpolated(new(savedMap.Width/2f - .5f, 0, -.5f))), 
                                new Vector3(savedMap.Width, 1f, WallThickness).AsScaled(Manager.mazeGrid.cellSize));

                //y-axis edge
                Gizmos.DrawCube(Manager.mazeGrid.LocalToWorld(Manager.mazeGrid.CellToLocalInterpolated(new(-.5f, 0, savedMap.Height/2f - .5f))), 
                                new Vector3(WallThickness, 1f, savedMap.Height).AsScaled(Manager.mazeGrid.cellSize));
            }

            if (Manager)
            {
                Gizmos.color = cursorColor;
                Gizmos.DrawCube(Manager.mazeGrid.CellToWorld(cursor), Manager.mazeGrid.cellSize);
            }
        }
#endif
    }
}