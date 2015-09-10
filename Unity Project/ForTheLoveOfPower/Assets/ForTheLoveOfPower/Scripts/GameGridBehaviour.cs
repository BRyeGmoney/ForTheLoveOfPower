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

	PointyHexPoint startPoint;
	UnitCell startCell;
	UnitCell prevClickedCell;
	PointyHexPoint prevClickedPoint;
	PointyHexPoint endPoint;
	Touch prevTouch;
	IEnumerable<PointyHexPoint> path;
	PointyHexTileGridBuilder me;
	BuildMenuBehaviour buildScreenSettings;
	
	//Player playingPlayer;
	//AIPlayer aiPlayer;
	public Player[] listOfPlayers;
	public Text moneyText;
	public Text touchCounts;
	public Text touchTypes;
	public GameObject buildScreen;
	public GameObject[] unitTypes;
	public GameObject[] structureTypes;

	float timer = 0f;
	bool startChosen = false;

	private List<Combat> listofCurrentCombats;

	// Use this for initialization
	void Start () {
		listofCurrentCombats = new List<Combat> ();
		//playingPlayer = gameObject.Find ("playerOne").GetComponent<Player>();
		//aiPlayer = gameObject.Find ("playerTwoAI").GetComponent<AIPlayer> ();

		me = gameObject.GetComponent<PointyHexTileGridBuilder> ();
		buildScreenSettings = buildScreen.GetComponent<BuildMenuBehaviour> ();

		//Create ai player's beginnings
		CreateNewMilitaryUnit (listOfPlayers[1], (int)MilitaryUnitType.Dictator, Grid [new PointyHexPoint (-3, 17)] as UnitCell, new PointyHexPoint (-3, 17));
		CreateNewSettlement (listOfPlayers[1], Grid [new PointyHexPoint (-3, 18)] as UnitCell, new PointyHexPoint (-3, 18), GetSurroundingTiles (new PointyHexPoint(-3, 18)));
		CreateNewMilitaryUnit (listOfPlayers[1], (int)MilitaryUnitType.Infantry, Grid [new PointyHexPoint (-4, 17)] as UnitCell, new PointyHexPoint (-4, 17)); 
		moneyText.color = listOfPlayers[0].PlayerColor;
	}
	
	// Update is called once per frame
	void Update () {
		CheckEndGame ();

		if (timer > 1f) {
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
			unit.UpdateUnit(Grid, listOfPlayers);

			if (unit.GetUnitAmount () < 1) {
				StartCoroutine (DestroyUnitAfterAnimation(unit, listOfPlayers[0]));
			} else if (unit.combatToUpdateGame != null) {
				this.listofCurrentCombats.Add (unit.combatToUpdateGame);
				unit.combatToUpdateGame = null;
			}
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

	public void OnClick(PointyHexPoint clickedPoint)
	{
		UnitCell clickedCell = Grid [clickedPoint] as UnitCell;
		if (prevClickedCell != null)
			prevClickedCell.HighlightOn = false;

		clickedCell.HighlightOn = true;
		touchCounts.text = Input.touchCount.ToString ();

		if (Input.touchSupported) {
			touchCounts.text = String.Format ("D: {0}", Input.touchCount.ToString ());
			if (Input.touchCount == 1) {
				Touch curTouch = Input.GetTouch (0);

				touchTypes.text = String.Format ("{0} & {1}", curTouch.phase.ToString (), prevTouch.phase.ToString ());

				if (curTouch.phase.Equals (TouchPhase.Ended) && prevTouch.phase.Equals (TouchPhase.Began)) { //single press 
					PressTile (clickedCell, clickedPoint);
				} else if (startChosen && curTouch.phase.Equals (TouchPhase.Moved)) {
					if (prevClickedPoint != clickedPoint) {
						PointList<PointyHexPoint> prevPath = path.ToPointList ();

						path = GetGridPath ();

						foreach (PointyHexPoint point in path) {
							prevPath.Remove (point);
							(Grid [point] as SpriteCell).HighlightOn = true;
						}

						foreach (PointyHexPoint point in prevPath)
							(Grid [point] as SpriteCell).HighlightOn = false;
					}
				} else if (curTouch.phase.Equals (TouchPhase.Moved) && prevTouch.phase.Equals (TouchPhase.Began) || prevTouch.phase.Equals (TouchPhase.Stationary)) { //we'll try to see if there was a unit to move in the prevClicked
					MilitaryUnit unitOnTile = listOfPlayers [0].milUnits.Find (unit => unit.TilePoint.Equals (prevClickedPoint));

					if (unitOnTile != null) {
						startChosen = true;
						startPoint = prevClickedPoint;
					}
				}

				prevTouch = curTouch;

			} else {
				touchTypes.text = String.Format ("none & {0}", prevTouch.phase.ToString ());
				if (startChosen) { //that means we've decided the final point for the unit
					MilitaryUnit unitOnTile = listOfPlayers [0].milUnits.Find (unit => unit.TilePoint.Equals (startPoint));

					if (unitOnTile != null)
						unitOnTile.SetMovementPath (path.ToPointList ());

					timer = 0f;
					startChosen = false;
				}
			}

		} else {
			//If there is a unit on this tile, or if we have previously chosen a unit
			if (Input.GetMouseButtonDown (1)) {
				if ((clickedCell.unitOnTile) || startChosen) {
					MilitaryUnit unitOnTile;

					if (startChosen) {
						unitOnTile = listOfPlayers [0].milUnits.Find (unit => unit.TilePoint.Equals (prevClickedPoint));
						endPoint = clickedPoint;
					
						path = GetGridPath ();
						foreach (PointyHexPoint point in path) 
							(Grid [point] as SpriteCell).HighlightOn = true;
						unitOnTile.SetMovementPath (path.ToPointList ());
					
						//start the timer to make it dissapear and then reset the flag that tells
						//us if we're trying to move something
						timer = 0f;
						startChosen = false;
					} else if (!startChosen) {
						if (listOfPlayers [0].milUnits.Exists (unit => unit.TilePoint.Equals (clickedPoint))) {
							startPoint = clickedPoint;
							startChosen = true;
						}
					}
				}
			} else if (Input.GetMouseButtonDown (0)) {
				PressTile (clickedCell, clickedPoint);
			}
		}

		prevClickedPoint = clickedPoint;
		prevClickedCell = clickedCell;
	}



	private void PressTile(UnitCell clickedCell, PointyHexPoint clickedPoint)
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
		} else if (clickedCell.Color == listOfPlayers[0].PlayerColor) {
			me.enabled = false;
			SetupBuildMenu (false);
		} else {
			//if the player has no units, then this is how we let them place the dictator
			if (listOfPlayers[0].milUnits.IsEmpty()) {
				CreateNewMilitaryUnit (listOfPlayers[0], (int)MilitaryUnitType.Dictator, clickedCell, clickedPoint);
			} else { //else, bring up the bulding selection screen
				me.enabled = false;
				SetupBuildMenu (true);
			}
			clickedCell.HighlightOn = false;
		}
	}

	private void SetupBuildMenu(bool isSettlementMenu)
	{
		buildScreenSettings.structChosen += HandlestructChosen;
		buildScreenSettings.SetBGColor(listOfPlayers[0].PlayerColor);
		buildScreenSettings.DoSettlementMenu(isSettlementMenu);
		buildScreen.SetActive (true);
	}

	void HandlestructChosen (object sender, BuildingChosenArgs e)
	{
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

		me.enabled = true;
		buildScreenSettings.structChosen -= HandlestructChosen;
		buildScreen.SetActive (false);
		prevClickedCell.HighlightOn = false;
	}

	void CreateNewMilitaryUnit(Player player, int unitType, UnitCell gridCell, PointyHexPoint gridPoint)
	{
		MilitaryUnit newUnit = Instantiate (unitTypes [unitType], gridCell.transform.position, Quaternion.identity).GetComponent<MilitaryUnit> ();
		newUnit.Initialize (player.PlayerColor, (MilitaryUnitType)unitType, gridPoint);
		player.milUnits.Add (newUnit);
		gridCell.unitOnTile = true;
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

	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
		
		GUIStyle style = new GUIStyle();
		
		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.LowerRight;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
		float msec = Time.deltaTime * 1000.0f;
		float fps = 1.0f / Time.deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
}
