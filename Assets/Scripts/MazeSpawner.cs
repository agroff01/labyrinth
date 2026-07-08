using System.Collections.Generic;
using System.Linq;
using CustomInspector;
using CustomUtils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Labyrinth
{
    public class MazeSpawner : MonoBehaviour, IActive
    {
        [SelfFill] public MapManager mapManager = null;

        [AssetsOnly] public TileSet3D tileSet = null;

        public GameObject parentObject = null;

        public Dictionary<Cell, GameObject> CellGameObjects = new();


        [Button(nameof(TestPositionGridTileLayout), true),Space2(10), LabelSettings(LabelStyle.NoLabel)] public int2 TestPosition = int2.zero;

        
        public UnityEvent<MazeSpawner> OnMazeSpawned = new();
        private bool _mazeSpawned = false;
        public bool Active => _mazeSpawned;

        void Awake()
        {
            mapManager.OnGenerationFinished.AddListener(SpawnMap);
        }
        void OnDestroy()
        {
            mapManager.OnGenerationFinished.RemoveListener(SpawnMap);
            
        }

        void SpawnMap(Map map)
        {
            if (!parentObject)
            {
                Debug.LogError("Please provide an object for maze spawner to use as the parent.");
                return;
            }

            Pose CellToSpawnLocation(Vector3Int mazeCoord, float rotation)
            {
                
                Pose spawnPose = new()
                {
                    rotation = Quaternion.Euler(rotation * Vector3.up),
                    position = mapManager.mazeGrid.CellToLocal(mazeCoord)
                };


                // Convert to World Space
                spawnPose.position = mapManager.mazeGrid.LocalToWorld(spawnPose.position);
                spawnPose.rotation = mapManager.mazeGrid.transform.rotation * spawnPose.rotation;

                return spawnPose;
            }

            foreach (var obj in CellGameObjects)
            {
                Destroy(obj.Value);
            }
            CellGameObjects.Clear();
            

            //Spawn Rooms
            foreach(var roomCell in map.modifiers.Where(uc => uc.type == Map.UniqueCell.UniqueCellType.Room))
            {
                if (roomCell != null && map.rooms.TryGetValue(roomCell.stringData[Map.UniqueCell.ROOM_ID], out var prefab))
                {
                    var spawnPose = CellToSpawnLocation(roomCell.cell.Coords3D, 90 * roomCell.data[Map.UniqueCell.ROOM_ROTATION]);

                    // Spawn
                    var spawnedRoom = Instantiate(prefab, spawnPose.position, spawnPose.rotation);
                    spawnedRoom.transform.SetParent(parentObject.transform);

                    CellGameObjects.Add(roomCell.cell, spawnedRoom.gameObject);
                }
            }

            foreach(var cell in map.layout.Cast<Cell>().Where(c => c.gridSpaceType == GridSpace.Path))
            {
                if (tileSet.GetBestTile(map, cell.Coords, out var tile, out int rotateAngle))
                {
                    // Debug.Log($"Placing {tile.prefab.name} Tile at {cell.Coords} with rotation {rotateAngle}");
                    var spawnPose = CellToSpawnLocation(cell.Coords3D, rotateAngle);

                    // Spawn
                    var spawnedTile = Instantiate(tile.prefab, spawnPose.position, spawnPose.rotation);
                    spawnedTile.transform.SetParent(parentObject.transform);

                    CellGameObjects.Add(cell, spawnedTile);
                    
                }
                else
                {
                    Debug.LogError($"Could not find a matching cell for {cell.Coords} position.");
                }
            }

            _mazeSpawned = true;
            OnMazeSpawned.Invoke(this);
        }

        public void TestPositionGridTileLayout(int2 coord)
        {
            if (tileSet != null && mapManager && mapManager.map != null)
            {
                TileSet3D.Tile.TileOrientationOptions[,] result = new TileSet3D.Tile.TileOrientationOptions[3,3];
                tileSet.CheckTileForSurroundingTypes(mapManager.map, coord, ref result);

                Debug.Log($"Test of coordinate {coord} on the map has a 3x3 grid result of:\n{result.MatrixToString()}");
            }
        }
    }

}
