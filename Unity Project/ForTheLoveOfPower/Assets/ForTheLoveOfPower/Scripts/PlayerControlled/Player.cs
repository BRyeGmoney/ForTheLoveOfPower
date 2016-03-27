using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grids;
using AssemblyCSharp;
using Photon;
using DG.Tweening;
using Vectrosity;

public class Player : Photon.MonoBehaviour {

	public List<MilitaryUnit> milUnits;
	public List<Settlement> settlements;
	public List<PointyHexPoint> ownedTiles;
	public Int32 Cash { get; set; }

    public Color PlayerColor = new Color(0, 255, 255);
    
	public Boolean DictatorAlive { get; set; }

    bool startChosen = false;

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

    private bool tappingInput;
    private bool holdingInput;
    private bool draggingInput;
    private float holdTimer;

    private VectorLine usableLine;
    private bool drawLine;
    Tween myTween;

    float playerTimer = 0f;

    // Use this for initialization
    void Start () {
		InitBasePlayer ();
        Cash = 10000;
        PlayerColor = PlayerPrefsX.GetColor(SaveData.PlayerColor.ToString(), PlayerColor);
        path = new PointList<PointyHexPoint>();

        prevPath = new PointList<PointyHexPoint>();

        usableLine = new VectorLine("movementPath", new List<Vector3>(), GameGridBehaviour.instance.lineTex, 5.0f, LineType.Continuous, Joins.Weld);
        usableLine.color = PlayerColor;
    }
	
	protected void InitBasePlayer()
	{
		milUnits = new List<MilitaryUnit> ();
		settlements = new List<Settlement> ();
        
        DictatorAlive = true;
	}

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

	public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, AssemblyCSharp.Settlement ownSettlement, Player enemyPlayer, PointList<PointyHexPoint> pointsToAdd)
	{
		foreach (PointyHexPoint point in TileDoesNotBelongToOtherSettlement(gameGrid, ownSettlement, enemyPlayer, pointsToAdd)) {
			if (!ownSettlement.tilesOwned.Contains (point)) {
				ownSettlement.tilesOwned.Add (point);
				//ownedTiles.Add (point);
				(gameGrid [point] as UnitCell).SetTileColorStructure (PlayerColor);
			}
		}
	}

	public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, Player enemyPlayer, PointyHexPoint pointToAdd)
	{
		if (!(gameGrid [pointToAdd] as UnitCell).buildingOnTile) {
			ownedTiles.Add (pointToAdd);
		}
	}

	public void RemoveFromOwnedTiles(PointyHexPoint pointToRemove)
	{
		ownedTiles.Remove (pointToRemove);
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
            PointyHexPoint clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(curTouch.position)];
            UnitCell clickedCell;

            if (clickedPoint != new PointyHexPoint())
            {
                clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

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
        if (Input.touchCount == 1)
        {
            Touch curTouch = Input.GetTouch(0);
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

                            //(Grid[point] as UnitCell).SetTileColorPath(listOfPlayers[localPlayer].PlayerColor);
                        }

                        usableLine.points3.Clear();
                        for (int i = 0; i < path.Count; i++)
                            usableLine.points3.Add((GameGridBehaviour.instance.Grid[path[i]] as UnitCell).transform.position);

                        BlowupLine();

                        /*if (prevPath.Count > 0)
                        {
                            foreach (PointyHexPoint point in prevPath)
                                (Grid[point] as UnitCell).SetTileColorUnPath();
                        }*/
                    }
                }
                else if (!startChosen && draggingInput)
                { //we'll try to see if there was a unit to move in the prevClicked
                    prevClickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
                    prevClickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

                    usableLine.active = true;

                    //if the initial click succesfully targets a unit, lets set them
                    if ((prevClickedCell.unitOnTile))
                    {
                        if (!startChosen)
                        {
                            if (this.milUnits.Exists(unit => unit.TilePoint.Equals(prevClickedPoint)))
                            {//quick check that someone is actually here
                                startPoint = clickedPoint;
                                unitToMove = this.milUnits.Find(unit => unit.TilePoint.Equals(startPoint));
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
            usableLine.active = true;

            clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
            clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

            //if the initial click succesfully targets a unit, lets set them
            if ((clickedCell.unitOnTile))
            {
                if (!startChosen)
                {
                    if (this.milUnits.Exists(unit => unit.TilePoint.Equals(clickedPoint)))
                    {//quick check that someone is actually here
                        startPoint = clickedPoint;
                        unitToMove = this.milUnits.Find(unit => unit.TilePoint.Equals(startPoint));
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

                usableLine.points3.Clear();
                for (int i = 0; i < path.Count; i++)
                    usableLine.points3.Add((GameGridBehaviour.instance.Grid[path[i]] as UnitCell).transform.position);

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

            clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
            clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

            MousePressTile(clickedCell, clickedPoint);

            prevClickedPoint = clickedPoint;
            prevClickedCell = clickedCell;
        }
    }

    

    void CheckMouseInputPreGame()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickedPoint = GameGridBehaviour.instance.Map[GridBuilderUtils.ScreenToWorld(Input.mousePosition)];
            clickedCell = GameGridBehaviour.instance.Grid[clickedPoint] as UnitCell;

            MousePressTile(clickedCell, clickedPoint);
        }
    }

    private void MousePressTile(UnitCell clickedCell, PointyHexPoint clickedPoint)
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
                    MilitaryUnit unitOnTile = this.milUnits.Find(unit => unit.TilePoint.Equals(clickedPoint));

                    if (unitOnTile != null)
                        unitOnTile.AddUnits(1);
                }
                else if (clickedCell.structureOnTile.StructureType.Equals(StructureUnitType.Barracks))
                {
                    GameGridBehaviour.instance.CreateNewMilitaryUnit(this, (int)MilitaryUnitType.Infantry, clickedCell, clickedPoint);
                }
                else if (clickedCell.structureOnTile.StructureType.Equals(StructureUnitType.TankDepot))
                {
                    GameGridBehaviour.instance.CreateNewMilitaryUnit(this, (int)MilitaryUnitType.Tank, clickedCell, clickedPoint);
                }
                else
                {
                    GameGridBehaviour.instance.CreateNewMilitaryUnit(this, (int)MilitaryUnitType.Jet, clickedCell, clickedPoint);
                }
            }
        }
        else if (clickedCell.Color == this.PlayerColor && (!clickedCell.unitOnTile || this.TileBelongsToSettlements(clickedPoint)))
        { //if its the same color and there isn't a unit or it is part of a settlement
            GameGridBehaviour.instance.SetupBuildMenu(false, HandlestructChosen);
        }
        else
        {
            //if the player has no units, then this is how we let them place the dictator
            if (this.milUnits.Count <= 0)
            {
                GameGridBehaviour.instance.CreateNewMilitaryUnit(this, (int)MilitaryUnitType.Dictator, clickedCell, clickedPoint);
            }
            else if (clickedCell.Color == PlayerColor && !this.TileBelongsToSettlements(clickedPoint))
            { //else, bring up the bulding selection screen
                GameGridBehaviour.instance.SetupBuildMenu(true, HandlestructChosen);
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
                    MilitaryUnit unitOnTile = this.milUnits.Find(unit => unit.TilePoint.Equals(clickedPoint));

                    if (unitOnTile != null)
                        unitOnTile.AddUnits(1);
                }
                else if (clickedCell.structureOnTile.StructureType.Equals(StructureUnitType.Barracks))
                {
                    GameGridBehaviour.instance.CreateNewMilitaryUnit(this, (int)MilitaryUnitType.Infantry, clickedCell, clickedPoint);
                }
                else if (clickedCell.structureOnTile.StructureType.Equals(StructureUnitType.TankDepot))
                {
                    GameGridBehaviour.instance.CreateNewMilitaryUnit(this, (int)MilitaryUnitType.Tank, clickedCell, clickedPoint);
                }
                else
                {
                    GameGridBehaviour.instance.CreateNewMilitaryUnit(this, (int)MilitaryUnitType.Jet, clickedCell, clickedPoint);
                }
            }
        }
        else
        {
            //if the player has no units, then this is how we let them place the dictator
            if (this.milUnits.Count <= 0)
                GameGridBehaviour.instance.CreateNewMilitaryUnit(this, (int)MilitaryUnitType.Dictator, clickedCell, clickedPoint);

        }
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

    void HandlestructChosen(object sender, BuildingChosenArgs e)
    {
        if (!e.toBuild.Equals(StructureUnitType.None))
        {
            int cost = StructureUnit.CostOfStructure(e.toBuild);

            if (cost < this.Cash)
            {
                this.Cash -= cost;

                PointList<PointyHexPoint> surroundingTiles = GameGridBehaviour.instance.GetSurroundingTiles(prevClickedPoint);

                if (e.IsSettlement)
                {
                    if (!GameGridBehaviour.instance.IsBuildingInArea(prevClickedPoint, surroundingTiles, this.PlayerColor))
                        GameGridBehaviour.instance.CreateNewSettlement(this, prevClickedCell, prevClickedPoint, surroundingTiles);
                }
                else
                {
                    Settlement owningSettlement = GameGridBehaviour.instance.FindOwningSettlement(prevClickedPoint, surroundingTiles, this.PlayerColor);

                    if (owningSettlement != null)
                    {
                        GameGridBehaviour.instance.CreateNewStructure(this, (int)e.toBuild, prevClickedCell, prevClickedPoint, surroundingTiles, owningSettlement);
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

    private void BlowupLine()
    {
        DOTween.To(x => usableLine.lineWidth = x, 1f, 9f, 0.6f).SetEase(Ease.OutElastic);
    }

    void ClearPath()
    {
        DOTween.To(() => usableLine.color, x => usableLine.color = x, new Color(1, 1, 1, 0), 0.4f).SetOptions(true).OnComplete(FadeTweenComplete);

        path.Clear();
    }

    private void FadeTweenComplete()
    {
        usableLine.points3.Clear();
        usableLine.active = false;
        usableLine.color = new Color(usableLine.color.r, usableLine.color.g, usableLine.color.b, 1f);
    }

    void Update()
    {
        if (GameGridBehaviour.instance.GetCurrentGameState().Equals(GameState.RegGameState))
        {
            PlayGameState();
        }
        else if (GameGridBehaviour.instance.GetCurrentGameState().Equals(GameState.PlayerSetupState))
        {
            if (this.milUnits.Count <= 0)
                PlayerSetupState();
        }

        if (!GameGridBehaviour.isMP || (GameGridBehaviour.isMP && photonView.isMine))
        {
            //Display the player's money
            GameGridBehaviour.instance.moneyText.text = String.Concat("Money: ", this.Cash);
        }
    }

    public void PlayGameState()
    {
        if (!GameGridBehaviour.instance.IsBuildScreenBlocking())
        {
            if (Input.touchSupported)
                CheckTouchInput();
            else
                CheckMouseInput();
        }

        milUnits.ForEach(unit => {

             if (unit != null)
             {
                 unit.UpdateUnit(GameGridBehaviour.instance.Grid, GameGridBehaviour.instance.listOfPlayers);

                 if (unit.GetUnitAmount() < 1)
                 {
                     StartCoroutine(GameGridBehaviour.instance.DestroyUnitAfterAnimation(unit, this));
                 }
                 else if (unit.combatToUpdateGame != null)
                 {
                     GameGridBehaviour.instance.listofCurrentCombats.Add(unit.combatToUpdateGame);
                     unit.combatToUpdateGame = null;
                 }
             }
             else
                 this.milUnits.Remove(unit);
         });

        settlements.ForEach(settle =>
        {
            settle.UpdateBuildingList(this);
        });

        if (playerTimer > 1f && !startChosen)
        {
            if (path.Count > 0)
                ClearPath();
        }

        //if (drawLine)
        if (usableLine.active)
            usableLine.Draw3D();

        playerTimer += Time.deltaTime;
    }

    public void PlayerSetupState()
    {
        if (Input.touchCount > 0)
            CheckTouchInputPreGame();
        else if (!Input.touchSupported)
            CheckMouseInputPreGame();
    }


}
