using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Tile", fileName="tiles", order=1)]
public class Tile : ScriptableObject {

	public bool[] tiles = new bool[25];

	void OnEnable(){
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				this [i, j] = false;
			}
		}
		this [2, 2] = true;
	}

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
