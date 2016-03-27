using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grids;
using AssemblyCSharp;

public class AIPlayer : Player {

    private AIState myState;

	// Use this for initialization
	void Start () {
		InitBasePlayer ();
        myState = AIState.InitState;
		//PlayerColor = new Color32 (50, 205, 50, 255);
	}

	// Update is called once per frame
	void Update () {
        if (myState.Equals(AIState.DefenseState))
        {

        }
        else if (myState.Equals(AIState.InitState))
            CreateInitialBuildings();
	}

    void CreateInitialBuildings()
    {
        CreateNewUnit(new PointyHexPoint(1, 13), MilitaryUnitType.Dictator);
        BuildNewSettlement(new PointyHexPoint(2, 13));
        BuildNewStructure(new PointyHexPoint(2, 12), StructureUnitType.Market, this.settlements[0]);
        BuildNewStructure(new PointyHexPoint(3, 12), StructureUnitType.Airport, this.settlements[0]);
        BuildNewStructure(new PointyHexPoint(1, 13), StructureUnitType.Factory, this.settlements[0]);
        CreateNewUnit(new PointyHexPoint(3, 13), MilitaryUnitType.Infantry, 5);
        myState = AIState.DefenseState;
    }

    void BuildNewSettlement(PointyHexPoint buildPoint)
    {
        GameGridBehaviour.instance.CreateNewSettlement(this, 
            GameGridBehaviour.instance.Grid[buildPoint] as UnitCell, 
            buildPoint, 
            GameGridBehaviour.instance.GetSurroundingTiles(buildPoint));
    }

    void BuildNewStructure(PointyHexPoint buildPoint, StructureUnitType structType, Settlement owningSettlement)
    {
        GameGridBehaviour.instance.CreateNewStructure(this,
            (int)structType,
            GameGridBehaviour.instance.Grid[buildPoint] as UnitCell,
            buildPoint,
            GameGridBehaviour.instance.GetSurroundingTiles(buildPoint),
            owningSettlement);
    }

    void CreateNewUnit(PointyHexPoint buildPoint, MilitaryUnitType milType)
    {
        CreateNewUnit(buildPoint, milType, 1);
    }

    void CreateNewUnit(PointyHexPoint buildPoint, MilitaryUnitType milType, int amountOf)
    {
        GameGridBehaviour.instance.CreateNewMilitaryUnit(this,
            (int)milType,
            GameGridBehaviour.instance.Grid[buildPoint] as UnitCell,
            buildPoint);
    }
}
enum AIState
{
    InitState,
    DefenseState,
}
