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
        if (GameGridBehaviour.instance.GetCurrentGameState().Equals(GameState.RegGameState))
        {
            DetermineNextAction();
            playerArmy.UpdateUnits(this);
            UpdateSettlements();
        }
        else if (GameGridBehaviour.instance.GetCurrentGameState().Equals(GameState.PlayerSetupState))
        {
            if (myState.Equals(AIState.InitState))
                CreateInitialBuildings();
        }
	}

    void CreateInitialBuildings()
    {
        playerArmy.CreateNewUnit(new PointyHexPoint(1, 13), MilitaryUnitType.Dictator, this);
        BuildNewSettlement(new PointyHexPoint(2, 13));
        BuildNewStructure(new PointyHexPoint(2, 12), StructureUnitType.Market, FindSettlementByID(0));
        BuildNewStructure(new PointyHexPoint(3, 12), StructureUnitType.Airport, FindSettlementByID(0));
        BuildNewStructure(new PointyHexPoint(1, 13), StructureUnitType.Factory, FindSettlementByID(0));
        playerArmy.CreateNewUnit(new PointyHexPoint(3, 13), MilitaryUnitType.Infantry, 5, this);
        myState = AIState.DefenseState;
    }

    void DetermineNextAction()
    {

    }
}
enum AIState
{
    InitState,
    DefenseState,
}
