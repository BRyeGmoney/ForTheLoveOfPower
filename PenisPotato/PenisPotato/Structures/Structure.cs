using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PenisPotato.Graphics;

namespace PenisPotato.Structures
{
    public class Structure
    {
        public Texture2D pieceTexture;
        public Vector2 piecePosition;
        public Color playerColor;
        public byte pieceType;
        public float lastTime;
        public int tileWidth;
        public float built = 0.0f;
        public float conquered = 0.0f;
        public int buildTime;
        public bool displayModifier = false;
        public int modifierIndex;
        private float modifierTimer;
        public int settlementOwnerIndex;
        public bool doneModifying = false;
        //public short foodConsumption = 1;

        public Structure() { }

        public Structure(Vector2 position, Texture2D pieceTex)
        {
            piecePosition = position;
            pieceTexture = pieceTex;
        }

        public virtual void LoadContent(ContentManager Content) { }

        public virtual void Update(GameTime gameTime, Player.Player player)
        {
            if (built < 100)
            {
                built += buildTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
                built = MathHelper.Clamp(built, 0, 100);
            }
            else if (built.Equals(100))
            {
                player.dupeBuildingTiles.Clear();
                //player.buildingTiles.AddRange(GetSurroundingTiles());
                player.playerSettlements.ForEach(pS =>
                {
                    player.dupeBuildingTiles.AddRange(pS.GetAllTilesBelongingToSettlement());
                });
                player.buildingTiles = player.dupeBuildingTiles;
                player.dupeBuildingTiles = player.dupeBuildingTiles.GroupBy(x => x)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key)
                             .ToList();

                player.buildingTiles = player.buildingTiles.Except(player.dupeBuildingTiles).ToList();

                //conquered = 0.0f;
                built = 101;
            }
        }

        public virtual void Update(GameTime gameTime, Player.EnemyPlayer enemyPlayer)
        {
            if (built < 100)
            {
                built += buildTime * (float)gameTime.ElapsedGameTime.TotalSeconds;
                built = MathHelper.Clamp(built, 0, 100);
            }
            else
            {
                enemyPlayer.buildingTiles.AddRange(GetSurroundingTiles());
            }
        }

        public virtual void Clicked(GameTime gameTime, Player.MainPlayer player)
        {
        }

        public List<Vector2> GetSurroundingTiles()
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

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime, StateSystem.StateManager stateManager, Player.Player player, bool isSettlement)
        {
            int y = (int)(piecePosition.Y * tileWidth - Math.Abs(piecePosition.X % 2) * (tileWidth / 2));
            int b = (int)(tileWidth * (built / 100));

            spriteBatch.Draw(pieceTexture, new Vector2(piecePosition.X * tileWidth, y), playerColor);

            if (conquered > 0)
                spriteBatch.Draw(pieceTexture, new Vector2((int)piecePosition.X * tileWidth, y), new Rectangle(0, 0, tileWidth, (int)(tileWidth * (conquered / 100))), player.playerSettlements[settlementOwnerIndex].invadingPlayerColor);

            if (built < 100)
                spriteBatch.FillRectangle(new Rectangle((int)piecePosition.X * tileWidth, y + tileWidth - 5, (int)(tileWidth * (built / 100)), 10), playerColor);
            else if (displayModifier)
            {
                modifierTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                spriteBatch.Draw(stateManager.textureRepo[modifierIndex], new Rectangle((int)piecePosition.X * tileWidth, y + (int)(15 - (15 * (modifierTimer / 2))), tileWidth / 6, tileWidth / 6), Color.White * (1 - (modifierTimer / 2)));//(byte)(255 - (124 * modifierTimer))
                if (modifierTimer > 2)
                {
                    modifierTimer = 0;
                    displayModifier = false;
                }
            }

            if (isSettlement)
                spriteBatch.DrawString(stateManager.Font, (this as Structures.Civil.Settlement).settlementMorale.ToString(), new Vector2(piecePosition.X * tileWidth + 8, y + tileWidth - (stateManager.Font.LineSpacing * 3)), playerColor);
            //spriteBatch.Draw(pieceTexture, new Vector2(piecePosition.X * tileWidth, y), new Rectangle(0, 0, tileWidth, b), playerColor);
            
            //spriteBatch.Draw(pieceTexture, new Rectangle((int)(piecePosition.X * tileWidth), y, tileWidth, tileWidth), new Rectangle(0, 45, tileWidth, tileWidth), playerColor);
            
        }
    }

    public enum PieceTypes
    {
        Settlement = 0,
        TownHall = 1,
        CityHall = 2,
        Capitol = 3,
        Factory = 4,
        Exporter = 5,
        Market = 6,
        Barracks = 7,
        AirBase = 8,
        TankDepot = 9,
        LabourCamp = 10,
        MilitaryContractor = 11,
        Propaganda = 12,
    }
}
