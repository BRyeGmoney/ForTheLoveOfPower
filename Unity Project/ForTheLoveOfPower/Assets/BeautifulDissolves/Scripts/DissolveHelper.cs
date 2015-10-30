using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public static class DissolveHelper {
		
		public static int emissionColorID = Shader.PropertyToID("_EmissionColor");
		public static int dissolveAmountID = Shader.PropertyToID("_DissolveAmount");
		public static int glowColorID = Shader.PropertyToID("_GlowColor");
		public static int glowIntensityID = Shader.PropertyToID("_GlowIntensity");
		
		public static IEnumerator LinearDissolve(Material mat, float from, float to, float time)
		{
			float elapsedTime = 0f;
			
			while (elapsedTime < time) {
				if (mat.HasProperty(dissolveAmountID)) {
					mat.SetFloat(dissolveAmountID, Mathf.Lerp(from, to, elapsedTime/time));
				}
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			if (mat.HasProperty(dissolveAmountID)) {
				mat.SetFloat (dissolveAmountID, to);
			}
		}

		public static IEnumerator LinearDissolve(Material[] mats, float from, float to, float time)
		{
			float elapsedTime = 0f;
			
			while (elapsedTime < time) {
				for (int i = 0; i < mats.Length; i++) {
					if (mats[i].HasProperty(dissolveAmountID)) {
						mats[i].SetFloat(dissolveAmountID, Mathf.Lerp(from, to, elapsedTime/time));
					}
				}
				
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			
			for (int i = 0; i < mats.Length; i++) {
				if (mats[i].HasProperty(dissolveAmountID)) {
					mats[i].SetFloat(dissolveAmountID, to);
				}
			}
		}

		public static IEnumerator CurveDissolve(Material mat, AnimationCurve dissolveCurve, float time, float curveStartPercentage, float speed)
		{
			float elapsedTime = curveStartPercentage;
			
			while (elapsedTime <= 1f && elapsedTime >= 0f) {
				if (mat.HasProperty(dissolveAmountID)) {
					mat.SetFloat(dissolveAmountID, Mathf.Clamp01(dissolveCurve.Evaluate(elapsedTime)));
				}
				elapsedTime += Time.deltaTime/time * speed;
				yield return null;
			}

			if (mat.HasProperty(dissolveAmountID)) {
				mat.SetFloat(dissolveAmountID, Mathf.Clamp01(dissolveCurve.Evaluate(Mathf.Clamp01(elapsedTime))));
			}
		}

		public static IEnumerator CurveDissolve(Material[] mats, AnimationCurve dissolveCurve, float time, float curveStartPercentage, float speed)
		{
			float elapsedTime = curveStartPercentage;
			
			while (elapsedTime <= 1f && elapsedTime >= 0f) {
				for (int i = 0; i < mats.Length; i++) {
					if (mats[i].HasProperty(dissolveAmountID)) {
						mats[i].SetFloat(dissolveAmountID, Mathf.Clamp01(dissolveCurve.Evaluate(elapsedTime)));
					}
				}
				elapsedTime += Time.deltaTime/time * speed;
				yield return null;
			}

			for (int i = 0; i < mats.Length; i++) {
				if (mats[i].HasProperty(dissolveAmountID)) {
					mats[i].SetFloat(dissolveAmountID, Mathf.Clamp01(dissolveCurve.Evaluate(Mathf.Clamp01(elapsedTime))));
				}
			}
		}

		public static bool IsInLayerMask(this GameObject obj, LayerMask mask) {
			return ((mask.value & (1 << obj.layer)) > 0);
		}
	}
}