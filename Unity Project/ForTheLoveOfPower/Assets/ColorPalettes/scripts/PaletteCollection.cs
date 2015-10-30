using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HtmlSharp;
using HtmlSharp.Elements;
using System;
using System.IO;

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
		/// The PaletteCollection holds <see cref="ColorPalette.PaletteCollectionData"/> which has multiple Palettes.
		/// </summary>
		[Serializable]	
		public class PaletteCollection : MonoBehaviour
		{
				/// <summary>
				/// The <see cref="ColorPalette.PaletteCollectionData"/> stores multiple <see cref="ColorPalette.PaletteData"/>.
				/// </summary>
				public PaletteCollectionData Data = null;

				[SerializeField]
				[HideInInspector]
				private bool
						isColourLovers = false;
				[SerializeField]
				[HideInInspector]
				private bool
						isPLTTS = false;
				[SerializeField]
				[HideInInspector]
				private bool
						isLocalFile = false;
		
				[SerializeField]
				[HideInInspector]
				public bool
						loadPercent = true;
		
				[SerializeField]
				[HideInInspector]
				public string
						paletteURL = "";


				public void CreatePaletteCollection (string name)
				{
						if (string.IsNullOrEmpty (name)) {
								name = ColorPaletteStatics.GetRandomCollectionName ();
						}

						PaletteCollectionData data = ColorPaletteStatics.CreateColorPaletteCollection (name);

						if (data != null) {
								Data = data;
						} else {
								Debug.LogError (name + " PaletteCollection already exists, choose another Name!");
						}
				}


				/// <summary>
				/// Imports the palette from the given URL.
				/// </summary>
				/// <param name="newURL">New URL</param>
				public void ImportPalette (string newURL)
				{
						reset ();
						StartCoroutine (ImportPaletteFromURL (newURL));
				}

				private void reset ()
				{
						this.isColourLovers = false;
						this.isLocalFile = false;
						this.isPLTTS = false;
				}

				private IEnumerator ImportPaletteFromURL (string newURL)
				{
						analizeURL (newURL);
			
						WWW html = new WWW (newURL);

						while (!html.isDone) {
				
								if (!string.IsNullOrEmpty (html.error)) {
										throw new UnityException ("error loading URL: " + html.error);
								}

								// don't yield anything, so the user input is blocked while downloading!

								//Debug.Log ("downloading Palette " + html.progress + "%");
						}

						Debug.Log ("download finished, loaded " + html.bytesDownloaded + " bytes");

						this.paletteURL = newURL;
						string[] splitted = newURL.Split ('/');
						string paletteName = splitted [splitted.Length - 1];
			
						HtmlParser parser = new HtmlParser ();
						Document doc = parser.Parse (html.text);

						//Debug.Log (doc.ToString ());

						PaletteData extractedData = null;

						if (isColourLovers) {				
								extractedData = PaletteImporter.extractFromColorlovers (paletteName, doc, this.loadPercent);

						} else if (isPLTTS) {
								extractedData = PaletteImporter.extractFromPLTTS (paletteName, doc, this.loadPercent);
						}

						if (extractedData != null) {
								this.Data.AddNewPalette (extractedData);
						} else {
								Debug.LogError ("Error during the Import of URL: " + newURL);
						}

						// add extracted Palette to the collection
/*						this._collectionData.setSize (this._collectionData.palettes.Count + 1,
			                                   new KeyValuePair<string, PaletteData> (extractedData.name, extractedData));
*/
						yield return null;
				}


				private void analizeURL (string URL)
				{
						if (URL.Contains ("colourlovers")) {
								isColourLovers = true;
								Debug.Log ("recognized colourlovers URL: " + URL);
						} else if (URL.Contains ("pltts")) {
								isPLTTS = true;
								Debug.Log ("recognized pllts URL: " + URL);
						} else if (URL.Contains ("file:")) {
								isLocalFile = true;
						} else {
								throw new UnityException ("Unkown URL, so far only colourlovers.com and pltts.me is supported! " + URL);
						}
			
						if (isLocalFile) {
				
								string fileName = Path.GetFileNameWithoutExtension (URL);
								int filenr = 0;
								if (int.TryParse (fileName, out filenr)) {
										isPLTTS = true;
								} else {
										isColourLovers = true;
										Debug.LogWarning ("reading local file: " + fileName + " expecting it to be from Colourlovers.com");
								}
						}
			
						this.paletteURL = URL;
				}



		}


}