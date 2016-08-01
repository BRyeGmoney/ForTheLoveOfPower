using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Gamelogic.Grids;
using DG.Tweening;

public class UnitCell : SpriteCell {

	//public SpriteRenderer foreground;
	public bool isStart;
	//public AssemblyCSharp.MilitaryUnit unitOnTile;
	public AssemblyCSharp.StructureUnit structureOnTile;
	public bool unitOnTile;
	public bool buildingOnTile;
	public int playerOnTile;
	private Color ownedTileColor = Color.black;
    //private Color baseTileColor = GameGridBehaviour.baseFloorColor;//new Color32 (83, 199, 175, 255);
    private Color unitTileColor = Color.black;
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
        SetOwned();
	}

	public void SetTileColorUnPath()
	{
        if (ownedTileColor != Color.black)
            this.Color = ownedTileColor;
        else if (unitOnTile)
            this.Color = unitTileColor;
        else
            this.Color = GameGridBehaviour.baseFloorColor;//baseTileColor;

        SetUnowned();
	}

    public void SetTileColorUnOwn()
    {
        ownedTileColor = Color.black;
        //this.Color = GameGridBehaviour.baseFloorColor;//baseTileColor;
        DOTween.To(() => this.Color, x => this.Color = x, GameGridBehaviour.baseFloorColor, 0.5f);
        SetUnowned();
    }

	public void SetTileColorUnit(Color tileColor)
	{
        unitTileColor = tileColor;
		//this.Color = tileColor;
        DOTween.To(() => this.Color, x => this.Color = x, tileColor, 0.5f);
        SetOwned();
	}

	public void SetTileColorStructure(Color tileColor)
	{
		ownedTileColor = tileColor;
        DOTween.To(() => this.Color, x => this.Color = x, ownedTileColor, 1f);
		//this.Color = ownedTileColor;
        SetOwned();
	}

	public void SetTileColorBuildable(Color tileColor)
	{
		ownedTileColor = tileColor;
        DOTween.To(() => this.Color, x => this.Color = x, ownedTileColor, 1f);
        SetOwned();
	}

    private void SetOwned()
    {
        FrameIndex = 1;
    }

    private void SetUnowned()
    {
        FrameIndex = 0;
    }

	public void RemoveUnit()
	{
		unitOnTile = false;
        unitTileColor = Color.black;

		if (!buildingOnTile) {
			if (ownedTileColor != Color.black)
                DOTween.To(() => this.Color, x => this.Color = x, ownedTileColor, 0.5f);
            else
                DOTween.To(() => this.Color, x => this.Color = x, GameGridBehaviour.baseFloorColor, 0.5f);//baseTileColor;
                                                                                                          /*if (!prevTileColor.Equals (Color.black)) 
                                                                                                              SetTileColorUnit (prevTileColor);
                                                                                                          else
                                                                                                              SetTileColorUnit (baseTileColor);*/
        } else {
			this.Color = ownedTileColor;
		}
	}
}
