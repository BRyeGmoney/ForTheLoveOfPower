using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Gamelogic.Grids;

public class UnitCell : SpriteCell {

	public SpriteRenderer foreground;
	public bool isStart;
	public AssemblyCSharp.MilitaryUnit unitOnTile;
	public AssemblyCSharp.StructureUnit buildingOnTile;
	public Color prevTileColor;

	private Sprite holdingSprite;

	public void AddUnitToTile(AssemblyCSharp.MilitaryUnit unitToDisplay, Sprite[] unitSprites)
	{
		unitOnTile = unitToDisplay;
		if (foreground == null) {
			foreground = this.gameObject.AddComponent<SpriteRenderer> ();
		} else {
			holdingSprite = foreground.sprite;
		}

		foreground.sprite = unitSprites[unitOnTile.IdleAnimation];
		foreground.color = unitOnTile.UnitColor;
		SetTileColor (unitOnTile.UnitColor);
	}

	public void AddStructureToTile(AssemblyCSharp.StructureUnit structToDisplay, Sprite[] structSprites)
	{
		buildingOnTile = structToDisplay;
		if (foreground == null) {
			foreground = this.gameObject.AddComponent<SpriteRenderer> ();
		}
		foreground.sprite = structSprites [buildingOnTile.StructureSpriteIndex];
		foreground.color = buildingOnTile.StructColor;
	}

	public void SetTileColor(Color tileColor)
	{
		prevTileColor = this.Color;
		this.Color = tileColor;
	}

	public void RemoveUnit()
	{
		unitOnTile = null;
		if (foreground != null)
			foreground.sprite = null;

		if (buildingOnTile != null) {
			foreground.sprite = holdingSprite;
			foreground.color = buildingOnTile.StructColor;
		} else {
			if (prevTileColor != null) 
				SetTileColor (prevTileColor);
			else
				SetTileColor (Color.white);
		}
	}
}
