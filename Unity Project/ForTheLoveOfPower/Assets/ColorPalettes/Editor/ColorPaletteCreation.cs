using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace MrWhaleGames.ColorPalette
{

		public static class ColorPaletteCreation
		{

				[MenuItem("Assets/Create/Color Palette Data")]
				public static PaletteData CreateBasicPaletteWindow ()
				{
						PaletteData data = ColorPaletteStatics.CreateColorPalette (ColorPaletteStatics.GetRandomPaletteName ());

						AssetDatabase.Refresh ();
						EditorUtility.FocusProjectWindow ();
						Selection.activeObject = data;

						return data;
				}

		
				[MenuItem("Assets/Create/Color Palette Collection Data")]
				public static void CreateCollectionDataScritableObj ()
				{		
						PaletteCollectionData data = ColorPaletteStatics.CreateColorPaletteCollection (ColorPaletteStatics.GetRandomCollectionName ());

						AssetDatabase.Refresh ();
						EditorUtility.FocusProjectWindow ();
						Selection.activeObject = data;
				}

/*				[MenuItem("ColorPalettes/Create/Reload ColorPalettes from JSON ")]
				public static void ConvertJSONToScritable ()
				{

						string[] scriptableFiles = Directory.GetFiles (Application.dataPath + "/" + ColorPaletteStatics.palettesFolder);
						Debug.Log ("found " + scriptableFiles.Length + " jsonfiles");
			
						int files = 0;
						int matches = 0;
						int overwritten = 0;
						int reloaded = 0;

						foreach (string scriptable in scriptableFiles) {

								if (!Path.GetExtension (scriptable).Equals (".meta")) {
										files++;
										string paletteName = Path.GetFileNameWithoutExtension (scriptable);

										string matchedFile = FindJSONFile (paletteName);

										if (!string.IsNullOrEmpty (matchedFile)) {
												matches++;
										} else {
												Debug.Log ("no match for " + paletteName);
										}

										//Debug.Log ("machted name: " + paletteName + " with file: " + matchedFile);

										if (!string.IsNullOrEmpty (matchedFile)) {
												//Debug.Log ("found scritable " + paletteName);
												//PaletteData data = PaletteData.getInstance (paletteName);
												Object data = AssetDatabase.LoadAssetAtPath ("Assets/" + ColorPaletteStatics.palettesFolder + "/" + Path.GetFileName (scriptable), typeof(Object)) as Object;
												PaletteData pData = (PaletteData)data;
												//Debug.Log ("loaded Asset from path " + "Assets/" + ColorPaletteStatics.palettesFolder + "/" + Path.GetFileName (scriptable) + " to " + data);

												if (!ReferenceEquals (pData, null)) {
														pData.LoadFromJson (matchedFile);
														reloaded++;
												}
										}
								}
						}

						Debug.Log ("found " + files + " ColorPalettes with " + matches + " matches and reloaded " + reloaded);

				}


				private static string FindJSONFile (string paletteName)
				{
						string[] jsonFiles = Directory.GetFiles (Application.dataPath + JSONPersistor.FilePathInUnity + "/ColorPalettes/");
						//Debug.Log ("found " + jsonFiles.Length + " jsonfiles");
			
						int i = 0;
						foreach (string jsonFile in jsonFiles) {
				
								if (!Path.GetExtension (jsonFile).Equals (".meta")) {
					
										JSONNode node = JSONNode.LoadFromFile (jsonFile);
										JSONClass jClass = node.AsObject;

										if (jClass.Count > 0) {
			
												//Debug.Log ("check " + jsonFile + " for name " + jClass ["name"] + " equals " + paletteName);
												if (jClass ["name"].Value.Equals (paletteName.Trim ())) {
														Debug.Log ("found a match for name " + jClass ["name"] + " equals " + paletteName + " in file " + jsonFile);
														return jsonFile;
												}
										}
								}
						}

						return null;
				}*/

/*				[MenuItem("ColorPalettes/Create/Convert ColorPalettes")]
				public static void ConvertJSONToScritable ()
				{
						string[] jsonFiles = Directory.GetFiles (Application.dataPath + JSONPersistor.FilePathInUnity + "/ColorPalettes/");
						Debug.Log ("found " + jsonFiles.Length + " jsonfiles");

						int i = 0;
						foreach (string jsonFile in jsonFiles) {

								if (!Path.GetExtension (jsonFile).Equals (".meta")) {

										JSONNode node = JSONNode.LoadFromFile (jsonFile);
										JSONClass jClass = node.AsObject;

										PaletteData old = PaletteData.getInstance (jClass ["name"], jClass);

										if (old.colors.Length > 0) {

												PaletteData data = ColorPaletteStatics.CreateColorPalette (old.name);
												data.ClearPalette ();

												foreach (Color col in old.colors) {
														data.colors.Add (col);
												}
												foreach (float alp in old.alphas) {
														data.alphas.Add (alp);
												}
												foreach (float pct in old.percentages) {
														data.percentages.Add (pct);
												}

												//Debug.Log ("convert " + old);

												i++;
										}
								}
						}

						Debug.Log ("convert " + i + " ColorPalettes");
				}*/




		}
}
