using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using HtmlSharp;
using HtmlSharp.Elements;
using HtmlSharp.Extensions;

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
		/// The PaletteImporter is an extension of the normal Palette.
		/// It has the additional functionality to import a palette from colourlovers.com or pltts.me.
		/// </summary>
		[Serializable]	
		public class PaletteImporter : MonoBehaviour
		{

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

				/// <summary>
				/// The PaletteImporterData stores all the actuall data of the <see cref="ColorPalette.PaletteImporterData"/>.
				/// </summary>
				public PaletteData Data = null;

				public void CreateStandardPalette (string name)
				{
						Data = ColorPaletteStatics.CreateColorPalette (name);
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

/*						this.myImporterData.colors = new Color[5];
						this.myImporterData.percentages = new float[5];
*/
						PaletteData extractedData = null;

						if (isColourLovers) {
								extractedData = extractFromColorlovers (paletteName, doc, this.loadPercent);

						} else if (isPLTTS) {
								extractedData = extractFromPLTTS (paletteName, doc, this.loadPercent);
						}

						if (extractedData != null) {
								this.Data = extractedData;
						} else {
								Debug.LogError ("Error during the Import of URL: " + newURL);
						}

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
								throw new UnityException ("Unkown URL '" + URL + "' , so far only colourlovers.com and pltts.me is supported! ");
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

						paletteURL = URL;
				}

				/// <summary>
				/// Extracts a <see cref="ColorPalette.PaletteData"/> from colorlovers, the percentages only the pixel-width, so it as to be divied with the totalwidth
				/// </summary>
				/// <returns>The from colorlovers.</returns>
				/// <param name="doc">Document.</param>
				/// <param name="loadPercentages">If set to <c>true</c> load percentages.</param>
				public static PaletteData extractFromColorlovers (string name, Document doc, bool loadPercentages)
				{
						float totalWidth = 0;
						List<Color> colors = new List<Color> ();
						List<float> percentages = new List<float> ();

/*
 						IEnumerable<Tag> headerTags = doc.FindAll ("h1");
						foreach (Tag headerTag in headerTags) {
//								if (!string.IsNullOrEmpty (headerTag.c)) {


								//palette.name = headerTag.ToString ();
								Debug.Log (headerTag.ToString ().HtmlDecode ());
								//["class"] == "feature-detail-container") {

//								}
						}
			 */


						IEnumerable<Tag> links = doc.FindAll ("a");


						foreach (Tag a in links) {
								if (a ["class"] == "left pointer block") {

										string style = a ["style"];
										//Debug.Log ("style.Split (';') " + style.Split (';').Length);

										foreach (string styleCss in style.Split (';')) {
												if (loadPercentages && styleCss.Contains ("width")) {

														string width = styleCss.Split (':') [1];
														width = width.Substring (0, width.IndexOf ("px"));
														float widthF = float.Parse (width.Trim ());

														//Debug.Log ("found % " + widthF + " from " + styleCss);

														totalWidth += widthF;
														percentages.Add (widthF);

												} else if (styleCss.Contains ("background-color")) {

														string bgColor = styleCss.Split (':') [1];
														bgColor = bgColor.Trim ().Substring (1);
														colors.Add (ColorPaletteStatics.HexToColor (bgColor));
												}
										}

										//Debug.Log (style);
								}
						}
						//Debug.Log ("totalwidth " + totalWidth);

						if (loadPercentages) {
								for (int i = 0; i < percentages.Count; i++) {
										// totalWidth = 100% this.myData.percentages [i] = x%
										percentages [i] = percentages [i] / totalWidth;
										//Debug.Log (i + " % " + percentages [i]);
								}
						}
						
						PaletteData palette = ColorPaletteStatics.CreateColorPalette (name);
						palette.ClearPalette ();

						// add Colors and spread even
						for (int i = 0; i < colors.Count; i++) {
								palette.AddColor (colors [i], 1 / colors.Count);
						}
						if (percentages.Count > 0) {
								palette.SetPercentages (percentages);
						}

						return palette;
				}
	

				/// <summary>
				/// Extracts a <see cref="ColorPalette.PaletteData"/> from pltts, the percentages only the pixel-width, so it as to be divied with the totalwidth
				/// </summary>
				/// <returns>The from colorlovers.</returns>
				/// <param name="doc">Document.</param>
				/// <param name="loadPercentages">If set to <c>true</c> load percentages.</param>
				public static PaletteData extractFromPLTTS (string name, Document doc, bool loadPercentages)
				{
						List<Color> colors = new List<Color> ();
						List<float> percentages = new List<float> ();

						Tag colorBlock = doc.Find (".palette-colors");
						//Debug.Log (colorBlock);

						foreach (Element colorDiv in colorBlock.Children) {

								if (!string.IsNullOrEmpty (colorDiv.ToString ().Trim ())) {
										// can contain empty Elements!

										//Tag colorTag = (HtmlSharp.Elements.Tags.Div)colorDiv;
										Tag colorTag = (Tag)colorDiv;

										string style = colorTag ["style"];
										foreach (string styleCss in style.Split (';')) {
												if (loadPercentages && styleCss.Contains ("width")) {
						
														string width = styleCss.Split (':') [1];
														width = width.Substring (0, width.IndexOf ("%"));
														float widthF = float.Parse (width.Trim ());

														percentages.Add (widthF / 100);
						
												} else if (styleCss.Contains ("background-color")) {
						
														string bgColor = styleCss.Split (':') [1];
														bgColor = bgColor.Trim ().Substring (1);
														colors.Add (ColorPaletteStatics.HexToColor (bgColor));
												}
										}
				
										//Debug.Log (style);
								}
						}

						PaletteData palette = ColorPaletteStatics.CreateColorPalette (name);
						palette.ClearPalette ();

						// add Colors and spread even
						for (int i = 0; i < colors.Count; i++) {
								palette.AddColor (colors [i], 1 / colors.Count);
						}
						if (percentages.Count > 0) {
								palette.SetPercentages (percentages);
						}

						return palette;
				}


		}
}