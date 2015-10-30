using UnityEngine;
using System;
using System.IO;
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
using System.Collections;


namespace MrWhaleGames.ColorPalette
{

		/// <summary>
		/// Palette data is the model of the ColorPalettes and holds all the needed data. It's a scriptableObject which is saved as an assets.
		/// Register to the palette on the <c>OnChange</c> event to get notified when the data of a ColorPalette has changed.
		/// To create a new Palette use the <see cref="ColorPalette.ColorPaletteStatics"/> <c>CreateColorPalette()</c> method.
		/// </summary>
		[Serializable]	
		public class PaletteData : UnityEngine.ScriptableObject
		{
				public static readonly float minPct = 0.01f;
				public static readonly string namePrefix = "PaletteData";

				/// <summary>
				/// The delegate to register for an OnChange Event.
				/// </summary>
				public delegate void OnChangeHandler (PaletteData colorData);

				/// <summary>
				/// Register here to get notified when the Palette has changed.
				/// </summary>
				public event OnChangeHandler OnChange;


				/// <summary>
				/// Initializes a new instance of the <see using MrWhaleGames.ColorPalette_examples;WhaleGames.ColorPalette.PaletteData"/> class.
				/// If no name provided the namePrefix is used! If the <c>getInstance()</c> method is used, there is no need to call the <c>init()</c> method.
				/// The <c>init()</c> method can be used to reset a palette.
				/// </summary>
				/// <param name="name">Name.</param>
				public void init (string name = null)
				{
						if (string.IsNullOrEmpty (name)) {
								this.name = PaletteData.namePrefix + "_";
						} else {
								this.name = name;
						}
			
						this._colors = getDefaultColors ();
						this._percentages = getDefaultPercentages ();
						this._lockedPercentages = getDefaultLockPercentages ();
			
						raiseOnChange ();
				}


				[SerializeField]
				private List<Color>
						_colors;
				/// <summary>
				/// Get the colors. When changing make sure to do it via the provided methods like <c>ChangeColor()</c> otherwise the OnChange won't be raised!
				/// </summary>
				/// <value>The colors.</value>
				public List<Color> colors {
						get{ return _colors;}
				}

				[SerializeField]
				private List<float>
						_percentages;
				/// <summary>
				/// Get the percentages. When changing make sure to do it via the provided methods like <c>ChangePercentage()</c> otherwise the OnChange won't be raised!
				/// </summary>
				/// <value>The percentages.</value>
				public List<float> percentages {
						get{ return _percentages;}
				}

				[SerializeField]
				private List<bool>
						_lockedPercentages;
				/// <summary>
				/// Get the locked percentages. When changing make sure to do it via the provided methods like <c>ChangePercentage()</c> otherwise the OnChange won't be raised!
				/// </summary>
				/// <value>The locked percentages.</value>
				public List<bool> lockedPercentages {
						get{ return _lockedPercentages;}
				}

				private void raiseOnChange ()
				{
						if (this.OnChange != null) {
								this.OnChange (this);
						}
						//Debug.Log (this.name + " raisedOnChange");
						#if UNITY_EDITOR
						// is need to avoid overwriting the paleteData from their "prefab" or initial values.
						UnityEditor.EditorUtility.SetDirty (this);
						#endif
				}

		#region public_Methods

				/// <summary>
				/// Adds the new random color at the end of the color list.
				/// </summary>
				public void AddNewRandomColor ()
				{
						AddColor (ColorPaletteStatics.GetRandomColor ());
				}

				/// <summary>
				/// Adds the color at the end of the color list.
				/// </summary>
				/// <param name="color">Color.</param>
				/// <param name="percentage">Percentage.</param>
				public void AddColor (Color color, float percentage = 0.05f)
				{
						//Debug.Log ("adding color " + color + " with % " + percentage);
						_colors.Add (color);

						if (percentage == PaletteData.minPct) {
								// add a bigger value to force the adjustement of the other percentage values!
								_percentages.Add (PaletteData.minPct * 2);
								ChangePercentage (_percentages.Count - 1, percentage, true);
						} else {
								_percentages.Add (PaletteData.minPct);
								ChangePercentage (_percentages.Count - 1, percentage, true);
						}
				}

				/// <summary>
				/// Removes the last color in the list.
				/// </summary>
				/// <returns><c>true</c>, if color was removed, <c>false</c> otherwise.</returns>
				public bool RemoveColor ()
				{
						if (_colors.Count > 0) {
								return RemoveColor (_colors.Count - 1);
						}
						return false;
				}
				/// <summary>
				/// Removes the color at index.
				/// </summary>
				/// <returns><c>true</c>, if color was removed, <c>false</c> otherwise.</returns>
				/// <param name="index">Index.</param>
				public bool RemoveColor (int index)
				{
						if (index <= _colors.Count - 1) {
								_colors.RemoveAt (index);
								_percentages.RemoveAt (index);
								FillUpLastPercentage ();
								return true;
						}
						return false;
				}

				/// <summary>
				/// Changes the color at index.
				/// </summary>
				/// <param name="whichOne">Which one.</param>
				/// <param name="newColor">New color.</param>
				public void ChangeColor (int whichOne, Color newColor)
				{
						List<Color> cols = _colors;
						cols [whichOne] = newColor;
						_colors = cols;

						raiseOnChange ();
				}

				/// <summary>
				/// Changes the position of a color and its percentage & lockPercentage
				/// </summary>
				/// <param name="whichOne">Which one.</param>
				/// <param name="newPosition">New position.</param>
				public void ChangeColorPosition (int whichOne, int newPosition)
				{
						if (newPosition >= 0 && whichOne != newPosition) {

								if (newPosition > this._colors.Count) {
										newPosition = this._colors.Count;
								}

								Color colTemp = this._colors [newPosition];
								float pctTemp = this._percentages [newPosition];
								bool lockTemp = this._lockedPercentages [newPosition];

								this._colors [newPosition] = this._colors [whichOne];
								this._percentages [newPosition] = this._percentages [whichOne];
								this._lockedPercentages [newPosition] = this._lockedPercentages [whichOne];

								this._colors [whichOne] = colTemp;
								this._percentages [whichOne] = pctTemp;
								this._lockedPercentages [whichOne] = lockTemp;

								raiseOnChange ();
						}
				}
		
		
				/// <summary>
				/// Gets the total of all percentages.
				/// </summary>
				/// <returns>The total pct.</returns>
				public float getTotalPct ()
				{
						float total = 0;
						foreach (float pct in this._percentages) {
								total += pct;
						}
						return total;
				}


				/// <summary>
				/// Adjusts one percentage value with taking into account,
				/// that the max is 1f (100%) so the percentages next to it have to be adjusted too.
				/// </summary>
				/// <param name="i">The index.</param>
				/// <param name="newPct">New pct.</param>
				/// <param name="adjustPCTBefore">If set to <c>true</c> adjust PCT before.</param>
				public void ChangePercentage (int whichOne, float newPct, bool adjustPCTBefore)
				{
						int lockAmount = GetLockedPctAmount ();

						// only change if it isn't locked and at least one other percentage is unlocked!
						if (!this._lockedPercentages [whichOne] && lockAmount < this._percentages.Count - 1) {

								float maxPct = 1.0f - (this._percentages.Count - 1) * PaletteData.minPct;
								//Debug.Log ("max % " + maxPct);
								float currentPct = this._percentages [whichOne];

								if (newPct < PaletteData.minPct) {
										newPct = PaletteData.minPct;
								}

								if (newPct > maxPct) {
										newPct = maxPct;
								}

								if (newPct != currentPct) {

										this._percentages [whichOne] = newPct;
										adjustNeighborPCT (whichOne, currentPct - newPct, adjustPCTBefore);

										// changes been made to neighbors, check if totalPcts is still 1 -> 100%
										float totalPcts = getTotalPct ();
				
										if (totalPcts >= 1f) {
												float rounding = totalPcts - 1f;
												if (rounding > 0) {
														//Debug.Log ("rounding " + newPct + " minus " + rounding + " to " + (newPct - rounding));
														// always cut the last for the rounding!
														this._percentages [this._percentages.Count - 1] -= rounding;
												} else if (rounding < 0) {
														this._percentages [this._percentages.Count - 1] += rounding;
												}
										}

										raiseOnChange ();
								}
						}
			
				}

				/// <summary>
				/// Gets the amount of how many percentages are locked.
				/// </summary>
				/// <returns>The amount of locked percentages.</returns>
				public int GetLockedPctAmount ()
				{
						int amount = 0;
						foreach (bool locked in this._lockedPercentages) {
								if (locked) {
										amount++;
								}
						}
						return amount;
				}

				public void ChangeLockPercentage (int whichOne, bool locked)
				{
						if (whichOne >= this._lockedPercentages.Count) {
								this._lockedPercentages.Add (locked);
						} else {
								this._lockedPercentages [whichOne] = locked;
						}

						raiseOnChange ();
				}

				/// <summary>
				/// Sets the percentages, a change in the amount of percentage values is not possible!
				/// </summary>
				/// <param name="newPercentages">New percentages.</param>
				public void SetPercentages (List<float> newPercentages)
				{
						if (newPercentages.Count == this._percentages.Count) {
								this._percentages = newPercentages;
						} else {
								Debug.LogError ("SetPercentages can't change the size of the percentages! Existing size " + this._percentages.Count + " new size " + newPercentages.Count);
						}
				}

				/// <summary>
				/// Fills up last percentage, only call when the amout of Colors has changed!
				/// </summary>
				public void FillUpLastPercentage ()
				{
						float currentTotal = getTotalPct ();
						if (currentTotal < 1) {
								this._percentages [this._percentages.Count - 1] += (1 - currentTotal);
						}

						raiseOnChange ();
				}

				/// <summary>
				/// Clears the palette, it's empty afterwards
				/// </summary>
				public void ClearPalette ()
				{
						this._colors.Clear ();
						this._percentages.Clear ();

						raiseOnChange ();
				}


		
		#endregion


				/// Adjusts the neighbor Percentage, returns false if the neighbor value woulde be under this.minPct!.
				/// </summary>
				/// <returns><c>true</c>, if neighbor PC was adjusted, <c>false</c> otherwise.</returns>
				/// <param name="i">The index.</param>
				/// <param name="pctDiff">Pct diff.</param>
				protected void adjustNeighborPCT (int whiceOne, float pctDiff, bool adjustPCTBefore)
				{
						int neighbourIndex = whiceOne;
			
						if (adjustPCTBefore) {
								if (whiceOne - 1 >= 0) {
										neighbourIndex = whiceOne - 1;
								} else {
										neighbourIndex = this._percentages.Count - 1;
								}
						} else {
								if (whiceOne + 1 <= this._percentages.Count - 1) {
										neighbourIndex = whiceOne + 1;
								} else {
										neighbourIndex = 0;
								}
						}

						if (neighbourIndex != whiceOne) {

								if (this._lockedPercentages [neighbourIndex]) {

										// it's locked can't change, try with the next one!
										adjustNeighborPCT (neighbourIndex, pctDiff, adjustPCTBefore);
								} else {

										// can change neighbor
										this._percentages [neighbourIndex] += pctDiff;
										float newNeighbourValue = this._percentages [neighbourIndex];

										if (newNeighbourValue < PaletteData.minPct) {
												this._percentages [neighbourIndex] = PaletteData.minPct;
												adjustNeighborPCT (neighbourIndex, newNeighbourValue - PaletteData.minPct, adjustPCTBefore);
										}
								}
					
						}

				} 




		#region static_Methods

				/// <summary>
				/// Get an instance. Use this method instead of the usuall constructor because it derives from <c>scritableObject</c>
				/// </summary>
				/// <returns>The instance.</returns>
				/// <param name="jClass">J class.</param>
				public static PaletteData getInstance (string name = null)
				{
						PaletteData data = ScriptableObject.CreateInstance<PaletteData> ();
						data.init (name);
						return data;
				}

				/// <summary>
				/// Gets the default colors.
				/// </summary>
				/// <returns>The default colors.</returns>
				public static List<Color> getDefaultColors ()
				{
						List<string> strList = new List<string> (){"69D2E7", "A7DBD8", "E0E4CC", "F38630", "FA6900"};
						return ColorPaletteStatics.getColorsArrayFromHex (strList);
				}
			
				/// <summary>
				/// Gets the default percentages. Even spread for five colors (all with a value of 0.2f);
				/// </summary>
				/// <returns>The default percentages.</returns>
				public static List<float> getDefaultPercentages ()
				{
						return new List<float> (){0.2f, 0.2f, 0.2f, 0.2f, 0.2f};
				}

				/// <summary>
				/// Gets the default lock percentages.
				/// </summary>
				/// <returns>The default lock percentages.</returns>
				public static List<bool> getDefaultLockPercentages ()
				{
						return new List<bool> (){false, false, false, false, false};
				}

		#endregion

/*				public override string ToString ()
				{
						JSONClass jClass = getJsonPalette ();
						return "[" + this.GetType () + "] name: " + this.name + " colors: " + jClass ["colors"].ToString ()
								+ " percentages: " + jClass ["percentages"].ToString ();
				}*/

		}
}

