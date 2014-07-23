using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PenisPotato.Units
{
    public class Combat
    {
        private List<Units.Unit> attacker;
        private List<Units.Unit> defender;
        float timeToFight = 0.0f;

        private StateSystem.Screens.GameplayScreen masterState;

        public Combat() 
        {
            InitializeCombat();
        }

        public Combat(Unit attacker, Unit defender, StateSystem.Screens.GameplayScreen masterState)
        {
            InitializeCombat();
            this.masterState = masterState;
            this.attacker.Add(attacker);
            this.defender.Add(defender);
        }

        /*public Combat(List<Unit> attacks, List<Unit> defends, StateSystem.Screens.GameplayScreen masterState)
        {
            InitializeCombat();
            this.masterState = masterState;
            attacker = attacks;
            defender = defends;
        }*/

        private void InitializeCombat()
        {
            attacker = new List<Unit>();
            defender = new List<Unit>();
        }

        private void ClearUnitsLists()
        {
            attacker.RemoveRange(1, attacker.Count);
            defender.RemoveRange(1, defender.Count);
        }


        /// <summary>
        /// The reason I search for neighbouring enemies instead of allies is because an ally could be behind the current attacking
        /// unit and not directly touching an enemy unit tile. This way i get all enemies surrounding a unit. This is done every time 
        /// the two are about to battle in order to get the freshest information off the battlefield.
        /// </summary>
        /// <param name="toSearchAround"></param>
        /// <param name="isAttacker"></param>
        private void FindNeighboringEnemies(Unit toSearchAround, bool isAttacker)
        {
            List<Vector2> surroundingTiles = GetSurroundingTiles(toSearchAround.piecePosition);

            masterState.players.Find(player => !player.playerColor.Equals(toSearchAround.playerColor)).playerUnits.ForEach(pU =>
            {
                if (surroundingTiles.Contains(pU.piecePosition))
                {
                    if (isAttacker && !defender.Contains(pU))
                        defender.Add(pU);
                    else if (!isAttacker && !attacker.Contains(pU))
                        attacker.Add(pU);
                }
            });
        }

        public List<Vector2> GetSurroundingTiles(Vector2 piecePosition)
        {
            List<Vector2> st = new List<Vector2>();

            st.Add(new Vector2(piecePosition.X, piecePosition.Y));
            st.Add(new Vector2(piecePosition.X + 1, piecePosition.Y));
            st.Add(new Vector2(piecePosition.X - 1, piecePosition.Y));
            st.Add(new Vector2(piecePosition.X, piecePosition.Y + 1));
            st.Add(new Vector2(piecePosition.X, piecePosition.Y - 1));
            if (Math.Abs(piecePosition.X % 2) != 0)
            {
                st.Add(new Vector2(piecePosition.X - 1, piecePosition.Y - 1));
                st.Add(new Vector2(piecePosition.X + 1, piecePosition.Y - 1));
            }
            else
            {
                st.Add(new Vector2(piecePosition.X - 1, piecePosition.Y + 1));
                st.Add(new Vector2(piecePosition.X + 1, piecePosition.Y + 1));
            }

            return st;
        }

        public void AddUnit(Unit unit, bool isAttacker)
        {
            if (isAttacker)
                attacker.Add(unit);
            else
                defender.Add(unit);
        }

        public void AddUnit(List<Unit> unit, bool isAttacker)
        {
            if (isAttacker)
                attacker.AddRange(unit);
            else
                defender.AddRange(unit);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (timeToFight > 0.3)
            {
                Random random = new Random();
                int attackPower = 0, defendPower = 0;

                //Run the process now to find all of the units around the attacker and defender and add them to the list
                FindNeighboringEnemies(attacker[0], true);
                FindNeighboringEnemies(defender[0], false);


                attacker.ForEach(s =>
                {
                    //Standard random attack power that everyone gets. No modifiers put in yet
                    for (int x = 0; x < s.numUnits; x++)
                        attackPower += random.Next(0, 6);
                });

                defender.ForEach(s =>
                {
                    //Standard random attack power that everyone gets. No modifiers put in yet
                    for (int x = 0; x < s.numUnits; x++)
                        defendPower += random.Next(0, 6);
                });

                if (attackPower > defendPower)
                    if (defender.Count > 0)
                        defender[random.Next(0, defender.Count)].KillUnit();
                else if (defendPower > attackPower)
                    if (attacker.Count > 0)
                        attacker[random.Next(0, attacker.Count)].KillUnit();

                if (attacker.Count <= 0 || defender.Count <= 0)
                    masterState.combat.Remove(this);

                //Reset the timer
                timeToFight = 0.0f;
            }
            
            timeToFight += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}
