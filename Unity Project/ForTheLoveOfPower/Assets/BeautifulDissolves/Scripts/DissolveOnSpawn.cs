using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class DissolveOnSpawn : Dissolve {

		void Reset()
		{
			dissolveCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
		}

		void OnEnable()
		{
			TriggerDissolve();
		}
	}
}