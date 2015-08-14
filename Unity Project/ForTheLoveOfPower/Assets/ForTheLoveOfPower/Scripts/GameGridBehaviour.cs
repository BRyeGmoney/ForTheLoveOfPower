using UnityEngine;
using UnityEditor;
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
	Sprite[] structureSprites;
	Sprite[] unitSprites;


	Player playingPlayer;
	AIPlayer aiPlayer;
	public Text moneyText;
	public GameObject buildScreen;

	float timer = 0f;
	bool startChosen = false;

	private List<Combat> listofCurrentCombats;

	// Use this for initialization
	void Start () {
		listofCurrentCombats = new List<Combat> ();
		playingPlayer = GameObject.Find ("playerOne").GetComponent<Player>();
		aiPlayer = GameObject.Find ("playerTwoAI").GetComponent<AIPlayer> ();
		structureSprites = Resources.LoadAll<Sprite> ("Sprites/Buildings");
		unitSprites = Resources.LoadAll<Sprite> ("Sprites/Units");

		aiPlayer.CreateBasePlayer (unitSprites, structureSprites, Grid);
	}
	
	// Update is called once per frame
	void Update () {
		CheckEndGame ();

		Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * 200f;
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);

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
			unit.UpdateUnit(Grid, unitSprites, playingPlayer);
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
			unit.UpdateUnit (Grid, unitSprites, aiPlayer);
			if (unit.GetUnitAmount () < 1) {
				aiPlayer.milUnits.Remove (unit);
				(Grid[unit.TilePoint] as UnitCell).RemoveUnit ();
				if (unit.UnitType.Equals (MilitaryUnitType.Dictator))
					aiPlayer.DictatorAlive = false;
			} else if (unit.combatToUpdateGame != null) {
				this.listofCurrentCombats.Add (unit.combatToUpdateGame);
				unit.combatToUpdateGame = null;
			}
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
			if(clickedCell.unitOnTile != null || startChosen) {
				if (startChosen) {
					endPoint = clickedPoint;
					
					path = GetGridPath ();
					foreach (PointyHexPoint point in path) 
						(Grid [point] as SpriteCell).HighlightOn = true;
					prevClickedCell.unitOnTile.SetMovementPath (path.ToPointList ());
					
					//start the timer to make it dissapear and then reset the flag that tells
					//us if we're trying to move something
					timer = 0f;
					startChosen = false;
				} else if (!startChosen) {
					startPoint = clickedPoint;
					startChosen = true;
				}
			}
		} else if (Input.GetMouseButtonDown (0)) {
			if (clickedCell.buildingOnTile != null) {
				if (clickedCell.buildingOnTile.StructureType.Equals (StructureUnitType.Barracks)) {
					if (clickedCell.unitOnTile != null) { //if someone's already here, just add to their count
						clickedCell.unitOnTile.AddUnits (1);
					} else { //create a unit here if not and of same type
						MilitaryUnit infantryMan = CreateMilitaryUnit.CreateInfantry (playingPlayer.PlayerColor, clickedPoint);
						playingPlayer.milUnits.Add (infantryMan);
						clickedCell.AddUnitToTile (infantryMan, unitSprites);
					}
				}
			} else if (clickedCell.Color == playingPlayer.PlayerColor) {
				PointyHexTileGridBuilder me = this.GetComponent<PointyHexTileGridBuilder>();
				me.enabled = false;
				SetupBuildMenu (false);
			} else {
				//if the player has no units, then this is how we let them place the dictator
				if (playingPlayer.milUnits.IsEmpty()) {
					MilitaryUnit dictator = CreateMilitaryUnit.CreateDictator (playingPlayer.PlayerColor, clickedPoint);
					playingPlayer.milUnits.Add (dictator);
					clickedCell.AddUnitToTile (dictator, unitSprites);
					clickedCell.HighlightOn = false;
				} else { //else, bring up the bulding selection screen
					PointyHexTileGridBuilder me = this.GetComponent<PointyHexTileGridBuilder>();
					me.enabled = false;
					SetupBuildMenu (true);

					clickedCell.HighlightOn = false;
				}
			}
		}

		prevClickedPoint = clickedPoint;
		prevClickedCell = clickedCell;
	}

	private void SetupBuildMenu(bool isSettlementMenu)
	{
		BuildMenuBehaviour buildScreenSettings = buildScreen.GetComponent<BuildMenuBehaviour> ();
		buildScreenSettings.structChosen += HandlestructChosen;
		buildScreenSettings.BgColor = playingPlayer.PlayerColor;
		buildScreenSettings.DoSettlementMenu(isSettlementMenu);
		buildScreen.SetActive (true);
	}

	void HandlestructChosen (object sender, BuildingChosenArgs e)
	{
		if (e.toBuild != null)
		{
			StructureUnit buildingBuilt = null;
			Settlement ownSettlement = null;
			PointList<PointyHexPoint> surroundingTiles = GetSurroundingTiles(prevClickedPoint);

			if (e.IsSettlement) {
				buildingBuilt = CreateStructureUnit.CreateSettlement(prevClickedPoint, playingPlayer.PlayerColor);
				ownSettlement = buildingBuilt as Settlement;
				playingPlayer.settlements.Add (ownSettlement);
			} else {
				ownSettlement = FindOwningSettlement (prevClickedPoint, surroundingTiles, playingPlayer.PlayerColor);

				if (ownSettlement != null)
				{
					buildingBuilt = CreateStructureUnit.CreateFromType (e.toBuild, prevClickedPoint, playingPlayer.PlayerColor);
					buildingBuilt.OwningSettlement = ownSettlement;
					ownSettlement.cachedBuildingList.Add (buildingBuilt);
				}
			}

			if (buildingBuilt != null) {
				prevClickedCell.AddStructureToTile (buildingBuilt, structureSprites);
				playingPlayer.AddToOwnedTiles(Grid, ownSettlement, aiPlayer, surroundingTiles);
			}
		}

		PointyHexTileGridBuilder me = this.GetComponent<PointyHexTileGridBuilder>();
		me.enabled = true;
		buildScreen.GetComponent<BuildMenuBehaviour> ().structChosen -= HandlestructChosen;
		buildScreen.SetActive (false);
		prevClickedCell.HighlightOn = false;
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
			if (currCell.buildingOnTile != null && currCell.buildingOnTile.StructColor.Equals (owningPlayerColor)) {
				if (currCell.buildingOnTile.StructureType.Equals (StructureUnitType.Settlement))
				    return currCell.buildingOnTile as Settlement;
				else
					return currCell.buildingOnTile.OwningSettlement;
			}
		}

		return null;
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
