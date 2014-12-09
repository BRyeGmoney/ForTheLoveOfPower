using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
        public Graphics.Animation.AnimationPlayer animPlayer;
        public List<Unit> followingUnits;
        private int leaderUnitIndex = -1;

        public int numUnits = 1;
        private bool needsUpdate = false;

        private SpriteEffects unitEffects;

        public Unit() { }

        public Unit(Vector2 position, Texture2D pieceText)
        {
            this.piecePosition = position;
            this.pieceTexture = pieceText;
            this.movementPoints = new List<Vector2>();
            this.numUnits = 1;
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
            needsUpdate = true;
        }

        public void KillUnit()
        {
            numUnits -= 1;
            needsUpdate = true;
        }

        public void AddFollowingUnit(Unit followingUnit, Player.Player player)
        {
            if (followingUnits == null)
                followingUnits = new List<Unit>();

            followingUnits.Add(followingUnit);
            followingUnit.leaderUnitIndex = player.playerUnits.IndexOf(this); 
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


        public virtual void Update(GameTime gameTime, Player.Player player)
        {
            //canBuild = true;

            if (movementPoints.Count > 0)
            {
                moveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (leaderUnitIndex > -1)
                    player.playerUnits[leaderUnitIndex].RemoveFollowingUnit(this);

                FlipUnit();


                if (movementPoints.Count > 0 && moveTime > unitSpeed)
                {
                    Unit unitOnTile = null;
                    moveTime = 0f;

                    AnimateMovement(player);

                    player.masterState.players.ForEach(curPlayer =>
                        {
                            if (curPlayer.playerUnits.Exists(pU => pU.piecePosition.Equals(movementPoints[0]) && !pU.Equals(this)))
                                unitOnTile = curPlayer.playerUnits.Find(pU => ((pU.piecePosition.Equals(movementPoints[0]) && (this.followingUnits == null) || (this.followingUnits != null && !this.followingUnits.Contains(pU)))));

                            if (curPlayer.playerSettlements.Exists(pS => pS.piecePosition.Equals(movementPoints[0])))
                            {
                                Structures.Civil.Settlement curSettlement = curPlayer.playerSettlements.Find(pS => pS.piecePosition.Equals(movementPoints[0]));
                                curSettlement.isCityBeingConquered = true;
                                if (player.netPlayer != null)
                                    player.netPlayer.packetsToSend.Enqueue(new Player.StructureNetworkPacket() { packetType = (byte)Player.PacketType.SETTLEMENT_UPDATE, building = curSettlement, invaderId = player.netPlayer.uniqueIdentifer, defenderId = (curPlayer as Player.NetworkPlayer).uniqueIdentifer, lengthOfTransmission = 4 });
                            }
                        });

                    if (unitOnTile == null)
                    {
                        //if there is no unit here then we continue on as if nothing
                        Vector2 diff = new Vector2(movementPoints[0].X - piecePosition.X, movementPoints[0].Y - piecePosition.Y);
                        piecePosition = movementPoints[0];

                        if (followingUnits != null)
                        {
                            followingUnits.ForEach(fU =>
                                {
                                    fU.AnimateMovement(player);
                                    fU.unitEffects = this.unitEffects;
                                    fU.piecePosition = new Vector2(fU.piecePosition.X + diff.X, fU.piecePosition.Y + diff.Y);
                                });
                        }
                        movementPoints.RemoveAt(0);
                        canBuild = CheckIfNoEnemyOnTile(player, piecePosition);
                    }
                    else if (unitOnTile.playerColor.Equals(player.playerColor))
                    {
                        //if the unit on this tile is one of ours then we clear any further movement
                        movementPoints.Clear();

                        //if the unit is also the same type, we add our 
                        if (unitOnTile.unitType.Equals(this.unitType))
                        {
                            unitOnTile.AddUnits(this.numUnits);
                            this.numUnits -= this.numUnits;
                        }
                        else
                        {
                            unitOnTile.AddFollowingUnit(this, player);
                        }
                    }
                    else
                    {
                        movementPoints.Clear();
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
            else
            {
                if ((animPlayer.Animation != null && leaderUnitIndex < 0) ^ (leaderUnitIndex > -1 && player.playerUnits[leaderUnitIndex].movementPoints.Count.Equals(0)))//this.GetType().Equals(typeof(Units.Tank)))
                    animPlayer.KillAnimation();
                    //animPlayer.PlayAnimation(player.ScreenManager.animationsRepo[0]);
            }

            if (player.netPlayer != null && needsUpdate)
            {
                player.netPlayer.unitsToUpdate.Enqueue(this);
                needsUpdate = false;
            }

            canBuild = canBuild && (!player.buildingTiles.Contains(piecePosition) && !player.dupeBuildingTiles.Contains(piecePosition));
        }

        private void AnimateMovement(Player.Player player)
        {
            if (unitType.Equals((byte)UnitType.Infantry))
                animPlayer.PlayAnimation(player.ScreenManager.animationsRepo[0]);
            else if (unitType.Equals((byte)UnitType.Tank))
                animPlayer.PlayAnimation(player.ScreenManager.animationsRepo[1]);
            else if (unitType.Equals((byte)UnitType.Jet))
                animPlayer.PlayAnimation(player.ScreenManager.animationsRepo[2]);
        }

        private void FlipUnit()
        {
            if (movementPoints[0].X > piecePosition.X)
                unitEffects = SpriteEffects.None;
            else if (movementPoints[0].X < piecePosition.X)
                unitEffects = SpriteEffects.FlipHorizontally;
        }

        private bool CheckIfNoEnemyOnTile(Player.Player player, Vector2 pos)
        {
            bool noEnemy = true;
            /*if (player.netPlayer != null)
            {
                player.netPlayer.peers.ForEach(peer =>
                    {
                        peer.buildingTiles.ForEach(bT =>
                            {
                                if (bT.Equals(pos))
                                    noEnemy = false;
                            });
                    });
            }
            else
            {*/
                player.masterState.players.ForEach(playee =>
                    {
                        playee.buildingTiles.ForEach(bT =>
                            {
                                if (bT.Equals(pos))
                                    noEnemy = false;
                            });
                    });
            //}
            return noEnemy;
        }

        public virtual void Update(GameTime gameTime, Player.EnemyPlayer enemyPlayer)
        {
            if (numUnits < 1)
                enemyPlayer.playerUnits.Remove(this);
            
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
