using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Card", fileName="card", order=2)]
public class Card : ScriptableObject {
	new public string name;
	public int range;
	public int attack;
	public int defense;
	public int speed;
	public int health;
	public GameObject model;
	public Tile tile;
}
