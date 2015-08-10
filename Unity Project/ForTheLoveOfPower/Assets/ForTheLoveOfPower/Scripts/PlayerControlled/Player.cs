using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grids;

public class Player : MonoBehaviour {

	public List<AssemblyCSharp.MilitaryUnit> milUnits;
	public List<AssemblyCSharp.StructureUnit> structUnits;
	public Int32 Cash { get; set; }

	public Color PlayerColor { get { return playerColor; } }
	private Color playerColor;

	private PointList<PointyHexPoint> ownedTiles;

	// Use this for initialization
	void Start () {
		milUnits = new List<AssemblyCSharp.MilitaryUnit> ();
		structUnits = new List<AssemblyCSharp.StructureUnit> ();

		ownedTiles = new PointList<PointyHexPoint> ();

		//temp player color
		playerColor = new Color (0, 255, 255);
	}

	public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, PointyHexPoint pointToAdd)
	{
		ownedTiles.Add (pointToAdd);
		(gameGrid [pointToAdd] as UnitCell).SetTileColor (PlayerColor);
	}

	public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, PointList<PointyHexPoint> pointsToAdd)
	{
		foreach (PointyHexPoint point in pointsToAdd) {
			ownedTiles.Add (point);
			(gameGrid [point] as UnitCell).SetTileColor (PlayerColor);
		}
	}

}
