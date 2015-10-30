using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Color palettes gives you the possibility to setup various color schemes for your games. It can be used for SpriteRenderer and Material tinting, color transitions, customisation of characters, vertex coloring or simply applying color to any component which has a color.
/// 
/// <list type="bullet">
/// <item>
/// <term>Author</term>
/// <description>Dominik Haas</description>
/// </item>
/// <item>
/// <term>Email</term>
/// <description>dominik.haas@gmx.ch</description>
/// </item>
/// <item>
/// <term>GameDesign Portfolio</term>
/// <description>http://www.dominikhaas.ch</description>
/// </item>
/// <item>
/// <term>Blog</term>
/// <description>http://domdomhaas.github.io/</description>
/// </item>
/// </list>
/// 
/// </summary>
namespace MrWhaleGames.ColorPalette
{
		/// <summary>
		/// The Palette collection data holds multiple <see cref="ColorPalette.PaletteData"/> in a List.
		/// </summary>
		[Serializable]	
		public class PaletteCollectionData : UnityEngine.ScriptableObject
		{
				public static readonly string namePrefix = "PaletteCollectionData_";

				/// <summary>
				/// The delegate to register for an OnChange Event
				/// </summary>
				public delegate void OnChangeHandler (PaletteCollectionData collectionData);
		
				/// <summary>
				/// Register here to get notified when the Palette has changed.
				/// </summary>
				public event OnChangeHandler OnChange;

				[SerializeField]
				private List<PaletteData>
						_palettes;
				/// <summary>
				/// The palettes of the Collection. Use the <c>AddNewPalette</c> or <c>RemovePalette</c> methods otherwise the <c>OnChange</c> Events won't be fired!
				/// </summary>
				public List<PaletteData> palettes {
						get{ return _palettes;}
				}
		
		#region public_Methods

				/// <summary>
				/// Initializes a new instance of the <see cref="ColorPalette.PaletteData"/> class.
				/// If no name provided the namePrefix is used!
				/// </summary>
				/// <param name="name">Name.</param>
				public void init (string name = null)
				{
						if (string.IsNullOrEmpty (name)) {
								this.name = PaletteCollectionData.namePrefix + "_";
						} else {
								this.name = name;
						}

						this._palettes = new List<PaletteData> ();

						raiseOnChange ();
				}


				public void ClearCollection ()
				{
						this._palettes.Clear ();
						raiseOnChange ();
				}

				/// <summary>
				/// Gets the palette by name.
				/// </summary>
				/// <returns>The palette.</returns>
				/// <param name="name">Name.</param>
				public PaletteData GetPalette (string name)
				{
						foreach (PaletteData palette in palettes) {
								if (palette.name.Equals (name)) {
										return palette;
								}
						}

						return null;
				}

				public PaletteData GetRandomPalette ()
				{
						if (palettes.Count > 0) {
								int randomIndex = UnityEngine.Random.Range (0, palettes.Count);

								return palettes [randomIndex];
						} else {
								return null;
						}
				}

				public void AddStandardPalette (string name)
				{
						AddNewPalette (ColorPaletteStatics.CreateColorPalette (name));
				}
		
				public void AddNewPalette (PaletteData palette)
				{
						_palettes.Add (palette);
						raiseOnChange ();
				}

				public void RemovePalette (int whichOne)
				{
						_palettes.RemoveAt (whichOne);
						raiseOnChange ();
				}

				public override string ToString ()
				{
						string str = "[" + this.GetType () + "] ";
						foreach (PaletteData data in this._palettes) {
								str += data.ToString () + " ";
						}

						return str;
				}

		#endregion

				private void raiseOnChange ()
				{
						if (this.OnChange != null) {
								this.OnChange (this);
						}
						//Debug.Log (this.name + " Collection raise on change");

						#if UNITY_EDITOR
						// is need to avoid overwriting the paleteData from their "prefab" or initial values.
						UnityEditor.EditorUtility.SetDirty (this);
						#endif
				}

		#region static_Methods

				/// <summary>
				/// Get an instance when having a JSONClass. Use this method instead of the usuall constructor "new PaletteData()"
				/// </summary>
				/// <returns>The instance.</returns>
				/// <param name="jClass">J class.</param>
				public static PaletteCollectionData getInstance (string name = null)
				{
						PaletteCollectionData data = ScriptableObject.CreateInstance<PaletteCollectionData> ();
						data.init (name);
						return data;
				}

		#endregion

/*				private bool CreatePalette (KeyValuePair<string, PaletteData> kvp = new KeyValuePair<string, PaletteData> ())
				{
						if (string.IsNullOrEmpty (kvp.Key)) {
								try {
										palettes.Add ("newPalette", PaletteData.getInstance ("newPalette"));
								} catch (System.ArgumentException e) {
										Debug.LogWarning (e);
										Debug.LogWarning (" make sure you change the 'newPalette' name first before adding a new palette!");
										return false;
								}
				
								return true;
				
						} else {
								if (palettes.Contains (kvp.Key)) {
										Debug.LogWarning ("Palette '" + kvp.Key + "' already exists! Change that name before adding a new palette!");
										return false;
								} else {
										palettes.Add (kvp);
										return true;
								}
						}
				}*/

		}
}

