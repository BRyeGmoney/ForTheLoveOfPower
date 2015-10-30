using UnityEngine;
using System.Collections;
using MrWhaleGames.ColorPalette;


namespace MrWhaleGames.ColorPalette_examples
{
		/// <summary>
		/// Background palette is an example of planes which could be used in the background.
		/// The color and the size is represented by the values of the palette.
		/// Switch into the play mode of Unity and change the <see cref="ColorPalette.PaletteData"/> values to see the change!
		/// It makes use registering at the <c>OnChange</c> Event of the <see cref="ColorPalette.PaletteData"/>.
		/// </summary>
		public class AdditiveMaskAnimation : MonoBehaviour
		{
				public PaletteData myPaletteData;

				public Transform[] planes;

				private float minTextureOffset = -0.5f;
				private float maxTextureOffset = 0.9f;

				// Use this for initialization
				void Awake ()
				{
						planes = GetOnlyChildren ();

						Palette palette = this.GetComponent<Palette> ();
			
						if (palette != null) {
								myPaletteData = palette.Data;
						} else {
								PaletteImporter paletteImporter = this.GetComponent<PaletteImporter> ();
								myPaletteData = paletteImporter.Data;
						}

				}

				void Start ()
				{
						myPaletteData.OnChange += new PaletteData.OnChangeHandler (changePlanes);

						// first call to update intially
						changePlanes (myPaletteData);
				}
	
				void OnDisable ()
				{
						if (myPaletteData != null) {
								myPaletteData.OnChange -= new PaletteData.OnChangeHandler (changePlanes);
						}
				}

				private void changePlanes (PaletteData colorData)
				{
		
						for (int i = 0; i < colorData.percentages.Count; i++) {
								Transform trans = planes [i];
			
								// 0.8 - 0.2 -> 0.6 / 0.25 = 0.15 -> 0.2 + 0.15 = 0.35
/*								float diffPct = (this.maxTextureOffset - this.minTextureOffset);
								float newOffset = this.maxTextureOffset - diffPct * colorData.percentages [i];
								//Debug.Log ("diff " + diffPct + " newOffset " + newOffset);
								newOffset = Mathf.Clamp (newOffset, this.minTextureOffset, this.maxTextureOffset);
*/
								//float newOffset = Mathf.SmoothStep (this.minTextureOffset, this.maxTextureOffset, 1 - colorData.percentages [i]);
								float newOffset = Mathf.Lerp (this.minTextureOffset, this.maxTextureOffset, 1 - colorData.percentages [i]);

								trans.gameObject.GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", new Vector2 (0, newOffset));

								trans.GetComponent<Renderer> ().material.color = colorData.colors [i];
						}
				}


				public Transform[] GetOnlyChildren ()
				{
						Transform[] children = this.transform.GetComponentsInChildren<Transform> ();
						//Debug.Log ("length " + children.Length);
		
						if (children.Length > 1) {
								Transform[] exceptMySelf = new Transform [children.Length - 1];
								int newIndex = 0;
			
								for (int i = 0; i < children.Length; i++) {
										Transform child = children [i];
										if (child != this.transform) {
												exceptMySelf [newIndex] = child;
												newIndex++;
										}
								}
			
								//Debug.Log ("return length " + exceptMySelf.Length);
								return exceptMySelf;
						}
		
						return null;
				}



		}

}