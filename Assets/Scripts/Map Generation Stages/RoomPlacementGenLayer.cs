using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Labyrinth
{
    [CreateAssetMenu(fileName = "Room Placement Layer", menuName = "Map Gen Layers/Room Placement Generation Layer")]
    public class RoomPlacementLayer : ObjectPlacementLayer
    {

        public List<MazeRoom> mazeRoomPool;

        public int roomCountToSpawn = 1;
        public int retryAttemptsPerRoom = 5;
        public bool removeFromPoolOnceUsed = false;

        public override Map Execute(Map map, ref System.Random rand)
        {
            if (mazeRoomPool.Select(u => u.roomID).Distinct().Count() != mazeRoomPool.Count)
            {
                Debug.LogError("Contains duplicate room IDs in rooms to spawn list");
            }

            var pool = new List<MazeRoom>(mazeRoomPool);

            for (int roomNum = 0; roomNum < roomCountToSpawn && pool.Count > 0; roomNum++)
            {
                var currRoom = pool[rand.Next(pool.Count)];
                // Debug.Log($"Testing room home {currRoom.name}");
                
                int tries = 0;
                for (tries = 0; tries < retryAttemptsPerRoom; tries++)
                {
                    if (TryFindRoomPosition(map, ref rand, currRoom, out var coordinate, out var rotation))
                    {
                        Cell TranslatePositionToMap(Vector3Int position)
                        {
                            for (int j = 1; j <= rotation; j++) position = RotatePointClockwise90(position);
                            return map.layout[position.x + coordinate.x, position.z + coordinate.y];
                        }

                        var blockedSpaces = currRoom.AllRoomBlockedSpaces.Select(TranslatePositionToMap);
                        var doorwayCells = currRoom.AllDoorwaySpaces.Select(TranslatePositionToMap);

                        /// Mark on Map any unique tiles (entry & exit)
                        var perRoomID = rand.Next();

                        // Mark Room blocks
                        foreach (var cell in blockedSpaces)
                        {
                            cell.gridSpaceType = GridSpace.Room;
                        }

                        // Mark Doorways
                        foreach (var cell in doorwayCells)
                        {
                            cell.gridSpaceType = GridSpace.Doorway;
                            map.modifiers.Add( new(){cell = cell, 
                                                     type = Map.UniqueCell.UniqueCellType.Doorway,
                                                    data = new() {[Map.UniqueCell.PER_ROOM_ID] = perRoomID}});
                        }

                        /// Mark all connected cells within a room

                        var AllTranslatedCells = blockedSpaces.Union(doorwayCells);
                        //FIXME: Attempting O(n^2) to connect all spaces is stupid, should prob be DFS
                        foreach (var cell1 in AllTranslatedCells)
                        {
                            foreach (var cell2 in AllTranslatedCells)
                            {
                                if (map.AreNeighbors(cell1, cell2))
                                    map.ConnectCells(cell1, cell2);
                            }
                        }
                        // Debug.LogWarning("placed room home");
                        /// Save Room For Spawning Later (origin and rotation)
                        map.modifiers.Add( new Map.UniqueCell(){cell = map.layout[coordinate.x,coordinate.y], 
                                                data = new()
                                                {
                                                    [Map.UniqueCell.PER_ROOM_ID] = perRoomID,
                                                    [Map.UniqueCell.ROOM_ROTATION] = rotation,
                                                },
                                                stringData = new()
                                                {
                                                    [Map.UniqueCell.ROOM_ID] = currRoom.roomID,
                                                },
                                                type = Map.UniqueCell.UniqueCellType.Room,});
                        
                        map.rooms.TryAdd(currRoom.roomID, currRoom);

                        // Break out from retry loop
                        break;
                    }
                }

                if (tries >= retryAttemptsPerRoom) 
                    Debug.LogWarning($"Room Failed To Spawn in {tries} attempts!");

                if (removeFromPoolOnceUsed)
                    pool.Remove(currRoom);
            }

            return map;
        }




        bool TryFindRoomPosition(Map map, ref System.Random rand, MazeRoom room, out int2 coordinate, out int y90RotationCount)
        {
            // Find Open spot on map  (Could be a Square of length of largest bounding extent? Random Open Cell?)
            // Try All 90 degree Rotations
            // Mark Tiles for "room"

            var onlyOpen = map.layout.Cast<Cell>().Where(c => c.gridSpaceType == GridSpace.Valid).ToArray();
            var selectedOpen = onlyOpen[rand.Next(onlyOpen.Length)];
            
            var roomBounds = room.Bounds;
            for (int i = 0; i < 4; i++)
            {
                if (CanPlaceRoomAt(map, roomBounds, new(selectedOpen.x, 0, selectedOpen.y), GridSpace.Valid, out var overlappingCells))
                {
                    
                    coordinate = selectedOpen.Coords;
                    y90RotationCount = i;
                    return true;
                }
                else
                {
                    roomBounds = RotateIntBoundsClockwise90(roomBounds, room.grid);
                }
            }

            coordinate = int2.zero;
            y90RotationCount = int.MaxValue;
            return false;
        }

        int2 RotatePointClockwise90(int2 point) => new(point.y, -point.x);
        Vector3Int RotatePointClockwise90(Vector3Int point) => new(point.z, point.y, -point.x);
        BoundsInt RotateIntBoundsClockwise90(BoundsInt bounds, Grid grid, int count = 1)
        {
            // 360 rotation back to start
            if (count % 4 == 0) return bounds;

            Vector3 offset = grid.cellSize/2;
            Vector3 Max = grid.CellToLocal(bounds.max), Min = grid.CellToLocal(bounds.min);
            var maxDirection = Max - offset;
            var minDirection = Min - offset;

            var rotationAngle = count * 90;
            Vector3 rotatedMax = offset + (Quaternion.Euler(0, rotationAngle, 0) * maxDirection);
            Vector3 rotatedMin = offset + (Quaternion.Euler(0, rotationAngle, 0) * minDirection);

            BoundsInt newBounds = new();

            newBounds.SetMinMax(grid.LocalToCell(Vector3.Min(rotatedMax, rotatedMin)), grid.LocalToCell(Vector3.Max(rotatedMax, rotatedMin)));

            // Debug.Log($"Start bounds ({bounds}) led to ({newBounds})");

            return newBounds;
        }

        bool CanPlaceRoomAt(Map map, BoundsInt roomBounds, Vector3Int gridPos, GridSpace allowedTypes, out List<Cell> overlappingCells)
        {
            overlappingCells = new();

            if (!map.IsPosInsideMapBounds(new(gridPos.x, gridPos.z))) return false;

            for (int X = roomBounds.xMin; X < roomBounds.xMax; X++)
            {
                for (int Z = roomBounds.zMin; Z < roomBounds.zMax; Z++)
                {
                    // Debug.Log($"Checking if room at {gridPos} with bounds {roomBounds} off map at {new int2(gridPos.x + X, gridPos.z + Z)} results in {!map.IsPosInsideMapBounds(new(gridPos.x + X, gridPos.z + Z))}");
                    // if cell is not within the allowed overlap types
                    if (!map.IsPosInsideMapBounds(new(gridPos.x + X, gridPos.z + Z)) || (map.layout[X + gridPos.x, Z + gridPos.z].gridSpaceType & allowedTypes) == 0)
                    {
                        overlappingCells.Clear();
                        overlappingCells = null;
                        return false;
                    }
                    else
                    {
                        overlappingCells.Add(map.layout[X + gridPos.x, Z + gridPos.z]);
                    }
                }
            }


            return true;
        }
    }
}