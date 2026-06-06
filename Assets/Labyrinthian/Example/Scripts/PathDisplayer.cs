using Labyrinthian;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Component that displays maze path using Line Renderer.
/// </summary>
[RequireComponent(typeof(LineRenderer), typeof(GenerationVisualizer))]
public class PathDisplayer : MonoBehaviour
{
    private LineRenderer m_lineRenderer;
    private GenerationVisualizer m_visualizer;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_visualizer = GetComponent<GenerationVisualizer>();

        m_visualizer.generated += OnMazeGenerated;
    }

    private void OnMazeGenerated()
    {
        Maze maze = m_visualizer.maze;
        if (maze.Paths.Count < 1) return;

        var segments = maze.Paths[0].GetSegments();
        List<Vector3> points = new()
        {
            m_visualizer.TransformMazePoint(segments.First().StartPoint)
        };

        foreach (PathSegment segment in segments)
        {
            points.Add(m_visualizer.TransformMazePoint(segment.EndPoint));
        }

        m_lineRenderer.positionCount = points.Count;
        m_lineRenderer.SetPositions(points.ToArray());
    }
}
