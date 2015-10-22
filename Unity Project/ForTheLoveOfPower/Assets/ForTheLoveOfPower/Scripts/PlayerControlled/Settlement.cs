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
        public SpriteRenderer spriteRenderer;

		public List<StructureUnit> cachedBuildingList;
		public PointList<PointyHexPoint> tilesOwned;
		public float updateTimer;

		public float costOfUnitsInTown = 1.0f;
		public int numEconUnitsProducedInTown = 10;

        private int curStateOfSettlement = 0;
        public bool newBuildingAdded = false;
        public bool isDictatorInCity = false;

		public Settlement ()
		{
			cachedBuildingList = new List<StructureUnit> ();
			tilesOwned = new PointList<PointyHexPoint> ();
		}

		public void UpdateBuildingList(Player owningPlayer)
		{

            //lets check if we can update the settlement based on whether it meets the criteria, the flags are set when a building is added, the dictator enters, or leaves
            if (newBuildingAdded || isDictatorInCity)
            {
                if (curStateOfSettlement == 0 && cachedBuildingList.Count > 9)
                {
                    curStateOfSettlement = 1;
                    spriteRenderer.sprite = MenuBehaviour.instance.townHall;
                }
                else if ((curStateOfSettlement == 1 && cachedBuildingList.Count > 19) || (curStateOfSettlement == 3 && !isDictatorInCity))
                {
                    curStateOfSettlement = 2;
                    spriteRenderer.sprite = MenuBehaviour.instance.cityHall;
                }
                else if (curStateOfSettlement == 2 && isDictatorInCity)
                {
                    curStateOfSettlement = 3;
                    spriteRenderer.sprite = MenuBehaviour.instance.capital;
                }

                newBuildingAdded = false;
            }
                

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

