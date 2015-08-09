using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Gamelogic;
using Gamelogic.Grids;

public class GameGridBehaviour : GridBehaviour<PointyHexPoint> {

	PointyHexPoint startPoint;
	UnitCell startCell;
	UnitCell prevClickedCell;
	PointyHexPoint endPoint;
	IEnumerable<PointyHexPoint> path;

	Player playingPlayer;

	float timer = 0f;
	bool startChosen = false;

	// Use this for initialization
	void Start () {
		playingPlayer = GameObject.Find ("playerOne").GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
		Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * 200f;
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);

		timer += Time.deltaTime;
		if (timer > 1f) {
			if (path != null) {
			foreach (PointyHexPoint point in path) 
				(Grid [point] as SpriteCell).HighlightOn = false;
			}
		}
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

				//start the timer to make it dissapear and then reset the flag that tells
				//us if we're trying to move something
				timer = 0f;
				startChosen = false;
			} else if (!startChosen) {
				startPoint = clickedPoint;
				startChosen = true;
			}


		} else if (clickedCell.buildingOnTile != null) {
		} else {
			//if the player has no units, then this is how we let them place the dictator
			if (playingPlayer.milUnits.IsEmpty()) {
				AssemblyCSharp.MilitaryUnit dictator = AssemblyCSharp.CreateMilitaryUnit.CreateDictator ();
				playingPlayer.milUnits.Add (dictator);
				clickedCell.AddUnitToTile (dictator);
			} else { //else, bring up the bulding selection screen
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
}
