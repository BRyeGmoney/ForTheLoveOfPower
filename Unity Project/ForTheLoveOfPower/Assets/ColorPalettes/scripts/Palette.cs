using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
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
		/// The Palette class is the core of the ColorPalette library. It holds and visualises the <see cref="ColorPalette.PaletteData"/>.
		/// </summary>
		[Serializable]	
		public class Palette : MonoBehaviour
		{
				public PaletteData Data = null;

				/// <summary>
				/// Creates a standard palette.
				/// </summary>
				/// <param name="name">Name.</param>
				public void CreateStandardPalette (string name)
				{
						if (string.IsNullOrEmpty (name)) {
								name = ColorPaletteStatics.GetRandomPaletteName ();
						}

						PaletteData data = ColorPaletteStatics.CreateColorPalette (name);

						if (data != null) {
								Data = data;
						} else {
								Debug.LogError (name + " Palette already exists, choose another Name!");
						}
				}

		}
}