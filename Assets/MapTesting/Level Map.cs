using System.Collections.Generic;
using System.Linq;
using Labyrinth;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelMap", menuName = "Scriptable Objects/LevelMap")]
public class LevelMap : ScriptableObject
{
    public int rowCount => cells.Count;
    public int columnCount => rowCount > 0 ? cells[0].Count : 0;
    public List<List<Cell>> cells = new();
    public List<Cell> AllFilled => cells.SelectMany(inner => inner).Where(c => c.gridSpaceType != GridSpace.empty).ToList();

    public LevelMap(int rows, int columns)
    {
        CreateEmptyMap(rows, columns);
    }

    public Cell GetCell(int row, int col, Object context = null)
    {
        Cell output = null;

        if (row > rowCount-1 || row < 0)
        {
            Debug.LogError($"Requested a cell in row index {row} outside of row bounds 0 <= # < {rowCount}.", context);
            return output;
        }
        if (col > columnCount-1 || col < 0)
        {
            Debug.LogError($"Requested a cell in column index {col} outside of column bounds 0 <= # < {columnCount}.", context);
            return output;
        }

        return cells[row][col];
    }

    public void EmptyMap()
    {
        foreach(var row in cells) row.Clear();
        cells.Clear();

    }

    public List<Cell> GetCellNeighbors(Cell c, bool includeEmpty = false) => GetCellNeighbors(c.row, columnCount, includeEmpty);
    public List<Cell> GetCellNeighbors(int r, int c, bool includeEmpty)
    {
        List<Cell> output = new();

        // Only 4 so we can just do manually
        if (r < rowCount-2) output.Add(cells[r+1][c]);
        if (0 < r) output.Add(cells[r-1][c]);
        if (c < columnCount-2) output.Add(cells[r][c+1]);
        if (0 < c) output.Add(cells[r][c-1]);

        if (!includeEmpty) output.RemoveAll(c => c.gridSpaceType == GridSpace.empty);

        return output;
    }

    

    public void CreateEmptyMap(int gridSize) => CreateEmptyMap(gridSize, gridSize);
    public void CreateEmptyMap(int rows, int columns)
    {
        EmptyMap();

        cells ??= new();

        for (int i = 0; i < rows; i++)
        {
            // init row list
            cells.Add(new());

            for (int j = 0; j < columns; j++)
            {
                cells[i].Add(new Cell(i,j));
            }
        }
        
    }

    public void SetAll(GridSpace type)
    {
        cells.ForEach(col => col.ForEach(cell => cell.gridSpaceType = type));
    }

    public string PrintMap()
    {
        var output = "Current map:\n";
        
        if(cells == null || cells.Count == 0)
        {
            output += "None\n";
        }
        else
        {
            for (int i = 0; i < cells.Count; i++)
            {
                for (int j = 0; j < cells[i].Count; j++)
                {
                    output += " " + (cells[i][j].gridSpaceType == GridSpace.filled ? "x" : "o");
                }
                output += "\n";
            }
        }


        return output;
    }
}
