using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grids;

public class Player : NetworkBehaviour {

	public List<AssemblyCSharp.MilitaryUnit> milUnits;
	public List<AssemblyCSharp.Settlement> settlements;
	public List<PointyHexPoint> ownedTiles;
	public Int32 Cash { get; set; }

    public Color PlayerColor;
    
	public Boolean DictatorAlive { get; set; }

	// Use this for initialization
	void Start () {
		InitBasePlayer ();
        Cash = 10000;
        PlayerColor = MenuBehaviour.instance.PlayerColor;
	}
	
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

	public bool TileBelongsToSettlements(PointyHexPoint tileToCheck)
	{
		bool belongs = false;

		settlements.ForEach (settlement => {
			if (!belongs)
				belongs = settlement.TileBelongsToSettlement (tileToCheck);
		});

		return belongs;
	}

	public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, AssemblyCSharp.Settlement ownSettlement, Player enemyPlayer, PointList<PointyHexPoint> pointsToAdd)
	{
		foreach (PointyHexPoint point in TileDoesNotBelongToOtherSettlement(gameGrid, ownSettlement, enemyPlayer, pointsToAdd)) {
			if (!ownSettlement.tilesOwned.Contains (point)) {
				ownSettlement.tilesOwned.Add (point);
				//ownedTiles.Add (point);
				(gameGrid [point] as UnitCell).SetTileColorStructure (PlayerColor);
			}
		}
	}

	public void AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, Player enemyPlayer, PointyHexPoint pointToAdd)
	{
		if (!(gameGrid [pointToAdd] as UnitCell).buildingOnTile) {
			ownedTiles.Add (pointToAdd);
		}
	}

	public void RemoveFromOwnedTiles(PointyHexPoint pointToRemove)
	{
		ownedTiles.Remove (pointToRemove);
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
                    if (settlement.tilesOwned.Contains(point))
                        pointsToRemove.Add(point);
				}
			}

		});

		pointsToRemove.ToList<PointyHexPoint>().ForEach (point => {
            (gameGrid[point] as UnitCell).SetTileColorUnOwn();
		});

		return pointsToAdd.Except (pointsToRemove).ToPointList<PointyHexPoint>();
	}
}

public struct PlayerObjects
{

}
