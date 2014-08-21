using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PenisPotato.Structures.Civil
{
    public class Settlement : Structure
    {
        public List<Structure> settlementProperties;
        public bool isDictatorInCity = false;
        public bool isCityBeingConquered = false;
        public int conqueredIndex = 0;
        public long invadingPlayerId = 0;

        public Settlement() { }

        public Settlement(Vector2 pos, Color color, Texture2D pieceTex)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.pieceType = (byte)PieceTypes.Settlement;
            this.tileWidth = Convert.ToInt16(Resources.tileWidth);
            this.settlementProperties = new List<Structure>();
            this.buildTime = 15;
        }

        public override void Update(GameTime gameTime, Player.Player player)
        {
            lastTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (isCityBeingConquered && lastTime > 2f)
                RunCityTakeover(player);


            if (built >= 100 && lastTime > 2f)
            {
                player.Money++;
                lastTime = 0.0f;
            }

            if (player.playerUnits[0].piecePosition.Equals(piecePosition))
                isDictatorInCity = true;
            else
                isDictatorInCity = false;

            if (settlementProperties.Count > 5)
                UpdateToTown(player.ScreenManager);
            if (settlementProperties.Count > 10)
                UpdateToCity(player.ScreenManager);
            if (pieceType.Equals(PieceTypes.CityHall) && isDictatorInCity)
                UpdateToCapitol(player.ScreenManager);

            base.Update(gameTime, player);
        }

        private void RunCityTakeover(Player.Player player)
        {
            Player.Player opposingPlayer = null;

            //Immediately assume that the threat to the city no longer exists on this update cycle
            isCityBeingConquered = false;

            //Let's just... as a precaution search to see if any enemy units are on the settlement space and set the isCityBeingConquered flag
            player.masterState.players.ForEach(playee =>
            {
                if (playee.playerUnits.Exists(pU => pU.piecePosition.Equals(this.piecePosition)))
                {
                    isCityBeingConquered = true;
                    opposingPlayer = playee;
                }
            });

            //sweet jesus let's start conquering the city if there is someone actually here
            if (isCityBeingConquered)
            {
                if (settlementProperties.Count() - 1 > conqueredIndex)
                {
                    this.settlementProperties[conqueredIndex].conquered = MathHelper.Clamp(this.settlementProperties[conqueredIndex].conquered + 20.0f, 0, 100);

                    if (player.netPlayer != null)
                        player.netPlayer.packetsToSend.Enqueue(new Player.StructureNetworkPacket() { packetType = (byte)Player.PacketType.STRUCTURE_UPDATE, defenderId = player.netPlayer.uniqueIdentifer, invaderId = invadingPlayerId, building = this, percentageConquered = (short)this.settlementProperties[conqueredIndex].conquered, conqueredIndex = (short)this.conqueredIndex });

                    if (this.settlementProperties[conqueredIndex].conquered >= 100.0f)
                        conqueredIndex++;
                }
                else
                {
                    this.conquered = MathHelper.Clamp(this.conquered + 30.0f, 0, 100.0f);

                    if (player.netPlayer != null)
                        player.netPlayer.packetsToSend.Enqueue(new Player.StructureNetworkPacket() { packetType = (byte)Player.PacketType.SETTLEMENT_UPDATE, defenderId = player.netPlayer.uniqueIdentifer, invaderId = invadingPlayerId, building = this, percentageConquered = (short)this.conquered, lengthOfTransmission = 6 });
                }

                if (this.conquered.Equals(100.0f) && opposingPlayer != null)
                    ChangeOwnership(opposingPlayer, player);
            }
        }

        public void ChangeOwnership(Player.Player opposingPlayer, Player.Player curPlayer)
        {
            //Remove the city and all its structures from current player's grasp
            curPlayer.playerSettlements.Remove(this);
            curPlayer.playerStructures.Remove(this);

            //if (curPlayer.netPlayer == null) //problem is that both the defending and attacking player are updating the attacking player's 
            //structure list. which means both structures get drawn. only do it for defending if defending player is updating this list
            //and only for attacking if attacking is updating this list.
            //{
                //Give the opposing player the city and all its structures
                opposingPlayer.playerSettlements.Add(this);
                opposingPlayer.playerStructures.Add(this);
            //}

            this.settlementProperties.ForEach(sP =>
            {
                //opposingPlayer.playerSettlements[opposingPlayer.playerSettlements.Count - 1].settlementProperties.Add(sP);
                //if (curPlayer.netPlayer == null)
                //{
                    opposingPlayer.playerStructures.Add(sP);
                    sP.playerColor = opposingPlayer.playerColor;
                //}
                //this.settlementProperties.Remove(sP);
                curPlayer.playerStructures.Remove(sP);
                
            });
            this.playerColor = opposingPlayer.playerColor;
        }

        private void UpdateToTown(StateSystem.StateManager stateManager)
        {
            this.pieceTexture = stateManager.buildItems[16].menuItem;
            //this.pieceType = (byte)PieceTypes.TownHall;
        }

        private void UpdateToCity(StateSystem.StateManager stateManager)
        {
            this.pieceTexture = stateManager.buildItems[17].menuItem;
            //this.pieceType = (byte)PieceTypes.CityHall;
        }

        private void UpdateToCapitol(StateSystem.StateManager stateManager)
        {
            this.pieceTexture = stateManager.buildItems[18].menuItem;
            //this.pieceType = (byte)PieceTypes.Capitol;
        }

        public List<Vector2> GetAllTilesBelongingToSettlement()
        {
            List<Vector2> tiles = new List<Vector2>();
            settlementProperties.ForEach(sP =>
                {
                    tiles.AddRange(sP.GetSurroundingTiles());
                });
            tiles.AddRange(this.GetSurroundingTiles());

            return tiles.Distinct().ToList();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime, StateSystem.StateManager stateManager)
        {
            settlementProperties.ForEach(sP =>
                {
                    sP.Draw(spriteBatch, gameTime, stateManager);
                });
            base.Draw(spriteBatch, gameTime, stateManager);
        }
    }
}
