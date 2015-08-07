using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Gamelogic;
using Gamelogic.Grids;

public class GameGridBehaviour : GridBehaviour<PointyHexPoint> {

	PointyHexPoint startPoint;
	UnitCell startCell;
	PointyHexPoint endPoint;
	IEnumerable<PointyHexPoint> path;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * 200f;
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);
	}

	public void OnClick(PointyHexPoint clickedPoint)
	{
		UnitCell clickedCell = Grid [clickedPoint] as UnitCell;


		if (startCell == null) {
			startPoint = clickedPoint;
			startCell = Grid[startPoint] as UnitCell;
			clickedCell.HighlightOn = true;
			clickedCell.foreground = new SpriteRenderer();
			Debug.Log ("Hey: " + Resources.Load<Sprite>("Sprites/Units/Unit_Dictator").name);
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
		}
		//Do something, such as
		//(Grid[clickedPoint] as SpriteCell).HighlightOn = !(Grid[clickedPoint] as SpriteCell).HighlightOn;
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
