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

	BuildMenuBehaviour buildScreenSettings;

	public Player[] listOfPlayers;
	public Text moneyText;
    public Text PlaceDictText;
    public RectTransform fadePanel;
	public GameObject buildScreen;
	public GameObject[] unitTypes;
	public GameObject[] structureTypes;
    public Text NotEnoughMoneyText;
    public Texture lineTex;

	float timer = 0f;

	private List<Combat> listofCurrentCombats;
    private GameState curGameState;

    
    //private VectorLine[] usableLines;
    //private ushort curVectorLine;

    // Use this for initialization
    void Start () {
		listofCurrentCombats = new List<Combat> ();

        buildScreenSettings = buildScreen.GetComponent<BuildMenuBehaviour>();
        baseFloorColor = GridBuilder.Colors[0];

        curGameState = GameState.InitState;
        instance = this;

        if (MenuBehaviour.instance.isMPGame)
            PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, 0);
        else
            MenuBehaviour.instance.CreatePlayerObjects(false);
    }

    private void InitializePlayers(GameObject[] playerObjects)
    {
        listOfPlayers = new Player[playerObjects.Length];

        if (listOfPlayers.Length > 1)
        {
            int c = 0;
            foreach (GameObject playerObj in playerObjects)
            {
                if (MenuBehaviour.instance != null && MenuBehaviour.instance.isMPGame)
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

            Debug.Log("About to play anim");
            Animator anim = PlaceDictText.GetComponent<Animator>();
            Debug.Log(anim);
            anim.SetTrigger("PlaceDictTrigger");
            Debug.Log("Played Anim");
        }
        
    }

    

	// Update is called once per frame
	void FixedUpdate () {
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
        if (listOfPlayers[localPlayer].milUnits.Count < 1)
        {
            listOfPlayers[localPlayer].PlayerSetupState();
        }
        
        if (listOfPlayers[0].milUnits.Count > 0 && listOfPlayers[1].milUnits.Count > 0)
            curGameState = GameState.RegGameState;

        
    }

    private void UpdatePlayGameState()
    {
        CheckEndGame();

        UpdateSPGame();

        listOfPlayers[0].PlayGameState(timer);

        //Display the player's money
        moneyText.text = String.Concat("Money: ", listOfPlayers[localPlayer].Cash);

        //update our timer
        timer += Time.deltaTime;
    }

    private void UpdateSPGame()
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

        //Update All the player's units, post fight, in case anyone died, we can get rid of them now
        listOfPlayers[localPlayer].milUnits.ForEach(unit => {

            if (unit != null)
            {
                unit.UpdateUnit(Grid, listOfPlayers);

                if (unit.GetUnitAmount() < 1)
                {
                    StartCoroutine(DestroyUnitAfterAnimation(unit, listOfPlayers[localPlayer]));
                }
                else if (unit.combatToUpdateGame != null)
                {
                    this.listofCurrentCombats.Add(unit.combatToUpdateGame);
                    unit.combatToUpdateGame = null;
                }
            }
            else
                listOfPlayers[localPlayer].milUnits.Remove(unit);
        });

        //Update all the second player's units, post fight, in case anyone died
        listOfPlayers[1].milUnits.ForEach(unit => {
            unit.UpdateUnit(Grid, listOfPlayers);

            if (unit.GetUnitAmount() < 1)
            {
                StartCoroutine(DestroyUnitAfterAnimation(unit, listOfPlayers[1]));
            }
            else if (unit.combatToUpdateGame != null)
            {
                this.listofCurrentCombats.Add(unit.combatToUpdateGame);
                unit.combatToUpdateGame = null;
            }
        });

        //Update All of the player's settlements, in case enemy units have died on something being taken over
        foreach (Player player in listOfPlayers)
        {
            player.settlements.ForEach(settle =>
            {
                settle.UpdateBuildingList(Array.IndexOf(listOfPlayers, player));
            });

        }
    }

	private IEnumerator DestroyUnitAfterAnimation(MilitaryUnit unit, Player playerUnitBelongsTo) 
	{
		Debug.Log ("destroying reference to unit");
		playerUnitBelongsTo.milUnits.Remove (unit);
		(Grid[unit.TilePoint] as UnitCell).RemoveUnit ();

		yield return new WaitForSeconds(unit.GetUnitAnimationTime());
				
		if (unit.UnitType.Equals (MilitaryUnitType.Dictator))
			listOfPlayers[1].DictatorAlive = false;
				
		Destroy (unit.gameObject);
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

    public void CreateNewMilitaryUnit(Player player, int unitType, UnitCell gridCell, PointyHexPoint gridPoint)
    {
        CreateNewMilitaryUnit(player, unitType, gridCell, gridPoint, 1);
    }

    public void CreateNewMilitaryUnit(Player player, int unitType, UnitCell gridCell, PointyHexPoint gridPoint, int amount)
    {
        MilitaryUnit newUnit = Instantiate(unitTypes[unitType], gridCell.transform.position, Quaternion.identity).GetComponent<MilitaryUnit>();
        newUnit.Initialize(player.PlayerColor, (MilitaryUnitType)unitType, gridPoint, amount);
        player.milUnits.Add(newUnit);
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
    }

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
}

public enum GameState
{
    None,
    ComeFromLoad,
    InitState,
    PlayerSetupState,
    RegGameState,
    EndGameState,
}
