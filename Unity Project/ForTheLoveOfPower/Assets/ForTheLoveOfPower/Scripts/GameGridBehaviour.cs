using UnityEngine;
using System;
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
	IEnumerable<PointyHexPoint> path;
	PointyHexTileGridBuilder me;
	BuildMenuBehaviour buildScreenSettings;
	
	Player playingPlayer;
	AIPlayer aiPlayer;
	public Text moneyText;
	public GameObject buildScreen;
	public GameObject[] unitTypes;
	public GameObject[] structureTypes;

	float timer = 0f;
	bool startChosen = false;

	private List<Combat> listofCurrentCombats;

	// Use this for initialization
	void Start () {
		listofCurrentCombats = new List<Combat> ();
		playingPlayer = GameObject.Find ("playerOne").GetComponent<Player>();
		aiPlayer = GameObject.Find ("playerTwoAI").GetComponent<AIPlayer> ();

		me = gameObject.GetComponent<PointyHexTileGridBuilder> ();
		buildScreenSettings = buildScreen.GetComponent<BuildMenuBehaviour> ();

		//aiPlayer.CreateBasePlayer (unitSprites, structureSprites, Grid);
		moneyText.color = playingPlayer.PlayerColor;
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
		playingPlayer.milUnits.ForEach (unit => {
			unit.UpdateUnit(Grid, playingPlayer);

			if (unit.GetUnitAmount () < 1) {
				playingPlayer.milUnits.Remove (unit);
				(Grid[unit.TilePoint] as UnitCell).RemoveUnit ();

				if (unit.UnitType.Equals (MilitaryUnitType.Dictator))
					playingPlayer.DictatorAlive = false;
			} else if (unit.combatToUpdateGame != null) {
				this.listofCurrentCombats.Add (unit.combatToUpdateGame);
				unit.combatToUpdateGame = null;
			}
		});

		//Update all the second player's units, post fight, in case anyone died
		aiPlayer.milUnits.ForEach (unit => {
			/*unit.UpdateUnit (Grid, unitSprites, aiPlayer);
			if (unit.GetUnitAmount () < 1) {
				aiPlayer.milUnits.Remove (unit);
				(Grid[unit.TilePoint] as UnitCell).RemoveUnit ();
				if (unit.UnitType.Equals (MilitaryUnitType.Dictator))
					aiPlayer.DictatorAlive = false;
			} else if (unit.combatToUpdateGame != null) {
				this.listofCurrentCombats.Add (unit.combatToUpdateGame);
				unit.combatToUpdateGame = null;
			}*/
		});

		//Update All of the player's settlements, in case enemy units have died on something being taken over
		playingPlayer.settlements.ForEach (settle => {
			settle.UpdateBuildingList(playingPlayer);
		});

		//Update All of the second player's settlements, in case one of our units has died on something being taken over
		aiPlayer.settlements.ForEach (settle => {
			settle.UpdateBuildingList (aiPlayer);
		});

		//Display the player's money
		moneyText.text = String.Concat ("Money: ", playingPlayer.Cash); 

		//update our timer
		timer += Time.deltaTime;
	}

	private void CheckEndGame ()
	{
		if (!playingPlayer.DictatorAlive || !aiPlayer.DictatorAlive) {

			if (!playingPlayer.DictatorAlive)
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

		//If there is a unit on this tile, or if we have previously chosen a unit
		if (Input.GetMouseButtonDown (1)) {
			if((clickedCell.unitOnTile) || startChosen) {
				MilitaryUnit unitOnTile;

				if (startChosen) {
					unitOnTile = playingPlayer.milUnits.Find (unit => unit.TilePoint.Equals (prevClickedPoint));
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
					if (playingPlayer.milUnits.Exists (unit => unit.TilePoint.Equals(clickedPoint))) {
						startPoint = clickedPoint;
						startChosen = true;
					}
				}
			}
		} else if (Input.GetMouseButtonDown (0)) {
			//if there's a building on this tile
			if (clickedCell.buildingOnTile) {
				//if it is of the military variety
				if (clickedCell.structureOnTile.tag.Equals ("Military")) {
					//if there's already a unit on this tile, we should just add to it
					if (clickedCell.unitOnTile) {
						MilitaryUnit unitOnTile = playingPlayer.milUnits.Find (unit => unit.TilePoint.Equals (clickedPoint));

						if (unitOnTile != null)
							unitOnTile.AddUnits (1);
					} else if (clickedCell.structureOnTile.StructureType.Equals (StructureUnitType.Barracks)) {
						CreateNewMilitaryUnit (playingPlayer, (int)MilitaryUnitType.Infantry, clickedCell, clickedPoint);
					} else if (clickedCell.structureOnTile.StructureType.Equals (StructureUnitType.TankDepot)) {
						CreateNewMilitaryUnit (playingPlayer, (int)MilitaryUnitType.Tank, clickedCell, clickedPoint);
					} else {
						CreateNewMilitaryUnit (playingPlayer, (int)MilitaryUnitType.Jet, clickedCell, clickedPoint);
					}
				}
			} else if (clickedCell.Color == playingPlayer.PlayerColor) {
				me.enabled = false;
				SetupBuildMenu (false);
			} else {
				//if the player has no units, then this is how we let them place the dictator
				if (playingPlayer.milUnits.IsEmpty()) {
					CreateNewMilitaryUnit (playingPlayer, (int)MilitaryUnitType.Dictator, clickedCell, clickedPoint);
				} else { //else, bring up the bulding selection screen
					me.enabled = false;
					SetupBuildMenu (true);
				}
				clickedCell.HighlightOn = false;
			}
		}

		prevClickedPoint = clickedPoint;
		prevClickedCell = clickedCell;
	}

	private void SetupBuildMenu(bool isSettlementMenu)
	{
		buildScreenSettings.structChosen += HandlestructChosen;
		buildScreenSettings.SetBGColor(playingPlayer.PlayerColor);
		buildScreenSettings.DoSettlementMenu(isSettlementMenu);
		buildScreen.SetActive (true);
	}

	void HandlestructChosen (object sender, BuildingChosenArgs e)
	{
		if (!e.toBuild.Equals (StructureUnitType.None))
		{
			PointList<PointyHexPoint> surroundingTiles = GetSurroundingTiles(prevClickedPoint);

			if (e.IsSettlement) {
				if (!IsBuildingInArea (prevClickedPoint, surroundingTiles, playingPlayer.PlayerColor))
					CreateNewSettlement (playingPlayer, prevClickedCell, prevClickedPoint, surroundingTiles);
			} else {
				Settlement owningSettlement = FindOwningSettlement (prevClickedPoint, surroundingTiles, playingPlayer.PlayerColor);

				if (owningSettlement != null) {
					CreateNewStructure(playingPlayer, (int)e.toBuild, prevClickedCell, prevClickedPoint, surroundingTiles, owningSettlement);
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
		player.AddToOwnedTiles (Grid, newSettlement, aiPlayer, surroundingTiles);
		gridCell.AddStructureToTile (newSettlement);
	}

	void CreateNewStructure(Player player, int structType, UnitCell gridCell, PointyHexPoint gridPoint, PointList<PointyHexPoint> surroundingTiles, Settlement owningSettlement)
	{
		StructureUnit newStructure = Instantiate (structureTypes [structType], gridCell.transform.position, Quaternion.identity).GetComponent<StructureUnit> ();
		newStructure.Initialize (player.PlayerColor, (StructureUnitType)structType, gridPoint, owningSettlement);
		owningSettlement.cachedBuildingList.Add (newStructure);
		player.AddToOwnedTiles (Grid, owningSettlement, aiPlayer, surroundingTiles);

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
