using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class UpdateLight : MonoBehaviour {

		[SerializeField] Renderer m_Renderer;
		[SerializeField] Light m_Light;

		private float m_OriginalValue;
		private Material m_Material;

		void Awake()
		{
			m_OriginalValue = m_Light.intensity;
			m_Material = m_Renderer.material;
		}

		void Update ()
		{
			m_Light.intensity = m_OriginalValue * (1f - m_Material.GetFloat(DissolveHelper.dissolveAmountID));
		}

		void OnDestory()
		{
			if (m_Material != null) {
				Destroy(m_Material);
			}
		}
	}
}
