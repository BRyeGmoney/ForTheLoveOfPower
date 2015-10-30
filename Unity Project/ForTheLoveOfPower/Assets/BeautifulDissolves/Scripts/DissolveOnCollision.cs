using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class DissolveOnCollision : Dissolve {

		public LayerMask collisionLayer = -1;

		void OnCollisionEnter(Collision col)
		{
			if (col.gameObject.IsInLayerMask (collisionLayer)) {
				TriggerDissolve ();
			}
		}
	}
}