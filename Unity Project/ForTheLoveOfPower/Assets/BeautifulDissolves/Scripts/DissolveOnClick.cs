using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace BeautifulDissolves {
	public class DissolveOnClick : Dissolve, IPointerClickHandler {

		void OnMouseDown()
		{
			if (!m_IsUIElement) {
				TriggerDissolve();
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (m_IsUIElement) {
				TriggerDissolve();
			}
		}
	}
}