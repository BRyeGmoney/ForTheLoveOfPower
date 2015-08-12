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
	protected Color playerColor;

	public Boolean DictatorAlive { get; set; }

	private PointList<PointyHexPoint> ownedTiles;

	// Use this for initialization
	void Start () {
		InitBasePlayer ();

		//temp player color
		playerColor = new Color (0, 255, 255);
	}

	protected void InitBasePlayer()
	{
		milUnits = new List<AssemblyCSharp.MilitaryUnit> ();
		structUnits = new List<AssemblyCSharp.StructureUnit> ();
		
		ownedTiles = new PointList<PointyHexPoint> ();
		DictatorAlive = true;
	}

	public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, PointyHexPoint pointToAdd)
	{
		if (!ownedTiles.Contains (pointToAdd)) {
			ownedTiles.Add (pointToAdd);
			(gameGrid [pointToAdd] as UnitCell).SetTileColor (PlayerColor);
		}
	}

	public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, PointList<PointyHexPoint> pointsToAdd)
	{
		foreach (PointyHexPoint point in pointsToAdd) {
			if (!ownedTiles.Contains (point)) {
				ownedTiles.Add (point);
				(gameGrid [point] as UnitCell).SetTileColor (PlayerColor);
			}
		}
	}

}
