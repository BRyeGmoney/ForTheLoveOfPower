using UnityEngine;
using System.Collections;
using MrWhaleGames.ColorPalette;


namespace MrWhaleGames.ColorPalette_examples
{
		public class ColorPaletteChanger : MonoBehaviour
		{

				public float from = 0.1f;
				public float to = 0.3f;
				public float time = 2;
	
				public int percentageValue = 0;
				public bool adjustPCTBefore = false;

				// Use this for initialization
				void Start ()
				{
						Palette palette = this.GetComponent<Palette> ();
		
						if (palette != null) {
								StartCoroutine (changeValueOverTime (palette.Data, from, to, time));
						} else {
								PaletteImporter paletteImporter = this.GetComponent<PaletteImporter> ();
								StartCoroutine (changeValueOverTime (paletteImporter.Data, from, to, time));
						}
				}

				private IEnumerator changeValueOverTime (PaletteData data, float from, float to, float time)
				{
						if (percentageValue < 0) {
								percentageValue = 0;
						}

						if (percentageValue >= data.percentages.Count) {
								percentageValue = data.percentages.Count - 1;
						}

						data.ChangePercentage (percentageValue, from, adjustPCTBefore);

						float valueDiff = (to - from) / time;

						for (float timeCount = 0; timeCount < time; timeCount += Time.deltaTime) {
								//Debug.Log ("next " + data.percentages [percentageValue] + valueDiff * Time.deltaTime + " on " + percentageValue);
								data.ChangePercentage (percentageValue, data.percentages [percentageValue] + valueDiff * Time.deltaTime, adjustPCTBefore);
			
								yield return new WaitForEndOfFrame ();
						}

				}

		}


}