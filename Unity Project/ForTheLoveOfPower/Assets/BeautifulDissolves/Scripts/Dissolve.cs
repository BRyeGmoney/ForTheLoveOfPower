using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BeautifulDissolves {
	public class Dissolve : MonoBehaviour {

		[SerializeField]
		protected bool m_IsUIElement;

		public bool oneTime = true;
		public AnimationCurve dissolveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField, Range(0f, 1f)]
		protected float m_DissolveStartPercent = 0f;
		[SerializeField]
		protected float m_Time = 4f;
		[SerializeField]
		protected float m_Speed = 1f;

		public UnityEvent OnDissolveStart;
		public UnityEvent OnDissolveFinish;

		protected Material[] m_Materials;
		protected IEnumerator m_CurrentCoroutine;
		protected bool m_Dissolving;

		void Awake()
		{
			if (m_IsUIElement) {
				Graphic graphic = GetComponentInChildren<Graphic>();

				if (graphic != null && graphic.materialForRendering != null) {
					Material mat = Instantiate(graphic.materialForRendering);
					graphic.material = mat;
					m_Materials = new Material[]{mat};
				}
			} else {
				// Finds the first renderer on itself or on any of its children
				Renderer rend = GetComponentInChildren<Renderer>();

				if (rend != null) {
					m_Materials = rend.materials;
				}
			}
		}

		public void TriggerDissolve()
		{
			TriggerDissolve(m_Time, m_DissolveStartPercent, m_Speed);
		}

		public void TriggerDissolve(float time, float dissolveStartPercent, float speed)
		{
			if (m_Materials != null && m_Materials.Length > 0) {
				if (!(oneTime && m_Dissolving)) {
					m_Dissolving = true;
					
					if (m_CurrentCoroutine != null) {
						StopCoroutine (m_CurrentCoroutine);
					}

					m_CurrentCoroutine = DissolveHelper.CurveDissolve(m_Materials, dissolveCurve, time, dissolveStartPercent, speed);
					StartCoroutine(YieldDissolve(m_CurrentCoroutine));
				}
			}
		}

		public void TriggerReverseDissolve()
		{
			TriggerDissolve(m_Time, 1f, m_Speed * -1f);
		}

		public void TriggerReverseDissolve(float time, float dissolveStartPercent, float speed)
		{
			TriggerDissolve(time, dissolveStartPercent, speed * -1f);
		}

		IEnumerator YieldDissolve(IEnumerator coroutine)
		{
			// Dispatch dissolve start event
			if (OnDissolveStart != null) {
				OnDissolveStart.Invoke();
			}

			// Yield until dissolve is finished
			yield return StartCoroutine(coroutine);

			// Dispatch dissolve finished event
			if (OnDissolveFinish != null) {
				OnDissolveFinish.Invoke();
			}
		}

		void OnDestroy()
		{
			if (m_Materials != null) {
				// Clean up material instance
				for (int i = 0; i < m_Materials.Length; i++) {
					if (m_Materials[i] != null) {
						Destroy(m_Materials[i]);
					}
				}
			}
		}
	}
}
