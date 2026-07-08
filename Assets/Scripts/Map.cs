using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomInspector;
using CustomUtils;
using Unity.Mathematics;
using UnityEngine;

namespace Labyrinth
{
    public class Map
    {
        public class UniqueCell 
        {
            public const string ROOM_ID = "ROOM_ID";
            public const string PER_ROOM_ID = "PER_ROOM_ID";
            public const string ROOM_ROTATION = "ROOM_ROTATION";

            public enum UniqueCellType
            {
                None = 0,
                Room = 1 << 0,
                Doorway = 1 << 1,
            }
            public Cell cell;
            public UniqueCellType type;

            [Dictionary] public ReorderableDictionary<string, int> data;
            [Dictionary] public ReorderableDictionary<string, string> stringData;
        }

        public class CellLink
        {
            // TODO: One Way direction functionality 
            public Cell cell1;
            public Cell cell2;

            public CellLink(Cell c1, Cell c2)
            {
                if (c1 == c2 || c1.Coords.Equals(c2.Coords)) throw new ArgumentException("A CellLink cannot be created from a cell to itself");

                // 1 comes first
                if (c1.y < c2.y || (c1.y == c2.y && c1.x < c2.x))
                {
                    cell1 = c1;
                    cell2 = c2;
                    return;
                }
                // 2 comes first
                else
                {
                    cell2 = c1;
                    cell1 = c2;
                    return;
                }

            }


            public override bool Equals(object obj)
            {
                if (obj is CellLink other)
                {
                    return (cell1.Coords.Equals(other.cell1.Coords) && cell2.Coords.Equals(other.cell2.Coords)) ||
                           (cell1.Coords.Equals(other.cell2.Coords) && cell2.Coords.Equals(other.cell1.Coords));
                }
                return false;
            }

            // Custom Hash Generation
            public override int GetHashCode()
            {
                return HashCode.Combine(cell1, cell2);
            }
        }


        public Cell[,] layout;

        public int2 gridSize;
        public int Width => gridSize.x;
        public int Height => gridSize.y;

        // 0-up, 1-right, 2-down, 3-left
        public static readonly List<int2> directions = new(){new(0,1), new(1,0), new(0,-1), new(-1,0)};

        public List<UniqueCell> modifiers = new();
        public Dictionary<string, MazeRoom> rooms = new();
        HashSet<CellLink> _connections = new();
        public HashSet<CellLink> Connections => _connections;

        // Primarily here for debug visualization
        public List<List<Cell>> pathways = new();

        public Map(int x, int y, Func<int2, bool> isCoordinateValid)
        {
            if (x <= 1)
            {
                throw new ArgumentOutOfRangeException("x");
            }

            if (y <= 1)
            {
                throw new ArgumentOutOfRangeException("x");
            }

            layout = new Cell[x,y];
            gridSize = new(x,y);


            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    layout[i,j] = new(i, j, isCoordinateValid(new(i,j)) ? GridSpace.Valid : GridSpace.None);
                }
            }
        }

        public bool IsPosInsideMapBounds(int2 coord) => 0 <= coord.x && coord.x < gridSize.x && 0 <= coord.y && coord.y < gridSize.y;
        public Cell GetRandomOpenCell(System.Random rng) => layout.Cast<Cell>().Where(c => (c.gridSpaceType & (GridSpace.Valid | GridSpace.Path)) != 0).RandomElement(rng);

        public List<Cell> GetNeighbors(Cell c, Predicate<Cell> predicate = null) => GetNeighbors(new int2(c.x, c.y), predicate);
        public List<Cell> GetNeighbors(int x, int y, Predicate<Cell> predicate = null) => GetNeighbors(new int2(x,y), predicate);
        public List<Cell> GetNeighbors(int2 coords, Predicate<Cell> predicate = null)
        {
            List<Cell> cells = new();

            foreach (var direction in directions)
            {
                var newCoords = coords + direction;
                //bounds check
                if (!IsPosInsideMapBounds(newCoords)) continue;

                var otherCell = layout[newCoords.x,newCoords.y];
                if (otherCell != null && otherCell.gridSpaceType != GridSpace.None && (predicate == null || predicate(otherCell)))
                {
                    cells.Add(otherCell);
                }
            }

            return cells;
        }
        public bool AreNeighbors(Cell c1, Cell c2) => AreNeighbors(c1.Coords, c2.Coords);
        public bool AreNeighbors(int2 p1, int2 p2)
        {
            // distance between points is exactly one unit
            return MathF.Abs(p1.x - p2.x) + MathF.Abs(p1.y - p2.y) == 1;
        }

        public int GridToIndex(int2 coord)
        {
            if (gridSize.x <= 0 || gridSize.y <= 0) return -1;

            // within bounds
            if (IsPosInsideMapBounds(coord))
            {
                return (coord.y * gridSize.x) + coord.x;
            }
            
            return -1;
        }

        public bool ConnectCells(Cell c1, Cell c2)
        {
            if (!AreNeighbors(c1, c2))
            {
                // Debug.LogWarning($"Cannot Connect cells that are not neighbors: {c1} and {c2}");
                return false;
            }

            CellLink pair = new(c1, c2);
            if(_connections.Contains(pair)) 
            {
                // Debug.LogWarning("Cannot Connect cells that are already connected");
                return false;
            }
            
            _connections.Add(pair);
            // Debug.Log($"Connected Cells {c1} and {c2}");
            return true;
            
        }

        public bool RemoveCellConnection(Cell c1, Cell c2)
        {
            if (!AreNeighbors(c1,c2)) return false;
            
            return _connections.Remove(new (c1, c2));
        }

        //FIXME: Finish this function
        // public List<CellLink> GetWalls(bool includeOuter = false)
        // {
        //     SortedSet<CellLink> output = new();

        //     var usedCells = layout.Cast<Cell>().Where(c => c.gridSpaceType != GridSpace.None);
            
        //     foreach (var cell in usedCells)
        //     {
        //         var neighbors = GetNeighbors(cell);
        //         output.Union(neighbors.Where())
        //     }
        // }


        public bool AreCellsConnected(Cell cell1, Cell cell2)
        {
            if (cell1 is null)
            {
                throw new ArgumentNullException("cell1", "cells cannot be null");
            }

            if (cell2 is null)
            {
                throw new ArgumentNullException("cell2", "cells cannot be null");
            }

            if (IsPosInsideMapBounds(cell1.Coords) && IsPosInsideMapBounds(cell2.Coords))
            {
                return _connections.Contains(new (cell1, cell2));
            }

            return false;
        }

        /// <summary>
        /// Creates a debug string printing the entire grid in a human friendly symbolic format
        /// </summary>
        /// <returns>Human readable map with symbols</returns>
        public new string ToString()
        {
            var output = new StringBuilder("Current map:\n");
            
            if(layout == null)
            {
                output.Append("None\n");
            }
            else
            {
                // top-to-bottom
                for (int i = gridSize.y-1; i >= 0; i--)
                {
                    var line = new StringBuilder();
                    // left-to-right
                    for (int j = 0; j < gridSize.x; j++)
                    {
                        if (layout[j,i] == null) line.Append("0");
                        else
                        {
                            line.Append( layout[j,i].gridSpaceType switch
                            {
                                GridSpace.None or GridSpace.Blocked => '0',
                                GridSpace.Room => 'R',
                                GridSpace.Doorway => 'D',
                                GridSpace.Valid => 'X',
                                GridSpace.Path => 'P',
                                _ => '?'
                            });
                        }
                    }
                    output.Append(line.Append("\n"));
                }
            }


            return output.ToString();
        }
    }
}
