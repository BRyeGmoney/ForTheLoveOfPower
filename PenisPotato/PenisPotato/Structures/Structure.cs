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
            else if (built.Equals(100) || (conquered.Equals(100) && player.playerStructures.Contains(this)))
            {
                player.buildingTiles.AddRange(GetSurroundingTiles());
                conquered = 0.0f;
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

        public virtual void Clicked(GameTime gameTime, Player.Player player)
        {
        }

        public List<Vector2> GetSurroundingTiles()
        {
            List<Vector2> st = new List<Vector2>();

            /*for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                    st.Add(new Vector2(piecePosition.X + x, piecePosition.Y + y));*/
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
            //if (piecePosition.X % 2 == 0 || piecePosition.Y % 2 == 0 && !(piecePosition.X % 2 == 0 && piecePosition.Y % 2 == 0))
            //{
            //    st.Add(new Vector2(piecePosition.X - 1, piecePosition.Y - 1));
            //    st.Add(new Vector2(piecePosition.X + 1, piecePosition.Y - 1));
            //}
            //else
            //{
            //    st.Add(new Vector2(piecePosition.X - 1, piecePosition.Y + 1));
            //    st.Add(new Vector2(piecePosition.X + 1, piecePosition.Y + 1));
            //}

            return st;
        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime, StateSystem.StateManager stateManager)
        {
            int y = (int)(piecePosition.Y * tileWidth - Math.Abs(piecePosition.X % 2) * (tileWidth / 2));
            int b = (int)(tileWidth * (built / 100));

            spriteBatch.Draw(pieceTexture, new Vector2(piecePosition.X * tileWidth, y), playerColor);

            if (built < 100)
                spriteBatch.FillRectangle(new Rectangle((int)piecePosition.X * tileWidth, y + tileWidth - 5, (int)(tileWidth * (built / 100)), 10), playerColor);
            else if (displayModifier)
            {
                modifierTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                spriteBatch.Draw(stateManager.textureRepo[modifierIndex], new Rectangle((int)piecePosition.X * tileWidth, y, tileWidth / 6, tileWidth / 6), Color.White * (1 - (modifierTimer / 2)));//(byte)(255 - (124 * modifierTimer))
                //spriteBatch.Draw(stateManager.textureRepo[modifierIndex], new Rectangle((int)piecePosition.X * tileWidth, y + 30, tileWidth / 6, tileWidth / 6), Color.White);
                if (modifierTimer > 2)
                {
                    modifierTimer = 0;
                    displayModifier = false;
                }
            }
            //spriteBatch.Draw(pieceTexture, new Vector2(piecePosition.X * tileWidth, y), new Rectangle(0, 0, tileWidth, b), playerColor);
            
            //spriteBatch.Draw(pieceTexture, new Rectangle((int)(piecePosition.X * tileWidth), y, tileWidth, tileWidth), new Rectangle(0, 45, tileWidth, tileWidth), playerColor);
            
        }
    }

    public enum PieceTypes
    {
        Settlement,
        TownHall,
        CityHall,
        Capitol,
        Factory,
        Exporter,
        Market,
        Barracks,
        AirBase,
        TankDepot,
        LabourCamp,
        MilitaryContractor,
        Propaganda,
    }
}
