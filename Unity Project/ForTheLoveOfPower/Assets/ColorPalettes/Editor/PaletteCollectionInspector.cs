using UnityEngine;
using UnityEditor;
using System.Collections; 
using System.Collections.Generic;
using System.Linq;

namespace MrWhaleGames.ColorPalette
{
		[CustomEditor(typeof(PaletteCollection))]
		public class PaletteCollectionInspector : PaletteInspector
		{ 

				protected bool showImporter = false;
				protected bool showFunctions = true;

				protected float palettHeights = 50;
				protected List<bool> showPalettes;
				protected PaletteCollection myCollection;

				private Object paletteToAdd;

				new public void OnEnable ()
				{
						myCollection = target as PaletteCollection;

						base.loadIcon (myCollection, ColorPaletteStatics.paletteCollectionIcon);

						this.height = palettHeights;
						initShowPalettes (myCollection.Data);
				}

				protected void initShowPalettes (PaletteCollectionData data)
				{
						if (showPalettes == null && data != null) {
								showPalettes = new List<bool> ();
								for (int i = 0; i < data.palettes.Count; i++) {
										showPalettes.Add (false);
								}
						}
				}

				public override void OnInspectorGUI ()
				{    
						base.DrawDefaultInspector ();
			
						// margin box before buttons
						GUILayout.Space (10);
			
						if (myCollection.Data == null) {
								drawCreateButton ();
						} else {

								showImporter = EditorGUILayout.Foldout (showImporter, " Import Palette to Collection");
		
								if (showImporter) {
										drawURLImporter ();
								}

								drawAllPalettes (this.showPalettes, this.myCollection.Data);

								showFunctions = EditorGUILayout.Foldout (showFunctions, " Collection functions");

								if (showFunctions) {
										drawSizeButtons ();
								}


								showColorOnMouse (this.mouseXOffset, this.mouseYOffset, dragColorTex);
						}

				} 

				protected void drawURLImporter ()
				{
						// margin box
						GUILayout.Space (5);

						GUILayout.Label ("Insert an URL:   (the import takes a few seconds!)");

						myCollection.paletteURL = EditorGUILayout.TextField (myCollection.paletteURL);

						GUILayout.Space (10);

						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.BeginVertical ();
		
						myCollection.loadPercent = GUILayout.Toggle (myCollection.loadPercent, " include Palette Percentage");
		
						EditorGUILayout.EndVertical ();
						EditorGUILayout.BeginVertical ();


						bool import = GUILayout.Button (new GUIContent ("Import Palette to Collection", "Might take a while!"),
		                                		GUILayout.Width (Screen.width / 2));

						EditorGUILayout.EndVertical ();
						EditorGUILayout.EndHorizontal ();


						if (import && !string.IsNullOrEmpty (myCollection.paletteURL)) {
								Debug.Log ("import started with " + myCollection.paletteURL);
								myCollection.ImportPalette (myCollection.paletteURL);
						}

						GUILayout.Space (10);
				}


				protected void drawAllPalettes (List<bool> showPalettes, PaletteCollectionData collectionData)
				{
						GUILayout.Space (5);

						int i = 0;
						if (showPalettes != null) {

								if (showPalettes.Count != collectionData.palettes.Count) {
										//Debug.Log ("having " + myCollection.Data.palettes.Count + " palettes and " + this.showPalettes.Count + " foldouts");
										changeShowPalette (collectionData.palettes.Count);
								}

								int paletteToRemove = -1;

								foreach (PaletteData palette in collectionData.palettes) {

										// in case the palette was delete, but the collection is still referecing it
										if (palette != null) {
												//Debug.Log ("draw all palettes " + palette);
												showPalettes [i] = EditorGUILayout.Foldout (showPalettes [i], palette.name);

												base.drawColorPalette (palette, !showPalettes [i], false, palettHeights);

												Rect paletteRect = GUILayoutUtility.GetLastRect ();

												if (showPalettes [i]) {

														base.drawColorsAndPercentages (palette);

														GUILayout.Space (10);

														base.drawSizeButtons (palette);

														paletteToRemove = drawRemovePalette (paletteRect, palette, i);

														GUILayout.Space (20);
												}

												i++;
										}
								}


								if (paletteToRemove > -1) {
										collectionData.RemovePalette (paletteToRemove);
								}

						}
								
				}

				private void changeShowPalette (int newSize)
				{
						if (this.showPalettes != null) {
								if (newSize > this.showPalettes.Count) {
										this.showPalettes.Add (true);
								} else {

										List<bool> newBools = new List<bool> ();
										for (int i = 0; i < newSize; i++) {
												newBools.Add (this.showPalettes [i]);
										}
										this.showPalettes = newBools;
								}
						} else {
								this.showPalettes = new List<bool> (){true};
						}
				}

				protected void drawSizeButtons ()
				{		
						GUILayout.Space (5);

						EditorGUILayout.BeginHorizontal ();

						paletteToAdd = EditorGUILayout.ObjectField ("Add existing Palette", paletteToAdd, typeof(PaletteData), false);
						if (paletteToAdd) {
								myCollection.Data.AddNewPalette ((PaletteData)paletteToAdd);
								paletteToAdd = null;
						}

						EditorGUILayout.EndHorizontal ();

						GUILayout.Space (20);

						Rect newPaletteRect = EditorGUILayout.BeginHorizontal ();

						newPaletteName = EditorGUILayout.TextField ("Create a new Palette:", newPaletteName, GUILayout.Width (Screen.width * 0.85f));

						if (GUI.Button (new Rect (Screen.width - buttonWidth - buttonMarginBetween, newPaletteRect.y - buttonHeight / 2, buttonWidth, buttonHeight),
			                new GUIContent (ColorPaletteStatics.addPaletteIcon, "Add new Palette"), EditorStyles.miniButton)) { 
								myCollection.Data.AddStandardPalette (newPaletteName);
						}

						EditorGUILayout.EndHorizontal ();

						GUILayout.Space (5);

				}

				private int drawRemovePalette (Rect paletteRect, PaletteData data, int paletteCount)
				{
						int paletteToRemove = -1;

						if (GUI.Button (new Rect (Screen.width - buttonWidth - buttonMarginBetween, paletteRect.y + buttonHeight, buttonWidth, buttonHeight),
			                new GUIContent (ColorPaletteStatics.removePaletteIcon, "Remove " + data.name), EditorStyles.miniButton)) { 
								paletteToRemove = paletteCount;
						}

						return paletteToRemove;
				}

				private void drawCreateButton ()
				{
						GUILayout.Space (10);

						EditorGUILayout.BeginHorizontal ();			
						EditorGUILayout.LabelField ("Drag and Drop a ColorPaletteCollection or create a new Collection");			
						EditorGUILayout.EndHorizontal ();

						GUILayout.Space (10);			
	
						Rect newCollectionRect = EditorGUILayout.BeginHorizontal ();
			
						newPaletteName = EditorGUILayout.TextField ("Enter Filename for Collection: ", newPaletteName, GUILayout.Width (Screen.width * 0.85f));
			
						if (GUI.Button (new Rect (Screen.width - buttonWidth - buttonMarginBetween, newCollectionRect.y - buttonHeight / 2, buttonWidth, buttonHeight),
			                new GUIContent (ColorPaletteStatics.paletteCollectionIcon, "Create new PaletteCollection"), EditorStyles.miniButton)) { 
								myCollection.CreatePaletteCollection (newPaletteName);
								OnEnable ();
						}
			
						EditorGUILayout.EndHorizontal ();
			
						GUILayout.Space (5);
				}

		}
}