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
	private Color baseTileColor = new Color32 (83, 199, 175, 255);
	private Color prevTileColor = Color.black;

	private Sprite holdingSprite;

	public void AddUnitToTile(AssemblyCSharp.MilitaryUnit unitToDisplay)
	{
		if (!buildingOnTile)
			SetTileColor (unitToDisplay.UnitColor);

		unitOnTile = true;
	} 

	public void AddStructureToTile(AssemblyCSharp.StructureUnit structToDisplay)
	{
		structureOnTile = structToDisplay;
		buildingOnTile = true;
	}

	public void SetTileColor(Color tileColor)
	{
		if (tileColor != this.Color || prevTileColor != this.Color)
			prevTileColor = this.Color;
		else
			prevTileColor = Color.black;

		this.Color = tileColor; 
	}

	public void RemoveUnit()
	{
		unitOnTile = false;
		if (!buildingOnTile) {
			if (!prevTileColor.Equals (Color.black)) 
				SetTileColor (prevTileColor);
			else
				SetTileColor (baseTileColor);
		}
	}
}
