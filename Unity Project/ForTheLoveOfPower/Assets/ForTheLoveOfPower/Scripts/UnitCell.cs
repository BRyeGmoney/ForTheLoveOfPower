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


	public void AddUnitToTile(AssemblyCSharp.MilitaryUnit unitToDisplay)
	{
		unitOnTile = unitToDisplay;
		foreground = this.gameObject.AddComponent<SpriteRenderer> ();
		foreground.sprite = Resources.Load<Sprite> ("Sprites/Units/Unit_Dictator");
	}
}
