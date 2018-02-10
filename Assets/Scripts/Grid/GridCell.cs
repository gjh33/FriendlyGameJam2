using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell {
    public enum State { Empty, Placed }

    // These values should not be changed after creation
    public Coordinate2 gridLocation { get; private set; }
    public GameObject occupant;
    public State state;

    // Private
    private Grid grid;

    // method variables
    // Convenience x
    public int x
    {
        get
        {
            return gridLocation.x;
        }
        set
        {
            gridLocation = new Coordinate2(value, gridLocation.y);
        }
    }
    // Convenience y
    public int y
    {
        get
        {
            return gridLocation.y;
        }
        set
        {
            gridLocation = new Coordinate2(gridLocation.x, value);
        }
    }

    // Matching structure
    public GridCell(Grid grid, Coordinate2 gridLocation)
    {
        this.grid = grid;
        this.gridLocation = gridLocation;
        this.occupant = null;
        this.state = State.Empty;
    }

    // Alternate structure
    public GridCell(Grid grid, int x, int y)
    {
        this.grid = grid;
        this.gridLocation = new Coordinate2(x, y);
        this.occupant = null;
        this.state = State.Empty;
    }

    public Vector3 getWorldPosition()
    {
        // Math break down:
        // first we find the bottom left corner by calculating half the total width (size.x * cellWidth) / 2
        // and subtracting that from current position (transform.position.x)
        // Finally we adjust to the center of the cell instead of the bottom left corner +(cellWidth / 2)
        float bottomLeftX = (grid.transform.position.x - ((grid.size.x * grid.cellWidth) / 2)) + (grid.cellWidth / 2);
        float bottomLeftY = (grid.transform.position.y - ((grid.size.y * grid.cellHeight) / 2)) + (grid.cellHeight / 2);

        return new Vector3(bottomLeftX + (x * grid.cellWidth), bottomLeftY + (y * grid.cellHeight));
    }
}
