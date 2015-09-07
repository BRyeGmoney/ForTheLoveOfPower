using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grids;

public class Player : MonoBehaviour {

	public List<AssemblyCSharp.MilitaryUnit> milUnits;
	public List<AssemblyCSharp.Settlement> settlements;
	public Int32 Cash { get; set; }

	public Color PlayerColor { get { return playerColor; } }
	protected Color playerColor;

	public Boolean DictatorAlive { get; set; }

	//private PointList<PointyHexPoint> ownedTiles;

	// Use this for initialization
	void Start () {
		InitBasePlayer ();

		//temp player color
		playerColor = new Color32 (0, 255, 255, 255);
	}

	/*protected void InitBasePlayer()
	{
		milUnits = new List<AssemblyCSharp.MilitaryUnit> ();
		settlements = new List<AssemblyCSharp.Settlement> ();
		
		//ownedTiles = new PointList<PointyHexPoint> ();
		DictatorAlive = true;
	}*/
	protected void InitBasePlayer()
	{
		milUnits = new List<AssemblyCSharp.MilitaryUnit> ();
		settlements = new List<AssemblyCSharp.Settlement> ();
		DictatorAlive = true;
	}

	public PointList<PointyHexPoint> GetOwnedTiles()
	{
		PointList<PointyHexPoint> currList = new PointList<PointyHexPoint> ();

		settlements.ForEach (settlement => {
			currList = currList.Union (settlement.tilesOwned).ToPointList<PointyHexPoint>();
		});

		return currList;
	}

	/*public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, PointyHexPoint pointToAdd)
	{
		if (!GetOwnedTiles ().Contains (pointToAdd)) {
			ownedTiles.Add (pointToAdd);
			(gameGrid [pointToAdd] as UnitCell).SetTileColor (PlayerColor);
		}
	}*/

	public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, AssemblyCSharp.Settlement ownSettlement, Player enemyPlayer, PointList<PointyHexPoint> pointsToAdd)
	{
		//Add code to ensure player doesn't build two settlements together here
		foreach (PointyHexPoint point in TileDoesNotBelongToOtherSettlement(gameGrid, ownSettlement, enemyPlayer, pointsToAdd)) {
			if (!ownSettlement.tilesOwned.Contains (point)) {
				ownSettlement.tilesOwned.Add (point);
				(gameGrid [point] as UnitCell).SetTileColor (PlayerColor);
			}
		}
	}

	private PointList<PointyHexPoint> TileDoesNotBelongToOtherSettlement(IGrid<PointyHexPoint> gameGrid, AssemblyCSharp.Settlement ownSettlement, Player enemyPlayer, PointList<PointyHexPoint> pointsToAdd) 
	{
		PointList<PointyHexPoint> pointsToRemove = new PointList<PointyHexPoint>();

		//lets cut out as many points as possible for the next loop to be faster
		pointsToAdd.RemoveAll (point => enemyPlayer.GetOwnedTiles ().Contains (point));
		pointsToRemove.Clear ();

		//make sure two neighboring cities don't collide
		settlements.ForEach (settlement => {
			if (!settlement.Equals (ownSettlement)) { //if we're not currently looking at the same settlement
				foreach (PointyHexPoint point in pointsToAdd) {
					if (settlement.tilesOwned.Contains (point))
						pointsToRemove.Add (point);
				}
			}

		});

		pointsToRemove.ToList<PointyHexPoint>().ForEach (point => {
			(gameGrid[point] as UnitCell).SetTileColor (new Color32(83, 199, 175, 255));
		});

		return pointsToAdd.Except (pointsToRemove).ToPointList<PointyHexPoint>();
	}
}
