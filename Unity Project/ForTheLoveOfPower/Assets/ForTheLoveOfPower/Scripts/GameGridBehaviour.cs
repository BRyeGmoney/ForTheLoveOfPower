using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Gamelogic;
using Gamelogic.Grids;
using AssemblyCSharp;

public class GameGridBehaviour : GridBehaviour<PointyHexPoint> {
	static public bool didWin;
	static public Color baseFloorColor;// = new Color (0.327f, 0.779f, 0.686f, 1f);

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
	//PointyHexTileGridBuilder me;
	BuildMenuBehaviour buildScreenSettings;
	
	//Player playingPlayer;
	//AIPlayer aiPlayer;
	public Player[] listOfPlayers;
	public Text moneyText;
	public Text touchTypes;
	public GameObject buildScreen;
	public GameObject[] unitTypes;
	public GameObject[] structureTypes;

	float timer = 0f;
	bool startChosen = false;
	private int bloop;

	private List<Combat> listofCurrentCombats;

	// Use this for initialization
	void Start () {
		listofCurrentCombats = new List<Combat> ();
		//playingPlayer = gameObject.Find ("playerOne").GetComponent<Player>();
		//aiPlayer = gameObject.Find ("playerTwoAI").GetComponent<AIPlayer> ();

		//me = gameObject.GetComponent<PointyHexTileGridBuilder> ();
		buildScreenSettings = buildScreen.GetComponent<BuildMenuBehaviour> ();
		baseFloorColor = GridBuilder.Colors [0];

		//Input.simulateMouseWithTouches = true;

		//Create ai player's beginnings
		CreateNewMilitaryUnit (listOfPlayers[1], (int)MilitaryUnitType.Dictator, Grid [new PointyHexPoint (2, 13)] as UnitCell, new PointyHexPoint (2, 13));
		CreateNewSettlement (listOfPlayers[1], Grid [new PointyHexPoint (2, 12)] as UnitCell, new PointyHexPoint (2, 12), GetSurroundingTiles (new PointyHexPoint(2, 12)));
		CreateNewMilitaryUnit (listOfPlayers[1], (int)MilitaryUnitType.Infantry, Grid [new PointyHexPoint (3, 13)] as UnitCell, new PointyHexPoint (3, 13)); 
		moneyText.color = listOfPlayers[0].PlayerColor;
	}

	/// <summary>
	/// Checks the touch input.
	/// </summary>
	private void CheckTouchInput()
	{
		Debug.Log (String.Format ("We're Checking Touch Input"));

		if (Input.touchCount == 1) {
			Touch curTouch = Input.GetTouch (0);
			PointyHexPoint clickedPoint = Map[GridBuilderUtils.ScreenToWorld (curTouch.position)];
			UnitCell clickedCell;

			if (clickedPoint != new PointyHexPoint()) {
				clickedCell = Grid[clickedPoint] as UnitCell;
			
				touchTypes.text = String.Format ("{0} & {1}", curTouch.phase.ToString (), prevTouch.phase.ToString ());
			
				if (curTouch.phase.Equals (TouchPhase.Ended) && (prevTouch.phase.Equals (TouchPhase.Began) || prevTouch.phase.Equals (TouchPhase.Stationary))) { //single press 
					PressTile (clickedCell, clickedPoint);
				} else if (startChosen && curTouch.phase.Equals (TouchPhase.Moved)) {
					if (prevClickedPoint != clickedPoint) {
						prevPath = path;
					
						path = GetGridPath ().ToPointList ();
					

						foreach (PointyHexPoint point in path) {
							prevPath.Remove (point);
							(Grid [point] as SpriteCell).HighlightOn = true;
						}
					
						foreach (PointyHexPoint point in prevPath)
							(Grid [point] as SpriteCell).HighlightOn = false;
					}
				} else if (curTouch.phase.Equals (TouchPhase.Moved) && prevTouch.phase.Equals (TouchPhase.Began) || prevTouch.phase.Equals (TouchPhase.Stationary)) { //we'll try to see if there was a unit to move in the prevClicked
					unitToMove = listOfPlayers [0].milUnits.Find (unit => unit.TilePoint.Equals (prevClickedPoint));
				
					if (unitToMove != null) {
						startChosen = true;
						startPoint = prevClickedPoint;
					}
				}
			
				prevTouch = curTouch;
				prevClickedPoint = clickedPoint;
				prevClickedCell = clickedCell;
			}
			
		} else {
			//touchTypes.text = String.Format ("none & {0}", prevTouch.phase.ToString ());
			if (startChosen) { //that means we've decided the final point for the unit
				unitToMove = listOfPlayers [0].milUnits.Find (unit => unit.TilePoint.Equals (startPoint));
				
				if (unitToMove != null)
					unitToMove.SetMovementPath (path.ToPointList ());
				
				timer = 0f;
				startChosen = false;
			}
		}
	}

	/// <summary>
	/// Checks the mouse input.
	/// </summary>
	void CheckMouseInput()
	{
		//We're checking for right mouse input, the first frame of it being down
		if (Input.GetMouseButtonDown (1)) {
			clickedPoint = Map [GridBuilderUtils.ScreenToWorld (Input.mousePosition)];
			clickedCell = Grid [clickedPoint] as UnitCell;

			//if the initial click succesfully targets a unit, lets set them
			if ((clickedCell.unitOnTile)) {
				if (!startChosen) {
					if (listOfPlayers [0].milUnits.Exists (unit => unit.TilePoint.Equals (clickedPoint))) {//quick check that someone is actually here
						startPoint = clickedPoint;
						unitToMove = listOfPlayers [0].milUnits.Find (unit => unit.TilePoint.Equals (startPoint));
						unitToMove.ClearMovementPath ();
						startChosen = true;
					}
				}
			}

			prevClickedPoint = clickedPoint;
			prevClickedCell = clickedCell;
		} else if (Input.GetMouseButton (1) && startChosen) { //we're polling for if the right mouse is held down, and if the start tile has been chosen
			clickedPoint = Map [GridBuilderUtils.ScreenToWorld (Input.mousePosition)];

			if (clickedPoint != prevClickedPoint || clickedPoint != startPoint) {
				endPoint = clickedPoint;
				clickedCell = Grid [clickedPoint] as UnitCell;

				if (path != null)
					prevPath = path;

				path = GetGridPath ().ToPointList();

				foreach (PointyHexPoint point in path) {
					if (prevPath != null && prevPath.Contains (point))
						prevPath.Remove(point);

					(Grid [point] as UnitCell).SetTileColorPath (listOfPlayers[0].PlayerColor);
				}

				if (prevPath != null) {
					foreach (PointyHexPoint point in prevPath) 
						(Grid[point] as UnitCell).SetTileColorUnPath ();
				}
			}
		} else if (startChosen) {
			if (path.Count > 1) {
				unitToMove.SetMovementPath (path);
				unitToMove.ChangeSpriteDirection (Grid [path [1]] as UnitCell);
				path.Clear ();
				prevPath.Clear ();
			}

			timer = 0f;
			startChosen = false;
		}
		
		if (Input.GetMouseButtonDown (0)) {
			clickedPoint = Map[GridBuilderUtils.ScreenToWorld (Input.mousePosition)];
			clickedCell = Grid[clickedPoint] as UnitCell;

			PressTile (clickedCell, clickedPoint);

			prevClickedPoint = clickedPoint;
			prevClickedCell = clickedCell;
		}
	}
	
	// Update is called once per frame
	void Update () {
		CheckEndGame ();
		
		if (!buildScreenSettings.isActiveAndEnabled) {
			//Debug.Log ("Build settings are inactive");
			if (Input.touchCount > 0)
				CheckTouchInput ();
			else if (!Input.touchSupported)
				CheckMouseInput ();
		}

		if (timer > 1f && !startChosen) {
			if (path != null) {
				foreach (PointyHexPoint point in path) 
					(Grid [point] as SpriteCell).HighlightOn = false;
				path = null;
			}
		}

		//Update All the Fights Going On
		listofCurrentCombats.ForEach (fight => {
			fight.Update ();

			if (fight.fightOver) {
				listofCurrentCombats.Remove (fight);
				fight = null;
			}
		});

		//Update All the player's units, post fight, in case anyone died, we can get rid of them now
		listOfPlayers[0].milUnits.ForEach (unit => {

			if (unit != null) {
			unit.UpdateUnit(Grid, listOfPlayers);

			if (unit.GetUnitAmount () < 1) {
				StartCoroutine (DestroyUnitAfterAnimation(unit, listOfPlayers[0]));
			} else if (unit.combatToUpdateGame != null) {
				this.listofCurrentCombats.Add (unit.combatToUpdateGame);
				unit.combatToUpdateGame = null;
			}
			} else
				listOfPlayers[0].milUnits.Remove (unit);
		});

		//Update all the second player's units, post fight, in case anyone died
		listOfPlayers[1].milUnits.ForEach (unit => {
			unit.UpdateUnit (Grid, listOfPlayers);

			if (unit.GetUnitAmount () < 1) {
				StartCoroutine (DestroyUnitAfterAnimation (unit, listOfPlayers[1]));
			} else if (unit.combatToUpdateGame != null) {
				this.listofCurrentCombats.Add (unit.combatToUpdateGame);
				unit.combatToUpdateGame = null;
			}
		});

		//Update All of the player's settlements, in case enemy units have died on something being taken over
		listOfPlayers[0].settlements.ForEach (settle => {
			settle.UpdateBuildingList(listOfPlayers[0]);
		});

		//Update All of the second player's settlements, in case one of our units has died on something being taken over
		listOfPlayers[0].settlements.ForEach (settle => {
			settle.UpdateBuildingList (listOfPlayers[1]);
		});

		//Display the player's money
		moneyText.text = String.Concat ("Money: ", listOfPlayers[0].Cash); 

		//update our timer
		timer += Time.deltaTime;
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

			Application.LoadLevel ("StatsScreen");
		}
	}

	private void PressTile(UnitCell clickedCell, PointyHexPoint clickedPoint)
	{
		Debug.Log ("We've pressed a tile");
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
		} else if (clickedCell.Color == listOfPlayers[0].PlayerColor) {
			//me.enabled = false;
			SetupBuildMenu (false);
		} else {
			//if the player has no units, then this is how we let them place the dictator
			if (listOfPlayers[0].milUnits.IsEmpty()) {
				CreateNewMilitaryUnit (listOfPlayers[0], (int)MilitaryUnitType.Dictator, clickedCell, clickedPoint);
			} else { //else, bring up the bulding selection screen
				//me.enabled = false;
				SetupBuildMenu (true);
			}
			clickedCell.HighlightOn = false;
		}
	}

	private void SetupBuildMenu(bool isSettlementMenu)
	{
		Debug.Log ("Setting up the build menu");
		buildScreenSettings.structChosen += HandlestructChosen;
		buildScreenSettings.SetBGColor(listOfPlayers[0].PlayerColor);
		buildScreenSettings.DoSettlementMenu(isSettlementMenu);
		buildScreen.SetActive (true);
	}

	void HandlestructChosen (object sender, BuildingChosenArgs e)
	{
		Debug.Log ("The Handler StructChosen is run");
		if (!e.toBuild.Equals (StructureUnitType.None))
		{
			PointList<PointyHexPoint> surroundingTiles = GetSurroundingTiles(prevClickedPoint);

			if (e.IsSettlement) {
				if (!IsBuildingInArea (prevClickedPoint, surroundingTiles, listOfPlayers[0].PlayerColor))
					CreateNewSettlement (listOfPlayers[0], prevClickedCell, prevClickedPoint, surroundingTiles);
			} else {
				Settlement owningSettlement = FindOwningSettlement (prevClickedPoint, surroundingTiles, listOfPlayers[0].PlayerColor);

				if (owningSettlement != null) {
					CreateNewStructure(listOfPlayers[0], (int)e.toBuild, prevClickedCell, prevClickedPoint, surroundingTiles, owningSettlement);
				}
			}
		}

		//me.enabled = true;
		buildScreenSettings.structChosen -= HandlestructChosen;
		buildScreen.SetActive (false);
		prevClickedCell.HighlightOn = false;
	}

	void CreateNewMilitaryUnit(Player player, int unitType, UnitCell gridCell, PointyHexPoint gridPoint)
	{
		Debug.Log ("new military unit created");
		MilitaryUnit newUnit = Instantiate (unitTypes [unitType], gridCell.transform.position, Quaternion.identity).GetComponent<MilitaryUnit> ();
		newUnit.Initialize (player.PlayerColor, (MilitaryUnitType)unitType, gridPoint);
		player.milUnits.Add (newUnit);
		gridCell.unitOnTile = true;
	}
	
	void CreateNewSettlement(Player player, UnitCell gridCell, PointyHexPoint gridPoint, PointList<PointyHexPoint> surroundingTiles )
	{
		Debug.Log ("new settlement created");
		Settlement newSettlement = Instantiate (structureTypes [(int)StructureUnitType.Settlement], gridCell.transform.position, Quaternion.identity).GetComponent<Settlement> ();
		newSettlement.Initialize (player.PlayerColor, StructureUnitType.Settlement, gridPoint);
		player.settlements.Add (newSettlement);
		player.AddToOwnedTiles (Grid, newSettlement, listOfPlayers[1], surroundingTiles);
		gridCell.AddStructureToTile (newSettlement);
	}

	void CreateNewStructure(Player player, int structType, UnitCell gridCell, PointyHexPoint gridPoint, PointList<PointyHexPoint> surroundingTiles, Settlement owningSettlement)
	{
		Debug.Log ("new structure created");
		StructureUnit newStructure = Instantiate (structureTypes [structType], gridCell.transform.position, Quaternion.identity).GetComponent<StructureUnit> ();
		newStructure.Initialize (player.PlayerColor, (StructureUnitType)structType, gridPoint, owningSettlement);
		owningSettlement.cachedBuildingList.Add (newStructure);
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
		Debug.Log ("trying to find owning settlement");
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
		Debug.Log ("trying to find a building in the area");
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
}
