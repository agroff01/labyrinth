using CustomUtils;
using UnityEngine;
using QFSW.QC;
using Labyrinthian;
using System.Collections;
using Unity.Mathematics;
using SaintsField.Playa;
using System.Linq;

namespace Labyrinth
{
        

    public class MapGen : MonoBehaviour
    {
        

        public Grid layout;

        [SerializeField] public LevelMap map = null;


        [Header("Runtime")]
       
        [Min(0)] public int colorScale = 10;

        void Start()
        {
            if (map)
            {
                map.EmptyMap();
                map.CreateEmptyMap(25);  
            }
            else
            {
                map = new(25,25);

            }
        }

        [ContextMenu("Clear Map")]
        void Test()
        {
            map.EmptyMap();
        }

        

        

        public void OnDrawGizmos()
        {
            if (map != null)
            {
                var rows = map.rowCount;
                var column = map.columnCount;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < column; j++)
                    {
                        if (map.GetCell(i,j,this).gridSpaceType == GridSpace.filled)
                        {
                            // We can use the green channel for shading later?
                            var newColor = (new Vector3(i , j).AbsCopy() * colorScale).Modulo(255) * (1/255f);
                            Gizmos.color = new Color(newColor.x, newColor.y, newColor.z);

                            Gizmos.DrawCube(layout.CellToWorld(new(i - (Mathf.FloorToInt(rows/2)),0,j  - (Mathf.FloorToInt(column/2)))), layout.cellSize);
                            
                        }
                    }
                }
                Gizmos.color = Color.black;
                Gizmos.DrawCube(layout.CellToWorld(new(-1 - (Mathf.FloorToInt(rows/2)),0,-1  - (Mathf.FloorToInt(column/2)))), layout.cellSize);
                
                foreach (var C in Enumerable.Range(0,rows))
                {
                    Gizmos.DrawCube(layout.CellToWorld(new(C - (Mathf.FloorToInt(rows/2)),0,0  - (Mathf.FloorToInt(column/2)))), layout.cellSize);
                }
                foreach (var C in Enumerable.Range(0,column))
                {
                    Gizmos.DrawCube(layout.CellToWorld(new(0 - (Mathf.FloorToInt(rows/2)),0,C  - (Mathf.FloorToInt(column/2)))), layout.cellSize);
                }
            }
        }



        void OnDestroy()
        {
            map.EmptyMap();
        }

        [ContextMenu("Print Map"), Command]
        public void PrintMap()
        {
            Debug.Log(map.PrintMap(), this);
        }
        [ContextMenu("Print Image"), Command]
        public void PrintImage()
        {
            Debug.Log(mazeImage.PrintValidImage(), this);
        }


        [ContextMenu ("Random Fill Map")]
        public void RandomMapFill()
        {
            map.CreateEmptyMap(25);
            var rows = map.rowCount;
            var column = map.columnCount;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    if (UnityEngine.Random.value > .5f)
                    {
                        map.GetCell(i,j,this).gridSpaceType = GridSpace.filled;
                    }   
                }
            }
                
            PrintMap();
        }

        
        
        public static bool IncludeCell(GridPoint2D point2D)
        {
            return (point2D.Row == 0 || point2D.Row == 24 || point2D.Row != point2D.Column);
        }

        [ContextMenu("Wilson Walk")]
        public void WilsonWalk()
        {
            Maze maze = new OrthogonalMaze(25,25, IncludeCell);

            MazeGenerator mazeGenerator = new WilsonGeneration(maze);
            mazeGenerator.Generate();

        }


        void ShowMaze(Maze m)
        {
            map.CreateEmptyMap(Mathf.FloorToInt(m.Width2D), Mathf.FloorToInt(m.Height2D));
            foreach (var cell in m.Cells)
            {
                var point = m.GetCellPoint(cell,0);
                map.cells[Mathf.FloorToInt(point[0])][Mathf.FloorToInt(point[1])].gridSpaceType = GridSpace.filled;
            }
        }

        [ContextMenu("timed test")]
        public void TimedWilsonTest()
        {
            StartCoroutine(TimedWilson());
        }

        IEnumerator TimedWilson()
        {
            Maze maze = new OrthogonalMaze(25,25/* , c=> Mathf.FloorToInt(Time.realtimeSinceStartup/2) == c.Row */);
            MazeGenerator mazeGenerator = new WilsonGeneration(maze);

            print("Starting Test for 1 Wilson");
            var timestamp = Time.realtimeSinceStartup;

            mazeGenerator.Generate();

            print("Finished test for 1 wilson with a time of " + (Time.realtimeSinceStartup - timestamp));
            ShowMaze(maze);
            

            yield return new WaitForSeconds(5);
            
            print("Starting Another Test for 1 Wilson");
            timestamp = Time.realtimeSinceStartup;

            mazeGenerator.Generate();

            print("Finished Another test for 1 wilson with a time of " + (Time.realtimeSinceStartup - timestamp));
            ShowMaze(maze);
            

            yield return new WaitForSeconds(5);
            
            print("Starting Another Test for 10 Wilson");
            timestamp = Time.realtimeSinceStartup;

            for (int i = 0; i < 10; i++)
            {
                mazeGenerator.Generate();
            }

            print("Finished Another test for 10 wilson with a time of " + (Time.realtimeSinceStartup - timestamp));
            ShowMaze(maze);
            

            yield return new WaitForSeconds(5);
            
            print("Starting Another Test for 1 Wilson on 100x100 grid");
            maze = new OrthogonalMaze(100,100/* , c=> Mathf.FloorToInt(Time.realtimeSinceStartup/2) == c.Row */);
            mazeGenerator = new WilsonGeneration(maze);
            timestamp = Time.realtimeSinceStartup;

            mazeGenerator.Generate();

            print("Finished Another test for 1 wilson on 100x100 grid with a time of " + (Time.realtimeSinceStartup - timestamp));
            ShowMaze(maze);
            

            yield return new WaitForSeconds(5);
            
            print("Starting Another Test for 20 Wilson on 100x100 grid");
            maze = new OrthogonalMaze(100,100/* , c=> Mathf.FloorToInt(Time.realtimeSinceStartup/2) == c.Row */);
            mazeGenerator = new WilsonGeneration(maze);
            timestamp = Time.realtimeSinceStartup;

            for (int i = 0; i < 20; i++)
            {
                mazeGenerator.Generate();  
            }

            print("Finished Another test for 20 wilson on 100x100 grid with a time of " + (Time.realtimeSinceStartup - timestamp));
            ShowMaze(maze);
            

            yield return new WaitForSeconds(5);
            
            print("Starting Another Test for 1 Wilson on 250x250 grid");
            maze = new OrthogonalMaze(250,250/* , c=> Mathf.FloorToInt(Time.realtimeSinceStartup/2) == c.Row */);
            mazeGenerator = new WilsonGeneration(maze);
            timestamp = Time.realtimeSinceStartup;

            mazeGenerator.Generate();

            print("Finished Another test for 1 wilson on 250x250 grid with a time of " + (Time.realtimeSinceStartup - timestamp));
            ShowMaze(maze);
            
        }



        public int2 manualWilsonSize = new(25,25);
        [Min(1)] public int wilsonIterations = 1;
        [ContextMenu("Manual Settings Wilson")]
        public void ManualWilson()
        {
            StartCoroutine(ManualWilsonCoroutine());
        }

        IEnumerator ManualWilsonCoroutine()
        {            
            print($"Starting Another Test for {wilsonIterations} Wilson on {manualWilsonSize.x}x{manualWilsonSize.y} grid");
            Maze maze = new OrthogonalMaze(manualWilsonSize.x,manualWilsonSize.y);
            MazeGenerator mazeGenerator = new WilsonGeneration(maze);

            yield return null; // wait for next frame before starting timer

            float timestamp = Time.realtimeSinceStartup;

            for (int i = 0; i < wilsonIterations; i++)
            {
                mazeGenerator.Generate();  
            }

            print($"Finished Another test for {wilsonIterations} Wilson on {manualWilsonSize.x}x{manualWilsonSize.y} grid with a time of " + (Time.realtimeSinceStartup - timestamp));
            ShowMaze(maze);
            
            
        }
        

        public MazeImageSettings mazeImage = null;

        public bool AllowCell(GridPoint2D gridPoint)
        {
            var data = mazeImage.IsCellValid(gridPoint.Column, gridPoint.Row);
            return data;
        }
        [Button]
        public void MazeFromImage()
        {
            if (Application.isPlaying)
            {

                print($"Starting Maze gen from Image blueprint");
                Maze maze = new OrthogonalMaze(mazeImage.Image.width, mazeImage.Image.height, AllowCell );
                MazeGenerator mazeGenerator = new PrimGeneration(maze);

                mazeGenerator.Generate();  
                

                ShowMaze(maze);
            
            }
        }


        [Button]
        public void ShowImageMap()
        {
            map.CreateEmptyMap(Mathf.FloorToInt(mazeImage.Image.height), Mathf.FloorToInt(mazeImage.Image.width));

            for (int i = 0; i < mazeImage.visitedCells.Length; i++)
            {
                if (mazeImage.visitedCells[i])
                {
                    var c = mazeImage.GetCoordinatesFromArrayIndex(i, mazeImage.Image.width, mazeImage.Image.height);
                    map.cells[c.x][c.y].gridSpaceType = GridSpace.filled;
                    
                }
            }
        }
    }

}
