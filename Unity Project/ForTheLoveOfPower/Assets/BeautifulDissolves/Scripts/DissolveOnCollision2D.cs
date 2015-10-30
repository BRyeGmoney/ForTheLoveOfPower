using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class DissolveOnCollision2D : Dissolve {

		public LayerMask collisionLayer = -1;

		void OnCollisionEnter2D(Collision2D col)
		{
			if (col.gameObject.IsInLayerMask (collisionLayer)) {
				TriggerDissolve ();
			}
		}
	}
}