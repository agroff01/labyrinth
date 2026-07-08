using System;
using System.Collections.Generic;
using System.Linq;
using CustomInspector;
using CustomUtils;
using Unity.Mathematics;
using UnityEngine;

namespace Labyrinth
{

    [CreateAssetMenu(fileName = "Connect Doorways Gen Layer", menuName = "Map Gen Layers/Connect Doorways Generation Layer")]
    public class ConnectingPathCarvingGenLayer : PathCarvingLayer
    {
        [Preview, SerializeField] private Texture2D _noiseMap = null;
        public Texture2D NoiseMap => _noiseMap;
        [Min(0), Tooltip("1 = no noise injected")] public float noiseStrength = 1;

        [SerializeField] float[] values = null;

        public int NoiseWidth => _noiseMap ? _noiseMap.width : int.MinValue;
        public int NoiseHeight => _noiseMap ? _noiseMap.height : int.MinValue;



        
        // Inspector Helpers
        [Button(nameof(BakeNoiseData))]
        [SerializeField, ReadOnly, ShowMethod(nameof(MapWidthM), label = "Logical Width"), ShowMethod(nameof(MapHeightM), label = "Logical Height")] bool _baked = false;
        public bool Baked => _baked;
        int MapWidthM() => NoiseWidth;
        int MapHeightM() => NoiseHeight;

        public override Map Execute(Map map, ref System.Random random)
        {
            
            List<Map.UniqueCell> doors = map.modifiers.Where(c => c.type == Map.UniqueCell.UniqueCellType.Doorway).ToList();
            
            // Theres 1 or none doors to connect on the map
            if (doors.Count <= 1) return map;

            doors.Shuffle(random);


            /// Ensure all rooms are linked with at least one connecting path
            var requiredDoorways = doors.DistinctBy(uc => uc.data[Map.UniqueCell.PER_ROOM_ID]).ToList();

            var extraDoorways = doors.Except(requiredDoorways).ToList();
            var usedDoorways = new List<Map.UniqueCell>();

            

            // TODO: This will always make 2 pathways off of one doorway. 
            // This should ideally try to grab a subsequent path off a different doorway of the same room

            for (int i = 0; i < requiredDoorways.Count-1; i++)
            {
                MakeMapPath(requiredDoorways[i].cell, requiredDoorways[i+1].cell, map);
            }

            usedDoorways.AddRange(requiredDoorways);

            while (extraDoorways.Count > 0)
            {
                var fromCell = extraDoorways[0];
                var toCellIndex = extraDoorways.FindIndex(c => c.data[Map.UniqueCell.PER_ROOM_ID] != fromCell.data[Map.UniqueCell.PER_ROOM_ID]);
                bool extraDoorwaysListUsed = toCellIndex >= 0;
                var toCell = toCellIndex >= 0 ? extraDoorways[toCellIndex] : usedDoorways[random.Next(usedDoorways.Count)];

                MakeMapPath(fromCell.cell, toCell.cell, map);
                
                if(extraDoorwaysListUsed) extraDoorways.RemoveAt(toCellIndex);
                extraDoorways.RemoveAt(0);

                usedDoorways.Add(fromCell);
                usedDoorways.Add(toCell);
            }
            

            return map;
        }


        void MakeMapPath(Cell start, Cell end, Map map)
        {
            
            var path = AStar(start, end, map);

            if (path == null)
            {
                Debug.LogError($"No Path found between cells ({start}) and ({end})");
                return;
            }

            // Mark path in Map
            Cell last = null;
            foreach (var cell in path)
            {
                cell.gridSpaceType = GridSpace.Path;

                // Basically skip index 0
                if (last != null)
                {
                    map.ConnectCells(last, cell);
                }

                last = cell;
            }

            // Debug.Log(path.Stringify());
            map.pathways.Add(path);
        }


        List<Cell> AStar(Cell start, Cell goal, Map map)
        {
            var openList = new List<Cell> { start };
            var closedList = new HashSet<Cell>();

            // Dictionaries to hold g(n), h(n), and parent pointers
            var gScore = new Dictionary<int2, double> { [start.Coords] = 0 };
            var hScore = new Dictionary<int2, double> { [start.Coords] = Heuristic(start, goal) };
            var parentMap = new Dictionary<int2, Cell>();

            while (openList.Count > 0)
            {

                // Find Cell in open list with the lowest F score
                var current = openList.OrderBy(Cell => gScore[Cell.Coords] + hScore[Cell.Coords]).First();

                if (current.Coords.Equals(goal.Coords))
                {
                    return ReconstructPath(parentMap, current);
                }

                openList.Remove(current);
                closedList.Add(current);

                foreach (var neighbor in map.GetNeighbors(current))
                {
                    if (neighbor == null || (neighbor.gridSpaceType & (GridSpace.Valid|GridSpace.Doorway|GridSpace.Path)) == 0 || closedList.Contains(neighbor)) continue;

                    // Tentative gScore (current gScore + distance to neighbor)
                    double tentativeGScore = gScore[current.Coords] + 1;

                    if (!gScore.ContainsKey(neighbor.Coords) || tentativeGScore < gScore[neighbor.Coords])
                    {
                        // Update gScore and hScore
                        gScore[neighbor.Coords] = tentativeGScore;
                        hScore[neighbor.Coords] = Heuristic(neighbor, goal);

                        // Set the current Cell as the parent of the neighbor
                        parentMap[neighbor.Coords] = current;

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            return null; // No path found
        }

        List<Cell> ReconstructPath(Dictionary<int2, Cell> parentMap, Cell current)
        {
            var path = new List<Cell> { current };
            
            while (parentMap.ContainsKey(current.Coords))
            {
                current = parentMap[current.Coords];
                path.Add(current);
            }
            
            path.Reverse();
            return path;
        }

        double Heuristic(Cell current, Cell goal)
        {
            // Manhattan distance between cells
            double val = Math.Abs(current.x - goal.x) + Math.Abs(current.y - goal.y);

            if (Baked && noiseStrength > 1)
            {
                // introduce noise from texture and factor
                val *= Mathf.Lerp(1, noiseStrength, values[GetNoiseIndex(current.x, current.y)]);
            }
            return val;
        }

        void BakeNoiseData()
        {
            var pixels = _noiseMap.GetPixels();
            
            values = new float[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                values[i] = pixels[i].grayscale;
            }

            _baked = true;
        }

        public int GetNoiseIndex(int x, int y)
        {
            if (_noiseMap)
            {
                int wrappedX = x % NoiseWidth, wrappedY = y % NoiseHeight;

                return (wrappedY * NoiseWidth) + wrappedX;
            }
            else throw new NullReferenceException();
        }
        public void GetCoordinates(int index, out int2 output)
        {
            if (_noiseMap)
                GetCoordinatesFromArrayIndex(index, _noiseMap.width, _noiseMap.height, out output);
            else
                output = new(-1,-1);
        }

        public static int2 GetCoordinatesFromArrayIndex(int index, int width, int height)
        {
            GetCoordinatesFromArrayIndex(index, width, height, out int2 output);
            return output;
        }
        public static void GetCoordinatesFromArrayIndex(int index, int width, int height, out int2 output)
        {
            if (index > (width * height) - 1 || index < 0)
            {
                throw new IndexOutOfRangeException($"Index of {index} should be out of bounds for coordnate grid ({width} x {height})");
            }
            output = new(index % width, index/width);
        }


        void OnValidate()
        {
            if (!_noiseMap.isReadable) Debug.Log($"The noise texture for {name} named {_noiseMap.name} is not marked for readability.");
        }
    }
}