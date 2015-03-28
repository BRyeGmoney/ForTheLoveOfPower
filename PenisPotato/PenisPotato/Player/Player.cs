
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PenisPotato.Player
{
    public class Player
    {
        public ContentManager Content;
        public GraphicsDevice graphics;
        public StateSystem.StateManager ScreenManager;
        public StateSystem.Screens.GameplayScreen masterState;
       
        public String playerName;
        public NetworkPlayer netPlayer;
        private Int32 UnitIDTracker = -1;

        public Int32 Money { get { return money; } set { money = value; } }
        public Color playerColor;
        public List<Structures.Structure> playerStructures;
        public List<Structures.Civil.Settlement> playerSettlements;
        public List<Units.Unit> playerUnits;
        public List<Vector2> buildingTiles;
        public List<Vector2> dupeBuildingTiles;
        public List<Vector2> movementTiles;
        public List<Units.Combat> combat;
        public Units.Unit navigatingUnit;

        public Texture2D dictatorTex;
        public int tileWidth = 256;
        public int tileHeight = 256;
        
        public int prevStructures;
        public int prevUnits;
        public bool performedAction = false;
        public int wingus, dingus;

        public int money = 0;
        public bool hasLost = false;

        public String PlayerName { get { return playerName; } set { playerName = value; } }

        public Player() { }

        public virtual void LoadContent()
        {
            dictatorTex = Content.Load<Texture2D>("Textures/Units/Unit_Dictator");
        }

        public virtual void Update(GameTime gameTime)
        {
            UpdateStructures(gameTime);
            playerUnits.ForEach(pU =>
            {
                pU.Update(gameTime, this);

                if (pU.numUnits < 1 || (pU.numUnits < 1 && netPlayer != null && !netPlayer.unitsToUpdate.Contains(pU)))
                {
                    if (pU.unitType.Equals((byte)Units.UnitType.Dictator))
                        this.hasLost = true;

                    //if there is an animation and its not a death animation or if it is a death animation and its stopped
                    if (pU.numUnits < 0 || (pU.animPlayer.Animation != null && (pU.animPlayer.Animation.IsDeathAnimation && pU.animPlayer.Animation.IsStopped)))
                    {
                        playerUnits.Remove(pU);
                        pU = null;
                    }
                    else if (pU.animPlayer.Animation == null)
                        pU.AnimateDeath(this);
                }
            });
        }

        public virtual void UpdateStructures(GameTime gameTime)
        {
            playerStructures.ForEach(pS => pS.Update(gameTime, this));
        }

        public void SendStructureInfo(int diff)
        {
            for (int x = diff; x > 0; x--)
            {
                Structures.Structure structure = playerStructures[playerStructures.Count - diff];
                if (structure is Structures.Civil.Settlement)
                    netPlayer.packetsToSend.Enqueue(new StructureNetworkPacket() { packetType = (byte)PacketType.SETTLEMENT_ADD, building = structure });
                else
                    netPlayer.packetsToSend.Enqueue(new StructureNetworkPacket() { packetType = (byte)PacketType.STRUCTURE_ADD, building = structure });

                //netPlayer.structuresToSend.Enqueue(playerStructures[playerStructures.Count - diff]);
            }
        }

        public int GetNextUnitId()
        {
            UnitIDTracker += 1;
            return UnitIDTracker;
        }

        public int ToRoundX(float num)
        {
            if (num < 0)
                return (int)Math.Floor(num);
            else
                return (int)num;
        }

        public int ToRoundY(float num)
        {
            if (num < 0)
                return (int)Math.Floor(num);
            else
                return (int)num;
        }

        

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime, Camera camera)
        {
            float minboundsx = (camera.Pos.X * camera.Zoom) - camera._viewportWidth, maxboundsx = (camera.Pos.X * camera.Zoom) + camera._viewportWidth;
            float minboundsy = (camera.Pos.Y * camera.Zoom) - camera._viewportHeight, maxboundsy = (camera.Pos.Y * camera.Zoom) + camera._viewportHeight;
            float modTileWidth = tileWidth * camera.Zoom;

            //camera.numStructsDrawn = 0; camera.numUnitsDrawn = 0;

            foreach (Units.Unit unit in playerUnits)
                if (isInRange(unit.piecePosition.X * modTileWidth, minboundsx - modTileWidth, maxboundsx) && isInRange(unit.piecePosition.Y * modTileWidth, minboundsy - modTileWidth, maxboundsy))
                    unit.Draw(gameTime, spriteBatch, ScreenManager.Font, ScreenManager);
            foreach (Structures.Civil.Settlement settlement in playerSettlements)
                if (isInRange(settlement.piecePosition.X * modTileWidth, minboundsx - modTileWidth, maxboundsx) && isInRange(settlement.piecePosition.Y * modTileWidth, minboundsy - modTileWidth, maxboundsy))
                    settlement.Draw(spriteBatch, gameTime, ScreenManager, this, true);
            /*foreach (Structures.Structure structure in playerStructures)
                if (isInRange(structure.piecePosition.X * modTileWidth, minboundsx - modTileWidth, maxboundsx) && isInRange(structure.piecePosition.Y * modTileWidth, minboundsy - modTileWidth, maxboundsy))
                    structure.Draw(spriteBatch, gameTime, ScreenManager, this);*/
        }

        private bool isInRange(float value, float min, float max)
        {
            return value > min && value < max;
        }
    }
}
