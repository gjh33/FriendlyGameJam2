using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour {

	private static GameSystem _instance;

	// Needs to instatiated at the start of a game
	public Grid grid;
	public Player playerOne;
	public Player playerTwo;

	private static object _lock = new object();

	public static GameSystem instance
	{
		get
		{
			if (applicationIsQuitting) {
				Debug.LogWarning("[Singleton] Instance '"+ typeof(GameSystem) +
					"' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			lock(_lock)
			{
				if (_instance == null)
				{
					_instance = (GameSystem) FindObjectOfType(typeof(GameSystem));

					if ( FindObjectsOfType(typeof(GameSystem)).Length > 1 )
					{
						Debug.LogError("[Singleton] Something went really wrong " +
							" - there should never be more than 1 singleton!" +
							" Reopening the scene might fix it.");
						return _instance;
					}

					if (_instance == null)
					{
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<GameSystem>();
						singleton.name = "(singleton) "+ typeof(GameSystem).ToString();

						DontDestroyOnLoad(singleton);

						Debug.Log("[Singleton] An instance of " + typeof(GameSystem) + 
							" is needed in the scene, so '" + singleton +
							"' was created with DontDestroyOnLoad.");
					} else {
						Debug.Log("[Singleton] Using instance already created: " +
							_instance.gameObject.name);
					}
				}

				return _instance;
			}
		}
	}

	private static bool applicationIsQuitting = false;
	/// <summary>
	/// When Unity quits, it destroys objects in a random order.
	/// In principle, a Singleton is only destroyed when application quits.
	/// If any script calls Instance after it have been destroyed, 
	///   it will create a buggy ghost object that will stay on the Editor scene
	///   even after stopping playing the Application. Really bad!
	/// So, this was made to be sure we're not creating that buggy ghost object.
	/// </summary>
	public void OnDestroy () {
		applicationIsQuitting = true;
	}

	public bool DrawCard(Player player) {
		if (player.deck.Count > 0) {
			player.hand.Add (player.deck [0]);
			player.deck.RemoveAt (0);
			return true;
		}
		return false;
	}

	public bool PlayCard(Player player, Card card) {
		if (player.hand.Contains (card)) {
			player.hand.Remove (card);
			return true;
		}
		return false;
	}

	public GameObject Spawn(Card card, int x, int y, Player player) {
		// There exists a valid tile
		if (grid [x, y].GetState () == GridCell.State.Placed) {
			// No existing monster there
			if (grid [x, y].occupant == null) {
				// Create a new card
				GameObject character = new GameObject("(instance) "+card.name);
				Creature creature = character.AddComponent<Creature> ();
				creature.card = card;
				creature.owner = player;
				grid [x, y].occupant = character;
				return character;
			}
		}
		return null;
	}

	/// <summary>
	/// Places the tile where x and y are the center of the grid.
	/// </summary>
	/// <returns><c>true</c>, if tile was placed, <c>false</c> otherwise.</returns>
	public bool PlaceTile(Tile tile, int x, int y, Player player) {
		// Check for space validity
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				// Check if tile is checked in the tile scheme
				if (tile [i, j]) {
					int test_x = x + i - 2;
					int test_y = y + j - 2;
					// Bound check
					if (test_x >= 0 && test_x < grid.size.x && test_y >= 0 && test_y < grid.size.y) {
						// Check to see if grid is taken
						if (grid [test_x, test_y].GetState () == GridCell.State.Placed) {
							return false;
						}
					} else {
						// If grid falls outside the grid
						return false;
					}
				}
			}
		}

		// Check for connection validity
		// Set the tiles as placed
		HashSet<GridCell> valid = new HashSet<GridCell>(); 
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				// Check if tile is checked in the tile scheme
				if (tile [i, j]) {
					int real_x = x + i - 2;
					int real_y = y + j - 2;
					grid [real_x, real_y].SetState (GridCell.State.Placed);
					valid.Add (grid [real_x, real_y]);
				}
			}
		}

		Creature kingCreature = player.king.GetComponent<Creature> (); 
		// If DFS fails then remove the tiles and return false
		if (!King_DFS(grid[kingCreature.x, kingCreature.y], new HashSet<GridCell>(), valid)) {
			for (int i = 0; i < 5; i++) {
				for (int j = 0; j < 5; j++) {
					// Check if tile is checked in the tile scheme
					if (tile [i, j]) {
						int real_x = x + i - 2;
						int real_y = y + j - 2;
						grid [real_x, real_y].SetState (GridCell.State.Empty);
					}
				}
			}
			return false;
		}
		return true;
	}

	public bool Move(Creature creature, int x, int y) {
		if (Local_DFS (grid [creature.x, creature.y], grid [x, y], creature.card.speed)) {
			grid [creature.x, creature.y].occupant = null;
			grid [x, y].occupant = creature.gameObject;
			creature.x = x;
			creature.y = y;
			return true;
		}
		return false;
	}

	public bool Kill(Creature creature) {
		grid [creature.x, creature.y].occupant = null;
		return true;
	}

	/// <summary>
	/// Battle the specified attacker and defender. Returns true is a battle is 
	/// resolvable, false otherwise.
	/// </summary>
	public bool Battle(Creature attacker, Creature defender) {
		int manhattan = Mathf.Abs (attacker.x - defender.x) + Mathf.Abs (attacker.y - defender.y);
		// Resolve range
		if (manhattan > attacker.card.range) {
			// The distance between the two units is too great
			return false;
		}

		// Attacker attacks first
		int attackDamage = Mathf.Clamp (attacker.card.attack - defender.card.defense, 0, defender.health);
		defender.health = defender.health - attackDamage;
		if (defender.health == 0) {
			return true;
		}

		if (manhattan <= defender.card.range) {
			// Defender retaliate
			int defenderDamage = Mathf.Clamp (defender.card.attack - attacker.card.defense, 0, attacker.health);
			attacker.health = attacker.health - defenderDamage;
			return true;
		}
		return false;
	}

	private bool Local_DFS(GridCell cell, GridCell target, int depth) {
		if (cell == target) {
			return true;
		}
		if (depth == 0) {
			return false;
		}
		foreach (GridCell neighbour in grid.GetNeighborsFor(cell)) {
			if (Local_DFS (neighbour, target, depth - 1)) {
				return true;
			}
		}
		return false;
	}

	private bool King_DFS(GridCell cell, HashSet<GridCell> visited, HashSet<GridCell> valid) {
		if (valid.Contains(cell)) {
			return true;
		}
		visited.Add (cell);
		foreach (GridCell neighbour in grid.GetNeighborsFor(cell)) {
			if (!visited.Contains (neighbour)) {
				if (King_DFS (neighbour, visited, valid)) {
					return true;
				}
			}
		}
		return false;
	}
}
