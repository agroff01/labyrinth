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
    [CreateAssetMenu(fileName = "3DTileSet", menuName = "Scriptable Objects/3DTileSet")]
    public class TileSet3D : ScriptableObject
    {
        [Serializable]
        public class Tile
        {
            public enum TileType
            {
                Open = 0,
                Four_Way = 1 << 0,
                Three_Way = 1 << 1,
                Tunnel = 1 << 2,
                Corner = 1 << 3,
                End = 1 << 4,
                None = 1 << 5
            }

            public enum TileOrientationOptions
            {
                Any = 0,
                Open = 1 << 0,
                Blocked = 1 << 1,
                Tile = 1 << 2,
            }

            [Preview] public GameObject prefab;
            public TileType type;
            
            [Array2D, Unfold]
            public Array2D<TileOrientationOptions> placementKey = new(3,3);

            public TileOrientationOptions[,] placementKeyArray
            {
                get
                {
                    var output = new TileOrientationOptions[3,3];
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            output[i,j] = placementKey[i,j];
                        }
                    }
                    return output;
                }
            }

            public TileOrientationOptions[,] placementKeyArrayCoordinateOriented => placementKeyArray.RotateMatrixClockwise();
        }

        void OnValidate()
        {
            foreach (var rule in tileSet)
            {
                rule.placementKey.SetElement(Tile.TileOrientationOptions.Tile,1,1);
            }
        }

        public List<Tile> tileSet = new();
        List<int2> directions = new() {new(0,1), new(1,0), new(0,-1), new(-1,0)};

        public IEnumerable<Tile> GetTileType(Tile.TileType type) => tileSet.Where(t => t.type == type);

        static public Tile.TileType CheckTileType<T>(T[,] section, params T[] validOpenTypes) where T :  struct, IConvertible
        {
            if (!typeof(T).IsEnum) 
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (section.GetLength(0) != 3 || section.GetLength(1) != 3)
            {
                throw new ArgumentOutOfRangeException("Section of grid to check must provide a 3x3 layout");
            }
            
            bool IsValid(T first)
            {
                return validOpenTypes.Contains(first);
            }


            int tally = 0;
            if (IsValid(section[1,0])) tally++;
            if (IsValid(section[0,1])) tally++;
            if (IsValid(section[1,2])) tally++;
            if (IsValid(section[2,1])) tally++;

            switch(tally)
            {
                case 1:
                    return Tile.TileType.End;
                case 2:
                    // Check either side for validity
                    // If they are both the same then the path must be going directly across in this direction or the other
                    // Otherwise it can be assumed that if only one is true, this is a corner
                    return section[0,1].Equals(validOpenTypes) == section[2,1].Equals(validOpenTypes) ? Tile.TileType.Tunnel : Tile.TileType.Corner;
                case 3:
                    return Tile.TileType.Three_Way;
                case 4: 
                    int cornerTally = 0;
                    if (IsValid(section[0,0])) cornerTally++;
                    if (IsValid(section[0,2])) cornerTally++;
                    if (IsValid(section[2,0])) cornerTally++;
                    if (IsValid(section[2,2])) cornerTally++;

                    return cornerTally == 0 ? Tile.TileType.Four_Way : Tile.TileType.Open;

                default:
                    return Tile.TileType.None;
            }
        }


        public void CheckTileForSurroundingTypes(Map map, int2 mapTileCoord, ref Tile.TileOrientationOptions[,] result) => RecursiveTileCheck(map, ref result, new(1), new(int.MaxValue), originCoord: mapTileCoord - new int2(1));
    
        // Check how a 3x3 grid is connected in the map
        void RecursiveTileCheck(Map map, ref Tile.TileOrientationOptions[,] result, int2 pos, int2 last, int depth = 0, int2 originCoord = default)
        {
            // bounds check
            if (pos.x < 0 || pos.y < 0 || pos.x >= 3 || pos.y >= 3) return;
            if (depth > 2) return;
            
            
            
            if (depth == 0) 
            {
                result[pos.x, pos.y] = Tile.TileOrientationOptions.Tile;
            }
            
            if (depth > 0)
            {
                int tileInvalidTally = result[pos.x, pos.y]==Tile.TileOrientationOptions.Open || result[pos.x, pos.y]==Tile.TileOrientationOptions.Any ? 0 : 1;
                // If out of map bounds then default blocked
                if (!map.IsPosInsideMapBounds(pos + originCoord)) tileInvalidTally++;
                // If this cell is already marked blocked or the previous cell is marked blocked, then leave blank as ANY
                // Otherwise check if the last cell is connected to our current position
                else
                {
                    var connectedToLast = map.AreCellsConnected(map.layout[originCoord.x +last.x, originCoord.y +last.y], map.layout[originCoord.x + pos.x, originCoord.y + pos.y]);
                    if (!connectedToLast || result[last.x,last.y] == Tile.TileOrientationOptions.Blocked) tileInvalidTally++;
                    // Debug.Log($"Checked link between cells ({originCoord.x +last.x},{originCoord.y +last.y}) and ({originCoord.x +pos.x},{originCoord.y +pos.y}) is {connectedToLast}");
                    
                }


                
                result[pos.x, pos.y] = tileInvalidTally switch
                {
                    0 => Tile.TileOrientationOptions.Open,
                    _ => Tile.TileOrientationOptions.Blocked,
                    // _ => Tile.TileOrientationOptions.Any 
                };
            }

            foreach (var direction in directions)
            {
                if (direction.Equals(last-pos)) continue;

                RecursiveTileCheck(map, ref result, direction + pos, pos, depth + 1, originCoord);
            }
        }


        public bool GetBestTile(Map map, int2 coord, out Tile output, out int rotate)
        {
            Tile.TileOrientationOptions[,] surroundingSpaces = new Tile.TileOrientationOptions[3,3];

            int2 originCoord = coord - new int2(1,1);


            CheckTileForSurroundingTypes(map, coord, ref surroundingSpaces);

            // FIXME: Filter room options before searching through all options
            // var intersectionType = CheckTileType(surroundingSpaces, Tile.TileOrientationOptions.Open, Tile.TileOrientationOptions.Any);
            // Debug.LogWarning("Intersection type " + intersectionType);
            // var tileOptions = tileSet.Where(t => t.type == intersectionType);
            
            // Debug.Log($"Printing original of cell {coord}:\n{surroundingSpaces.MatrixToString()}\n");

            var finalOptions = new List<(Tile, int)>();
            for (int i = 0; i < 4; i++)
            {
                foreach (var tileRule in tileSet)
                {
                    // Debug.Log($"Original tile of {tileRule.prefab.name}:\n{tileRule.placementKeyArrayCoordinateOriented.MatrixToString()}");
                    if (PlacementPatternMatch(surroundingSpaces, tileRule.placementKeyArrayCoordinateOriented))
                    {
                        // Debug.Log($"Pattern Match found with original of cell {coord} And tile of {tileRule.prefab.name} with rotation {i*90}:\n{surroundingSpaces.MatrixToString()}\nTile:\n{tileRule.placementKeyArrayCoordinateOriented.MatrixToString()}");
                        finalOptions.Add((tileRule, i*90));
                    }
                }

                surroundingSpaces = surroundingSpaces.RotateMatrixCounterClockwise();
            }

            if (finalOptions.Count == 0)
            {
                Debug.LogError($"Could not find a matching cell for {coord} position.\nIt had a surrounding matrix of {surroundingSpaces.MatrixToString()}");


                output = null;
                rotate = int.MaxValue;
                return false;
            }

            var winningPair = finalOptions[0];
            output = winningPair.Item1;
            rotate = winningPair.Item2;
            return true;
        }


        static public bool PlacementPatternMatch(Tile.TileOrientationOptions[,] set1, Tile.TileOrientationOptions[,] set2)
        {
            if (set1.GetLength(0) == set2.GetLength(0) && set1.GetLength(1) == set2.GetLength(1))

            for (int X = 0; X < set1.GetLength(0); X++)
            {
                for (int Y = 0; Y < set1.GetLength(1); Y++)
                {
                    if (set1[X,Y] == Tile.TileOrientationOptions.Any || set2[X,Y] == Tile.TileOrientationOptions.Any) continue;

                    if (set1[X,Y] != set2[X,Y]) return false;
                }
            }

            return true;
        }
    }
}
