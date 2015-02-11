using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PenisPotato.Graphics.Animation;

namespace PenisPotato.Units
{
    public class Unit
    {
        public Texture2D pieceTexture;
        public Vector2 piecePosition;
        public Color playerColor;
        public float moveTime;
        public List<Vector2> movementPoints;
        public float unitSpeed;
        public int tileWidth;
        public byte unitType;
        public UnitType goodAgainst;
        public bool canBuild = true;
        public bool inCombat = false;
        public Graphics.Animation.AnimationPlayer animPlayer;
        public List<Unit> followingUnits;
        public int unitID;
        private int leaderUnitIndex = -1;
        public int numUnits = 1;
        private bool needsUpdate = false;

        private SpriteEffects unitEffects;

        public Unit() { }

        public Unit(Vector2 position, Texture2D pieceText, int unitId)
        {
            this.piecePosition = position;
            this.pieceTexture = pieceText;
            this.movementPoints = new List<Vector2>();
            this.numUnits = 1;
            this.unitID = unitId;
        }

        public virtual void LoadContent(ContentManager Content) { }

        public void AddUnits(int unitsToAdd)
        {
            numUnits += unitsToAdd;
            needsUpdate = true;
        }

        public void RemoveUnits(int unitsToRemove)
        {
            numUnits -= unitsToRemove;
            if (numUnits < 1)
                this.animPlayer.Animation = null;

            needsUpdate = true;
        }

        public void KillUnit()
        {
            numUnits -= 1;
            if (numUnits < 1)
                this.animPlayer.Animation = null;

            needsUpdate = true;
        }

        public void AddFollowingUnit(Unit followingUnit, Player.Player player)
        {
            if (followingUnits == null)
                followingUnits = new List<Unit>();

            followingUnits.Add(followingUnit);
            followingUnit.leaderUnitIndex = this.unitID;

            if (followingUnit.followingUnits != null)
            {
                followingUnit.followingUnits.ForEach(fU => fU.leaderUnitIndex = this.unitID);
                followingUnits.AddRange(followingUnit.followingUnits);
                followingUnit.followingUnits.Clear();
                followingUnit.followingUnits = null;
            }
        }

        public void RemoveFollowingUnit(Unit followingUnit)
        {
            if (followingUnits.Contains(followingUnit))
            {
                followingUnit.leaderUnitIndex = -1;
                followingUnits.Remove(followingUnit);
            }

            if (followingUnits.Count.Equals(0))
                followingUnits = null;
        }

        public Unit CheckIfEnemyOnTile(Player.Player player, Unit checkingUnit)
        {
            Unit unitOnTile = null;

            player.masterState.players.ForEach(curPlayer =>
            {
                if (curPlayer.playerUnits.Exists(pU => pU.piecePosition.Equals(checkingUnit.movementPoints[0]) && !pU.Equals(checkingUnit)))
                    unitOnTile = curPlayer.playerUnits.Find(pU => ((pU.piecePosition.Equals(checkingUnit.movementPoints[0]))));

                if (unitOnTile != null && checkingUnit.followingUnits != null && checkingUnit.followingUnits.Contains(unitOnTile))
                    unitOnTile = null;

                if (curPlayer.playerSettlements.Exists(pS => pS.piecePosition.Equals(checkingUnit.movementPoints[0])))
                {
                    Structures.Civil.Settlement curSettlement = curPlayer.playerSettlements.Find(pS => pS.piecePosition.Equals(checkingUnit.movementPoints[0]));
                    curSettlement.isCityBeingConquered = true;
                    if (player.netPlayer != null)
                        player.netPlayer.packetsToSend.Enqueue(new Player.StructureNetworkPacket() { packetType = (byte)Player.PacketType.SETTLEMENT_UPDATE, building = curSettlement, invaderId = player.netPlayer.uniqueIdentifer, defenderId = (curPlayer as Player.NetworkPlayer).uniqueIdentifer, lengthOfTransmission = 4 });
                }
            });

            return unitOnTile;
        }

        public virtual void Update(GameTime gameTime, Player.Player player)
        {
            //canBuild = true;

            if (movementPoints.Count > 0)
            {
                moveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                //If a unit tries to move and is owned by a leader, remove this unit from the leader's units.
                if (leaderUnitIndex > -1)
                {
                    if (player.playerUnits.Where(u => u.unitID.Equals(leaderUnitIndex)).Count() > 0)
                        player.playerUnits.Find(pU => pU.unitID.Equals(leaderUnitIndex)).RemoveFollowingUnit(this);
                    else
                        leaderUnitIndex = -1;
                }

                FlipUnit();


                if (movementPoints.Count > 0 && moveTime > unitSpeed)
                {
                    Unit unitOnTile = null;
                    moveTime = 0f;

                    AnimateMovement(player);

                    unitOnTile = CheckIfEnemyOnTile(player, this);

                    if (unitOnTile == null || (followingUnits != null && followingUnits.Contains(unitOnTile)))
                    {
                        //if there is no unit here then we continue on as if nothing
                        Vector2 diff = new Vector2(movementPoints[0].X - piecePosition.X, (movementPoints[0].Y - piecePosition.Y));
                        bool canMove = true;
                        //see if the unit can move
                        canBuild = CheckIfBuildOnTile(player, movementPoints[0]);

                        if (followingUnits != null && canBuild)
                        {
                            List<Vector2> unitPoss = new List<Vector2>();
                            //bool allowY = piecePosition.X > diff.X & piecePosition.Y >= diff.Y || piecePosition.X < diff.Y & piecePosition.Y <= diff.Y;
                            followingUnits.ForEach(fU =>
                                {
                                    if (canMove)
                                    {
                                        fU.AnimateMovement(player);
                                        fU.unitEffects = this.unitEffects;
                                        if ((fU.piecePosition.X - piecePosition.X) % 2 != 0)
                                        {
                                            if (Math.Abs(diff.X) > 0)
                                            {
                                                if ((fU.piecePosition.X + diff.X) % 2 != 0)
                                                {
                                                    if (diff.Y < 0)
                                                        fU.movementPoints.Add(new Vector2(fU.piecePosition.X + diff.X, fU.piecePosition.Y));//fU.piecePosition = new Vector2(fU.piecePosition.X + diff.X, fU.piecePosition.Y);
                                                    else
                                                        fU.movementPoints.Add(new Vector2(fU.piecePosition.X + diff.X, fU.piecePosition.Y + 1));
                                                }
                                                else
                                                {
                                                    if (diff.Y == 0)
                                                        fU.movementPoints.Add(new Vector2(fU.piecePosition.X + diff.X, fU.piecePosition.Y - 1));
                                                    else
                                                        fU.movementPoints.Add(new Vector2(fU.piecePosition.X + diff.X, fU.piecePosition.Y));
                                                }
                                            }
                                            else
                                                fU.movementPoints.Add(new Vector2(fU.piecePosition.X, fU.piecePosition.Y + diff.Y));
                                        }
                                        else
                                            fU.movementPoints.Add(new Vector2(fU.piecePosition.X + diff.X, fU.piecePosition.Y + diff.Y));

                                        if (CheckIfBuildOnTile(player, fU.movementPoints.First()))
                                            fU.canBuild = true;
                                        else
                                            fU.canBuild = false;
                                        unitOnTile = CheckIfEnemyOnTile(player, fU);
                                        if (unitOnTile != null && unitOnTile != this && !followingUnits.Contains(unitOnTile))
                                        {
                                            canMove = false;
                                            //If we want the following units to do something other than just stop the whole group, this is the place to do it
                                            if (!unitOnTile.playerColor.Equals(this.playerColor))
                                            {
                                                if (player.netPlayer != null)
                                                {
                                                    player.combat.Add(new SkeletonCombat(fU, unitOnTile, player.netPlayer.uniqueIdentifer, player.netPlayer.peers.Find(peer => peer.playerUnits.Contains(unitOnTile)).uniqueIdentifer));
                                                    player.netPlayer.ongoingFights.Enqueue(player.combat[player.combat.Count - 1]);
                                                }
                                                else
                                                    player.combat.Add(new Combat(fU, unitOnTile, player.masterState));
                                            }
                                        }
                                    }
                                });

                                followingUnits.ForEach(fU =>
                                {
                                    if (canMove)
                                        fU.piecePosition = fU.movementPoints[0];

                                    fU.movementPoints.Clear();
                                });
                        }

                        if (canMove)
                        {
                            piecePosition = movementPoints[0];
                            movementPoints.RemoveAt(0);
                        }
                        else
                            movementPoints.Clear();                  
                    }
                    else if (unitOnTile.playerColor.Equals(player.playerColor))
                    {
                        //if the unit on this tile is one of ours and isn't part of this group then we clear any further movement
                        movementPoints.Clear();

                        //if the unit is also the same type, we add our 
                        if (unitOnTile.unitType.Equals(this.unitType) && unitOnTile.movementPoints.Count.Equals(0))
                        {
                            unitOnTile.AddUnits(this.numUnits);
                            this.numUnits -= this.numUnits + 1;
                        }
                        else
                        {
                            if (unitOnTile.leaderUnitIndex < 0)
                                unitOnTile.AddFollowingUnit(this, player);
                            else
                                player.playerUnits.Find(pU => pU.unitID.Equals(unitOnTile.leaderUnitIndex)).AddFollowingUnit(this, player);
                        }
                    }
                    else
                    {
                        movementPoints.Clear();

                        if (followingUnits != null)
                        {
                            followingUnits.ForEach(fU =>
                               {
                                   fU.leaderUnitIndex = -1;
                               });

                            followingUnits.Clear();
                        }

                        if (player.netPlayer != null)
                        {
                            player.combat.Add(new SkeletonCombat(this, unitOnTile, player.netPlayer.uniqueIdentifer, player.netPlayer.peers.Find(peer => peer.playerUnits.Contains(unitOnTile)).uniqueIdentifer));
                            player.netPlayer.ongoingFights.Enqueue(player.combat[player.combat.Count - 1]);
                        }
                        else
                            player.combat.Add(new Combat(this, unitOnTile, player.masterState));
                    }

                    //if there's a movement just update anyways
                    if (player.netPlayer != null)
                        needsUpdate = true;

                }
            }
            else if (animPlayer.Animation != null && !animPlayer.Animation.IsDeathAnimation)
            {
                if (!inCombat && (animPlayer.Animation != null && leaderUnitIndex < 0) ^ (leaderUnitIndex > -1 && player.playerUnits.Find(pu => pu.unitID.Equals(leaderUnitIndex)).movementPoints.Count.Equals(0)))
                    animPlayer.KillAnimation();
            }

            if (player.netPlayer != null && needsUpdate)
            {
                player.netPlayer.unitsToUpdate.Enqueue(this);
                needsUpdate = false;
            }

            canBuild = canBuild && (!player.buildingTiles.Contains(piecePosition) && !player.dupeBuildingTiles.Contains(piecePosition));
        }

        public void AnimateMovement(Player.Player player)
        {
            if (unitType.Equals((byte)UnitType.Infantry))
                animPlayer.PlayAnimation(new Animation(player.ScreenManager.animationsRepo[0], 0.125f, true, 0));
            else if (unitType.Equals((byte)UnitType.Tank))
                animPlayer.PlayAnimation(new Animation(player.ScreenManager.animationsRepo[3], 0.2f, true, 2));
            else if (unitType.Equals((byte)UnitType.Jet))
                animPlayer.PlayAnimation(new Animation(player.ScreenManager.animationsRepo[6], 0.2f, true, 0));
        }

        public void AnimateCombat(Player.Player player)
        {
            if (unitType.Equals((byte)UnitType.Infantry))
                animPlayer.PlayAnimation(new Animation(player.ScreenManager.animationsRepo[1], 0.2f, true));
            else if (unitType.Equals((byte)UnitType.Tank))
                animPlayer.PlayAnimation(new Animation(player.ScreenManager.animationsRepo[4], 0.2f, true));
            else if (unitType.Equals((byte)UnitType.Jet))
                animPlayer.PlayAnimation(new Animation(player.ScreenManager.animationsRepo[7], 0.1f, true));
        }

        public void AnimateDeath(Player.Player player)
        {
            if (unitType.Equals((byte)UnitType.Infantry))
                animPlayer.PlayAnimation(new Animation(player.ScreenManager.animationsRepo[2], 0.205f, false) { IsDeathAnimation = true });
            else if (unitType.Equals((byte)UnitType.Tank))
                animPlayer.PlayAnimation(new Animation(player.ScreenManager.animationsRepo[5], 0.2f, false) { IsDeathAnimation = true });
            else if (unitType.Equals((byte)UnitType.Jet))
                animPlayer.PlayAnimation(new Animation(player.ScreenManager.animationsRepo[8], 0.215f, false) { IsDeathAnimation = true });
        }

        private void FlipUnit()
        {
            if (movementPoints[0].X > piecePosition.X)
                unitEffects = SpriteEffects.None;
            else if (movementPoints[0].X < piecePosition.X)
                unitEffects = SpriteEffects.FlipHorizontally;
        }

        private bool CheckIfBuildOnTile(Player.Player player, Vector2 pos)
        {
            bool noEnemy = true;
                player.masterState.players.ForEach(playee =>
                    {
                        playee.playerStructures.ForEach(bT =>
                            {
                                if (bT.Equals(pos))
                                    noEnemy = false;
                            });
                    });
            return noEnemy;
        }

        public virtual void Update(GameTime gameTime, Player.EnemyPlayer enemyPlayer)
        {
            if (numUnits < 1)
            {
                if (this.unitType.Equals((byte)UnitType.Dictator))
                    enemyPlayer.hasLost = true;

                if (this.animPlayer.Animation != null && this.animPlayer.Animation.IsStopped)
                    enemyPlayer.playerUnits.Remove(this);

                this.AnimateDeath(enemyPlayer);
            }
            
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteFont font, StateSystem.StateManager ScreenManager)
        {
            Rectangle pieceRect = new Rectangle((int)(piecePosition.X * tileWidth), (int)(piecePosition.Y * tileWidth - Math.Abs(piecePosition.X % 2) * (tileWidth / 2)), tileWidth, tileWidth);
            if (canBuild)
                spriteBatch.Draw(ScreenManager.tile, pieceRect, playerColor);


            if (animPlayer.Animation != null)//this.GetType().Equals(typeof(Units.Tank)))
                animPlayer.Draw(gameTime, spriteBatch, pieceRect, unitEffects, playerColor);
            else
                //spriteBatch.Draw(pieceTexture, pieceRect, playerColor);
                spriteBatch.Draw(pieceTexture, pieceRect, new Rectangle(0, 0, tileWidth, tileWidth), playerColor, 0.0f, Vector2.Zero, unitEffects, 0.0f);

            if (numUnits > 1)
                spriteBatch.DrawString(font, numUnits.ToString(), new Vector2(pieceRect.X + 5, pieceRect.Y + pieceRect.Height - (font.LineSpacing * 3)), playerColor, 0.0f, Vector2.Zero, 3.0f, SpriteEffects.None, 0.0f);

            if (followingUnits != null)
                spriteBatch.Draw(ScreenManager.textureRepo[2], new Rectangle(pieceRect.X + tileWidth - 80, pieceRect.Y + 10, 80, 80),  playerColor);
                //spriteBatch.DrawString(font, "L", new Vector2(pieceRect.X + tileWidth - 30, pieceRect.Y + 10), playerColor);
        }
    }

    public enum UnitType
    {
        Infantry,
        Jet,
        Tank,
        Dictator,
    }
}
