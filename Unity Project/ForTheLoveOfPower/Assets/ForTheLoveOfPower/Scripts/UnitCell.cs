﻿using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Gamelogic.Grids;

public class UnitCell : SpriteCell {

	public SpriteRenderer foreground;
	public bool isStart;
	public AssemblyCSharp.MilitaryUnit unitOnTile;
	public AssemblyCSharp.StructureUnit buildingOnTile;


	public void AddUnitToTile(AssemblyCSharp.MilitaryUnit unitToDisplay, Sprite[] unitSprites)
	{
		unitOnTile = unitToDisplay;
		if (foreground == null) {
			foreground = this.gameObject.AddComponent<SpriteRenderer> ();
		}
		foreground.sprite = unitSprites[unitOnTile.IdleAnimation];
		foreground.color = unitOnTile.UnitColor;
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
		this.Color = tileColor;
	}

	public void RemoveUnit()
	{
		unitOnTile = null;
		foreground.sprite = null;
	}
}
