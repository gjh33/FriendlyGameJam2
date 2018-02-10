using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispatcher : MonoBehaviour {

	public Dictionary<int, Creature> creatureLookUp = new Dictionary<int, Creature> ();

	private static Dispatcher _instance;

	private static object _lock = new object();

	public static Dispatcher instance
	{
		get
		{
			if (applicationIsQuitting) {
				Debug.LogWarning("[Singleton] Instance '"+ typeof(Dispatcher) +
					"' already destroyed on application quit." +
					" Won't create again - returning null.");
				return null;
			}

			lock(_lock)
			{
				if (_instance == null)
				{
					_instance = (Dispatcher) FindObjectOfType(typeof(Dispatcher));

					if ( FindObjectsOfType(typeof(Dispatcher)).Length > 1 )
					{
						Debug.LogError("[Singleton] Something went really wrong " +
							" - there should never be more than 1 singleton!" +
							" Reopening the scene might fix it.");
						return _instance;
					}

					if (_instance == null)
					{
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<Dispatcher>();
						singleton.name = "(singleton) "+ typeof(Dispatcher).ToString();

						DontDestroyOnLoad(singleton);

						Debug.Log("[Singleton] An instance of " + typeof(Dispatcher) + 
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

	public void Command(string command, params object[] args) {
		if (command == "selfplaycard") {
			int cardIndex = (int) args[0];
			int x = (int) args[1];
			int y = (int) args[2];
			Card card = GameSystem.instance.playerOne.hand [cardIndex];
			if (GameSystem.instance.PlaceTile (card.tile, x, y, GameSystem.instance.playerOne)) {
				if (GameSystem.instance.PlayCard (GameSystem.instance.playerOne, card)) {
					// Basically guarenteed (MAYBE)
					GameSystem.instance.Spawn (card, x, y, GameSystem.instance.playerOne);
					// ADD NETWORKING
				}
			}
		} else if (command == "enemyplaycard") {
			int cardIndex = (int) args[0];
			int x = (int) args[1];
			int y = (int) args[2];
			Card card = GameSystem.instance.playerTwo.hand [cardIndex];
			if (GameSystem.instance.PlaceTile (card.tile, x, y, GameSystem.instance.playerTwo)) {
				if (GameSystem.instance.PlayCard (GameSystem.instance.playerTwo, card)) {
					// Basically guarenteed (MAYBE)
					GameSystem.instance.Spawn (card, x, y, GameSystem.instance.playerTwo);
					// ADD NETWORKING
				}
			}
		} else if (command == "move") {
			int creatureId = (int)args [0];
			int x = (int)args [1];
			int y = (int)args [2];
			Creature creature = creatureLookUp[creatureId];
			GameSystem.instance.Move (creature, x, y);
			// ADD NETWORKING
		} else if (command == "attack") {
			int attackerId = (int)args [0];
			int defenderId = (int)args [1];
			Creature attacker = creatureLookUp[attackerId];
			Creature defender = creatureLookUp[defenderId];
			if (GameSystem.instance.Battle (attacker, defender)) {
				if (attacker.health == 0) {
					GameSystem.instance.Kill (attacker);
					creatureLookUp.Remove (attacker.id);
				}
				if (defender.health == 0) {
					GameSystem.instance.Kill (defender);
					creatureLookUp.Remove (defender.id);
				}
				// ADD NETWORKING
			}
		} else {
			Debug.LogWarning (string.Format("Unknown command {0}", command));
		}
	}

	private void AddNewId(Creature creature) {
		int key = 0;
		while (creatureLookUp.ContainsKey(key)) {
			key++;
		}
		creature.id = key;
	}
}
