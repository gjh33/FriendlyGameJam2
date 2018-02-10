using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispatcher : MonoBehaviour {

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
		if (command == "summon") {
			Creature creature = (Creature) args[0];
			int x = (int) args[1];
			int y = (int) args[2];
			int player = (int) args[3];
			if (GameSystem.instance.PlaceTile (creature.card.tile, x, y, player)) {
				// Basically guarenteed (MAYBE)
				GameSystem.instance.Spawn (creature, x, y);
				// ADD NETWORKING
			}
		} else if (command == "move") {
			Creature creature = (Creature) args[0];
			int x = (int) args[1];
			int y = (int) args[2];
			GameSystem.instance.Move (creature, x, y);
			// ADD NETWORKING
		} else if (command == "attack") {
			Creature attacker = (Creature) args[0];
			Creature defender = (Creature) args[1];
			if (GameSystem.instance.Battle (attacker, defender)) {
				if (attacker.health == 0) {
					GameSystem.instance.Kill (attacker);
				}
				if (defender.health == 0) {
					GameSystem.instance.Kill (attacker);
				}
				// ADD NETWORKING
			}
		} else {
			Debug.LogWarning (string.Format("Unknown command {0}", command));
		}
	}
}
