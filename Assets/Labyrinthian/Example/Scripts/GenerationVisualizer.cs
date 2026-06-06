using Labyrinthian;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NVector2 = System.Numerics.Vector2;

/// <summary>
/// Component for visualizing a generation process of 2D maze.
/// </summary>
public class GenerationVisualizer : MonoBehaviour
{
    private MazeGenerator m_generator;
    public Maze maze { get; private set; }

    private MeshRenderer[] m_cells;
    private Dictionary<MazeEdge, GameObject> m_walls;

    private Vector3 m_offset;

    #region Unity fields
    [Header("Size")]
    [SerializeField, Min(3)]
    private int m_width = 10;
    [SerializeField, Min(3)]
    private int m_height = 10;
    [SerializeField, Min(0)]
    private int m_innerWidth = 0;
    [SerializeField, Min(0)]
    private int m_innerHeight = 0;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject m_wallPrefab;
    [SerializeField]
    private MeshRenderer m_cellPrefab;

    [Header("Transform")]
    [SerializeField, Min(0f)]
    private float m_yOffset = 0.5f;
    [SerializeField, Min(0f)]
    private float m_cellSize = 1f;

    [Header("Visualization")]
    [SerializeField, Min(0f)]
    private float m_delay = 0.2f;
    [SerializeField]
    private Color m_unvisitedCellColor = Color.black;
    [SerializeField]
    private Color m_highlightedCellColor = Color.cyan;
    [SerializeField]
    private Color m_selectedCellColor = Color.red;
    #endregion

    public event Action generated;

    private void OnValidate()
    {
        if (m_innerWidth > m_width - 2)
        {
            m_innerWidth = m_width - 2;
        }
        if (m_innerHeight > m_height - 2)
        {
            m_innerHeight = m_height - 2;
        }
    }

    private void CreateWalls()
    {
        m_walls = new Dictionary<MazeEdge, GameObject>();
        foreach (MazeEdge wall in maze.GetWalls())
        {
            GameObject wallObject = Instantiate(m_wallPrefab, transform);
            PathSegment wallSegment = maze.GetWallPosition(wall);

            Vector3 wallStart = TransformMazePoint(wallSegment.StartPoint);
            Vector3 wallCenter = TransformMazePoint(wallSegment.Center);
            Vector3 wallEnd = TransformMazePoint(wallSegment.EndPoint);

            Vector3 wallDirection = wallEnd - wallStart;
            Quaternion rotation = Quaternion.LookRotation(wallDirection);

            wallObject.transform.SetPositionAndRotation(wallCenter, rotation);

            Vector3 newScale = wallObject.transform.localScale;
            newScale.z = wallDirection.magnitude;
            wallObject.transform.localScale = newScale;

            m_walls.Add(wall, wallObject);
        }
    }

    private void CreateCells()
    {
        m_cells = new MeshRenderer[maze.Cells.Length];
        for (int i = 0; i < m_cells.Length; i++)
        {
            MeshRenderer cellRenderer = Instantiate(m_cellPrefab, transform);
            // Set cell position:
            NVector2 mazePoint = maze.GetCellCenter2D(maze.Cells[i]);
            cellRenderer.transform.position = TransformMazePoint(mazePoint);
            // Set default color
            cellRenderer.material.color = m_unvisitedCellColor;

            m_cells[i] = cellRenderer;
        }
    }

    private IEnumerator Start()
    {
        // Create a maze
        maze = new OrthogonalMaze(m_width, m_height, m_innerWidth, m_innerHeight);
        // Add entry and exit
        MazeEdge entry = maze.GetOuterWalls().First();
        MazeEdge exit = maze.GetOuterWalls().Last();
        maze.Paths.Add(new MazePath(maze, entry, exit));

        m_offset = new Vector3(maze.Sizes[0] / 2f, 0f, -maze.Sizes[1] / 2f);

        // Create a generator
        m_generator = new PrimGeneration(maze);

        // Subscribe for maze update events
        maze.EdgeChanged += OnWallChanged;
        m_generator.CellStateChanged += OnCellStateChanged;

        // Initialize walls and cells
        CreateWalls();
        CreateCells();

        // Visualize generation step by step
        foreach (Maze frame in m_generator.GenerateStepByStep())
        {
            yield return new WaitForSeconds(m_delay);
        }

        generated?.Invoke();
    }

    private void OnWallChanged(Maze owner, MazeEdge wall, bool isConnected)
    {
        m_walls[wall].SetActive(!isConnected);
    }

    private void OnCellStateChanged(MazeCell cell)
    {
        Color color;
        if (cell == m_generator.SelectedCell)
        {
            color = m_selectedCellColor;
        }
        else if (m_generator.HighlightedCells[cell])
        {
            color = m_highlightedCellColor;
        }
        else if (!m_generator.VisitedCells[cell])
        {
            color = m_unvisitedCellColor;
        }
        else
        {
            m_cells[cell.Index].gameObject.SetActive(false);
            return;
        }
        
        m_cells[cell.Index].gameObject.SetActive(true);
        m_cells[cell.Index].material.color = color;
    }

    public Vector3 TransformMazePoint(NVector2 mazePoint)
    {
        Vector3 position = new(mazePoint.X, m_yOffset, -mazePoint.Y);
        return (position - m_offset) * m_cellSize;
    }
}
