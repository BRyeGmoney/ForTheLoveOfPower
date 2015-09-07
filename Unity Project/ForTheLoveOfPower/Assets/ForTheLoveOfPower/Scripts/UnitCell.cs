using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Gamelogic.Grids;

public class UnitCell : SpriteCell {

	//public SpriteRenderer foreground;
	public bool isStart;
	//public AssemblyCSharp.MilitaryUnit unitOnTile;
	public AssemblyCSharp.StructureUnit structureOnTile;
	public bool unitOnTile;
	public bool buildingOnTile;
	public int playerOnTile;
	private Color prevTileColor = Color.black;

	private Sprite holdingSprite;

	public void AddUnitToTile(AssemblyCSharp.MilitaryUnit unitToDisplay)
	{
		/*unitOnTile = unitToDisplay;
		if (foreground == null) {
			foreground = this.gameObject.AddComponent<SpriteRenderer> ();
		} else {
			holdingSprite = foreground.sprite;
		}

		foreground.sprite = unitSprites[unitOnTile.IdleAnimation];
		foreground.color = unitOnTile.UnitColor;*/
		SetTileColor (unitToDisplay.UnitColor);
		unitOnTile = true;
	} 

	public void AddStructureToTile(AssemblyCSharp.StructureUnit structToDisplay)
	{
		structureOnTile = structToDisplay;
		buildingOnTile = true;
		/*buildingOnTile = structToDisplay;
		if (foreground == null) {
			foreground = this.gameObject.AddComponent<SpriteRenderer> ();
		}
		foreground.sprite = structSprites [buildingOnTile.StructureSpriteIndex];
		foreground.color = buildingOnTile.StructColor;*/
	}

	public void SetTileColor(Color tileColor)
	{
		prevTileColor = this.Color;
		this.Color = tileColor;
	}

	public void RemoveUnit()
	{
		unitOnTile = false;
		if (!buildingOnTile) {
			if (!prevTileColor.Equals (Color.black)) 
				SetTileColor (prevTileColor);
			else
				SetTileColor (new Color32 (83, 199, 175, 255));
		}
		/*unitOnTile = null;
		if (foreground != null)
			foreground.sprite = null;

		if (buildingOnTile != null) {
			foreground.sprite = holdingSprite;
			foreground.color = buildingOnTile.StructColor;
		} else {
			if (!prevTileColor.Equals (Color.black)) 
				SetTileColor (prevTileColor);
			else
				SetTileColor (new Color32(83, 199, 175, 255));
		}*/
	}
}
