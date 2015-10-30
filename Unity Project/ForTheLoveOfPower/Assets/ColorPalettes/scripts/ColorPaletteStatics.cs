using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace MrWhaleGames.ColorPalette
{

		/// <summary>
		/// Color palette statics holds static helper methods.
		/// </summary>
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoad]
		#endif
		public static class ColorPaletteStatics
		{

		#region Loadup_methods

		#if UNITY_EDITOR
				static ColorPaletteStatics ()
				{
						//Debug.Log ("ColorPaletteStatics InitializeOnLoad");
						loadButtonIcons ();
						loadPaletteIcons ();
						loadLockIcons ();
				}
		#endif

				public static readonly string PalettesAssetPath = "ColorPalettes/palettes";
				public static readonly string ColorPaletteFullPath = Application.dataPath + "/ColorPalettes";
				public static readonly string ColorPaletteEditorFullPath = Application.dataPath + "/ColorPalettes/Editor";
				//public static readonly string ColorPaletteEditorResourcesPath = Application.dataPath + "/ColorPalettes/Editor Default Resources";

				public static Texture2D paletteIcon = null;
				public static Texture2D paletteCollectionIcon = null;

				public static Texture2D addColorIcon = null;
				public static Texture2D removeColorIcon = null;

				public static Texture2D addPaletteIcon = null;
				public static Texture2D removePaletteIcon = null;
	
				public static Texture2D colorLocked = null;
				public static Texture2D colorNotLocked = null;

				public static Texture2D moveColor = null;
				public static Texture2D moveColorDrag = null;

				private static void loadLockIcons ()
				{
#if UNITY_EDITOR


						//colorLocked = UnityEditor.EditorGUIUtility.FindTexture ("d_winbtn_mac_close");
						//colorNotLocked = UnityEditor.EditorGUIUtility.FindTexture ("d_winbtn_mac_inact");

						colorLocked = ColorPaletteStatics.GetSolidTexture (5, 5, Color.red);
						//colorLocked = UnityEditor.EditorGUIUtility.FindTexture ("dragdot_active");
						//colorNotLocked = Resources.GetBuiltinResource<Texture2D> ("dragdotDimmed");

						moveColor = UnityEditor.EditorGUIUtility.FindTexture ("d_MoveTool");
						moveColorDrag = UnityEditor.EditorGUIUtility.FindTexture ("d_MoveTool on");

						colorNotLocked = ColorPaletteStatics.GetSolidTexture (5, 5, Color.green);

/*						List<UnityEngine.Object> objs = new List<UnityEngine.Object> (Resources.FindObjectsOfTypeAll (typeof(Texture)));
			
						foreach (Object obj in objs) {
								if (obj.name.Equals ("dragdot_active")) {
										colorLocked = (Texture2D)obj;
										Debug.Log ("found colorLocked " + colorLocked);
								}

								if (obj.name.Equals ("dragdotDimmed")) {
										colorNotLocked = (Texture2D)obj;
										Debug.Log ("found colorNotLocked " + colorNotLocked);

//EditorGUIUtility.FindTexture( "d_MoveTool" );
//EditorGUIUtility.FindTexture( "d_MoveTool on" );
								}
						}*/

#endif
				}

				private static void loadPaletteIcons ()
				{
						string folderPath = "";
						folderPath = ColorPaletteEditorFullPath + "/";
			
						paletteIcon = ColorPaletteStatics.getTextureFromWWW (folderPath + "tiny_palette_icon.png");
						paletteIcon.hideFlags = HideFlags.HideAndDontSave;
						paletteCollectionIcon = ColorPaletteStatics.getTextureFromWWW (folderPath + "paletteCollection_icon.png");
						paletteCollectionIcon.hideFlags = HideFlags.HideAndDontSave;
				}

				private static void loadButtonIcons ()
				{
						string folderPath = "";
						folderPath = ColorPaletteEditorFullPath + "/";
		
						addColorIcon = ColorPaletteStatics.getTextureFromWWW (folderPath + "plus_color.png");
						addColorIcon.hideFlags = HideFlags.HideAndDontSave;
						removeColorIcon = ColorPaletteStatics.getTextureFromWWW (folderPath + "minus_color.png");
						removeColorIcon.hideFlags = HideFlags.HideAndDontSave;

						addPaletteIcon = ColorPaletteStatics.getTextureFromWWW (folderPath + "plus_palette.png");
						addPaletteIcon.hideFlags = HideFlags.HideAndDontSave;
						removePaletteIcon = ColorPaletteStatics.getTextureFromWWW (folderPath + "remove_palette.png");
						removePaletteIcon.hideFlags = HideFlags.HideAndDontSave;
				}
	
/*				protected virtual void loadIcon (Component comp, string iconFileName, string pathToTextures = null)
				{
						string folderPath = "";
						if (string.IsNullOrEmpty (pathToTextures)) {
								folderPath = ColorPaletteEditorFullPath + "/";
						} else {
								folderPath = pathToTextures;
						}
		
						Texture2D iconTex = ColorPaletteStatics.getTextureFromWWW (folderPath + iconFileName);
						iconTex.hideFlags = HideFlags.HideAndDontSave;
		
						System.Type editorGUIUtilityType = typeof(EditorGUIUtility);
						BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
		
						object[] args = new object[] {comp, iconTex};
						editorGUIUtilityType.InvokeMember ("SetIconForObject", bindingFlags, null, null, args);
				}*/

	#endregion

		#region Texture_methods

				public static void solidFillTexture (Texture2D tex, Color col)
				{
						for (int i = 0; i < tex.width; i++) {
								for (int j = 0; j < tex.height; j++) {
										tex.SetPixel (i, j, col);
								}
						}
						tex.Apply ();
				}

				public static Texture2D GetSolidTexture (int width, int height, Color col, int borderWidth = 0)
				{
						return GetBorderTexture (width, height, col, Color.white, borderWidth);
				}

				public static Texture2D GetBorderTexture (int width, int height, Color col, Color borderCol, int borderWidth)
				{
						//Texture2D tex = new Texture2D (width, height);
						Texture2D tex = new Texture2D (width, height, TextureFormat.ARGB32, false);
			
						for (int x = 0; x < width; x++) {
								for (int y = 0; y < height; y++) {
										if (x < borderWidth || y < borderWidth) {
												tex.SetPixel (x, y, borderCol);
										} else if (x > width - borderWidth || y > height - borderWidth) {
												tex.SetPixel (x, y, borderCol);
										} else {
												tex.SetPixel (x, y, col);
										}
								}
						}
						tex.Apply ();
			
						tex.hideFlags = HideFlags.HideAndDontSave;
			
						return tex;
				}
		
		#endregion
		
		#region ColorPalette_Creation
				public static PaletteData CreateColorPalette (string name = null)
				{
						PaletteData data = null;
						string assetPath = "Assets/" + ColorPaletteStatics.PalettesAssetPath + "/" + name + ".asset";

						if (File.Exists (assetPath)) {
								string oldName = name;
								name = GetRandomPaletteName ();
								assetPath = "Assets/" + ColorPaletteStatics.PalettesAssetPath + "/" + name + ".asset";
								Debug.LogWarning ("Create randomised Palette: " + name + " because '" + oldName + "' already exists");
						}

						data = PaletteData.getInstance (name);

#if UNITY_EDITOR
						if (data != null) {
								UnityEditor.AssetDatabase.CreateAsset (data, assetPath);
								UnityEditor.AssetDatabase.SaveAssets ();
						} else {
								Debug.LogError ("Error during creation of Palette with name: '" + name + "', try chosing another Name!");
						}
#endif			
						return data;
				}

				public static string GetRandomPaletteName ()
				{
						return "NewPalette" + UnityEngine.Random.Range (int.MinValue, int.MaxValue);
				}

				public static string GetRandomCollectionName ()
				{
						return "NewPaletteCollection" + UnityEngine.Random.Range (int.MinValue, int.MaxValue);
				}

				public static PaletteCollectionData CreateColorPaletteCollection (string name = null)
				{
						PaletteCollectionData data = null;

						string assetPath = "Assets/" + ColorPaletteStatics.PalettesAssetPath + "/" + name + ".asset";

						if (File.Exists (assetPath)) {
								string oldName = name;
								name = GetRandomCollectionName ();
								assetPath = "Assets/" + ColorPaletteStatics.PalettesAssetPath + "/" + name + ".asset";
								Debug.LogWarning ("Create randomised PaletteCollection: " + name + " because '" + oldName + "' already exists");
						}

						data = PaletteCollectionData.getInstance (name);
								
#if UNITY_EDITOR
						if (data != null) {
								UnityEditor.AssetDatabase.CreateAsset (data, assetPath);
								UnityEditor.AssetDatabase.SaveAssets ();
						} else {
								Debug.LogError ("Error during creation of PaletteCollection with name: '" + name + "', try chosing another Name!");
						}			
#endif			

						return data;
				}

	#endregion

	#region ColorHelper_methods

				public static string ColorToHex (Color32 color)
				{
						string hex = color.r.ToString ("X2") + color.g.ToString ("X2") + color.b.ToString ("X2");
						return hex;
				}
	
				public static Color HexToColor (string hex)
				{
						if (hex.Length < 6) {
								throw new UnityException ("Hexadecimal Color Value is too short!");
						} else {
								byte r = byte.Parse (hex.Substring (0, 2), System.Globalization.NumberStyles.HexNumber);
								byte g = byte.Parse (hex.Substring (2, 2), System.Globalization.NumberStyles.HexNumber);
								byte b = byte.Parse (hex.Substring (4, 2), System.Globalization.NumberStyles.HexNumber);
								return new Color32 (r, g, b, 255);
						}
				}
	
				public static string[] getHexArrayFromColors (Color[] colors)
				{
						string[] hexArray = new string[colors.Length];
						for (int i = 0; i < colors.Length; i++) {
								hexArray [i] = ColorPaletteStatics.ColorToHex (colors [i]);
						}
						return hexArray;
				}
	
				public static Color[] getColorsArrayFromHex (string[] hexArray)
				{
						Color[] colors = new Color[hexArray.Length];
						for (int i = 0; i < hexArray.Length; i++) {
								colors [i] = ColorPaletteStatics.HexToColor (hexArray [i]);
						}
						return colors;
				}


				public static List<string> getHexArrayFromColors (List<Color> colors)
				{
						List<string> hexArray = new List<string> ();
						for (int i = 0; i < colors.Count; i++) {
								hexArray.Add (ColorPaletteStatics.ColorToHex (colors [i]));
						}
						return hexArray;
				}
	
				public static List<Color> getColorsArrayFromHex (List<string> hexArray)
				{
						List<Color> colors = new List<Color> ();
						for (int i = 0; i < hexArray.Count; i++) {
								colors.Add (ColorPaletteStatics.HexToColor (hexArray [i]));
						}
						return colors;
				}

				public static Color GetRandomColor ()
				{
						Color randColor = new Color ();
						randColor.a = 1f;
						randColor.r = Random.Range (0f, 1f);
						randColor.g = Random.Range (0f, 1f);
						randColor.b = Random.Range (0f, 1f);
						return randColor;
				}

	#endregion

				public static Texture2D getTextureFromWWW (string fullPath)
				{
						fullPath = "file:///" + fullPath;
						WWW wwwLoader = new WWW (fullPath);
						//Debug.Log ("loading texture " + fullPath + " via www, loaded: " + www);
						return wwwLoader.texture;
				}
		
				public static Sprite getSpriteFromWWW (string fullPath, bool packTight = false)
				{
						Texture2D tex = getTextureFromWWW (fullPath);
			
						Rect rect = new Rect (0, 0, tex.width, tex.height);
						SpriteMeshType meshType = SpriteMeshType.FullRect;
						if (packTight) {
								meshType = SpriteMeshType.Tight;
						}
			
						// use 100f to scale down
						Sprite spr = Sprite.Create (tex, rect, new Vector2 (0.5f, 0.5f), 100f, 0, meshType);
						spr.name = fullPath.Substring (fullPath.LastIndexOf ("/") + 1);
						return spr;
				}



		}


}