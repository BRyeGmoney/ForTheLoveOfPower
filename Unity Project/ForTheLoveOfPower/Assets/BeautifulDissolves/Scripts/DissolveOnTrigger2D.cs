using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class DissolveOnTrigger2D : Dissolve {

		public LayerMask collisionLayer = -1;

		void OnTriggerEnter2D(Collider2D col)
		{
			if (col.gameObject.IsInLayerMask (collisionLayer)) {
				TriggerDissolve ();
			}
		}
	}
}