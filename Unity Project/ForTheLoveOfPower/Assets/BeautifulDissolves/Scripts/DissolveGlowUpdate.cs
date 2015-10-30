using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	/*NOTE: This script uses "DissolveGlowUpdateEditor.cs" script as a Custom Editor
	 * (The editor script is located in the BeautifulDissolves/Editor folder) */
	public class DissolveGlowUpdate : MonoBehaviour
	{
		enum StartMode {
			OnAwake,
			OnStart,
			ByScript
		};
		
		enum UpdateRate {
			EveryFrame,
			EveryNthFrame,
			CustomFixedTimestep
		};
		
		enum GlowSource {
			Emissive,
			Light
		};

		[SerializeField] StartMode m_StartMode = StartMode.OnAwake;
		[SerializeField] UpdateRate m_UpdateRate = UpdateRate.EveryFrame;
		[SerializeField] GlowSource m_GlowSource = GlowSource.Emissive;

		[Range(0f, 1f)]
		[SerializeField] float m_GlowCutoff = 1f;
		[SerializeField] int m_FrameDelay = 2;
		[SerializeField] float m_UpdateTimestep = 0.02f;
		[SerializeField] Light m_GlowLightSource;

		private Renderer m_Renderer;
		private Material m_Material;
		private int m_UpdateFrames = 1;
		private bool m_GlowUpdating;

		void Awake()
		{
			// Finds the first renderer on itself or on any of its children
			m_Renderer = GetComponentInChildren<Renderer>();

			if (m_Renderer != null) {
				m_Material = m_Renderer.material;
			}

			if (m_StartMode == StartMode.OnAwake) {
				StartGlowUpdate ();
			}
		}
		
		void Start()
		{
			if (m_StartMode == StartMode.OnStart) {
				StartGlowUpdate ();
			}

			if (m_GlowLightSource != null) {
				m_GlowLightSource.enabled = true;
			}
		}
		
		public void StartGlowUpdate()
		{
			if (m_Renderer == null) {
				Debug.LogError("Cannot start dissolve glow update (Cannot find a 'Renderer' attached to the \"" + gameObject.name + "\" game object or on any of its children).");
				return;
			}

			if (m_GlowSource == GlowSource.Light) {
				if (m_GlowLightSource == null) {
					Debug.LogError("Cannot start dissolve glow update [Light Mode] (Glow light source is not defined yet for the \"" + gameObject.name + "\" game object).");
					return;
				}

				m_Material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
			}

			m_GlowUpdating = true;

			if (m_UpdateRate == UpdateRate.CustomFixedTimestep) {
				StartCoroutine(FixedGlowUpdate());
			} else {
				m_UpdateFrames = (m_UpdateRate == UpdateRate.EveryFrame ? 1 : m_FrameDelay);

				StartCoroutine(FrameGlowUpdate());
			}
		}
		
		public void StopGlowUpdate()
		{
			m_GlowUpdating = false;
			StopAllCoroutines();
		}
		
		void GlowUpdate()
		{
			if (m_GlowSource == GlowSource.Emissive) {
				DynamicGI.SetEmissive(m_Renderer, GetEmissiveColor(m_Material));
			} else {
				m_GlowLightSource.color = GetEmissiveColor(m_Material);
			}
		}

		IEnumerator FixedGlowUpdate()
		{
			while (m_GlowUpdating) {
				GlowUpdate();
				yield return new WaitForSeconds(m_UpdateTimestep);
			}
		}

		IEnumerator FrameGlowUpdate()
		{
			while (m_GlowUpdating) {
				int frame = 0;

				GlowUpdate();

				while (frame < m_UpdateFrames) {
					yield return new WaitForEndOfFrame();
					frame++;
				}
			}
		}

		Color GetEmissiveColor(Material mat)
		{
			Color emis = new Color();
			Color glow = new Color();

			if (mat.IsKeywordEnabled("_EMISSION") && mat.HasProperty(DissolveHelper.emissionColorID)) {
				emis += mat.GetColor(DissolveHelper.emissionColorID);
			}

			if (mat.IsKeywordEnabled("_DISSOLVEMAP")) {
				float dissolveAmount = mat.GetFloat(DissolveHelper.dissolveAmountID);

				if (mat.IsKeywordEnabled("_DISSOLVEGLOW_ON")) {
					float multiplier = Mathf.Max(0f, (m_GlowCutoff - dissolveAmount));
					glow = mat.GetColor(DissolveHelper.glowColorID) * mat.GetFloat(DissolveHelper.glowIntensityID)/5f * dissolveAmount;
					glow *= multiplier;

					if (!mat.IsKeywordEnabled("_SUBMAP")) {
						emis *= multiplier;
					}
				}

				emis += glow;
			}

			return emis;
		}

		// Called from the DissolveGlowUpdate Editor Script
		public void CreateLightSource()
		{
			Light[] lights = gameObject.GetComponentsInChildren<Light>(true);
			Renderer renderer = GetComponent<Renderer>();

			foreach (Light l in lights) {
				if (l.name == "DissolveGlowLight") {
					if (renderer != null) {
						l.color = GetEmissiveColor(GetComponent<Renderer>().sharedMaterial);
					}

					m_GlowLightSource = l;
					return;
				}
			}
			
			GameObject lightSource = new GameObject("DissolveGlowLight");
			Light newLight = (Light)lightSource.AddComponent<Light>();

			if (renderer != null) {
				newLight.color = GetEmissiveColor(GetComponent<Renderer>().sharedMaterial);
			}

			lightSource.transform.SetParent(transform, false);
			m_GlowLightSource = newLight;
		}

		void OnDestroy()
		{
			if (m_Material != null) {
				Destroy(m_Material);
			}
		}
	}
}
