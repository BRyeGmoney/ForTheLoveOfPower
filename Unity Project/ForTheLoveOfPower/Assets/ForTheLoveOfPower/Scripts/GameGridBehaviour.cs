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

    PointyHexPoint startPoint;
	UnitCell startCell;
	UnitCell clickedCell;
	UnitCell prevClickedCell;
	PointyHexPoint clickedPoint;
	PointyHexPoint prevClickedPoint;
	PointyHexPoint endPoint;
	MilitaryUnit unitToMove;
	Touch prevTouch;
	PointList<PointyHexPoint> path;
	PointList<PointyHexPoint> prevPath;

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
	bool startChosen = false;

	private List<Combat> listofCurrentCombats;
    private GameState curGameState;

    private bool tappingInput;
    private bool holdingInput;
    private bool draggingInput;
    private float holdTimer;

    private VectorLine usableLine;
    private bool drawLine;
    Tween myTween;
    //private VectorLine[] usableLines;
    //private ushort curVectorLine;

    // Use this for initialization
    void Start () {
		listofCurrentCombats = new List<Combat> ();
        prevPath = new PointList<PointyHexPoint>();
        path = new PointList<PointyHexPoint>();

        buildScreenSettings = buildScreen.GetComponent<BuildMenuBehaviour>();
        baseFloorColor = GridBuilder.Colors[0];

        curGameState = GameState.InitState;
        instance = this;
    }

    private void InitializePlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("PlayerType");
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

                    if (listOfPlayers[c].isLocalPlayer)
                        localPlayer = c;
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

            usableLine = new VectorLine("movementPath", new List<Vector3>(), lineTex, 5.0f, LineType.Continuous, Joins.Weld);
            usableLine.color = listOfPlayers[localPlayer].PlayerColor;

            //usableLines = new VectorLine[5] { setupVL, setupVL, setupVL, setupVL, setupVL };
            //myTween = DOTween.To(() => usableLine.lineWidth, x => usableLine.lineWidth = x, 15f, 0.5f);
            //myTween.easeOvershootOrAmplitude = 2f;


            moneyText.color = listOfPlayers[localPlayer].PlayerColor;
            curGameState = GameState.PlayerSetupState;

            Debug.Log("About to play anim");
            Animator anim = PlaceDictText.GetComponent<Animator>();
            Debug.Log(anim);
            anim.SetTrigger("PlaceDictTrigger");
            //PlaceDictText.GetComponent<Animator>().SetTrigger("PlaceDictTrigger");
            Debug.Log("Played Anim");
        }
        
    }

    void CheckTouchInputPreGame()
    {
        if (Input.touchCount == 1)
        {
            Touch curTouch = Input.GetTouch(0);
            PointyHexPoint clickedPoint = Map[GridBuilderUtils.ScreenToWorld(curTouch.position)];
            UnitCell clickedCell;

            if (clickedPoint != new PointyHexPoint())
            {
                clickedCell = Grid[clickedPoint] as UnitCell;

                CheckTypeOfTouchInput(curTouch);

                if (tappingInput)
                { //single press 
                    TouchPressTile(clickedCell, clickedPoint);
                }
            }
        }
    }

    private void CheckTypeOfTouchInput(Touch curTouch)
    {
        if (curTouch.phase.Equals(TouchPhase.Began))
        {
            Debug.Log("Beginning");
            holdingInput = false;
            draggingInput = false;
            tappingInput = true;
        }
        else if (curTouch.phase.Equals(TouchPhase.Stationary) && holdTimer < 0.15f && !holdingInput)
        {
            Debug.Log("Tracking hold");
            tappingInput = false;
            holdTimer += Time.deltaTime;
        }
        else if (holdTimer > 0.15f && curTouch.phase.Equals(TouchPhase.Stationary))
        {
            Debug.Log("We're now holding");
            holdTimer = 0f;
            holdingInput = true;
        }
        else if (curTouch.phase.Equals(TouchPhase.Moved))
        {
            Debug.Log("Now dragging");
            holdTimer = 0f;
            holdingInput = false;
            tappingInput = false;
            draggingInput = true;
        }
        else if (!holdingInput && !draggingInput && tappingInput && curTouch.phase.Equals(TouchPhase.Ended)) //authentication for the tap
        {
            Debug.Log("We only tapped them shits");
            holdTimer = 0f;
            tappingInput = true;
            draggingInput = false;
            holdingInput = false;
        }
    }

    /// <summary>
    /// Checks the touch input.
    /// </summary>
    private void CheckTouchInput()
	{
		if (Input.touchCount == 1) {
			Touch curTouch = Input.GetTouch (0);
			PointyHexPoint clickedPoint = Map[GridBuilderUtils.ScreenToWorld (curTouch.position)];
			UnitCell clickedCell;

			if (clickedPoint != new PointyHexPoint()) {
				clickedCell = Grid[clickedPoint] as UnitCell;

                //Lets flag what the user is trying to do here
                CheckTypeOfTouchInput(curTouch);

                //Lets actually react to what the user is trying to do here
                if (tappingInput) {//single press 
                    TouchPressTile(clickedCell, clickedPoint);
                    tappingInput = false;
                } else if (holdingInput) {
                    HoldTile(clickedCell, clickedPoint);
                    holdingInput = false;
                } else if (startChosen && draggingInput) {
                    clickedPoint = Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];

                    if (clickedPoint != prevClickedPoint && clickedPoint != startPoint)
                    {
                        endPoint = clickedPoint;
                        clickedCell = Grid[clickedPoint] as UnitCell;

                        if (path.Count > 0)
                            prevPath = path;

                        path = GetGridPath().ToPointList();

                        foreach (PointyHexPoint point in path)
                        {
                            if (prevPath != null && prevPath.Contains(point))
                                prevPath.Remove(point);

                            //(Grid[point] as UnitCell).SetTileColorPath(listOfPlayers[localPlayer].PlayerColor);
                        }

                        usableLine.points3.Clear();
                        for (int i = 0; i < path.Count; i++)
                            usableLine.points3.Add((Grid[path[i]] as UnitCell).transform.position);

                        BlowupLine();

                        /*if (prevPath.Count > 0)
                        {
                            foreach (PointyHexPoint point in prevPath)
                                (Grid[point] as UnitCell).SetTileColorUnPath();
                        }*/
                    }
                } else if (!startChosen && draggingInput) { //we'll try to see if there was a unit to move in the prevClicked
                    prevClickedPoint = Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
                    prevClickedCell = Grid[clickedPoint] as UnitCell;

                    usableLine.active = true;

                    //if the initial click succesfully targets a unit, lets set them
                    if ((prevClickedCell.unitOnTile))
                    {
                        if (!startChosen)
                        {
                            if (listOfPlayers[localPlayer].milUnits.Exists(unit => unit.TilePoint.Equals(prevClickedPoint)))
                            {//quick check that someone is actually here
                                startPoint = clickedPoint;
                                unitToMove = listOfPlayers[localPlayer].milUnits.Find(unit => unit.TilePoint.Equals(startPoint));
                                unitToMove.ClearMovementPath();
                                startChosen = true;

                                if (path.Count > 0)
                                    ClearPath();
                            }
                        }
                    }
                }

                prevClickedPoint = clickedPoint;
                prevClickedCell = clickedCell;
            }
		} else if (Input.touchCount < 1 && startChosen) {//that means we've decided the final point for the unit
            if (path.Count > 0)
            {
                Debug.Log("Clearing Path, moving unit");
                //AnimateTileSmall(path[path.Count - 1]);
                unitToMove.SetMovementPath(path);
                unitToMove.ChangeSpriteDirection(Grid[path[1]] as UnitCell);
                //path.Clear ();
                prevPath.Clear();
            }

            timer = 0f;
            startChosen = false;

            Debug.Log("Done clearing path");
        }
	}

	/// <summary>
	/// Checks the mouse input.
	/// </summary>
	void CheckMouseInput()
	{
		//We're checking for right mouse input, the first frame of it being down
		if (Input.GetMouseButtonDown (2)) {
            //drawLine = true;
            usableLine.active = true;
            
			clickedPoint = Map [GridBuilderUtils.ScreenToWorld (Input.mousePosition)];
			clickedCell = Grid [clickedPoint] as UnitCell;

			//if the initial click succesfully targets a unit, lets set them
			if ((clickedCell.unitOnTile)) {
				if (!startChosen) {
					if (listOfPlayers [localPlayer].milUnits.Exists (unit => unit.TilePoint.Equals (clickedPoint))) {//quick check that someone is actually here
						startPoint = clickedPoint;
						unitToMove = listOfPlayers [localPlayer].milUnits.Find (unit => unit.TilePoint.Equals (startPoint));
						unitToMove.ClearMovementPath ();
						startChosen = true;

						if (path.Count > 0) 
							ClearPath ();
					}
				}
			}

			prevClickedPoint = clickedPoint;
			prevClickedCell = clickedCell;
		} else if (Input.GetMouseButton (2) && startChosen) { //we're polling for if the right mouse is held down, and if the start tile has been chosen
			clickedPoint = Map [GridBuilderUtils.ScreenToWorld (Input.mousePosition)];

			if (clickedPoint != prevClickedPoint && clickedPoint != startPoint) {
				endPoint = clickedPoint;
				//AnimateTileBig (clickedPoint);
				//AnimateTileSmall (prevClickedPoint);
				clickedCell = Grid [clickedPoint] as UnitCell;

				if (path.Count > 0)
					prevPath = path;

				path = GetGridPath ().ToPointList();

                

                foreach (PointyHexPoint point in path) {
					if (prevPath != null && prevPath.Contains (point))
						prevPath.Remove(point);

                    
					//(Grid [point] as UnitCell).SetTileColorPath (listOfPlayers[localPlayer].PlayerColor);
				}

                usableLine.points3.Clear();
                for (int i = 0; i < path.Count; i++)
                    usableLine.points3.Add((Grid[path[i]] as UnitCell).transform.position);

                BlowupLine();

                prevClickedPoint = clickedPoint;
                prevClickedCell = clickedCell;
                /*if (prevPath.Count > 0) {
					foreach (PointyHexPoint point in prevPath) 
						(Grid[point] as UnitCell).SetTileColorUnPath ();
				}*/
            }
		} else if (startChosen) {
			if (path.Count > 0) {
				unitToMove.SetMovementPath (path);
				unitToMove.ChangeSpriteDirection (Grid [path [1]] as UnitCell);
				//path.Clear ();
				prevPath.Clear ();
			}

			timer = 0f;
			startChosen = false;
		}
		
		if (Input.GetMouseButtonDown (0)) {
			clickedPoint = Map[GridBuilderUtils.ScreenToWorld (Input.mousePosition)];
			clickedCell = Grid[clickedPoint] as UnitCell;

			MousePressTile (clickedCell, clickedPoint);

			prevClickedPoint = clickedPoint;
			prevClickedCell = clickedCell;
		}
	}

    private void BlowupLine()
    {
        DOTween.To(x => usableLine.lineWidth = x, 1f, 9f, 0.6f).SetEase(Ease.OutElastic);
    }

    void CheckMouseInputPreGame()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickedPoint = Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
            clickedCell = Grid[clickedPoint] as UnitCell;

            MousePressTile(clickedCell, clickedPoint);
        }
    }

	void ClearPath()
	{
        //foreach (PointyHexPoint point in path)
        //	(Grid [point] as UnitCell).SetTileColorUnPath ();
        DOTween.To(() => usableLine.color, x => usableLine.color = x, new Color(1, 1, 1, 0), 0.4f).SetOptions(true).OnComplete(FadeTweenComplete);

        path.Clear ();
	}

    private void FadeTweenComplete()
    {
        usableLine.points3.Clear();
        usableLine.active = false;
        usableLine.color = new Color(usableLine.color.r, usableLine.color.g, usableLine.color.b, 1f);
    }

	// Update is called once per frame
	void Update () {
        if (curGameState.Equals(GameState.RegGameState))
            UpdatePlayGameState();
        else if (curGameState.Equals(GameState.InitState))
            InitializePlayers();
        else if (curGameState.Equals(GameState.PlayerSetupState))
            PlayerSetupState();
        else if (curGameState.Equals(GameState.EndGameState))
            EndGameState();

	}

    private void PlayerSetupState()
    {
        if (listOfPlayers[localPlayer].milUnits.Count < 1)
        {
            if (Input.touchCount > 0)
                CheckTouchInputPreGame();
            else if (!Input.touchSupported)
                CheckMouseInputPreGame();
        }
        
        //Create ai player's beginnings
        if (MenuBehaviour.instance == null || !MenuBehaviour.instance.isMPGame)
        {
            if (listOfPlayers[localPlayer].milUnits.Count > 0)
            {
                CreateNewMilitaryUnit(listOfPlayers[1], (int)MilitaryUnitType.Dictator, Grid[new PointyHexPoint(1, 13)] as UnitCell, new PointyHexPoint(1, 13));
                CreateNewSettlement(listOfPlayers[1], Grid[new PointyHexPoint(2, 13)] as UnitCell, new PointyHexPoint(2, 13), GetSurroundingTiles(new PointyHexPoint(2, 13)));
                CreateNewStructure(listOfPlayers[1], (int)StructureUnitType.Market, Grid[new PointyHexPoint(2, 12)] as UnitCell, new PointyHexPoint(2, 12), GetSurroundingTiles(new PointyHexPoint(2, 12)), listOfPlayers[1].settlements[0]);
                CreateNewStructure(listOfPlayers[1], (int)StructureUnitType.Airport, Grid[new PointyHexPoint(3, 12)] as UnitCell, new PointyHexPoint(3, 12), GetSurroundingTiles(new PointyHexPoint(3, 12)), listOfPlayers[1].settlements[0]);
                CreateNewStructure(listOfPlayers[1], (int)StructureUnitType.Factory, Grid[new PointyHexPoint(1, 13)] as UnitCell, new PointyHexPoint(1, 13), GetSurroundingTiles(new PointyHexPoint(1, 13)), listOfPlayers[1].settlements[0]);
                CreateNewMilitaryUnit(listOfPlayers[1], (int)MilitaryUnitType.Infantry, Grid[new PointyHexPoint(3, 13)] as UnitCell, new PointyHexPoint(3, 13), 5);

                curGameState = GameState.RegGameState;
            }
        }
        else
        {
            if (listOfPlayers[0].milUnits.Count > 0 && listOfPlayers[1].milUnits.Count > 0)
                curGameState = GameState.RegGameState;
        }

        
    }

    private void UpdatePlayGameState()
    {
        CheckEndGame();

        if (!buildScreenSettings.isActiveAndEnabled)
        {
            if (Input.touchSupported)
                CheckTouchInput();
            else
                CheckMouseInput();
        }

        if (timer > 1f && !startChosen)
        {
            if (path.Count > 0)
                ClearPath();
        }

        if (!MenuBehaviour.instance.isMPGame)
            UpdateSPGame();
        else
            UpdateMPGame();

        //if (drawLine)
            usableLine.Draw3D();

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

    private void UpdateMPGame()
    {

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

	private void MousePressTile(UnitCell clickedCell, PointyHexPoint clickedPoint)
	{
		//if there's a building on this tile
		if (clickedCell.buildingOnTile) {
			//if it is of the military variety
			if (clickedCell.structureOnTile.tag.Equals ("Military")) {
				//if there's already a unit on this tile, we should just add to it
				if (clickedCell.unitOnTile) {
					MilitaryUnit unitOnTile = listOfPlayers[0].milUnits.Find (unit => unit.TilePoint.Equals (clickedPoint));
					
					if (unitOnTile != null)
						unitOnTile.AddUnits (1);
				} else if (clickedCell.structureOnTile.StructureType.Equals (StructureUnitType.Barracks)) {
					CreateNewMilitaryUnit (listOfPlayers[0], (int)MilitaryUnitType.Infantry, clickedCell, clickedPoint);
				} else if (clickedCell.structureOnTile.StructureType.Equals (StructureUnitType.TankDepot)) {
					CreateNewMilitaryUnit (listOfPlayers[0], (int)MilitaryUnitType.Tank, clickedCell, clickedPoint);
				} else {
					CreateNewMilitaryUnit (listOfPlayers[0], (int)MilitaryUnitType.Jet, clickedCell, clickedPoint);
				}
			}
		} else if (clickedCell.Color == listOfPlayers[0].PlayerColor && (!clickedCell.unitOnTile || listOfPlayers[0].TileBelongsToSettlements (clickedPoint))) { //if its the same color and there isn't a unit or it is part of a settlement
			SetupBuildMenu (false);
		} else {
			//if the player has no units, then this is how we let them place the dictator
			if (listOfPlayers[0].milUnits.IsEmpty()) {
				CreateNewMilitaryUnit (listOfPlayers[0], (int)MilitaryUnitType.Dictator, clickedCell, clickedPoint);
			} else if (clickedCell.Color == listOfPlayers[localPlayer].PlayerColor && !listOfPlayers[localPlayer].TileBelongsToSettlements(clickedPoint)) { //else, bring up the bulding selection screen
				SetupBuildMenu (true);
			}
		}
	}

    private void TouchPressTile(UnitCell clickedCell, PointyHexPoint clickedPoint)
    {
        //if there's a building on this tile
        if (clickedCell.buildingOnTile)
        {
            //if it is of the military variety
            if (clickedCell.structureOnTile.tag.Equals("Military"))
            {
                //if there's already a unit on this tile, we should just add to it
                if (clickedCell.unitOnTile)
                {
                    MilitaryUnit unitOnTile = listOfPlayers[0].milUnits.Find(unit => unit.TilePoint.Equals(clickedPoint));

                    if (unitOnTile != null)
                        unitOnTile.AddUnits(1);
                }
                else if (clickedCell.structureOnTile.StructureType.Equals(StructureUnitType.Barracks)) {
                    CreateNewMilitaryUnit(listOfPlayers[0], (int)MilitaryUnitType.Infantry, clickedCell, clickedPoint);
                } else if (clickedCell.structureOnTile.StructureType.Equals(StructureUnitType.TankDepot)) {
                    CreateNewMilitaryUnit(listOfPlayers[0], (int)MilitaryUnitType.Tank, clickedCell, clickedPoint);
                } else {
                    CreateNewMilitaryUnit(listOfPlayers[0], (int)MilitaryUnitType.Jet, clickedCell, clickedPoint);
                }
            }
        }
        else
        {
            //if the player has no units, then this is how we let them place the dictator
            if (listOfPlayers[0].milUnits.IsEmpty())
                CreateNewMilitaryUnit(listOfPlayers[0], (int)MilitaryUnitType.Dictator, clickedCell, clickedPoint);

        }
    }

    private void HoldTile(UnitCell clickedCell, PointyHexPoint clickedPoint)
    {
        if (!clickedCell.structureOnTile)
        {
            if (clickedCell.Color == listOfPlayers[localPlayer].PlayerColor && (!clickedCell.unitOnTile || listOfPlayers[localPlayer].TileBelongsToSettlements(clickedPoint)))
            { //if its the same color and there isn't a unit or it is part of a settlement
                SetupBuildMenu(false);
            } else if (clickedCell.Color == listOfPlayers[localPlayer].PlayerColor && !listOfPlayers[localPlayer].TileBelongsToSettlements(clickedPoint)) {
                SetupBuildMenu(true);
            }
        }
    }

	private void SetupBuildMenu(bool isSettlementMenu)
	{
		buildScreenSettings.structChosen += HandlestructChosen;
		//buildScreenSettings.SetBGColor(listOfPlayers[0].PlayerColor);
		buildScreenSettings.DoSettlementMenu(isSettlementMenu);
		buildScreen.SetActive (true);
	}

	void HandlestructChosen (object sender, BuildingChosenArgs e)
	{

        if (!e.toBuild.Equals(StructureUnitType.None))
        {
            int cost = StructureUnit.CostOfStructure(e.toBuild);

            if (cost < listOfPlayers[localPlayer].Cash)
            {
                listOfPlayers[localPlayer].Cash -= cost;

                PointList<PointyHexPoint> surroundingTiles = GetSurroundingTiles(prevClickedPoint);

                if (e.IsSettlement)
                {
                    if (!IsBuildingInArea(prevClickedPoint, surroundingTiles, listOfPlayers[0].PlayerColor))
                        CreateNewSettlement(listOfPlayers[localPlayer], prevClickedCell, prevClickedPoint, surroundingTiles);
                }
                else
                {
                    Settlement owningSettlement = FindOwningSettlement(prevClickedPoint, surroundingTiles, listOfPlayers[0].PlayerColor);

                    if (owningSettlement != null)
                    {
                        CreateNewStructure(listOfPlayers[localPlayer], (int)e.toBuild, prevClickedCell, prevClickedPoint, surroundingTiles, owningSettlement);
                    }
                }
            }
            else
            {
                //Play the animation
                NotEnoughMoneyText.GetComponent<Animator>().SetTrigger("PlayerIsBroke");
            }
        }

		//me.enabled = true;
		buildScreenSettings.structChosen -= HandlestructChosen;
		buildScreen.SetActive (false);
		prevClickedCell.HighlightOn = false;
	}

	void CreateNewMilitaryUnit(Player player, int unitType, UnitCell gridCell, PointyHexPoint gridPoint)
	{
        CreateNewMilitaryUnit(player, unitType, gridCell, gridPoint, 1);
	}

    void CreateNewMilitaryUnit(Player player, int unitType, UnitCell gridCell, PointyHexPoint gridPoint, int amount)
    {
        MilitaryUnit newUnit = Instantiate(unitTypes[unitType], gridCell.transform.position, Quaternion.identity).GetComponent<MilitaryUnit>();
        newUnit.Initialize(player.PlayerColor, (MilitaryUnitType)unitType, gridPoint, amount);
        player.milUnits.Add(newUnit);
        gridCell.AddUnitToTile(newUnit);
    }

    void CreateNewSettlement(Player player, UnitCell gridCell, PointyHexPoint gridPoint, PointList<PointyHexPoint> surroundingTiles )
	{
		Settlement newSettlement = Instantiate (structureTypes [(int)StructureUnitType.Settlement], gridCell.transform.position, Quaternion.identity).GetComponent<Settlement> ();
		newSettlement.Initialize (player.PlayerColor, StructureUnitType.Settlement, gridPoint);
		player.settlements.Add (newSettlement);
		player.AddToOwnedTiles (Grid, newSettlement, listOfPlayers[1], surroundingTiles);
		gridCell.AddStructureToTile (newSettlement);
	}

	void CreateNewStructure(Player player, int structType, UnitCell gridCell, PointyHexPoint gridPoint, PointList<PointyHexPoint> surroundingTiles, Settlement owningSettlement)
	{
		StructureUnit newStructure = Instantiate (structureTypes [structType], gridCell.transform.position, Quaternion.identity).GetComponent<StructureUnit> ();
		newStructure.Initialize (player.PlayerColor, (StructureUnitType)structType, gridPoint, owningSettlement);
        //owningSettlement.cachedBuildingList.Add (newStructure);
        owningSettlement.AddToBuildingList(newStructure);
        owningSettlement.newBuildingAdded = true;
		player.AddToOwnedTiles (Grid, owningSettlement, listOfPlayers[1], surroundingTiles);

		gridCell.AddStructureToTile (newStructure);
    }

	/// <summary>
	/// We need to know which settlement a building belongs to. and since buildings HAVE to be connected by
	/// atleast a single other building, and from inception be told what their owning settlement is, we can
	/// pull the data from any neighbor of the same colour and not just a settlement
	/// </summary>
	/// <param name="pointToSearchFrom">Point to search from.</param>
	/// <param name="owningPlayerColor">Owning player color.</param>
	private Settlement FindOwningSettlement(PointyHexPoint pointToSearchFrom, PointList<PointyHexPoint> surroundingTiles, Color owningPlayerColor)
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

	private bool IsBuildingInArea(PointyHexPoint pointToSearchFrom, PointList<PointyHexPoint> surroundingTiles, Color owningPlayerColor)
	{
		foreach (PointyHexPoint point in surroundingTiles) {
			UnitCell currCell = (Grid[point] as UnitCell);

			if (currCell.buildingOnTile)
				return true;
		}

		return false;
	}

	public IEnumerable<PointyHexPoint> GetGridPath()
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

    /*private ushort HijackNextVectorLine()
    {
        if (curVectorLine > 4)
            curVectorLine = 0;
        else
            curVectorLine++;

        return curVectorLine;
    }*/
}

public enum GameState
{
    InitState,
    PlayerSetupState,
    RegGameState,
    EndGameState,
}
