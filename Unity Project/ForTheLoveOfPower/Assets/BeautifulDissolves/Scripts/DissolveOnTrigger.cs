using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class DissolveOnTrigger : Dissolve {

		public LayerMask collisionLayer = -1;

		void OnTriggerEnter(Collider col)
		{
			if (col.gameObject.IsInLayerMask (collisionLayer)) {
				TriggerDissolve ();
			}
		}
	}
}