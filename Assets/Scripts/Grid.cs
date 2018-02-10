using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType { FourWay, EightWay };

public class Grid : MonoBehaviour, IEnumerable {
    #region Main Code

    public GridType type;
    public Coordinate2 size;
    public float cellWidth = 1;
    public float cellHeight = 1;

    private GridCell[,] data;

    public GridCell this[int x, int y]
    {
        get
        {
            return data[x, y];
        }
        private set {
            data[x, y] = value;
        }
    }

    /// <summary>
    /// Use this for initialization of anything that doesn't reference dependencies
    /// Initialize variables and grid structure
    /// </summary>
    void Awake()
    {
        data = new GridCell[size.x, size.y];
        ReloadGrid();
    }

    /// <summary>
    /// Reloads the grid's shape and cells based on public attributes
    /// </summary>
    public void ReloadGrid()
    {
        // Set up each cell
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                data[x, y] = new GridCell(this, x, y);
            }
    }

    /// <summary>
    /// Gets neighbors for a given cell in this grid based on grid type
    /// </summary>
    /// <param name="cell">A cell in this grid</param>
    /// <returns>Neighbors of cell</returns>
    public List<GridCell> GetNeighborsFor(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        switch (type)
        {
            case GridType.FourWay:
                neighbors.Add(data[cell.x, cell.y + 1]);
                neighbors.Add(data[cell.x + 1, cell.y]);
                neighbors.Add(data[cell.x, cell.y - 1]);
                neighbors.Add(data[cell.x - 1, cell.y]);
                break;
            case GridType.EightWay:
                neighbors.Add(data[cell.x, cell.y + 1]);
                neighbors.Add(data[cell.x + 1, cell.y + 1]);
                neighbors.Add(data[cell.x + 1, cell.y]);
                neighbors.Add(data[cell.x + 1, cell.y - 1]);
                neighbors.Add(data[cell.x, cell.y - 1]);
                neighbors.Add(data[cell.x - 1, cell.y - 1]);
                neighbors.Add(data[cell.x - 1, cell.y]);
                neighbors.Add(data[cell.x - 1, cell.y + 1]);
                break;
        }

        return neighbors;
    }

    // Find a gridcell with a given game object
    public GridCell Find(GameObject occupant)
    {
        foreach (GridCell gc in data)
        {
            if (gc.occupant == occupant) return gc;
        }

        return null;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return data.GetEnumerator();
    }
    #endregion

    #region Editor Code
    private void OnDrawGizmosSelected()
    {
        float bottomLeftX = (transform.position.x - ((size.x * cellWidth) / 2)) + (cellWidth / 2);
        float bottomLeftY = (transform.position.y - ((size.y * cellHeight) / 2)) + (cellHeight / 2);
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(new Vector3(bottomLeftX + (x * cellWidth), bottomLeftY + (y * cellHeight)), new Vector3(cellWidth, cellHeight));
            }
    }
    #endregion
}