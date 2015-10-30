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
		public class BackgroundPalette : MonoBehaviour
		{
				public PaletteData myPaletteData;

				public Transform[] planes;

				public bool reverseSequence = false;

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
		
						for (int i = 0; i < planes.Length; i++) {
								Transform trans = planes [i];
			
								if (reverseSequence) {
										// reverse sequence
										trans.GetComponent<Renderer> ().material.color = colorData.colors [colorData.colors.Count - 1 - i];
										trans.localScale = new Vector3 (trans.localScale.x, trans.localScale.y, colorData.percentages [colorData.colors.Count - 1 - i] * 10);
								} else {
										// normal sequence
										trans.GetComponent<Renderer> ().material.color = colorData.colors [i];
										trans.localScale = new Vector3 (trans.localScale.x, trans.localScale.y, colorData.percentages [i] * 10);
								}
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