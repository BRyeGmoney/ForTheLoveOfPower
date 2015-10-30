using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class RespawnManager : MonoBehaviour {
		public GameObject[] zombies;
		public GameObject[] zombiePrefabs;
		public Transform[] spawnPositions;

		public void Respawn()
		{
			for (int i = 0; i < zombies.Length; i++) {
				if (zombies[i] != null) {
					Destroy(zombies[i]);
				}
			}

			for (int i = 0; i < zombiePrefabs.Length && i < spawnPositions.Length; i++) {
				zombies[i] = (GameObject)Instantiate(zombiePrefabs[i], spawnPositions[i].position, Quaternion.Euler(new Vector3(0f, 180f, 0f)));
			}
		}
	}
}
