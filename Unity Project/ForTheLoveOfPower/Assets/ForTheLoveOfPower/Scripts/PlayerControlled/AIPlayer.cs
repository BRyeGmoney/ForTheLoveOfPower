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
		PointyHexPoint dictPoint = new PointyHexPoint (-3, 18);
		PointyHexPoint infPoint = new PointyHexPoint (-4, 18);

		MilitaryUnit dictator = CreateMilitaryUnit.CreateDictator (this.PlayerColor, dictPoint);
		MilitaryUnit infMan = CreateMilitaryUnit.CreateInfantry (this.PlayerColor, infPoint);

		milUnits.Add (dictator);
		milUnits.Add (infMan);

		(gameGrid[dictPoint] as UnitCell).AddUnitToTile (dictator, unitSprites);
		(gameGrid [infPoint] as UnitCell).AddUnitToTile (infMan, unitSprites);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
