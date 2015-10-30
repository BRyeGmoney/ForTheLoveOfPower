using UnityEngine;
using UnityEditor;
using System.Collections; 
using MrWhaleGames.ColorPalette_examples;
using MrWhaleGames.ColorPalette;

[CustomEditor(typeof(PaletteGradient))]
public class PaletteGradientInspector : Editor
{ 
		public void OnSelectionChange ()
		{
				Repaint ();
		}

		public override void OnInspectorGUI ()
		{    
				// update it in every OnInspectorGUI to see the gradient changes!
				PaletteGradient myPaletteGradient = target as PaletteGradient;

				// uncomment for debugging
				base.DrawDefaultInspector ();			


				if (myPaletteGradient.gradient != null) {
						EditorGUILayout.BeginHorizontal ();
						Color pickedColor = myPaletteGradient.gradient.Evaluate (myPaletteGradient.pickColor);
						EditorGUILayout.ColorField (pickedColor);
			
						EditorGUILayout.EndHorizontal ();

						EditorGUILayout.BeginHorizontal ();

						EditorGUILayout.TextField ("Pick Hex", ColorPaletteStatics.ColorToHex (pickedColor).ToString (),
			                           									GUILayout.Width (Screen.width * 0.5f));

						EditorGUILayout.EndHorizontal ();
				}

				EditorGUILayout.BeginHorizontal ();

				if (GUILayout.Button ("Update Gradient", GUILayout.Width (Screen.width * 0.25f))) { 
						myPaletteGradient.UpdateGradient ();

						if (GUI.changed) {
								//Debug.Log ("gui changed!");

								//SerializedProperty sp = new SerializedObject (myPaletteGradient).FindProperty ("gradient");
								//Debug.Log ("found " + sp.);

								//sp.prefabOverride = false;
								//sp.serializedObject.ApplyModifiedProperties ();

								//PrefabUtility.ResetToPrefabState (myPaletteGradient);
						}

						Repaint ();
						SceneView.RepaintAll ();
						HandleUtility.Repaint (); 
				}

				EditorGUILayout.EndHorizontal ();


		} 
	
	


}
