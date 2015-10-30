using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using MrWhaleGames.ColorPalette;

namespace MrWhaleGames.ColorPalette_examples
{
		/// <summary>
		/// The Vertex switcher is an example to change the vertex colors of an object.
		/// It goes through the mesh and changes the color of each vertex.
		/// Adjust the <c>switchPauseTime</c> to change the speed.
		/// </summary>
		[RequireComponent(typeof(MeshFilter))]
		[RequireComponent(typeof(MeshRenderer))]
		public class VertexSwitcher : MonoBehaviour
		{
				[Range(0.00001f, 1)]
				public float
						switchPauseTime = 0.01f;
	
				public bool reverseSequence = false;

				private int colorCount = 0;
				private Mesh mesh;

				void Start ()
				{
						mesh = this.GetComponent<MeshFilter> ().mesh;

						Palette palette = this.GetComponent<Palette> ();

						if (palette != null) {
								StartCoroutine (changeVertexColor (palette.Data));
						} else {
								PaletteImporter paletteImporter = this.GetComponent<PaletteImporter> ();
								StartCoroutine (changeVertexColor (paletteImporter.Data));
						}
				}
	
				private IEnumerator changeVertexColor (PaletteData colorData)
				{
						while (true) {

								if (colorCount > colorData.colors.Count - 1) {
										colorCount = 0;
								}

								yield return StartCoroutine (switchToNextColor (colorData));


								colorCount++;
						}

				}

	

				private IEnumerator switchToNextColor (PaletteData colorData)
				{
						Color[] vertexColors = mesh.colors;

						if (reverseSequence) {

								for (int i = vertexColors.Length - 1; i > -1; i--) {
										vertexColors [i] = colorData.colors [colorCount];
										mesh.colors = vertexColors;
										yield return new WaitForSeconds (switchPauseTime);
								}
						} else {

								for (int i = 0; i < vertexColors.Length; i++) {
										vertexColors [i] = colorData.colors [colorCount];
										mesh.colors = vertexColors;
										yield return new WaitForSeconds (switchPauseTime);
								}
						}

				}



		}
	
}