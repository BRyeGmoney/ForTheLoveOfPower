//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using Gamelogic.Grids;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class Settlement : StructureUnit
	{
		public Boolean RefreshCachedBuildings { get; set; }

		public Sprite townHall;
		public Sprite cityHall;
		public Sprite capital;

		public List<StructureUnit> cachedBuildingList;
		public PointList<PointyHexPoint> tilesOwned;
		public float updateTimer;

		public float costOfUnitsInTown = 1.0f;
		public int numEconUnitsProducedInTown = 10;

		public Settlement ()
		{
			cachedBuildingList = new List<StructureUnit> ();
			tilesOwned = new PointList<PointyHexPoint> ();
		}

		public void UpdateBuildingList(Player owningPlayer)
		{
			if (RefreshCachedBuildings)
				RefreshBuildingsOwned ();

			if (updateTimer > 10f) {
				cachedBuildingList.ForEach (build => {
					if (build.StructureType.Equals (StructureUnitType.Factory)) {
						owningPlayer.Cash += 200;
						if (build.modifierAnim < 3) {
							build.AnimationController.SetTrigger ("modifierAnim");
							build.modifierAnim += 1;
						}
					}
				});

				owningPlayer.Cash += 200;

				updateTimer = 0f;
			}

			updateTimer += Time.deltaTime;
		}

		public void RefreshBuildingsOwned() 
		{

		}

		public bool TileBelongsToSettlement(PointyHexPoint tileToCheck) 
		{
			return tilesOwned.Contains (tileToCheck);
		}
	}
}

