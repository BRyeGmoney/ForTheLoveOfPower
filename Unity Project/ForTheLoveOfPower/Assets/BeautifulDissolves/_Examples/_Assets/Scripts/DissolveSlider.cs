using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace BeautifulDissolves {
	public class DissolveSlider : MonoBehaviour {

		[SerializeField] Slider m_Slider;
		[SerializeField] Renderer[] m_Renderers;

		public void UpdateDissolve()
		{
			for (int i = 0; i < m_Renderers.Length; i++) {
				m_Renderers[i].material.SetFloat(DissolveHelper.dissolveAmountID, m_Slider.value);
			}
		}
	}
}

