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
        public bool canBuild = false;

        public int numUnits = 1;

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
        }

        public void RemoveUnits(int unitsToRemove)
        {
            numUnits -= unitsToRemove;
        }

        public void KillUnit()
        {
            numUnits -= 1;
        }

        public virtual void Update(GameTime gameTime, Player.Player player)
        {
            if (numUnits < 1)
                player.playerUnits.Remove(this);
            else
            {
                moveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (movementPoints.Count > 0 && moveTime > unitSpeed)
                {
                    Unit unitOnTile = null;
                    moveTime = 0f;

                    player.masterState.players.ForEach(curPlayer => 
                        {
                            if (curPlayer.playerUnits.Exists(pU => pU.piecePosition.Equals(movementPoints[0]) && !pU.Equals(this)))
                                unitOnTile = curPlayer.playerUnits.Find(pU => pU.piecePosition.Equals(movementPoints[0]));

                            if (curPlayer.playerSettlements.Exists(pS => pS.piecePosition.Equals(movementPoints[0])))
                                curPlayer.playerSettlements.Find(pS => pS.piecePosition.Equals(movementPoints[0])).isCityBeingConquered = true;
                        });

                    if (unitOnTile == null)
                    {
                        //if there is no unit here then we continue on as if nothing
                        piecePosition = movementPoints[0];
                        movementPoints.RemoveAt(0);
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
                    }
                    else
                    {
                        movementPoints.Clear();
                        player.masterState.combat.Add(new Combat(this, unitOnTile, player.masterState));
                    }
                }

                canBuild = (!player.buildingTiles.Contains(piecePosition) && !player.dupeBuildingTiles.Contains(piecePosition));
            }
        }

        public virtual void Update(GameTime gameTime, Player.EnemyPlayer enemyPlayer)
        {
            if (numUnits < 1)
                enemyPlayer.playerUnits.Remove(this);
            
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, StateSystem.StateManager ScreenManager)
        {
            Rectangle pieceRect = new Rectangle((int)(piecePosition.X * tileWidth), (int)(piecePosition.Y * tileWidth - Math.Abs(piecePosition.X % 2) * (tileWidth / 2)), tileWidth, tileWidth);
            if (canBuild)
                spriteBatch.Draw(ScreenManager.tile, pieceRect, playerColor);

            spriteBatch.Draw(pieceTexture, pieceRect, playerColor);

            if (numUnits > 1)
                spriteBatch.DrawString(font, numUnits.ToString(), new Vector2(pieceRect.X + 5, pieceRect.Y + pieceRect.Height - (font.LineSpacing * 3)), playerColor, 0.0f, Vector2.Zero, 3.0f, SpriteEffects.None, 0.0f);
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
