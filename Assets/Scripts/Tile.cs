using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Tile", fileName="tiles", order=1)]
public class Tile : ScriptableObject {

	public bool[] tiles = new bool[25];

	public bool this[int i, int j]{
		get
		{
			return tiles[i*5+j];
		}
		set
		{
			tiles[i*5+j] = value;
		}
	}
}
