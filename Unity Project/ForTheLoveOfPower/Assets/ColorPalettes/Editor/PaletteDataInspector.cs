using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections; 

#pragma warning disable 3009

namespace MrWhaleGames.ColorPalette
{

		[CustomEditor(typeof(PaletteData))]
		public class PaletteDataInspector : PaletteInspector
		{ 
				private PaletteData myData;

				new public void OnEnable ()
				{
						myData = target as PaletteData;

						// check so existing PaletteData s will be updated!
						base.checkLockPercentages (myData);
				}


				public override void OnInspectorGUI ()
				{
						base.drawColorPalette (myData, false, false);

						base.DrawDefaultInspector ();
				}

		}
}