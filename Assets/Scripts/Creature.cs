using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour {

	public int health = 0;
	public int x = 0;
	public int y = 0;
	public bool attacked = false;
	private Card _card;

	public Card card {
		get {
			return _card;
		}
		set {
			health = value.health;
			_card = value;
		}
	}
}
