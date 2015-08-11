using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grids;
using AssemblyCSharp;

public class AIPlayer : Player {

	// Use this for initialization
	void Start () {
		InitBasePlayer ();
		playerColor = new Color (50, 205, 50);
	}

	//Create the basic setup for ai player
	public void CreateBasePlayer(Sprite[] unitSprites, Sprite[] structSprites, IGrid<PointyHexPoint> gameGrid) {
		MilitaryUnit dictator = CreateMilitaryUnit.CreateDictator (this.PlayerColor);
		MilitaryUnit infMan = CreateMilitaryUnit.CreateInfantry (this.PlayerColor);

		milUnits.Add (dictator);
		milUnits.Add (infMan);

		(gameGrid[new PointyHexPoint(-3, 18)] as UnitCell).AddUnitToTile (dictator, unitSprites);
		(gameGrid [new PointyHexPoint(-4, 18)] as UnitCell).AddUnitToTile (infMan, unitSprites);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
