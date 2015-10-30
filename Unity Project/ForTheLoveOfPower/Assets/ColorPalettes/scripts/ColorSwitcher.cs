using UnityEngine;
using System.Collections;
using MrWhaleGames.ColorPalette;

/// <summary>
/// Color palettes examples has various examples which can be used as a starting point to build up the specific uses in your game.
/// </summary>
namespace MrWhaleGames.ColorPalette_examples
{

		/// <summary>
		/// The Color switcher is an example that goes through a <see cref="ColorPalette.Palette"/> and affect its gameobject to change the material or light color.
		/// Either via hard switching the Colors or lerping them. Adjust the <c>lerpTime</c> or <c>switchPauseTime</c> to see the difference.
		/// </summary>
		public class ColorSwitcher : MonoBehaviour
		{
				public bool usePercentageForTime = false;
				[Range(0, 10)]
				public float
						maxTime = 5;

				public bool lerpColor = true;

				[Range(0, 10)]
				public float
						lerpTime = 1;

				public bool switchColor = true;

				[Range(0, 10)]
				public float
						switchPauseTime = 1;

				public bool affectMaterialColor = false;
				public bool affectLightColor = false;

				private int colorCount = 0;

				void Start ()
				{
						Palette palette = this.GetComponent<Palette> ();

						if (palette != null) {
								StartCoroutine (changeColor (palette.Data));
						} else {
								PaletteImporter paletteImporter = this.GetComponent<PaletteImporter> ();
								StartCoroutine (changeColor (paletteImporter.Data));
						}
				}
	

				private IEnumerator changeColor (PaletteData colorData)
				{

						while (true) {

								if (colorCount > colorData.colors.Count - 1) {
										colorCount = 0;
								}

								if (lerpColor) {
										int nextColor = colorCount + 1;				
										if (nextColor > colorData.colors.Count - 1) {
												nextColor = 0;
										}

										if (usePercentageForTime) {
												float percentageTime = maxTime * colorData.percentages [colorCount];
												yield return StartCoroutine (LerpColor (colorData.colors [colorCount], colorData.colors [nextColor], percentageTime));
										} else {
												yield return StartCoroutine (LerpColor (colorData.colors [colorCount], colorData.colors [nextColor], lerpTime));
										}


								} else if (switchColor) {
										yield return StartCoroutine (switchToNextColor (colorData));
								} else {
										// in case nothing is chosen
										yield return new WaitForSeconds (1);
								}


								colorCount++;
						}

				}

				private IEnumerator switchToNextColor (PaletteData colorData)
				{
						if (usePercentageForTime) {

								if (affectLightColor && this.GetComponent<Light>() != null) {
										this.GetComponent<Light>().color = colorData.colors [colorCount];
								} else if (affectMaterialColor && this.GetComponent<Renderer>() != null) {
										this.GetComponent<Renderer>().material.color = colorData.colors [colorCount];
								}
								yield return new WaitForSeconds (this.maxTime * colorData.percentages [colorCount]);

						} else {
								if (affectLightColor && this.GetComponent<Light>() != null) {
										this.GetComponent<Light>().color = colorData.colors [colorCount];
								} else if (affectMaterialColor && this.GetComponent<Renderer>() != null) {
										this.GetComponent<Renderer>().material.color = colorData.colors [colorCount];
								}

								yield return new WaitForSeconds (this.switchPauseTime);
						}
				}


				private IEnumerator LerpColor (Color from, Color to, float time)
				{
				
						for (float timeCount = 0; timeCount < time; timeCount += Time.deltaTime) {
					
								if (affectLightColor && this.GetComponent<Light>() != null) {
										this.GetComponent<Light>().color = Color.Lerp (from, to, timeCount / time);
								} else if (affectMaterialColor && this.GetComponent<Renderer>() != null) {
										this.GetComponent<Renderer>().material.color = Color.Lerp (from, to, timeCount / time);
								}


								yield return new WaitForEndOfFrame ();
						}
				}


		}

}