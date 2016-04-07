using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Gamelogic;
using Gamelogic.Grids;
using AssemblyCSharp;
using Vectrosity;
using DG.Tweening;

public class GameGridBehaviour : GridBehaviour<PointyHexPoint> {
    public static GameGridBehaviour instance = null;

    public static bool didWin;
	public static Color baseFloorColor;
    public static int localPlayer = 0;
    public static bool isMP = false;

	BuildMenuBehaviour buildScreenSettings;

	public Player[] listOfPlayers;
	public Text moneyText;
    public Text PlaceDictText;
    public RectTransform fadePanel;
	public GameObject buildScreen;
    public Text NotEnoughMoneyText;
    public Texture lineTex;

    public Sprite townHall;
    public Sprite cityHall;
    public Sprite capital;

    float timer = 0f;

	public List<Combat> listofCurrentCombats;
    private GameState curGameState;

    // Use this for initialization
    void Start () {
		listofCurrentCombats = new List<Combat> ();

        buildScreenSettings = buildScreen.GetComponent<BuildMenuBehaviour>();
        baseFloorColor = GridBuilder.Colors[0];

        curGameState = GameState.LoadState;
        instance = this;

        if (isMP)
            PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
        else
            CreatePlayerObjects(false);
    }

    public void CreatePlayerObjects(bool ismp)
    {
        GameObject P1;
        GameObject P2;

        P1 = Instantiate(Resources.Load("Player") as GameObject, Vector3.zero, Quaternion.identity) as GameObject;
        P2 = Instantiate(Resources.Load("AIPlayer") as GameObject, Vector3.zero, Quaternion.identity) as GameObject;
    }

    public void InformDoneLoading()
    {
        curGameState = GameState.InitState;
    }

    private void InitializePlayers(GameObject[] playerObjects)
    {
        listOfPlayers = new Player[playerObjects.Length];

        if (listOfPlayers.Length > 1)
        {
            int c = 0;
            foreach (GameObject playerObj in playerObjects)
            {
                if (isMP)
                {
                    Player netPlayer = playerObj.GetComponent<Player>();
                    listOfPlayers[c] = netPlayer;

                    //if (listOfPlayers[c])
                        localPlayer = 0;
                }
                else
                {
                    AIPlayer aiPlayer = playerObj.GetComponent<AIPlayer>();
                    if (aiPlayer != null)
                    {
                        listOfPlayers[c] = aiPlayer;
                    }
                    else
                        listOfPlayers[c] = playerObj.GetComponent<Player>();
                }

                c++;
            }

            buildScreenSettings.SetBGColor(listOfPlayers[localPlayer].PlayerColor);

            moneyText.color = listOfPlayers[localPlayer].PlayerColor;
            curGameState = GameState.PlayerSetupState;

            Animator anim = PlaceDictText.GetComponent<Animator>();
            anim.SetTrigger("PlaceDictTrigger");
        }
    }

    

	// Update is called once per frame
	void Update () {
        if (curGameState.Equals(GameState.RegGameState))
            UpdatePlayGameState();
        else if (curGameState.Equals(GameState.InitState))
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerType");
            if (players.Length >= 2)
                InitializePlayers(players);
        }
        else if (curGameState.Equals(GameState.PlayerSetupState))
            PlayerSetupState();
        else if (curGameState.Equals(GameState.EndGameState))
            EndGameState();

	}

    private void PlayerSetupState()
    {
        if (listOfPlayers[0].GetUnitCount() > 0 && listOfPlayers[1].GetUnitCount() > 0)
            curGameState = GameState.RegGameState;
    }

    private void UpdatePlayGameState()
    {
        CheckEndGame();

        UpdateGame();

        //update our timer
        timer += Time.deltaTime;
    }

    private void UpdateGame()
    {
        //Update All the Fights Going On
        listofCurrentCombats.ForEach(fight => {
            fight.Update();

            if (fight.fightOver)
            {
                listofCurrentCombats.Remove(fight);
                fight = null;
            }
        });
    }

	private void CheckEndGame ()
	{
		if (!listOfPlayers[0].DictatorAlive || !listOfPlayers[1].DictatorAlive) {

			if (!listOfPlayers[0].DictatorAlive)
				GameGridBehaviour.didWin = false;
			else
				GameGridBehaviour.didWin = true;

            timer = 0f;
            fadePanel.gameObject.SetActive(true);
            curGameState = GameState.EndGameState;
		}
	}

    private void EndGameState()
    {
        fadePanel.GetComponent<Animator>().SetTrigger("FadeToBlack");
        timer += Time.deltaTime;

        if (timer > 1.2f)
            SceneManager.LoadScene("StatsScreen");
    }

	

	/// <summary>
	/// We need to know which settlement a building belongs to. and since buildings HAVE to be connected by
	/// atleast a single other building, and from inception be told what their owning settlement is, we can
	/// pull the data from any neighbor of the same colour and not just a settlement
	/// </summary>
	/// <param name="pointToSearchFrom">Point to search from.</param>
	/// <param name="owningPlayerColor">Owning player color.</param>
	public Settlement FindOwningSettlement(PointyHexPoint pointToSearchFrom, PointList<PointyHexPoint> surroundingTiles, Color owningPlayerColor)
	{
		foreach (PointyHexPoint point in surroundingTiles) {
			UnitCell currCell = (Grid[point] as UnitCell);
			if (currCell.buildingOnTile && currCell.structureOnTile.StructColor.Equals (owningPlayerColor)) {
				if (currCell.structureOnTile.StructureType.Equals (StructureUnitType.Settlement))
					return currCell.structureOnTile as Settlement;
				else
					return currCell.structureOnTile.OwningSettlement;
			}
		}

		return null;
	}

	public bool IsBuildingInArea(PointyHexPoint pointToSearchFrom, PointList<PointyHexPoint> surroundingTiles, Color owningPlayerColor)
	{
		foreach (PointyHexPoint point in surroundingTiles) {
			UnitCell currCell = (Grid[point] as UnitCell);

			if (currCell.buildingOnTile)
				return true;
		}

		return false;
	}

	public IEnumerable<PointyHexPoint> GetGridPath(PointyHexPoint startPoint, PointyHexPoint endPoint)
	{
		//We use the original grid here, and not the 
		//copy, to preserve neighbor relationships. Therefore, we
		//have to cast the cell in the lambda expression below.
		var path = Algorithms.AStar(
			Grid,
			startPoint,
			endPoint,
			(p, q) => p.DistanceFrom(q),
			c => true,
			(p, q) => 1);
		
		return path;
	}

	public PointList<PointyHexPoint> GetSurroundingTiles(PointyHexPoint clickedPoint)
	{
		var path = Algorithms.GetPointsInRangeCost (Grid,
		                                           clickedPoint, 
		                                           tile => true,
		                                           (p,q) => 1,
		                                           1);

		return path.Keys.ToPointList<PointyHexPoint>();
	}

    #region Creation
    /*
    public void CreateNewMilitaryUnit(Player player, int unitType, UnitCell gridCell, PointyHexPoint gridPoint)
    {
        CreateNewMilitaryUnit(player, unitType, gridCell, gridPoint, 1);
    }


    public void CreateNewMilitaryUnit(Player player, int unitType, UnitCell gridCell, PointyHexPoint gridPoint, int amount)
    {
        MilitaryUnit newUnit = Instantiate(unitTypes[unitType], gridCell.transform.position, Quaternion.identity).GetComponent<MilitaryUnit>();
        newUnit.Initialize(player.GetNextUnitID(), player.PlayerColor, (MilitaryUnitType)unitType, gridPoint, amount);
        player.AddToUnits(newUnit);
        gridCell.AddUnitToTile(newUnit);
    }


    public void CreateNewSettlement(Player player, UnitCell gridCell, PointyHexPoint gridPoint, PointList<PointyHexPoint> surroundingTiles)
    {
        Settlement newSettlement = Instantiate(structureTypes[(int)StructureUnitType.Settlement], gridCell.transform.position, Quaternion.identity).GetComponent<Settlement>();
        newSettlement.Initialize(player.PlayerColor, StructureUnitType.Settlement, gridPoint);
        player.settlements.Add(newSettlement);
        player.AddToOwnedTiles(Grid, newSettlement, player, surroundingTiles);
        gridCell.AddStructureToTile(newSettlement);
    }

    public void CreateNewStructure(Player player, int structType, UnitCell gridCell, PointyHexPoint gridPoint, PointList<PointyHexPoint> surroundingTiles, Settlement owningSettlement)
    {
        StructureUnit newStructure = Instantiate(structureTypes[structType], gridCell.transform.position, Quaternion.identity).GetComponent<StructureUnit>();
        newStructure.Initialize(player.PlayerColor, (StructureUnitType)structType, gridPoint, owningSettlement);
        owningSettlement.AddToBuildingList(newStructure);
        owningSettlement.newBuildingAdded = true;
        player.AddToOwnedTiles(Grid, owningSettlement, player, surroundingTiles);

        gridCell.AddStructureToTile(newStructure);
    }*/

    #endregion

    public bool IsBuildScreenBlocking()
    {
        return buildScreenSettings.isActiveAndEnabled;
    }

    public void SetupBuildMenu(bool isSettlementMenu, BuildingChosenEventHandler HandleStructChosen)
    {
        buildScreenSettings.structChosen += HandleStructChosen;
        buildScreenSettings.ShowBuildMenu(isSettlementMenu);
    }

    public void RemoveBuildScreenHandler(BuildingChosenEventHandler HandleStructChosen)
    {
        buildScreenSettings.structChosen -= HandleStructChosen;
    }

    public GameState GetCurrentGameState()
    {
        return curGameState;
    }
}

public enum GameState
{
    None,
    ComeFromLoad,
    LoadState,
    InitState,
    PlayerSetupState,
    RegGameState,
    EndGameState,
}
