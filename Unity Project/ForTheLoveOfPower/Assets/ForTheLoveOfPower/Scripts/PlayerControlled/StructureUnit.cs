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
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Gamelogic.Grids;
using BeautifulDissolves;

namespace AssemblyCSharp
{
	public enum StructureUnitType
	{
		Factory,
		Exporter,
		Market,
		Barracks,
		TankDepot,
		Airport,
		Contractor,
		LabourCamp,
		Propaganda,
        Settlement,
		None,
	}

    public enum StructureState
    {
        BeingBuilt,
        Owned,
        BeingCaptured,
        Captured,
        BeingReclaimed,
    }

	public class StructureUnit : MonoBehaviour, Assets.ForTheLoveOfPower.Scripts.PlayerControlled.IObjectPoolItem
	{
        //Interface Properties
        public bool PoolObjInUse { get; set; }

        //Properties
        public Color StructColor { get; set; }
		public PointyHexPoint TilePoint { get; set; }

        public Int16 ID { get { return id; } }
        protected short id;

        public StructureUnitType StructureType
		{
			get { return structureType; }
			set { structureType = value; }
		}
		private StructureUnitType structureType;

		public Settlement OwningSettlement { get; set; }

        public Material MyMaterial;
        public SpriteRenderer spriteRender;

		public Animator AnimationController 
		{
			get { return animator; }
		}
		private Animator animator;

		public int modifierAnim;
        public float percentageConquered = 0;
        public StructureState currentState;

        public StructureUnit ()
		{
		}

		public void Initialize(short id, Color structColor, StructureUnitType structType, PointyHexPoint gridPoint)
		{
            this.id = id;
			StructColor = structColor;
			StructureType = structType;
			TilePoint = gridPoint;
            spriteRender = gameObject.GetComponent<SpriteRenderer>();
			spriteRender.color = StructColor;
            MyMaterial = spriteRender.material;
			animator = gameObject.GetComponent<Animator> ();

            SetTagByType();
            currentState = StructureState.Owned; //Temporary, change this to BeingBuilt
        }

		public void Initialize(short id, Color structColor, StructureUnitType structType, PointyHexPoint gridPoint, Settlement owningSettlement)
		{
			Initialize (id, structColor, structType, gridPoint);
			OwningSettlement = owningSettlement;
		}

		public void UpdateBuilding()
		{
            if (currentState.Equals(StructureState.BeingBuilt))
            {
                if (MyMaterial.GetFloat(DissolveHelper.dissolveAmountID) <= 0f)
                    currentState = StructureState.Owned;
            }
            if (currentState.Equals(StructureState.BeingCaptured))
            {
                percentageConquered = Mathf.Clamp(percentageConquered + (Time.deltaTime / 5), 0, 1);
                MyMaterial.SetFloat(DissolveHelper.dissolveAmountID, percentageConquered);

                if (percentageConquered >= 1)
                    currentState = StructureState.Captured;
            }
		}


        //Call this from the actual grid file
        public void BeginCapturing(Color newColor)
        {
            MyMaterial.SetColor("_SubColor", newColor);
        }

        private void SetTagByType()
        {
            int typeNum = (int)StructureType;
            if (typeNum < 3)
                this.tag = "Economy";
            else if (typeNum < 6)
                this.tag = "Military";
            else if (typeNum < 9)
                this.tag = "Manipulation";
        }

        public static int GetCostOfStructure(StructureUnitType tryingToBuild)
        {
            if (tryingToBuild.Equals(StructureUnitType.Settlement))
            {
                return 1000;
            }
            else if (tryingToBuild.Equals(StructureUnitType.Barracks))
            {
                return 1000;
            }
            else if (tryingToBuild.Equals(StructureUnitType.TankDepot))
            {
                return 2000;
            }
            else if (tryingToBuild.Equals(StructureUnitType.Airport))
            {
                return 3000;
            }
            else if (tryingToBuild.Equals(StructureUnitType.Factory))
            {
                return 1000;
            }
            else if (tryingToBuild.Equals(StructureUnitType.Exporter))
            {
                return 2000;
            }
            else if (tryingToBuild.Equals(StructureUnitType.Market))
            {
                return 3000;
            }
            else if (tryingToBuild.Equals(StructureUnitType.LabourCamp))
            {
                return 1000;
            }
            else if (tryingToBuild.Equals(StructureUnitType.Contractor))
            {
                return 2000;
            }
            else if (tryingToBuild.Equals(StructureUnitType.Propaganda))
            {
                return 3000;
            }
            else
            {
                return 1000000;
            }
        }
	}

    public class Civilization
    {
        public List<CivUpdate> dirtyUnits;
        public List<Settlement> settlements;

        List<PointyHexPoint> ownedTiles;

        short NextStructID;
        short NextSettleID;

        public Civilization()
        {
            dirtyUnits = new List<CivUpdate>();
            settlements = new List<Settlement>();
        }

        public Int16 GetNextSettleID()
        {
            return NextSettleID++;
        }

        public Int16 GetNextStructID()
        {
            return NextStructID++;
        }

        #region Settlements
        public Int32 GetSettlementsCount()
        {
            return settlements.Count;
        }

        public Settlement FindSettlementByID(short id)
        {
            return settlements.Find(settlement => settlement.ID == id);
        }

        public void AddToSettlements(Settlement newSettlement)
        {
            settlements.Add(newSettlement);
        }

        public IEnumerable<Settlement> SettlementsUnderAttack()
        {
            return settlements.Where(settlement => settlement.cityBeingConquered);
        }

        public void UpdateSettlements(Player owningPlayer)
        {
            settlements.ForEach(settlement =>
            {
                settlement.UpdateBuildingList(owningPlayer);
            });
        }

        public void RemoveFromSettlements(Settlement settlement)
        {
            settlements.Remove(settlement);
        }

        #endregion

        public void BuildNewSettlement(PointyHexPoint buildPoint, Player enemyPlayer, Color playerColor, short id = -1)
        {
            UnitCell gridCell = GameGridBehaviour.instance.Grid[buildPoint] as UnitCell;
            Settlement newSettlement;

            if (id < 0)
                newSettlement = ObjectPool.instance.PullNewSettlement(gridCell.transform.position);
            else
                newSettlement = ObjectPool.instance.PullNewSettlement(gridCell.transform.position, id);

            newSettlement.Initialize(GetNextSettleID(), playerColor, StructureUnitType.Settlement, buildPoint);
            AddToSettlements(newSettlement);
            AddToSettlementOwnedTiles(GameGridBehaviour.instance.Grid, newSettlement, enemyPlayer, GameGridBehaviour.instance.GetSurroundingTiles(buildPoint), playerColor);
            gridCell.AddStructureToTile(newSettlement);

            dirtyUnits.Add(new CivUpdate() { mpCommand = (short)CivMpCommands.NewSettlement, Structure = newSettlement });

            newSettlement.GetComponent<BeautifulDissolves.Dissolve>().TriggerDissolve();

        }

        public void BuildNewStructure(PointyHexPoint buildPoint, StructureUnitType structType, Settlement owningSettlement, Player enemyPlayer, Color playerColor, short id = -1)
        {
            UnitCell gridCell = GameGridBehaviour.instance.Grid[buildPoint] as UnitCell;
            StructureUnit newStruct;
            if (id < 0)
                newStruct = ObjectPool.instance.PullNewStructure(structType, gridCell.transform.position);
            else
                newStruct = ObjectPool.instance.PullNewStructure(structType, gridCell.transform.position, id);

            newStruct.Initialize(GetNextStructID(), playerColor, structType, buildPoint, owningSettlement);
            owningSettlement.AddToBuildingList(newStruct);
            AddToSettlementOwnedTiles(GameGridBehaviour.instance.Grid, owningSettlement, enemyPlayer, GameGridBehaviour.instance.GetSurroundingTiles(buildPoint), playerColor);
            gridCell.AddStructureToTile(newStruct);

            dirtyUnits.Add(new CivUpdate() { mpCommand = (short)CivMpCommands.NewStructure, Structure = newStruct });

            newStruct.GetComponent<BeautifulDissolves.Dissolve>().TriggerDissolve();
        }

        #region Tile Functions
        public PointList<PointyHexPoint> GetOwnedTiles()
        {
            PointList<PointyHexPoint> currList = new PointList<PointyHexPoint>();

            settlements.ForEach(settlement => {
                currList = currList.Union(settlement.tilesOwned).ToPointList<PointyHexPoint>();
            });

            return currList;
        }

        public bool TileBelongsToSettlements(PointyHexPoint tileToCheck)
        {
            bool belongs = false;

            settlements.ForEach(settlement => {
                if (!belongs)
                    belongs = settlement.TileBelongsToSettlement(tileToCheck);
            });

            return belongs;
        }

        public void AddToSettlementOwnedTiles(IGrid<PointyHexPoint> gameGrid, AssemblyCSharp.Settlement ownSettlement, Player enemyPlayer, PointList<PointyHexPoint> pointsToAdd, Color playerColor)
        {
            foreach (PointyHexPoint point in TileDoesNotBelongToOtherSettlement(gameGrid, ownSettlement, enemyPlayer, pointsToAdd))
            {
                if (!ownSettlement.tilesOwned.Contains(point))
                {
                    ownSettlement.tilesOwned.Add(point);
                    //ownedTiles.Add (point);
                    (gameGrid[point] as UnitCell).SetTileColorBuildable(playerColor);
                }
            }
        }

        public bool AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, PointyHexPoint pointToAdd)
        {
            if (!(gameGrid[pointToAdd] as UnitCell).buildingOnTile)
            {
                ownedTiles.Add(pointToAdd);
                return true;
            }
            else
                return false;
        }

        public bool AddToOwnedTiles(IGrid<PointyHexPoint> gameGrid, PointList<PointyHexPoint> pointsToAdd)
        {
            return pointsToAdd.All(point => AddToOwnedTiles(gameGrid, point));
        }

        public bool RemoveFromOwnedTiles(PointyHexPoint pointToRemove)
        {
            return ownedTiles.Remove(pointToRemove);
        }

        public bool RemoveFromOwnedTiles(PointList<PointyHexPoint> pointsToRemove)
        {
            return pointsToRemove.All(point => RemoveFromOwnedTiles(point));
        }

        private PointList<PointyHexPoint> TileDoesNotBelongToOtherSettlement(IGrid<PointyHexPoint> gameGrid, AssemblyCSharp.Settlement ownSettlement, Player enemyPlayer, PointList<PointyHexPoint> pointsToAdd)
        {
            PointList<PointyHexPoint> pointsToRemove = new PointList<PointyHexPoint>();

            //lets cut out as many points as possible for the next loop to be faster
            pointsToAdd.RemoveAll(point => enemyPlayer.playerCiv.GetOwnedTiles().Contains(point));
            pointsToRemove.Clear();

            //make sure two neighboring cities don't collide
            settlements.ForEach(settlement => {
                if (!settlement.Equals(ownSettlement))
                { //if we're not currently looking at the same settlement
                    foreach (PointyHexPoint point in pointsToAdd)
                    {
                        if (settlement.tilesOwned.Contains(point))
                            pointsToRemove.Add(point);
                    }
                }

            });

            pointsToRemove.ToList<PointyHexPoint>().ForEach(point => {
                (gameGrid[point] as UnitCell).SetTileColorUnOwn();
            });

            return pointsToAdd.Except(pointsToRemove).ToPointList<PointyHexPoint>();
        }
        #endregion
    }

    public class CivUpdate
    {
        public Int16 mpCommand { get; set; }
        public StructureUnit Structure { get; set; }
    }

    public enum CivMpCommands
    {
        NewSettlement = 400,
        NewStructure = 401
    }
}

