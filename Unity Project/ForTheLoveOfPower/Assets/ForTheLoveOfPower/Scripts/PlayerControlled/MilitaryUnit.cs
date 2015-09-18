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

	public class MilitaryUnit : MonoBehaviour
	{
		private PointList<PointyHexPoint> movementPath;
		private int unitAmount = 1;
		private float moveTime;
		private bool facingRight = true;

		//Properties
		public List<MilitaryUnit> Subordinates
		{
			get { return subordinates; }
		}
		private List<MilitaryUnit> subordinates;

		public MilitaryUnit commandingUnit;

		public MilitaryUnitType UnitType 
		{ 
			get { return unitType; } 
			set { unitType = value; } 
		}
		private MilitaryUnitType unitType;

		public Color UnitColor { get; set; }
		public float MoveTimeLimit { get; set; }
		public PointyHexPoint TilePoint { get; set; }

		public Combat combatToUpdateGame;

		public GameObject squadLeader;
		public TextMesh unitNumText;

		//private SpriteRenderer SpriteGuy { get; set; }

		public Animator AnimationController 
		{
			get { return animator; }
		}
		private Animator animator;


		public MilitaryUnit ()
		{
		}

		public void Initialize(Color unitColor, MilitaryUnitType uType, PointyHexPoint curPoint)
		{
			UnitColor = unitColor;
			TilePoint = curPoint;
			UnitType = uType;
			gameObject.GetComponentInChildren<SpriteRenderer> ().color = UnitColor;
			//SpriteGuy.color = UnitColor;
			animator = gameObject.GetComponentInChildren<Animator> ();

			if (!uType.Equals (MilitaryUnitType.Dictator)) {

				squadLeader = gameObject.transform.GetChild (1).gameObject;
				unitNumText = gameObject.GetComponentInChildren<TextMesh> ();
			}

			GetMoveTimeLimitByType ();
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
		public void MoveNextMoveInPath(IGrid<PointyHexPoint> grid, Player[] listOfPlayers)
		{
			UnitCell newCell;

			if (movementPath != null) {
				if (movementPath.Count > 1) {
					newCell = grid[movementPath[1]] as UnitCell;

					if (!newCell.unitOnTile || (subordinates != null && subordinates.Exists (sub => sub.TilePoint.Equals (movementPath[1])))) {
						if (subordinates != null && subordinates.Count > 0) {
							bool canMove = true;
							subordinates.ForEach (subby => {
								if (canMove) { //only continue the cycle if canmove hasn't already been set to false
									PointyHexPoint newPoint = subby.TilePoint + (movementPath[1] - movementPath[0]);
									canMove = subby.CheckIfNextSpotClear (newPoint, grid[newPoint] as UnitCell);
								}
							});

							if (canMove) { //move all subbs and the unit itself
								subordinates.ForEach (subby => {
									subby.MoveToNext (grid, grid[subby.movementPath[1]] as UnitCell);
								});

								MoveToNext (grid, newCell);
							}
						} else {
							MoveToNext (grid, newCell);
						}
					} else {
						//Stop the moving animation because we're going to be fighting now
						StartMovingAnimation (false);

						MilitaryUnit unitOnTile;

						if (newCell.Color.Equals (UnitColor)) { //if it is the same player's units
							unitOnTile = listOfPlayers[0].milUnits.Find (mU => mU.TilePoint.Equals (movementPath[1]));
							if (unitOnTile.unitType.Equals (UnitType)) {//if its the same as the current unit type
								unitOnTile.AddUnits (unitAmount);
								(grid[movementPath[0]] as UnitCell).RemoveUnit ();
								Destroy (gameObject);
							} else {
								if (subordinates != null && subordinates.Count > 0)
								{
									unitOnTile.AddSubordinates (subordinates);
									RemoveSubordinates (subordinates);
								}

								unitOnTile.AddSubordinate (this);
							}
						} else { //then it must be the other players'
							unitOnTile = listOfPlayers[1].milUnits.Find(unit => unit.TilePoint.Equals(movementPath[1]));

							if (unitOnTile != null) {
								combatToUpdateGame = new Combat();
								combatToUpdateGame.Setup (this, unitOnTile);
							}
						}
						movementPath.Clear ();
					}
				}
			}
		}

		public bool CheckIfNextSpotClear(PointyHexPoint nextPoint, UnitCell nextCell) 
		{
			bool canMove = false;

			if (nextCell.Color.Equals (UnitColor)) { //if its one of ours, check if he's part of the group
				if (!commandingUnit.TilePoint.Equals (nextPoint) && nextCell.unitOnTile)
					canMove = commandingUnit.CheckIfSubordinateExists (nextPoint);
				else
					canMove = true;
			} else if (nextCell.Color.Equals (GameGridBehaviour.baseFloorColor)) { //if its the same as the floor color
				canMove = true;
			}

			if (canMove)
				movementPath.Add (nextPoint);

			return canMove;
		}

		public void MoveToNext(IGrid<PointyHexPoint> grid, UnitCell newCell)
		{
			(grid [movementPath [0]] as UnitCell).RemoveUnit ();
			movementPath.RemoveAt (0);
			
			newCell.AddUnitToTile (this);
			gameObject.transform.position = newCell.transform.position;
			TilePoint = movementPath[0];
			
			if (movementPath.Count <= 1)
				StartMovingAnimation(false);
		}

		public void ChangeSpriteDirection(UnitCell nextPoint)
		{
			if (nextPoint.transform.position.x < gameObject.transform.position.x && facingRight)
			{
				animator.transform.Rotate (0, 180, 0);
				facingRight = false;
			}
			else if (nextPoint.transform.position.x > gameObject.transform.position.x && !facingRight)
			{
				animator.transform.Rotate (0, 180, 0);
				facingRight = true;
			}
		}

		void StartMovingAnimation(bool move) {
			animator.SetBool ("isMoving", move);
		}

		public void StartCombatAnimation() {
			animator.SetTrigger ("inCombat");
		}

		public void StopCombatAnimation() {
			animator.SetTrigger ("wonCombat");
		}

		public void StartDeathAnimation() {
			animator.SetTrigger ("isKilled");
		}

		public void SetMovementPath(PointList<PointyHexPoint> newPath)
		{
			movementPath = newPath;
			if (movementPath.Count > 0)
				StartMovingAnimation (true);
		}

		public void AddUnits(int amountToAdd)
		{
			unitAmount += amountToAdd;

			if (unitAmount > 1)
				unitNumText.text = unitAmount.ToString ();
		}

		public void RemoveUnits(int amountToRemove)
		{
			unitAmount -= amountToRemove;
		}

		public bool CheckIfSubordinateExists(PointyHexPoint pointToCheck) 
		{
			if (subordinates != null) {
				if (subordinates.Exists (unit => unit.TilePoint.Equals (pointToCheck)))
					return true;
				else
					return false;
			}

			return false;
		}

		public void AddSubordinate(MilitaryUnit unitToCommand)
		{
			if (subordinates == null)
				subordinates = new List<MilitaryUnit> ();

			squadLeader.SetActive (true);

			subordinates.Add (unitToCommand);
			unitToCommand.commandingUnit = this;
		}

		public void AddSubordinates(List<MilitaryUnit> unitsToCommand)
		{
			if (subordinates == null)
				subordinates = new List<MilitaryUnit> ();

			squadLeader.SetActive (true);

			unitsToCommand.ForEach (sub => sub.commandingUnit = this);

			subordinates.AddRange (unitsToCommand);
		}

		public void RemoveSubordinate(MilitaryUnit unitToRemove)
		{
			subordinates.Remove (unitToRemove);

			if (subordinates.Count < 1)
				squadLeader.SetActive (false);
		}


		public void RemoveSubordinates(List<MilitaryUnit> unitsToRemove)
		{
			unitsToRemove.ForEach (unit => {
				subordinates.Remove (unit);
			});

			if (subordinates.Count < 1)
				squadLeader.SetActive (false);
		}

		public void UpdateUnit(IGrid<PointyHexPoint> grid, Player[] listOfPlayers)
		{
			if (unitAmount <= 0) {
				listOfPlayers[0].milUnits.Remove (this);
			} else { //if he's still alive then lets update him
				if (moveTime > MoveTimeLimit) {
					MoveNextMoveInPath (grid, listOfPlayers);
					moveTime = 0f;
				}

				moveTime += Time.deltaTime;
			}
		}

		private void GetMoveTimeLimitByType() {
			if (UnitType.Equals (MilitaryUnitType.Dictator))
				MoveTimeLimit = 2f;
			else if (UnitType.Equals (MilitaryUnitType.Infantry))
				MoveTimeLimit = 1.75f;
			else if (UnitType.Equals (MilitaryUnitType.Tank))
				MoveTimeLimit = 1.25f;
			else
				MoveTimeLimit = 1f;
		}

		public float GetUnitAnimationTime()
		{
			if (UnitType.Equals (MilitaryUnitType.Infantry))
				return 0.7272727f;
			else if (UnitType.Equals (MilitaryUnitType.Tank))
				return 0.625f;
			else if (UnitType.Equals (MilitaryUnitType.Jet))
				return 0.7857143f;
			else
				return 0.5f;
		}
	}

	public static class CreateMilitaryUnit
	{
		/*public static MilitaryUnit CreateDictator(Color unitColor, PointyHexPoint currentPoint)
		{
			return new MilitaryUnit() { UnitType = MilitaryUnitType.Dictator, unitColor = unitColor, 
				MoveTimeLimit = 2f, tilePoint = currentPoint };
		}

		public static MilitaryUnit CreateInfantry(Color unitColor, PointyHexPoint currentPoint)
		{
			return new MilitaryUnit () { UnitType = MilitaryUnitType.Infantry, unitColor = unitColor, MoveTimeLimit = 1.75f, tilePoint = currentPoint };
		}

		public static MilitaryUnit CreateTank(Color unitColor, PointyHexPoint currentPoint)
		{
			return new MilitaryUnit () { UnitType = MilitaryUnitType.Tank, unitColor = unitColor, MoveTimeLimit = 1.25f, tilePoint = currentPoint };
		}

		public static MilitaryUnit CreatePlane(Color unitColor, PointyHexPoint currentPoint)
		{
			return new MilitaryUnit () { UnitType = MilitaryUnitType.Jet, unitColor = unitColor, MoveTimeLimit = 1f, tilePoint = currentPoint };
		}*/
	}
}