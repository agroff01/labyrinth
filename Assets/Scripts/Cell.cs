
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Labyrinth
{
    public class Cell
    {
        readonly public int x;
        readonly public int y;
        public int2 Coords => new(x,y);
        public Vector3Int Coords3D => new(x,0,y);


        public GridSpace gridSpaceType = GridSpace.None;



        public Cell(int2 pos, GridSpace gridSpaceType = GridSpace.None) : this(pos.x, pos.y, gridSpaceType){}

        public Cell(int x, int y, GridSpace gridSpaceType = GridSpace.None)
        {
            this.x = x;
            this.y = y;
            this.gridSpaceType = gridSpaceType;
        }


        public override string ToString()
        {
            return $"(X: {x} | Y: {y} | Type: {gridSpaceType})";
        }
    }
    
}
