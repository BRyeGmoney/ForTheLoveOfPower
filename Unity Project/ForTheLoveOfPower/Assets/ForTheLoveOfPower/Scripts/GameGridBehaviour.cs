﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Gamelogic;
using Gamelogic.Grids;
using AssemblyCSharp;

public class GameGridBehaviour : GridBehaviour<PointyHexPoint> {

	PointyHexPoint startPoint;
	UnitCell startCell;
	UnitCell prevClickedCell;
	PointyHexPoint endPoint;
	IEnumerable<PointyHexPoint> path;
	Sprite[] structureSprites;
	Sprite[] unitSprites;

	Player playingPlayer;
	AIPlayer aiPlayer;
	public Text moneyText;

	float timer = 0f;
	bool startChosen = false;

	// Use this for initialization
	void Start () {
		playingPlayer = GameObject.Find ("playerOne").GetComponent<Player>();
		aiPlayer = GameObject.Find ("playerTwoAI").GetComponent<AIPlayer> ();
		structureSprites = Resources.LoadAll<Sprite> ("Sprites/Buildings");
		unitSprites = Resources.LoadAll<Sprite> ("Sprites/Units");

		aiPlayer.CreateBasePlayer (unitSprites, structureSprites, Grid);
	}
	
	// Update is called once per frame
	void Update () {
		Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * 200f;
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);

		if (timer > 1f) {
			if (path != null) {
				foreach (PointyHexPoint point in path) 
					(Grid [point] as SpriteCell).HighlightOn = false;
				path = null;
			}
		}

		playingPlayer.milUnits.ForEach (unit => unit.Update (Grid, unitSprites));

		moneyText.text = String.Concat ("Money: ", playingPlayer.Cash); 

		//update our timer
		timer += Time.deltaTime;
	}

	public void OnClick(PointyHexPoint clickedPoint)
	{
		UnitCell clickedCell = Grid [clickedPoint] as UnitCell;
		if (prevClickedCell != null)
			prevClickedCell.HighlightOn = false;

		clickedCell.HighlightOn = true;

		//If there is a unit on this tile, or if we have previously chosen a unit
		if (clickedCell.unitOnTile != null || startChosen) {

			//if we've previously chosen a unit
			if (startChosen) {
				//this is the point he is moving to
				endPoint = clickedPoint;

				//get and draw the path
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


		} else if (clickedCell.buildingOnTile != null) {
			if (clickedCell.buildingOnTile.StructureType.Equals (StructureUnitType.Barracks)) {
				if (clickedCell.unitOnTile != null) { //if someone's already here, just add to their count
					clickedCell.unitOnTile.AddUnits (1);
				} else { //create a unit here if not and of same type
					MilitaryUnit infantryMan = CreateMilitaryUnit.CreateInfantry (playingPlayer.PlayerColor);
					playingPlayer.milUnits.Add (infantryMan);
					clickedCell.AddUnitToTile (infantryMan, unitSprites);
				}
			}
		} else if (clickedCell.Color == playingPlayer.PlayerColor) {
			StructureUnit newBuilding = CreateStructureUnit.CreateBarracks (playingPlayer.PlayerColor);
			playingPlayer.structUnits.Add (newBuilding);
			clickedCell.AddStructureToTile (newBuilding, structureSprites);

			playingPlayer.AddToOwnedTiles (Grid, GetSurroundingTiles (clickedPoint));
		} else {
			//if the player has no units, then this is how we let them place the dictator
			if (playingPlayer.milUnits.IsEmpty()) {
				MilitaryUnit dictator = CreateMilitaryUnit.CreateDictator (playingPlayer.PlayerColor);
				playingPlayer.milUnits.Add (dictator);
				clickedCell.AddUnitToTile (dictator, unitSprites);
				clickedCell.HighlightOn = false;
			} else { //else, bring up the bulding selection screen
				Settlement newSettlement = CreateStructureUnit.CreateSettlement (playingPlayer.PlayerColor);
				playingPlayer.structUnits.Add (newSettlement);
				clickedCell.AddStructureToTile (newSettlement, structureSprites);

				//playingPlayer.AddToOwnedTiles (Grid, clickedPoint);
				playingPlayer.AddToOwnedTiles(Grid, GetSurroundingTiles(clickedPoint));

				//Insert Build Menu here

				clickedCell.HighlightOn = false;
			}
		}

		/*if (startCell == null) {
			startPoint = clickedPoint;
			startCell = Grid[startPoint] as UnitCell;
			clickedCell.HighlightOn = true;

			clickedCell.foreground = clickedCell.gameObject.AddComponent<SpriteRenderer>();
			clickedCell.foreground.sprite = Resources.Load<Sprite>("Sprites/Units/Unit_Dictator");

		} else if (clickedCell != (Grid [startPoint] as UnitCell)) {
			if (path != null) {
				foreach (PointyHexPoint point in path) {
					clickedCell.HighlightOn = false;
				}
			}
			(Grid [endPoint] as UnitCell).HighlightOn = false;

			endPoint = clickedPoint;
			(Grid [endPoint] as UnitCell).HighlightOn = true;

			path = GetGridPath ();

			foreach (PointyHexPoint point in path) {
				(Grid [point] as SpriteCell).HighlightOn = true;
			}
		} else {
			(Grid [startPoint] as UnitCell).HighlightOn = false;
			(Grid [endPoint] as UnitCell).HighlightOn = false;
		}*/
		//Do something, such as
		//(Grid[clickedPoint] as SpriteCell).HighlightOn = !(Grid[clickedPoint] as SpriteCell).HighlightOn;
		prevClickedCell = clickedCell;
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
