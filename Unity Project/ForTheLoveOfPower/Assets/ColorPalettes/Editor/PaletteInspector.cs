using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#pragma warning disable 3009

namespace MrWhaleGames.ColorPalette
{

		[CustomEditor(typeof(Palette))]
		public class PaletteInspector : Editor
		{ 
				private static readonly string dragComponentKey = "ColorPalette_ComponentDrag";
				private static readonly string dragAColorKey = "ColorPalette_ColorDrag";

				protected float height = 100;

				protected float mouseXOffset = 20;
				protected float mouseYOffset = 20;

				protected bool showPalette = true;
				protected bool changeColors = true;

				protected bool adjustPCTBefore = false;

				protected float paletteHeight = 100;
				protected float paletteTopMargin = 40;
				protected float paletteBotMargin = 20;
				protected float paletteSideMargin = 10;

				protected float hexFieldWidth = 55;

				protected float colorChangerRowHeight = 20;
				protected float colorChangeLeftMargin = 5;
				protected float colorChangeRightMargin = 20;
				protected float colorChangeMarginBetween = 10;
				protected float colorPickerWidth = 125;
				protected float pctSliderWidth = 150;

				public string newPaletteName = "yourPaletteName";

				protected float buttonWidth = 32;
				protected float buttonHeight = 32;
				protected float buttonMarginBetween = 10;

				protected float lockButtonWidth = 16;
				protected float lockButtonMargin = 5;

				protected Palette myPaletteObj;
				protected Texture2D dragColorTex = null; //= new Texture2D (24, 24);



				public void OnEnable ()
				{
						myPaletteObj = target as Palette;

						if (myPaletteObj.Data != null) {
								checkLockPercentages (myPaletteObj.Data);
						}

						loadIcon (myPaletteObj, ColorPaletteStatics.paletteIcon);
				}

				protected void checkLockPercentages (PaletteData myData)
				{
						if (myData.lockedPercentages.Count != myData.colors.Count) {
								for (int i = 0; i < myData.colors.Count; i++) {
										myData.ChangeLockPercentage (i, false);
								}
						}			
				}


				protected virtual void loadIcon (Component comp, Texture2D iconTex)
				{
						System.Type editorGUIUtilityType = typeof(EditorGUIUtility);
						BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
			
						object[] args = new object[] {comp, iconTex};
						editorGUIUtilityType.InvokeMember ("SetIconForObject", bindingFlags, null, null, args);
				}

				public void OnDisable ()
				{
						DestroyImmediate (dragColorTex);
				}
	
				public override void OnInspectorGUI ()
				{    
						base.DrawDefaultInspector ();

						// margin box before buttons
						GUILayout.Space (10);

						if (myPaletteObj.Data == null) {
								drawCreateButton ();
						} else {

								showPalette = EditorGUILayout.Foldout (showPalette, myPaletteObj.Data.name);

								myPaletteObj.Data = drawColorPalette (myPaletteObj.Data, !showPalette, true, paletteHeight);

								changeColors = EditorGUILayout.Foldout (changeColors, " Change Colors");

								if (changeColors) {
										myPaletteObj.Data = drawColorsAndPercentages (myPaletteObj.Data);
								}

								// margin box
								GUILayout.Space (20);

								myPaletteObj.Data = drawSizeButtons (myPaletteObj.Data);

								// margin box
								GUILayout.Space (15);

								showColorOnMouse (this.mouseXOffset, this.mouseYOffset, dragColorTex);
						}

				} 

		#region Draw_Methods

				protected virtual PaletteData drawColorPalette (PaletteData data, bool drawTinyPalette = false, bool showHexValues = false, float paletteHeight = 100)
				{
						// palette height silder
						//height = EditorGUILayout.Slider ("Height", height, 100, 200);

						if (!drawTinyPalette) {
								GUILayout.Space (5);
								EditorGUILayout.BeginHorizontal ();

								string oldName = data.name;
								string newName = EditorGUILayout.TextField ("Palette name: ", oldName);

								if (!oldName.Equals (newName)) {
										data.name = newName;
										AssetDatabase.RenameAsset (AssetDatabase.GetAssetPath (data.GetInstanceID ()), newName);
								}

								EditorGUILayout.EndHorizontal ();
								GUILayout.Space (5);
						}

						Rect paletteRect;

						if (drawTinyPalette) {
								// use different heights
								paletteHeight = 35;
								paletteRect = GUILayoutUtility.GetRect (Screen.width * 0.15f, 15, GUILayout.ExpandWidth (true));
								paletteRect.x = Screen.width * 0.5f;
								paletteRect.y -= paletteHeight - 20;
								showHexValues = false;
						} else {
								if (showHexValues) {
										paletteRect = GUILayoutUtility.GetRect (Screen.width - 2 * paletteSideMargin, paletteHeight + paletteTopMargin);
								} else {
										paletteRect = GUILayoutUtility.GetRect (Screen.width - 2 * paletteSideMargin, paletteHeight);
								}
						}
		
						// change the cursor for the whole paletteRect
						if (Event.current.type == EventType.MouseDrag) {
								EditorGUIUtility.AddCursorRect (paletteRect, MouseCursor.ResizeHorizontal);				
						}
			
						if (data.colors != null) {

								// show the palette
								float start = paletteSideMargin;
								if (drawTinyPalette) {
										start = paletteRect.x;
										paletteRect.width *= 0.5f;
								}

								for (int i = 0; i < data.colors.Count; i++) {
										Color col = data.colors [i];
										float colWidth = data.percentages [i] * (paletteRect.width);

										//Debug.Log (i + " starts " + start + " width " + colWidth + " maxwidth " + Screen.width);
										float yPos = paletteRect.y;

										if (showHexValues) {
												yPos = paletteRect.y + paletteTopMargin;
										}

										Rect colRect = new Rect (start, yPos, colWidth,
			                         paletteHeight - paletteBotMargin);
			
										EditorGUIUtility.DrawColorSwatch (colRect, col);


										if (DragAndDrop.objectReferences.Count () > 0) {
												checkDragOnColor (colRect, col);
										} else {
												checkDragAColor (colRect, col, i, data);
										}
				

										if (showHexValues) {
												Rect lableRect = colRect;
												lableRect.width = 60;
												lableRect.height = 15;
			
												lableRect.y -= paletteTopMargin * 0.5f;
			
												if (i % 2 == 0) {
														lableRect.y -= 15;
												}
			
												string currentHex = ColorPaletteStatics.ColorToHex (col);
												Rect labelHexRect = new Rect (lableRect);
												labelHexRect.width = hexFieldWidth;

												string newHex = EditorGUI.TextField (labelHexRect, currentHex);


												if (!currentHex.Equals (newHex) && newHex.Length == 6) {
														changeColorViaHex (ref data, i, newHex);
												}
										}

			
										start += colWidth;
								}
						}


						CheckMouseDragOutsidePalette (paletteRect);


						return data;
				}

				protected virtual PaletteData drawSizeButtons (PaletteData data)
				{
						GUILayout.Space (5);

						Rect buttonRect = EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Change the size of the Palette: ");


						if (GUI.Button (new Rect (Screen.width - 50 - buttonWidth - buttonMarginBetween, buttonRect.y, buttonWidth, buttonHeight),
			                new GUIContent (ColorPaletteStatics.addColorIcon, "Add Color"), EditorStyles.miniButton)) { 
								data.AddNewRandomColor ();
						} 

						if (GUI.Button (new Rect (Screen.width - 50, buttonRect.y, buttonWidth, buttonHeight),
			                new GUIContent (ColorPaletteStatics.removeColorIcon, "Remove last Color"), EditorStyles.miniButton)) { 
								data.RemoveColor ();
						} 
			
						EditorGUILayout.EndHorizontal ();

						return data;
				}

				private void drawCreateButton ()
				{
						GUILayout.Space (10);			
						EditorGUILayout.BeginHorizontal ();

						EditorGUILayout.LabelField ("Drag and Drop a ColorPalette or create a new ColorPalette");

						EditorGUILayout.EndHorizontal ();

						GUILayout.Space (10);			

						Rect newPaletteRect = EditorGUILayout.BeginHorizontal ();
			
						newPaletteName = EditorGUILayout.TextField ("Enter Filename for Palette: ", newPaletteName, GUILayout.Width (Screen.width * 0.85f));
			
						if (GUI.Button (new Rect (Screen.width - buttonWidth - buttonMarginBetween, newPaletteRect.y - buttonHeight / 2, buttonWidth, buttonHeight),
			                new GUIContent (ColorPaletteStatics.addPaletteIcon, "Add new Palette"), EditorStyles.miniButton)) { 
								myPaletteObj.CreateStandardPalette (newPaletteName);
						}
			
						EditorGUILayout.EndHorizontal ();
			
						GUILayout.Space (5);
				}
		
		
				protected virtual PaletteData drawColorsAndPercentages (PaletteData data)
				{
						GUILayout.Space (5);

						adjustPCTBefore = GUILayout.Toggle (adjustPCTBefore, " adjust percentage to the left", GUILayout.Width (Screen.width * 0.5f));

						EditorGUILayout.BeginHorizontal ();

						Rect colorChangerRect = GUILayoutUtility.GetRect (Screen.width, data.colors.Count * colorChangerRowHeight);
						colorChangerRect.x += colorChangeLeftMargin;
						colorChangerRect.width -= colorChangeRightMargin;

						GUILayout.Space (5);

						float startY = colorChangerRect.y + 10;

						for (int i = 0; i < data.colors.Count; i++) {

								/*********** Color ************/
								// draw a little preview of the current color
								Rect colRect = new Rect (colorChangerRect.x, startY,
				                         colorPickerWidth, colorChangerRowHeight);
			
								Color currentColor = data.colors [i];
								Color newColor = EditorGUI.ColorField (colRect, currentColor);

								if (!currentColor.ToString ().Equals (newColor.ToString ())) {
										changeColor (ref data, i, newColor);
								}

								/*********** Hexadecimal Color ************/
								string currentHex = ColorPaletteStatics.ColorToHex (currentColor);

								Rect hexRect = new Rect (colorChangerRect.x + colRect.width + colorChangeMarginBetween / 2,
			                         startY, hexFieldWidth, colorChangerRowHeight);

								string newHex = EditorGUI.TextField (hexRect, currentHex);

								if (!currentHex.Equals (newHex) && newHex.Length == 6) {
										changeColorViaHex (ref data, i, newHex);
								}

								/*********** lock percentage ************/
								Rect lockRect = new Rect (colorChangerRect.x + colRect.width + colorChangeMarginBetween + hexRect.width + lockButtonMargin * 1.25f,
				                          startY + 1, lockButtonWidth, colorChangerRowHeight - 5);

								Texture2D lockedTex = ColorPaletteStatics.colorNotLocked;
								string lockedToolTip = "lock % for this Color";
								string undoToolTip = "locked % of ";

								if (data.lockedPercentages [i]) {
										lockedTex = ColorPaletteStatics.colorLocked;
										lockedToolTip = "unlock % for this Color";
										undoToolTip = "unlocked % of ";
								}

								if (GUI.Button (lockRect, new GUIContent (lockedTex, lockedToolTip), EditorStyles.miniButton)) {
										Undo.RecordObject (data, undoToolTip + data.name);
										data.ChangeLockPercentage (i, !data.lockedPercentages [i]);
										Repaint ();
										GUI.changed = true;
								}


				
								/*********** percentage ************/
								Rect silderRect = new Rect (colorChangerRect.x + colRect.width
										+ colorChangeMarginBetween + hexRect.width
										+ colorChangeMarginBetween + lockRect.width
				                            , startY,
				                            colorChangerRect.width - colorChangeMarginBetween
										- colRect.width - colorChangeMarginBetween
										- hexRect.width - colorChangeMarginBetween
										- lockRect.width,
			                            colorChangerRowHeight);

								float maxPct = 1.0f - (data.percentages.Count - 1) * PaletteData.minPct;
								float currentPct = data.percentages [i];
								float newPct = EditorGUI.Slider (silderRect, currentPct, PaletteData.minPct, maxPct);

								//if (!data.lockedPercentages [i] && newPct != currentPct) {
								if (newPct != currentPct) {
										//Debug.Log ("call adjustPCTOnPalette old " + currentPct + " new " + newPct);
										data = adjustPCTOnPalette (data, i, newPct, this.adjustPCTBefore);
								}
			

								startY += colorChangerRowHeight;
						}

						EditorGUILayout.EndHorizontal ();

						return data;
				}

		#endregion

				protected PaletteData adjustPCTOnPalette (PaletteData data, int whichOne, float newPct, bool adjustPCTBefore)
				{
						if (!isPaletteNull (data)) {
								Undo.RecordObject (data, "% change of " + data.name);

								data.ChangePercentage (whichOne, newPct, adjustPCTBefore);

								//Debug.Log ("% after change : " + data);
						} else {
								Debug.Log ("ref Data is supposed to be NULL " + data);
						}

						return data;
				}
		
				protected void changeColorViaHex (ref PaletteData data, int whichOne, string newHex)
				{
						Color newColor = ColorPaletteStatics.HexToColor (newHex);
						changeColor (ref data, whichOne, newColor);
				}
						
				protected void changeColor (ref PaletteData data, int whichOne, Color newColor)
				{
						if (!isPaletteNull (data)) {
								Undo.RecordObject (data, "Color change of " + data.name);
								data.ChangeColor (whichOne, newColor);
						} else {
								Debug.Log ("ref Data is supposed to be NULL " + data);
						}

				}

				public bool isPaletteNull (PaletteData data)
				{
						return ReferenceEquals (data, null);
				}


		#region DragNDrop_Methods


				private bool IsDragTargetValid (Object[] objs)
				{
						if (objs.Length > 0) {
								return true;
						}
						return false;
				}

				private void checkDragAColor (Rect colRect, Color currentColor, int currentPosition, PaletteData data)
				{

						if (Event.current.type == EventType.MouseDown
								&& colRect.Contains (Event.current.mousePosition)) {
								startColorDrag (colRect, currentColor, currentPosition);
						}

						if (DragAndDrop.GetGenericData (dragAColorKey) != null
								&& colRect.Contains (Event.current.mousePosition)) {
								//Debug.Log (Event.current.type + " GetGenericData: " + DragAndDrop.GetGenericData (dragAColorKey));

								if (Event.current.type == EventType.MouseDrag) {
										updateColorDrag (colRect, currentColor, currentPosition);
								}
			
								if (Event.current.type == EventType.MouseUp) {
										// only perform on the one which the mouse is right now!
										performColorDrag (currentPosition, data);
								}			
						}

				}
		
				private void startColorDrag (Rect colRect, Color currentColor, int currentPosition)
				{

						if (DragAndDrop.GetGenericData (dragAColorKey) == null) {
/*								DragAndDrop.PrepareStartDrag ();
								DragAndDrop.StartDrag (dragAColorKey);*/
								//Debug.Log ("startColorDrag to " + currentPosition);
								dragColorTex = ColorPaletteStatics.GetSolidTexture (24, 24, currentColor, 5);
								DragAndDrop.SetGenericData (dragAColorKey, currentPosition);
								//Event.current.Use ();
						}
						
				}
			
				private void updateColorDrag (Rect colRect, Color currentColor, int currentPosition)
				{
						if (DragAndDrop.GetGenericData (dragAColorKey) != null) {

								int pos = (int)DragAndDrop.GetGenericData (dragAColorKey);

								//Debug.Log ("updateColorDrag on pos " + currentPosition + " from " + pos);

								if (currentPosition == pos) {
										dragColorTex = ColorPaletteStatics.GetBorderTexture (24, 24, currentColor, Color.red, 5);
								} else {
										dragColorTex = ColorPaletteStatics.GetBorderTexture (24, 24, currentColor, Color.green, 5);
								}
						}				

				}
		
				private void performColorDrag (int currentPosition, PaletteData data)
				{
						//Debug.Log ("performColorDrag on " + currentPosition + " " + DragAndDrop.GetGenericData (dragAColorKey));

						if (DragAndDrop.GetGenericData (dragAColorKey) != null) {

								int newPosition = (int)DragAndDrop.GetGenericData (dragAColorKey);
								data.ChangeColorPosition (currentPosition, newPosition);

								DragAndDrop.SetGenericData (dragAColorKey, null);
								dragColorTex = null;
								Repaint ();
								Event.current.Use ();
						}
			
				}

				private void CheckMouseDragOutsidePalette (Rect paletteRect)
				{
						if (!paletteRect.Contains (Event.current.mousePosition)) {

								if (DragAndDrop.objectReferences.Count () > 0) {

										if (Event.current.type == EventType.DragUpdated
												&& DragAndDrop.GetGenericData (dragComponentKey) != null) {
												dragColorTex = null;
												Repaint ();
										}

										if ((Event.current.type == EventType.MouseUp || Event.current.type == EventType.DragPerform)
												&& DragAndDrop.GetGenericData (dragComponentKey) != null) {
												DragAndDrop.SetGenericData (dragComponentKey, null);
												dragColorTex = null;
												Repaint ();
										}

								} else {
										if (Event.current.type == EventType.MouseDrag
												&& DragAndDrop.GetGenericData (dragAColorKey) != null) {
												dragColorTex = null;
												Repaint ();
										}

										if (Event.current.type == EventType.MouseUp
												&& DragAndDrop.GetGenericData (dragAColorKey) != null) {
												DragAndDrop.SetGenericData (dragAColorKey, null);
												dragColorTex = null;
												Repaint ();
										}
								}

						}
				}
		
				private void checkDragOnColor (Rect colRect, Color currentColor)
				{
						if (colRect.Contains (Event.current.mousePosition)) {
								if (Event.current.type == EventType.DragUpdated) {
										updateComponentDrag (currentColor);
								}

								if (Event.current.type == EventType.DragPerform) {
										performComponentDrag ();
								}
						}

				}

				protected void updateComponentDrag (Color currentColor)
				{

						// if ok show texture Color
						if (gotComponentAColor ()) {
				
								DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				
								if (DragAndDrop.GetGenericData (dragComponentKey) == null) {
										DragAndDrop.SetGenericData (dragComponentKey, currentColor);
								}
								// firstTime on Color
/*										DragAndDrop.PrepareStartDrag ();
										DragAndDrop.StartDrag (dragComponentKey);
*/
								// already set a Color
								Color color = (Color)DragAndDrop.GetGenericData (dragComponentKey);
					
								if (currentColor != color) {
										DragAndDrop.SetGenericData (dragComponentKey, currentColor);
								}

								dragColorTex = ColorPaletteStatics.GetBorderTexture (24, 24, currentColor, Color.green, 5);
			
						} else {
								DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
								//dragColorTex = ColorPaletteStatics.GetBorderTexture (24, 24, currentColor, Color.red, 5);
						}
			
				}
		
		
				private void performComponentDrag ()
				{
						if (DragAndDrop.GetGenericData (dragComponentKey) != null) {
								Color color = (Color)DragAndDrop.GetGenericData (dragComponentKey);
								applyDragAndDropColor (color);

								DragAndDrop.SetGenericData (dragComponentKey, null);
								dragColorTex = null;
								Repaint ();
								SceneView.RepaintAll ();
								Event.current.Use ();
						}
			
				}

		
				private bool gotComponentAColor ()
				{
						return (gotDragableWithPartSys (DragAndDrop.objectReferences) || gotDragableWithText (DragAndDrop.objectReferences)
								|| gotDragableWithCamera (DragAndDrop.objectReferences) || gotDragableWithRender (DragAndDrop.objectReferences)
								|| gotDragableWithUI (DragAndDrop.objectReferences));
				}


				private void applyDragAndDropColor (Color color)
				{

						// check of the TEXT has to come first because TextMesh as a Renderer too
						// but only the TextMesh Component should change the Color!
						if (gotDragableWithText (DragAndDrop.objectReferences)) {
								applyColorToText (DragAndDrop.objectReferences, color);						

						} else if (gotDragableWithUI (DragAndDrop.objectReferences)) {
								applyColorToUI (DragAndDrop.objectReferences, color);

						} else if (gotDragableWithRender (DragAndDrop.objectReferences)) {
								applyColorToRenderer (DragAndDrop.objectReferences, color);
				
						} else if (gotDragableWithPartSys (DragAndDrop.objectReferences)) {
								applyColorToPartSys (DragAndDrop.objectReferences, color);
				
						} else if (gotDragableWithCamera (DragAndDrop.objectReferences)) {
								applyColorToCamera (DragAndDrop.objectReferences, color);
						}

				}

				private bool gotDragableWithCamera (Object[] objs)
				{
						foreach (Object obj in objs) {
				
								if (obj.GetType () == typeof(GameObject)) {
										if (((GameObject)obj).GetComponent<Camera> () != null) {
												return true;
										}
								}
				
						}
			
						return false;
				}

				private bool gotDragableWithPartSys (Object[] objs)
				{
						foreach (Object obj in objs) {
				
								if (obj.GetType () == typeof(GameObject)) {
										if (((GameObject)obj).GetComponent<ParticleSystem> () != null) {
												return true;
										}
								}
				
						}
			
						return false;
				}

				private bool gotDragableWithText (Object[] objs)
				{
						foreach (Object obj in objs) {

								if (obj.GetType () == typeof(GameObject)) {
										if (((GameObject)obj).GetComponent<TextMesh> () != null
												|| ((GameObject)obj).GetComponent<GUIText> () != null) {
												return true;
										}
								}
								
						}
			
						return false;
				}

				private bool gotDragableWithUI (Object[] objs)
				{

#if UNITY_4_6

						foreach (Object obj in objs) {
				
								if (obj.GetType () == typeof(GameObject)) {
										if (((GameObject)obj).GetComponent<UnityEngine.UI.Image> () != null
												|| ((GameObject)obj).GetComponent<UnityEngine.UI.Graphic> () != null) {
												return true;
										}
								}
				
						}
#endif
			
						return false;
				}


				private bool gotDragableWithRender (Object[] objs)
				{
						foreach (Object obj in objs) {

								if (obj.GetType () == typeof(GameObject)) {
										if (((GameObject)obj).GetComponent<Renderer> () != null) {
												return true;
										}
								}
								
						}

						return false;
				}

				private void applyColorToRenderer (Object[] objs, Color color)
				{

						foreach (Object obj in objs) {
								if (obj.GetType () == typeof(GameObject)) {

										SpriteRenderer sprR = ((GameObject)obj).GetComponent<SpriteRenderer> ();
										Renderer render = ((GameObject)obj).GetComponent<Renderer> ();
										if (sprR != null) {
												sprR.color = color;
										} else if (render != null) {
												//render.material.color = color;
												render.sharedMaterial.color = color;
										}

								}

						}
				}

				private void applyColorToText (Object[] objs, Color color)
				{
						foreach (Object obj in objs) {
								if (obj.GetType () == typeof(GameObject)) {

										GUIText guiT = ((GameObject)obj).GetComponent<GUIText> ();
										TextMesh meshT = ((GameObject)obj).GetComponent<TextMesh> ();

										if (guiT != null) {
												guiT.color = color;
										} else if (meshT != null) {
												meshT.color = color;
										}

								}
						}
				}

				private void applyColorToCamera (Object[] objs, Color color)
				{
						foreach (Object obj in objs) {
								if (obj.GetType () == typeof(GameObject)) {
					
										Camera cam = ((GameObject)obj).GetComponent<Camera> ();

										if (cam != null) {
												cam.backgroundColor = color;
										}
					
								}
						}
				}

				private void applyColorToPartSys (Object[] objs, Color color)
				{
						foreach (Object obj in objs) {
								if (obj.GetType () == typeof(GameObject)) {
					
										ParticleSystem partSys = ((GameObject)obj).GetComponent<ParticleSystem> ();
					
										if (partSys != null) {
												partSys.startColor = color;
										}					
								}
						}
				}

				private void applyColorToUI (Object[] objs, Color color)
				{
#if UNITY_4_6
						foreach (Object obj in objs) {
								if (obj.GetType () == typeof(GameObject)) {
					
										applyColorToGraphic (((GameObject)obj).GetComponent<UnityEngine.UI.Image> (), color);
										applyColorToGraphic (((GameObject)obj).GetComponent<UnityEngine.UI.Text> (), color);
										applyColorToGraphic (((GameObject)obj).GetComponent<UnityEngine.UI.RawImage> (), color);
								}
						}
						Canvas.ForceUpdateCanvases ();
#endif
				}

				private void applyColorToGraphic (UnityEngine.UI.Graphic graphic, Color color)
				{
						if (graphic != null) {
								graphic.color = color;
						}
				}

				public void showColorOnMouse (float mouseXOffset, float mouseYOffset, Texture2D tex)
				{
						if (tex != null) {
								EditorGUI.DrawTextureTransparent (new Rect (Event.current.mousePosition.x + mouseXOffset,
			                           Event.current.mousePosition.y + mouseYOffset,
				                           tex.width, tex.height), tex);
								Repaint ();
						}
				}
			
		#endregion

		}
}