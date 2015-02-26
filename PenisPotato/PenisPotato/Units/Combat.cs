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
        Vector2 searchPos;
        public List<Units.Unit> attacker;
        public List<Units.Unit> defender;
        private Color attackColor;
        private Color defendColor;
        public float timeToFight = 0.0f;

        private StateSystem.Screens.GameplayScreen masterState;
        static Random random = new Random();

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
            this.searchPos = attacker.piecePosition;
            attackColor = attacker.playerColor;
            defendColor = defender.playerColor;
        }

        /*public Combat(List<Unit> attacks, List<Unit> defends, StateSystem.Screens.GameplayScreen masterState)
        {
            InitializeCombat();
            this.masterState = masterState;
            attacker = attacks;
            defender = defends;
        }*/

        public void InitializeCombat()
        {
            attacker = new List<Unit>();
            defender = new List<Unit>();
        }

        public void ClearUnitsLists()
        {
            attacker.Clear();
            defender.Clear();
            /*if (attacker.Count > 1)
                attacker.RemoveRange(1, attacker.Count-1);
            if (defender.Count > 1)
                defender.RemoveRange(1, defender.Count-1);*/
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
            List<Vector2> surroundingTiles = GetSurroundingTiles(searchPos);

            masterState.players.Find(player => !player.playerUnits.Contains(toSearchAround)).playerUnits.ForEach(pU =>
            {
                if (surroundingTiles.Contains(pU.piecePosition) && pU.numUnits > 0)
                {
                    if (isAttacker && !defender.Contains(pU))
                        defender.Add(pU);
                    else if (!isAttacker && !attacker.Contains(pU))
                        attacker.Add(pU);
                }
            });
        }

        private void FindNeighboringEnemies(Color playerColor, bool isAttacker)
        {
            List<Vector2> surroundingTiles = GetSurroundingTiles(searchPos);

            masterState.players.Find(player => !player.playerColor.Equals(playerColor)).playerUnits.ForEach(pU =>
            {
                if (surroundingTiles.Contains(pU.piecePosition) && pU.numUnits > 0)
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

        public Double RandomNormal(double mean, double stdDev)
        {
            double r1 = random.NextDouble();
            double r2 = random.NextDouble();
            return mean + (stdDev * (Math.Sqrt(-2 * Math.Log(r1)) * Math.Cos(6.28 * r2)));
        }

        public int RandomNormal(int mean, int stdDev, int min, int max)
        {
            int r1 = random.Next(min, max);
            int r2 = random.Next(min, max);
            return (int)(mean + (stdDev * (Math.Sqrt(-2 * Math.Log(r1)) * Math.Cos(6.28 * r2))));
        }

        public void Update(GameTime gameTime)
        {
            if (timeToFight > 1)
            {
                int attackPower = 0, defendPower = 0;

                ClearUnitsLists();

                //Run the process now to find all of the units around the attacker and defender and add them to the list
                //FindNeighboringEnemies(attacker[0], true);
                //FindNeighboringEnemies(defender[0], false);
                FindNeighboringEnemies(attackColor, true);
                FindNeighboringEnemies(defendColor, false);


                attacker.ForEach(s =>
                {
                    s.inCombat = true;
                    if (s.animPlayer.Animation == null)
                        s.AnimateCombat(masterState.players.Find(player => player.playerUnits.Contains(s)));
                    
                    //Standard random attack power that everyone gets. No modifiers put in yet
                    for (int x = 0; x < s.numUnits; x++)
                        attackPower += RandomNormal(3, 3, 0, 7);
                });

                defender.ForEach(s =>
                {
                    s.inCombat = true;
                    if (s.animPlayer.Animation == null)
                        s.AnimateCombat(masterState.players.Find(player => player.playerUnits.Contains(s)));

                    //Standard random attack power that everyone gets. No modifiers put in yet
                    for (int x = 0; x < s.numUnits; x++)
                        defendPower += RandomNormal(3, 3, 0, 7);
                });

                if (attackPower > defendPower)
                {
                    if (defender.Count > 0)
                        defender[random.Next(0, defender.Count)].KillUnit();
                }
                else if (defendPower > attackPower)
                {
                    if (attacker.Count > 0)
                        attacker[random.Next(0, attacker.Count)].KillUnit();
                }

                //if (attacker.Count < 1 || defender.Count < 1)
                //    masterState.playerOne.combat.Remove(this);

                //Reset the timer
                timeToFight = 0.0f;
            }
            
            timeToFight += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public class SkeletonCombat : Combat
    {

        public long attackingNetworkID;
        public long defendingNetworkID;
        public int combatid;
        public bool combatReady = false;

        public SkeletonCombat(Unit attacker, Unit defender, long attackId, long defendId)
        {
            this.attacker.Add(attacker);
            this.defender.Add(defender);
            attackingNetworkID = attackId;
            defendingNetworkID = defendId;
            combatid = new Random().Next(1001, 5000);
        }

        public void Update(GameTime gameTime, Player.Player player)
        {
            if (attackingNetworkID.Equals(player.netPlayer.uniqueIdentifer))
            {
                if (timeToFight > 0.3)
                {
                    combatReady = true;
                    timeToFight = 0f;
                }
            }

            timeToFight += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// The reason I search for neighbouring enemies instead of allies is because an ally could be behind the current attacking
        /// unit and not directly touching an enemy unit tile. This way i get all enemies surrounding a unit. This is done every time 
        /// the two are about to battle in order to get the freshest information off the battlefield.
        /// </summary>
        /// <param name="toSearchAround"></param>
        /// <param name="isAttacker"></param>
        public List<int> FindNeighboringEnemies(Unit toSearchAround, bool isAttacker, List<Player.NetworkPlayer> peers, Player.MainPlayer mPlayer)
        {
            List<Vector2> surroundingTiles = GetSurroundingTiles(toSearchAround.piecePosition);
            List<int> attackerList = new List<int>();
            List<int> defenderList = new List<int>();

            Player.Player player;
            if (mPlayer == null)
                player = peers.Find(p => !p.playerUnits.Contains(toSearchAround));
            else
                player = mPlayer;

            foreach (Units.Unit pU in player.playerUnits)
            {
                if (surroundingTiles.Contains(pU.piecePosition))
                {
                    if (isAttacker && !defender.Contains(pU))
                        defenderList.Add(player.playerUnits.IndexOf(pU));
                    else if (!isAttacker && !attacker.Contains(pU))
                        attackerList.Add(player.playerUnits.IndexOf(pU));
                }
            }

            if (isAttacker)
                return defenderList;
            else
                return attackerList;
        }
    }

    public class ServerCombat : Combat
    {
        long attackingNetworkID;
        long defendingNetworkID;
        public bool lastWin = false;

        public ServerCombat(Unit attacker, Unit defender, long attackId, long defendId)
        {
            InitializeCombat();

            this.attacker.Add(attacker);
            this.defender.Add(defender);
            this.attackingNetworkID = attackId;
            this.defendingNetworkID = defendId;
        }

        public int Update()
        {

                Random random = new Random();
                int attackPower = 0, defendPower = 0;

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
                {
                    if (defender.Count > 0)
                    {
                        attackPower = random.Next(0, defender.Count);
                        defender[attackPower].KillUnit();
                        lastWin = true;
                        return attackPower;
                    }
                    else
                        return -1;
                }
                else if (defendPower > attackPower)
                {
                    if (attacker.Count > 0)
                    {
                        defendPower = random.Next(0, attacker.Count);
                        attacker[defendPower].KillUnit();
                        lastWin = false;
                        return defendPower;
                    }
                    else
                        return -1;
                }
                else
                {
                    return -1;
                }
        }
    }
}
