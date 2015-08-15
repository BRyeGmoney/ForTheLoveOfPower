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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;
using Gamelogic;
using Gamelogic.Grids;

namespace AssemblyCSharp
{
	public enum MilitaryUnitType
	{
		Dictator,
		Infantry,
		Tank,
		Jet,
	}

	public enum MilitaryUnitAnimationIndex
	{
		Circle = 0,
		Pentagon = 1,
		Square = 2,
		Triangle = 3,
		Infantry_Idle = 4,
		Infantry_Move = 5,
		Plane_Idle = 6,
		Plane_Combat = 7,
		Plane_Death = 8,
		Plane_Move = 9,
		Tank_Idle = 10,
		Tank_Combat = 11,
		Tank_Death = 12,
		Tank_Move = 13,
		Dictator_Idle = 14,
	}

	public class MilitaryUnit
	{
		private PointList<PointyHexPoint> movementPath;
		private int unitAmount = 1;
		private float moveTime;

		//Properties
		public List<MilitaryUnit> Subordinates
		{
			get { return subordinates; }
		}
		private List<MilitaryUnit> subordinates;

		public MilitaryUnitType UnitType 
		{ 
			get { return unitType; } 
			set { unitType = value; } 
		}
		private MilitaryUnitType unitType;



		public Int16 IdleAnimation { get; set; }
		public Int16 CombatAnimation { get; set; }
		public Int16 DeathAnimation { get; set; }
		public Int16 MoveAnimation { get; set; }

		public Color UnitColor { get; set; }
		public float MoveTimeLimit { get; set; }
		public PointyHexPoint TilePoint { get; set; }

		public Combat combatToUpdateGame;


		public MilitaryUnit ()
		{
		}

		/// <summary>
		/// Gets the amount of units on this one tile
		/// </summary>
		/// <returns>The unit amount.</returns>
		public Int32 GetUnitAmount()
		{
			return unitAmount;
		}

		/// <summary>
		/// Pulls the next position along the movement path previously defined for the unit
		/// and manually performs the actions to visually place it there too.
		/// </summary>
		/// <param name="grid">The playing Grid</param>
		/// <param name="unitSprites">An array of pre-loaded sprites</param>
		public void MoveNextMoveInPath(IGrid<PointyHexPoint> grid, Sprite[] unitSprites)
		{
			if (movementPath != null) {
				if (movementPath.Count > 1) {
					if ((grid[movementPath[1]] as UnitCell).unitOnTile == null) {
						(grid [movementPath [0]] as UnitCell).RemoveUnit ();
						movementPath.RemoveAt (0);
						(grid [movementPath [0]] as UnitCell).AddUnitToTile (this, unitSprites);
						TilePoint = movementPath[0];
					} else {
						MilitaryUnit unitOnTile = (grid[movementPath[1]] as UnitCell).unitOnTile;

						if (!unitOnTile.UnitColor.Equals (this.UnitColor)) {
							combatToUpdateGame = new Combat();
							combatToUpdateGame.Setup (this, unitOnTile);
						}
						movementPath.Clear ();
					}
				}
			}
		}

		public void SetMovementPath(PointList<PointyHexPoint> newPath)
		{
			movementPath = newPath;
		}

		public void AddUnits(int amountToAdd)
		{
			unitAmount += amountToAdd;
		}

		public void RemoveUnits(int amountToRemove)
		{
			unitAmount -= amountToRemove;
		}

		public void AddSubordinate(MilitaryUnit unitToCommand)
		{
			if (subordinates == null)
				subordinates = new List<MilitaryUnit> ();
			
			subordinates.Add (unitToCommand);
		}

		public void AddSubordinates(List<MilitaryUnit> unitsToCommand)
		{
			if (subordinates == null)
				subordinates = new List<MilitaryUnit> ();

			subordinates.AddRange (unitsToCommand);
		}

		public void UpdateUnit(IGrid<PointyHexPoint> grid, Sprite[] unitSprites, Player playerInChargeOfUnit)
		{
			if (unitAmount <= 0) {
				playerInChargeOfUnit.milUnits.Remove (this);
			} else { //if he's still alive then lets update him
				if (moveTime > MoveTimeLimit) {
					MoveNextMoveInPath (grid, unitSprites);
					moveTime = 0f;
				}

				moveTime += Time.deltaTime;
			}
		}
	}

	public static class CreateMilitaryUnit
	{
		public static MilitaryUnit CreateDictator(Color unitColor, PointyHexPoint currentPoint)
		{
			return new MilitaryUnit() { UnitType = MilitaryUnitType.Dictator, IdleAnimation = (short)MilitaryUnitAnimationIndex.Dictator_Idle, UnitColor = unitColor, 
				MoveTimeLimit = 2f, TilePoint = currentPoint };
		}

		public static MilitaryUnit CreateInfantry(Color unitColor, PointyHexPoint currentPoint)
		{
			return new MilitaryUnit () { UnitType = MilitaryUnitType.Infantry, IdleAnimation = (short)MilitaryUnitAnimationIndex.Infantry_Idle, 
				MoveAnimation = (short)MilitaryUnitAnimationIndex.Infantry_Move, UnitColor = unitColor, MoveTimeLimit = 1.75f, TilePoint = currentPoint };
		}

		public static MilitaryUnit CreateTank(Color unitColor, PointyHexPoint currentPoint)
		{
			return new MilitaryUnit () { UnitType = MilitaryUnitType.Tank, IdleAnimation = (short)MilitaryUnitAnimationIndex.Tank_Idle, 
				MoveAnimation = (short)MilitaryUnitAnimationIndex.Tank_Move, CombatAnimation = (short)MilitaryUnitAnimationIndex.Tank_Combat,
				DeathAnimation = (short)MilitaryUnitAnimationIndex.Tank_Death, UnitColor = unitColor, MoveTimeLimit = 1.25f, TilePoint = currentPoint };
		}

		public static MilitaryUnit CreatePlane(Color unitColor, PointyHexPoint currentPoint)
		{
			return new MilitaryUnit () { UnitType = MilitaryUnitType.Jet, IdleAnimation = (short)MilitaryUnitAnimationIndex.Plane_Idle,
				MoveAnimation = (short)MilitaryUnitAnimationIndex.Plane_Move, CombatAnimation = (short)MilitaryUnitAnimationIndex.Plane_Combat,
				DeathAnimation = (short)MilitaryUnitAnimationIndex.Plane_Death, UnitColor = unitColor, MoveTimeLimit = 1f, TilePoint = currentPoint };
		}
	}
}