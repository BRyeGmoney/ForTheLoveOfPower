using UnityEngine;
using UnityEditor;
using System.Collections; 

namespace MrWhaleGames.ColorPalette
{
		[CustomEditor(typeof(PaletteImporter))]
		public class PaletteImporterInspector : PaletteInspector
		{ 

				private bool showImporter = false;
				private PaletteImporter myPaletteImporter;


				new public void OnEnable ()
				{
						myPaletteImporter = target as PaletteImporter;

						base.loadIcon (myPaletteImporter, ColorPaletteStatics.paletteIcon);
				}


				public override void OnInspectorGUI ()
				{    
						base.DrawDefaultInspector ();
			
						// margin box before buttons
						GUILayout.Space (10);
			
						if (myPaletteImporter.Data == null) {

								showImporter = EditorGUILayout.Foldout (showImporter, " Import Palette");
				
								if (showImporter) {
										drawURLImporter ();
								}

								drawCreateButton ();

						} else {

								showImporter = EditorGUILayout.Foldout (showImporter, " Import Palette");
		
								if (showImporter) {
										drawURLImporter ();
								}

								showPalette = EditorGUILayout.Foldout (showPalette, myPaletteImporter.Data.name);
		
								myPaletteImporter.Data = drawColorPalette (myPaletteImporter.Data, !showPalette);
		
								changeColors = EditorGUILayout.Foldout (changeColors, " Change Colors");
		
								if (changeColors) {
										myPaletteImporter.Data = drawColorsAndPercentages (myPaletteImporter.Data);
								}
		
								// margin box
								GUILayout.Space (20);

								drawSizeButtons (myPaletteImporter.Data);

								// margin box
								GUILayout.Space (15);

								showColorOnMouse (this.mouseXOffset, this.mouseYOffset, dragColorTex);
						}

				} 

				protected void drawURLImporter ()
				{
						// margin box
						GUILayoutUtility.GetRect (Screen.width, 10);

						GUILayout.Label ("Insert an URL:   (the import takes a few seconds!)");
						GUILayout.Space (5);

						myPaletteImporter.paletteURL = EditorGUILayout.TextField (myPaletteImporter.paletteURL);

						GUILayout.Space (10);

						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.BeginVertical ();

						myPaletteImporter.loadPercent = GUILayout.Toggle (myPaletteImporter.loadPercent, " include Palette Percentage");
		
						EditorGUILayout.EndVertical ();

						bool import = GUILayout.Button (new GUIContent ("Import from URL", "this might take a few seconds!"),
		                                		GUILayout.Width (Screen.width / 2));

						EditorGUILayout.EndHorizontal ();
		
						if (import && !string.IsNullOrEmpty (myPaletteImporter.paletteURL)) {
								Debug.Log ("import started with " + myPaletteImporter.paletteURL);
								myPaletteImporter.ImportPalette (myPaletteImporter.paletteURL);			
						}

						GUILayout.Space (10);
				}

				private void drawCreateButton ()
				{
						GUILayout.Space (10);			
						EditorGUILayout.BeginHorizontal ();
			
						EditorGUILayout.LabelField ("Drag and Drop a ColorPalette or create a new ColorPalette");
			
						EditorGUILayout.EndHorizontal ();

						GUILayout.Space (10);			

						EditorGUILayout.BeginHorizontal ();
			
						newPaletteName = EditorGUILayout.TextField ("Enter Filename for Palette: ", newPaletteName);
			
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
			
						if (GUILayout.Button ("Create PaletteData", GUILayout.Width (Screen.width / 2))) { 
								myPaletteImporter.CreateStandardPalette (newPaletteName);
						}
			
						EditorGUILayout.EndHorizontal ();
				}


		}
}