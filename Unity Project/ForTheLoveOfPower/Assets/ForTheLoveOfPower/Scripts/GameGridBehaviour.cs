using UnityEngine;
using System;
using System.Collections.Generic;
using Gamelogic;
using Gamelogic.Grids;

public class GameGridBehaviour : GridBehaviour<PointyHexPoint> {

	PointyHexPoint startPoint;
	PointyHexPoint endPoint;
	IEnumerable<PointyHexPoint> path;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * 200f;
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 200, 1000);
		Debug.Log (Camera.main.orthographicSize);
	}

	public void OnClick(PointyHexPoint clickedPoint)
	{
		if (startPoint.Z == 0) 
		{
			startPoint = clickedPoint;
			(Grid[startPoint] as SpriteCell).HighlightOn = true;
		} 
		else if (clickedPoint != startPoint) 
		{
			if (path != null) 
			{
				foreach (PointyHexPoint point in path) {
					(Grid[point] as SpriteCell).HighlightOn = false;
				}
			}
			(Grid[endPoint] as SpriteCell).HighlightOn = false;

			endPoint = clickedPoint;
			(Grid[endPoint] as SpriteCell).HighlightOn = true;

			path = GetGridPath ();

			foreach (PointyHexPoint point in path) {
				(Grid[point] as SpriteCell).HighlightOn = true;
			}
		} 
		else 
		{
			(Grid[startPoint] as SpriteCell).HighlightOn = false;
			(Grid[endPoint] as SpriteCell).HighlightOn = false;
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
