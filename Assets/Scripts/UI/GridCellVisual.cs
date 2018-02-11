using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCellVisual : MonoBehaviour {

    public Material emptyStateMaterial;
    public Material occupiedStateMaterial;
    public Material hoverMaterial;

	public void setEmptyState()
    {
        gameObject.GetComponent<Renderer>().material = emptyStateMaterial;
    }

    public void setOccupiedState()
    {
        gameObject.GetComponent<Renderer>().material = occupiedStateMaterial;
    }

    public void setHoverState()
    {
        gameObject.GetComponent<Renderer>().material = hoverMaterial;
    }
}
