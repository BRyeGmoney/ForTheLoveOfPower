using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grids;
using AssemblyCSharp;
using Photon;
using DG.Tweening;
using Vectrosity;

public class Player : Photon.MonoBehaviour {

    #region Properties
    public Int32 Cash { get { return cash; } }
    private int cash;

    public Boolean DictatorAlive { get { return dictAlive; } }
    public bool dictAlive;

    public Army playerArmy;
	List<Settlement> settlements;
	List<PointyHexPoint> ownedTiles;

    PointyHexPoint startPoint;
    PointyHexPoint endPoint;
    UnitCell startCell;
    UnitCell clickedCell;
    UnitCell prevClickedCell;
    PointyHexPoint clickedPoint;
    PointyHexPoint prevClickedPoint;
    MilitaryUnit unitToMove;
    Touch prevTouch;
    PointList<PointyHexPoint> path;
    PointList<PointyHexPoint> prevPath;

    bool tappingInput;
    bool holdingInput;
    bool draggingInput;
    float holdTimer;
    bool startChosen = false;

    VectorLine movementLine;
    bool drawLine;
    Tween myTween;

    
    short NextStructID;
    short NextSettleID;

    float playerTimer = 0f;

    public Color PlayerColor = new Color(0, 255, 255);

    #endregion

    #region Init
    // Use this for initialization
    void Start () {
		InitBasePlayer ();
        AddCash(10000);
        PlayerColor = PlayerPrefsX.GetColor(SaveData.PlayerColor.ToString(), PlayerColor);

        path = new PointList<PointyHexPoint>();
        prevPath = new PointList<PointyHexPoint>();

        movementLine = new VectorLine("movementPath", new List<Vector3>(), GameGridBehaviour.instance.lineTex, 5.0f, LineType.Continuous, Joins.Weld);
        movementLine.color = PlayerColor;
        
    }
	
	protected void InitBasePlayer()
	{
        playerArmy = gameObject.AddComponent<Army>();
		settlements = new List<Settlement> ();
        
        dictAlive = true;
	}
    #endregion

    #region Tile Functions
    public PointList<PointyHexPoint> GetOwnedTiles()
	{
		PointList<PointyHexPoint> currList = new PointList<PointyHexPoint> ();

		settlements.ForEach (settlement => {
			currList = currList.Union (settlement.tilesOwned).ToPointList<PointyHexPoint>();
		});

		return currList;
	}

	public bool TileBelongsToSettlements(PointyHexPoint tileToCheck)
	{
		bool belongs = false;

		settlements.ForEach (settlement => {
			if (!belongs)
				belongs = settlement.TileBelongsToSettlement (tileToCheck);
		});

		return belongs;
	}

	public void AddToSettlementOwnedTiles(IGrid<PointyHexPoint> gameGrid, AssemblyCSharp.Settlement ownSettlement, Player enemyPlayer, PointList<PointyHexPoint> pointsToAdd)
	{
		foreach (PointyHexPoint point in TileDoesNotBelongToOtherSettlement(gameGrid, ownSettlement, enemyPlayer, pointsToAdd)) {
			if (!ownSettlement.tilesOwned.Contains (point)) {
				ownSettlement.tilesOwned.Add (point);
				//ownedTiles.Add (point);
				(gameGrid [point] as UnitCell).SetTileColorBuildable (PlayerColor);
			}
		}
	}

	public bool AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, PointyHexPoint pointToAdd)
	{
        if (!(gameGrid[pointToAdd] as UnitCell).buildingOnTile)
        {
            ownedTiles.Add(pointToAdd);
            return true;
        }
        else
            return false;
	}

    public bool AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, PointList<PointyHexPoint> pointsToAdd)
    {
        return pointsToAdd.All(point => AddToOwnedTiles(gameGrid, point));
    }

	public bool RemoveFromOwnedTiles(PointyHexPoint pointToRemove)
	{
		return ownedTiles.Remove (pointToRemove);
	}

    public bool RemoveFromOwnedTiles(PointList<PointyHexPoint> pointsToRemove)
    {
        return pointsToRemove.All(point => RemoveFromOwnedTiles(point));
    }

	private PointList<PointyHexPoint> TileDoesNotBelongToOtherSettlement(IGrid<PointyHexPoint> gameGrid, AssemblyCSharp.Settlement ownSettlement, Player enemyPlayer, PointList<PointyHexPoint> pointsToAdd) 
	{
		PointList<PointyHexPoint> pointsToRemove = new PointList<PointyHexPoint>();

		//lets cut out as many points as possible for the next loop to be faster
		pointsToAdd.RemoveAll (point => enemyPlayer.GetOwnedTiles ().Contains (point));
		pointsToRemove.Clear ();

		//make sure two neighboring cities don't collide
		settlements.ForEach (settlement => {
			if (!settlement.Equals (ownSettlement)) { //if we're not currently looking at the same settlement
				foreach (PointyHexPoint point in pointsToAdd) {
                    if (settlement.tilesOwned.Contains(point))
                        pointsToRemove.Add(point);
				}
			}

		});

		pointsToRemove.ToList<PointyHexPoint>().ForEach (point => {
            (gameGrid[point] as UnitCell).SetTileColorUnOwn();
		});

		return pointsToAdd.Except (pointsToRemove).ToPointList<PointyHexPoint>();
	}
    #endregion

    #region Input
    void CheckTouchInputPreGame()
    {
        if (Input.touchCount == 1)
        {
            Touch curTouch = Input.GetTouch(0);

            /*if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;*/

            PointyHexPoint clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(curTouch.position)];
            UnitCell clickedCell;

            if (clickedPoint != new PointyHexPoint())
            {
                clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

                CheckTypeOfTouchInput(curTouch);

                if (tappingInput) //single press 
                    playerArmy.TryToBuildDictator(clickedCell, clickedPoint, this);
            }
        }
    }

    void CheckMouseInputPreGame()
    {
        if (Input.GetMouseButtonDown(0))
        {
            /*if (EventSystem.current.IsPointerOverGameObject())
                return;*/

            clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
            clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

            playerArmy.TryToBuildDictator(clickedCell, clickedPoint, this);
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
        

        if (Input.touchCount == 1)
        {
            Touch curTouch = Input.GetTouch(0);

            /*if (EventSystem.current.IsPointerOverGameObject(curTouch.fingerId))
                return;*/

            PointyHexPoint clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(curTouch.position)];
            UnitCell clickedCell;

            if (clickedPoint != new PointyHexPoint())
            {
                clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

                //Lets flag what the user is trying to do here
                CheckTypeOfTouchInput(curTouch);

                //Lets actually react to what the user is trying to do here
                if (tappingInput)
                {//single press 
                    TouchPressTile(clickedCell, clickedPoint);
                    tappingInput = false;
                }
                else if (holdingInput)
                {
                    HoldTile(clickedCell, clickedPoint);
                    holdingInput = false;
                }
                else if (startChosen && draggingInput)
                {
                    clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];

                    if (clickedPoint != prevClickedPoint && clickedPoint != startPoint)
                    {
                        endPoint = clickedPoint;
                        clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

                        if (path.Count > 0)
                            prevPath = path;

                        path = GameGridBehaviour.instance.GetGridPath(startPoint, endPoint).ToPointList();

                        foreach (PointyHexPoint point in path)
                        {
                            if (prevPath != null && prevPath.Contains(point))
                                prevPath.Remove(point);
                        }

                        movementLine.points3.Clear();
                        for (int i = 0; i < path.Count; i++)
                            movementLine.points3.Add((GameGridBehaviour.instance.Grid[path[i]] as UnitCell).transform.position);

                        BlowupLine();
                    }
                }
                else if (!startChosen && draggingInput)
                { 
                    //we'll try to see if there was a unit to move in the prevClicked
                    prevClickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
                    prevClickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

                    movementLine.active = true;

                    //if the initial click succesfully targets a unit, lets set them
                    if ((prevClickedCell.unitOnTile))
                    {
                        if (!startChosen)
                        {
                            if (playerArmy.army.Exists(unit => unit.TilePoint.Equals(prevClickedPoint)))
                            {//quick check that someone is actually here
                                startPoint = clickedPoint;
                                unitToMove = playerArmy.army.Find(unit => unit.TilePoint.Equals(startPoint));
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
        }
        else if (Input.touchCount < 1 && startChosen)
        {//that means we've decided the final point for the unit
            if (path.Count > 0)
            {
                Debug.Log("Clearing Path, moving unit");
                //AnimateTileSmall(path[path.Count - 1]);
                unitToMove.SetMovementPath(path);
                unitToMove.ChangeSpriteDirection(GameGridBehaviour.instance.Grid[path[1]] as UnitCell);
                //path.Clear ();
                prevPath.Clear();
            }

            playerTimer = 0f;
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
        if (Input.GetMouseButtonDown(2))
        {

            Debug.Log("We right clicked");
            //drawLine = true;
            movementLine.active = true;

            clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
            clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

            //if the initial click succesfully targets a unit, lets set them
            if ((clickedCell.unitOnTile))
            {
                if (!startChosen)
                {
                    if (playerArmy.army.Exists(unit => unit.TilePoint.Equals(clickedPoint)))
                    {//quick check that someone is actually here
                        startPoint = clickedPoint;
                        unitToMove = playerArmy.army.Find(unit => unit.TilePoint.Equals(startPoint));
                        unitToMove.ClearMovementPath();
                        startChosen = true;

                        if (path.Count > 0)
                            ClearPath();
                    }
                }
            }

            prevClickedPoint = clickedPoint;
            prevClickedCell = clickedCell;
        }
        else if (Input.GetMouseButton(2) && startChosen)
        {
            Debug.Log("We right clicked");
            //we're polling for if the right mouse is held down, and if the start tile has been chosen
            clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];

            if (clickedPoint != prevClickedPoint && clickedPoint != startPoint)
            {
                endPoint = clickedPoint;
                clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

                if (path.Count > 0)
                    prevPath = path;

                path = GameGridBehaviour.instance.GetGridPath(startPoint, endPoint).ToPointList();



                foreach (PointyHexPoint point in path)
                {
                    if (prevPath != null && prevPath.Contains(point))
                        prevPath.Remove(point);
                }

                movementLine.points3.Clear();
                for (int i = 0; i < path.Count; i++)
                    movementLine.points3.Add((GameGridBehaviour.instance.Grid[path[i]] as UnitCell).transform.position);

                BlowupLine();

                prevClickedPoint = clickedPoint;
                prevClickedCell = clickedCell;
            }
        }
        else if (startChosen)
        {
            if (path.Count > 0)
            {
                unitToMove.SetMovementPath(path);
                unitToMove.ChangeSpriteDirection(GameGridBehaviour.instance.Grid[path[1]] as UnitCell);
                prevPath.Clear();
            }

            playerTimer = 0f;
            startChosen = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("We Left Clicked");

            /*if (EventSystem.current.IsPointerOverGameObject())
                return;*/

            clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
            clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

            MousePressTile(clickedCell, clickedPoint);

            prevClickedPoint = clickedPoint;
            prevClickedCell = clickedCell;
        }
    }

    private void MousePressTile(UnitCell clickedCell, PointyHexPoint clickedPoint)
    {
        //if there's a building on this tile
        if (clickedCell.buildingOnTile)
            playerArmy.TryToBuildUnit(clickedCell, clickedPoint, this);
        else if (clickedCell.Color == this.PlayerColor && (!clickedCell.unitOnTile || this.TileBelongsToSettlements(clickedPoint)))//if its the same color and there isn't a unit or it is part of a settlement
            GameGridBehaviour.instance.SetupBuildMenu(false, HandlestructChosen);
        else
        {
            if (clickedCell.Color == PlayerColor && !this.TileBelongsToSettlements(clickedPoint))
                GameGridBehaviour.instance.SetupBuildMenu(true, HandlestructChosen);
        }
    }

    private void TouchPressTile(UnitCell clickedCell, PointyHexPoint clickedPoint)
    {
        //if there's a building on this tile
        if (clickedCell.buildingOnTile)
            playerArmy.TryToBuildUnit(clickedCell, clickedPoint, this);
    }

    private void HoldTile(UnitCell clickedCell, PointyHexPoint clickedPoint)
    {
        if (!clickedCell.structureOnTile)
        {
            if (clickedCell.Color == this.PlayerColor && (!clickedCell.unitOnTile || this.TileBelongsToSettlements(clickedPoint)))
            { //if its the same color and there isn't a unit or it is part of a settlement
                GameGridBehaviour.instance.SetupBuildMenu(false, HandlestructChosen);
            }
            else if (clickedCell.Color == this.PlayerColor && !this.TileBelongsToSettlements(clickedPoint))
            {
                GameGridBehaviour.instance.SetupBuildMenu(true, HandlestructChosen);
            }
        }
    }

    #endregion

    #region Building Creation

    protected void BuildNewSettlement(PointyHexPoint buildPoint)
    {
        /*GameGridBehaviour.instance.CreateNewSettlement(this,
            GameGridBehaviour.instance.Grid[buildPoint] as UnitCell,
            buildPoint,
            GameGridBehaviour.instance.GetSurroundingTiles(buildPoint));*/

        UnitCell gridCell = GameGridBehaviour.instance.Grid[buildPoint] as UnitCell;
        Settlement newSettlement = ObjectPool.instance.PullNewSettlement(gridCell.transform.position); //(Instantiate(GameGridBehaviour.instance.structureTypes[(int)StructureUnitType.Settlement], gridCell.transform.position, Quaternion.identity) as GameObject).GetComponent<Settlement>();
        newSettlement.Initialize(GetNextSettleID(), PlayerColor, StructureUnitType.Settlement, buildPoint);
        AddToSettlements(newSettlement);
        AddToSettlementOwnedTiles(GameGridBehaviour.instance.Grid, newSettlement, this, GameGridBehaviour.instance.GetSurroundingTiles(buildPoint));
        gridCell.AddStructureToTile(newSettlement);
        newSettlement.GetComponent<BeautifulDissolves.Dissolve>().TriggerDissolve();
        
    }

    protected void BuildNewStructure(PointyHexPoint buildPoint, StructureUnitType structType, Settlement owningSettlement)
    {
        UnitCell gridCell = GameGridBehaviour.instance.Grid[buildPoint] as UnitCell;
        StructureUnit newStruct = ObjectPool.instance.PullNewStructure(structType, gridCell.transform.position);//(Instantiate(GameGridBehaviour.instance.structureTypes[(int)structType], gridCell.transform.position, Quaternion.identity) as GameObject).GetComponent<StructureUnit>();
        newStruct.Initialize(GetNextStructID(), PlayerColor, structType, buildPoint, owningSettlement);
        owningSettlement.AddToBuildingList(newStruct);
        AddToSettlementOwnedTiles(GameGridBehaviour.instance.Grid, owningSettlement, this, GameGridBehaviour.instance.GetSurroundingTiles(buildPoint));
        gridCell.AddStructureToTile(newStruct);
        newStruct.GetComponent<BeautifulDissolves.Dissolve>().TriggerDissolve();
        /*GameGridBehaviour.instance.CreateNewStructure(this,
            (int)structType,
            GameGridBehaviour.instance.Grid[buildPoint] as UnitCell,
            buildPoint,
            GameGridBehaviour.instance.GetSurroundingTiles(buildPoint),
            owningSettlement);*/
    }

    void HandlestructChosen(object sender, BuildingChosenArgs e)
    {
        if (!e.toBuild.Equals(StructureUnitType.None))
        {
            int cost = StructureUnit.GetCostOfStructure(e.toBuild);

            if (CanSpendCash(cost))
            {
                SpendCash(cost);

                PointList<PointyHexPoint> surroundingTiles = GameGridBehaviour.instance.GetSurroundingTiles(prevClickedPoint);

                if (e.IsSettlement)
                {
                    if (!GameGridBehaviour.instance.IsBuildingInArea(prevClickedPoint, surroundingTiles, this.PlayerColor))
                        BuildNewSettlement(prevClickedPoint);
                }
                else
                {
                    Settlement owningSettlement = GameGridBehaviour.instance.FindOwningSettlement(prevClickedPoint, surroundingTiles, this.PlayerColor);

                    if (owningSettlement != null)
                    {
                        BuildNewStructure(prevClickedPoint, e.toBuild, owningSettlement);
                    }
                }
            }
            else
            {
                //Play the animation
                GameGridBehaviour.instance.NotEnoughMoneyText.GetComponent<Animator>().SetTrigger("PlayerIsBroke");
            }
        }

        GameGridBehaviour.instance.RemoveBuildScreenHandler(HandlestructChosen);
        prevClickedCell.HighlightOn = false;
    }
    
    #endregion

    #region Settlements
    public Int32 GetSettlementsCount()
    {
        return settlements.Count;
    }

    public Settlement FindSettlementByID(short id)
    {
        return settlements.Find(settlement => settlement.ID == id);
    }

    public void AddToSettlements(Settlement newSettlement)
    {
        settlements.Add(newSettlement);
    }

    public IEnumerable<Settlement> SettlementsUnderAttack()
    {
        return settlements.Where(settlement => settlement.cityBeingConquered);
    }

    protected void UpdateSettlements()
    {
        settlements.ForEach(settlement =>
        {
            settlement.UpdateBuildingList(this);
        });
    }

    public void RemoveFromSettlements(Settlement settlement)
    {
        settlements.Remove(settlement);
    }

    #endregion

    #region Update States

    void Update()
    {
        if (GameGridBehaviour.instance.curCameraState == CameraState.Action)
            UpdateBasedOnState();

        UpdateCashText();
        
    }

    void UpdateBasedOnState()
    {
        if (GameGridBehaviour.instance.GetCurrentGameState().Equals(GameState.RegGameState))
        {
            PlayGameState();
        }
        else if (GameGridBehaviour.instance.GetCurrentGameState().Equals(GameState.PlayerSetupState))
        {
            if (playerArmy.army.Count <= 0)
                PlayerSetupState();
        }
    }

    void PlayGameState()
    {
        if ((!GameGridBehaviour.isMP || photonView.isMine) && !GameGridBehaviour.instance.IsBuildScreenBlocking())
        {
            if (Input.touchSupported)
                CheckTouchInput();
            else
                CheckMouseInput();
        }

        playerArmy.UpdateUnits(this);
        UpdateSettlements();

        if (playerTimer > 1f && !startChosen)
        {
            if (path.Count > 0)
                ClearPath();
        }

        //if (drawLine)
        if (movementLine.active)
            movementLine.Draw3D();

        playerTimer += Time.deltaTime;
    }

    void PlayerSetupState()
    {
        if (!GameGridBehaviour.isMP || photonView.isMine)
        {
            if (Input.touchCount > 0)
                CheckTouchInputPreGame();
            else if (!Input.touchSupported)
                CheckMouseInputPreGame();
        }
    }

    #endregion

    #region Networking

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            if (playerArmy != null && playerArmy.dirtyUnits.Count > 0)
            {
                playerArmy.dirtyUnits.Take(5).ToList().ForEach(dirtyUnit =>
                {
                    short mpComm = dirtyUnit.mpCommand;
                    short uid = dirtyUnit.Unit.ID;
                    short type = (short)dirtyUnit.Unit.UnitType;
                    Vector2 pos = new Vector2(dirtyUnit.Unit.TilePoint.X, dirtyUnit.Unit.TilePoint.Y);

                    stream.Serialize(ref mpComm);
                    stream.Serialize(ref uid);
                    stream.Serialize(ref type);
                    stream.Serialize(ref pos);

                    playerArmy.dirtyUnits.Remove(dirtyUnit);
                });
            }
        }
        else
        {
            short mpComm = 0;

            stream.Serialize(ref mpComm);
            
            if (mpComm == (short)MpMilitaryCommands.AddUnit)
            {
                short uid = 0;
                short type = 0;
                Vector2 pos = Vector2.zero;

                stream.Serialize(ref uid);
                stream.Serialize(ref type);
                stream.Serialize(ref pos);

                playerArmy.CreateNewUnit(new PointyHexPoint((int)pos.x, (int)pos.y), (MilitaryUnitType)type, this);
            }
            else if (mpComm == (short)MpMilitaryCommands.MoveUnit)
            {
                short uid = 0;
                short type = 0;
                Vector2 pos = Vector2.zero;

                stream.Serialize(ref uid);
                stream.Serialize(ref type);
                stream.Serialize(ref pos);

                StartCoroutine(SyncUnitPosition(uid, pos));
            }
        }
    }

    public IEnumerator SyncUnitPosition(short uid, Vector2 pos)
    {
        MilitaryUnit unitToMove = playerArmy.FindUnitByID(uid);

        unitToMove.MoveToNext(new PointyHexPoint((int)pos.x, (int)pos.y), unitToMove.TilePoint, true);

        yield break;
    }


    #endregion

    #region MovementLine & Tweens

    void BlowupLine()
    {
        DOTween.To(x => movementLine.lineWidth = x, 1f, 9f, 0.6f).SetEase(Ease.OutElastic);
    }

    void ClearPath()
    {
        DOTween.To(() => movementLine.color, x => movementLine.color = x, new Color(1, 1, 1, 0), 0.4f).SetOptions(true).OnComplete(FadeTweenComplete);

        path.Clear();
    }

    void FadeTweenComplete()
    {
        movementLine.points3.Clear();
        movementLine.active = false;
        movementLine.color = new Color(movementLine.color.r, movementLine.color.g, movementLine.color.b, 1f);
    }

    #endregion

    #region IDs

    
    public Int16 GetNextSettleID()
    {
        return NextSettleID++;
    }
    public Int16 GetNextStructID()
    {
        return NextStructID++;
    }

    #endregion

    #region Cash

    bool CanSpendCash(int amountToSpend)
    {
        return amountToSpend < Cash;
    }

    void SpendCash(int amountToSpend)
    {
        cash -= amountToSpend;
    }

    public void AddCash(int amountToAdd)
    {
        cash += amountToAdd;
    }

    void UpdateCashText()
    {
        if (!GameGridBehaviour.isMP || (GameGridBehaviour.isMP && photonView.isMine))
        {
            //Display the player's money
            GameGridBehaviour.instance.moneyText.text = String.Concat("Money: ", this.Cash);
        }
    }

    #endregion
}
