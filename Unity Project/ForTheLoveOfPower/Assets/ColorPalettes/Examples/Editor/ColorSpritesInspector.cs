using UnityEngine;
using UnityEditor;
using System.Collections;
using MrWhaleGames.ColorPalette_examples;
using MrWhaleGames.ColorPalette;


[CustomEditor(typeof(ColorSprites))]
public class ColorSpritesInspector : Editor
{

		private ColorSprites myColorSprites;
		private int usePalette = -1;
		private PaletteCollectionData collectionData = null;

		[ExecuteInEditMode]
		public void OnEnable ()
		{
				myColorSprites = target as ColorSprites;

				if (myColorSprites.GetComponent<PaletteCollection> () != null) {
						collectionData = myColorSprites.GetComponent<PaletteCollection> ().Data;
				}
				if (myColorSprites.usePalette < 0) {
						myColorSprites.usePalette = 0;
				}

				if (collectionData != null
						&& myColorSprites.usePalette > collectionData.palettes.Count) {
						myColorSprites.usePalette = collectionData.palettes.Count - 1;
				}
		}

		[ExecuteInEditMode]
		public void OnDisable ()
		{
		}
	
		public override void OnInspectorGUI ()
		{    
				// uncomment for debugging
				base.DrawDefaultInspector ();

				if (myColorSprites.useSpecificPalette) {

						if (collectionData != null
								&& myColorSprites.usePalette < collectionData.palettes.Count
								&& myColorSprites.usePalette > -1
								&& this.usePalette != myColorSprites.usePalette) {

								myColorSprites.changeColorForSpriteRenderers (myColorSprites.usePalette);
								this.usePalette = myColorSprites.usePalette;
						}

				}
	
		}


}
