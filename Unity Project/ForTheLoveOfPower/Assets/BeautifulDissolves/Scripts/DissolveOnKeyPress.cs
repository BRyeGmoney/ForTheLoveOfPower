using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class DissolveOnKeyPress : Dissolve {

		public KeyCode key;
		public bool isToggle;

		private bool toggle;

		void Update ()
		{
			if (Input.GetKeyDown(key)) {
				if (isToggle) {
					if (!toggle) {
						TriggerDissolve();
					} else {
						TriggerReverseDissolve();
					}

					toggle = !toggle;
				} else {
					TriggerDissolve();
				}
			}
		}
	}
}
