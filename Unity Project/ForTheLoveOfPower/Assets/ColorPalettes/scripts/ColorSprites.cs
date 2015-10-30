using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MrWhaleGames.ColorPalette;


namespace MrWhaleGames.ColorPalette_examples
{

		/// <summary>
		/// The example script Color sprites shows how a <see cref="ColorPalette.PaletteCollectionData"/> can be applied to SpriteRenderer.
		/// Let it either switch through the Palettes the Collections holds or use a specific Palette to color the Sprites.
		/// </summary>
		public class ColorSprites : MonoBehaviour
		{

				private int paletteCount = 0;

				[Tooltip("if TRUE only one Palette is used!")]
				public bool
						useSpecificPalette = false;

				[Tooltip("if useSpecificPalette == TRUE chose here which Palette")]
				public int
						usePalette = 0;

				[Tooltip("switch on and see it in playmode")]
				public bool
						switchPalettes = true;

				[Range(0, 10)]
				public float
						switchPauseTime = 1;

				private List<SpriteRenderer> grids = null;
				private List<SpriteRenderer> cylinders = null;
				private List<SpriteRenderer> engines = null;
				private List<SpriteRenderer> boxs = null;
				private List<SpriteRenderer> batteries = null;

				private PaletteCollection collection = null;

				void Start ()
				{
						setupCollection ();

						if (this.useSpecificPalette) {

								if (usePalette < 0) {
										usePalette = 0;
								}
				
								if (usePalette > collection.Data.palettes.Count) {
										usePalette = collection.Data.palettes.Count - 1;
								}

								changeColorForSpriteRenderers (usePalette);

						} else if (this.switchPalettes) {

								StartCoroutine (changeColor (collection.Data));
						}
				}


				private void setupCollection ()
				{
						if (collection == null) {
								collection = this.GetComponent<PaletteCollection> ();
						}
				}

				private void setupSpriteRenderers ()
				{

						this.grids = new List<SpriteRenderer> ();
						foreach (Transform trans in GetOnlyChildren("grid")) {
								if (trans.gameObject.GetComponent<SpriteRenderer> () != null) {
										this.grids.Add (trans.gameObject.GetComponent<SpriteRenderer> ());
								}
						}

						this.cylinders = new List<SpriteRenderer> ();
						foreach (Transform trans in GetOnlyChildren("cylinder")) {
								if (trans.gameObject.GetComponent<SpriteRenderer> () != null) {
										this.cylinders.Add (trans.gameObject.GetComponent<SpriteRenderer> ());
								}
						}

						this.engines = new List<SpriteRenderer> ();
						foreach (Transform trans in GetOnlyChildren("engine")) {
								if (trans.gameObject.GetComponent<SpriteRenderer> () != null) {
										this.engines.Add (trans.gameObject.GetComponent<SpriteRenderer> ());
								}
						}

						this.boxs = new List<SpriteRenderer> ();
						foreach (Transform trans in GetOnlyChildren("box")) {
								if (trans.gameObject.GetComponent<SpriteRenderer> () != null) {
										this.boxs.Add (trans.gameObject.GetComponent<SpriteRenderer> ());
								}
						}

						this.batteries = new List<SpriteRenderer> ();
						foreach (Transform trans in GetOnlyChildren("batteries")) {
								if (trans.gameObject.GetComponent<SpriteRenderer> () != null) {
										this.batteries.Add (trans.gameObject.GetComponent<SpriteRenderer> ());
								}
						}
				}

				public void changeColorForSpriteRenderers (int palette)
				{
						setupCollection ();

						if (this.grids == null) {
								setupSpriteRenderers ();
						}

						PaletteData paletteData = collection.Data.palettes [palette];

						foreach (SpriteRenderer sprRenderer in this.grids) {
								sprRenderer.color = paletteData.colors [0];
						}

						foreach (SpriteRenderer sprRenderer in this.cylinders) {
								sprRenderer.color = paletteData.colors [1];
						}

						foreach (SpriteRenderer sprRenderer in this.engines) {
								sprRenderer.color = paletteData.colors [2];
						}

						foreach (SpriteRenderer sprRenderer in this.boxs) {
								sprRenderer.color = paletteData.colors [3];
						}

						foreach (SpriteRenderer sprRenderer in this.batteries) {
								sprRenderer.color = paletteData.colors [4];
						}
				}
		
				private IEnumerator changeColor (PaletteCollectionData collectionData)
				{

						while (true) {
				
								if (paletteCount > collectionData.palettes.Count - 1) {
										paletteCount = 0;
								}				

								yield return StartCoroutine (switchToNextPalette (paletteCount));
				
								paletteCount++;
						}
			
				}
		
				private IEnumerator switchToNextPalette (int palette)
				{
						changeColorForSpriteRenderers (palette);

						yield return new WaitForSeconds (this.switchPauseTime);
				}


				public List<Transform> GetOnlyChildren (string onlyWithName = "")
				{
						Transform[] children = this.transform.GetComponentsInChildren<Transform> ();
						//Debug.Log ("length " + children.Length);
			
						if (children.Length > 1) {
								List<Transform> exceptMySelf = new List<Transform> (); // [children.Length - 1];
								int newIndex = 0;
				
								for (int i = 0; i < children.Length; i++) {
										Transform child = children [i];

										if (string.IsNullOrEmpty (onlyWithName)) {
												if (child != this.transform) {
														exceptMySelf.Add (child);// [newIndex] = child;
														newIndex++;
												}
										} else {
												if (child != this.transform && child.gameObject.name.Equals (onlyWithName)) {
														exceptMySelf.Add (child);// [newIndex] = child;
														newIndex++;
												}
										}
								}
				
								//Debug.Log ("return length " + exceptMySelf.Count);
								return exceptMySelf;
						}
			
						return null;
				}

		}

}