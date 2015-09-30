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
	private Color ownedTileColor = Color.black;
	private Color baseTileColor = new Color32 (83, 199, 175, 255);
	//private Color prevTileColor = Color.black;

	private Sprite holdingSprite;

	public void AddUnitToTile(AssemblyCSharp.MilitaryUnit unitToDisplay)
	{
		if (!buildingOnTile)
			SetTileColorUnit (unitToDisplay.UnitColor);

		unitOnTile = true;
	} 

	public void AddStructureToTile(AssemblyCSharp.StructureUnit structToDisplay)
	{
		structureOnTile = structToDisplay;
		SetTileColorStructure (structureOnTile.StructColor);
		buildingOnTile = true;
	}

	public void SetTileColorPath(Color tileColor)
	{
		/*if (tileColor != this.Color || prevTileColor != this.Color)
			prevTileColor = this.Color;
		else
			prevTileColor = Color.black;*/
			this.Color = tileColor; 
	}

	public void SetTileColorUnPath()
	{
		if (ownedTileColor != Color.black)
			this.Color = ownedTileColor;
		else
			this.Color = baseTileColor;
	}

	public void SetTileColorUnit(Color tileColor)
	{
		this.Color = tileColor;
	}

	public void SetTileColorStructure(Color tileColor)
	{
		ownedTileColor = tileColor;
		this.Color = ownedTileColor;
	}

	public void SetTileColorBuildable(Color tileColor)
	{
		ownedTileColor = tileColor;
		this.Color = ownedTileColor;
	}

	public void RemoveUnit()
	{
		unitOnTile = false;
		if (!buildingOnTile) {
			if (ownedTileColor != Color.black)
				this.Color = ownedTileColor;
			else
				this.Color = baseTileColor;
			/*if (!prevTileColor.Equals (Color.black)) 
				SetTileColorUnit (prevTileColor);
			else
				SetTileColorUnit (baseTileColor);*/
		} else {
			this.Color = ownedTileColor;
		}
	}
}
