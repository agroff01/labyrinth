using System;
using System.Collections.Generic;
using CustomUtils;
using Unity.Mathematics;
using UnityEngine;


namespace Labyrinth
{

    public class PathingGenerationLayer : PathCarvingLayer
    {

        public int PathsToGen = 3;
        public int TargetPathLength = 10;


        public override Map Execute(Map map, ref System.Random random)
        {
            
            for (int i = 0; i < PathsToGen; i++)
            {
                var start = map.GetRandomOpenCell(random);
                CarveWillsonPath(map, random, start);
            }

            return map;
        }


        public void CarveWillsonPath(Map map, System.Random rand, Cell start)
        {
            var SelectedCell = start;
            
            Cell previousCell = null;
            List<Cell> walk = new List<Cell>();
            while (walk.Count <= TargetPathLength )
            {
                if (walk.IndexOf(SelectedCell, out int resetIndex))
                {
                    EraseWalk(walk, resetIndex);
                }
                else
                {
                    walk.Add(SelectedCell);
                }

                List<Cell> list = map.GetNeighbors(SelectedCell, c => c != previousCell);
                if (list.Count != 0)
                {
                    int index2 = rand.Next(0, list.Count);
                    previousCell = SelectedCell;
                    SelectedCell = list[index2];
                }
                else
                {
                    EraseWalk(walk, 1);
                    SelectedCell = walk[0];
                }
            }

            walk.Add(SelectedCell);
            for (int num = 0; num < walk.Count - 1; num++)
            {
                Cell mazeCell = walk[num];
                map.ConnectCells(mazeCell, walk[num + 1]);
                mazeCell.gridSpaceType = GridSpace.Path;
            }

            walk[^1].gridSpaceType = GridSpace.Path;

            map.pathways.Add(walk);
        }

        private void EraseWalk<T>(List<T> walk, int start = 0)
        {
            for (int num = walk.Count - 1; num > start; num--)
            {
                walk.RemoveAt(num);
            }
        }
    }

}