using UnityEngine;
using System.Collections;
using MrWhaleGames.ColorPalette;


namespace MrWhaleGames.ColorPalette_examples
{

		public class PaletteGradient : MonoBehaviour
		{
				public Gradient gradient;

				[Range(0.0f, 1)]
				public float
						pickColor = 0;


				void Start ()
				{
						UpdateGradient ();
				}

				public void UpdateGradient ()
				{
						Palette palette = this.GetComponent<Palette> ();
						PaletteData data = null;
			
						if (palette != null) {
								data = palette.Data;
						} else {
								PaletteImporter paletteImporter = this.GetComponent<PaletteImporter> ();
								data = paletteImporter.Data;
						}

						if (data != null) {
								updateGradientKeys (data);
						}

				}

				private void updateGradientKeys (PaletteData data)
				{
						GradientColorKey[] colors = new GradientColorKey[data.colors.Count];
						GradientAlphaKey[] alphas = new GradientAlphaKey[data.colors.Count];

						float lastPCT = 0.0f;
						for (int i=0; i < data.colors.Count; i++) {
								lastPCT += data.percentages [i];
								colors [i].color = data.colors [i];
								colors [i].time = lastPCT;
						}

						lastPCT = 0.0f;
						for (int i=0; i < data.colors.Count; i++) {
								lastPCT += data.percentages [i];
								alphas [i].alpha = data.colors [i].a;
								alphas [i].time = lastPCT;
						}

						gradient.SetKeys (colors, alphas);

						//Debug.Log (gradient.Evaluate (0.25f));
				}

		}

}