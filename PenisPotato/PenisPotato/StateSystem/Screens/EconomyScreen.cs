using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PenisPotato.StateSystem.Screens
{
    class EconomyScreen : GameScreen
    {
        public Player.MainPlayer player;
        public StateManager stateManager;

        private Texture2D[] induTextures;
        private Vector2[] induBasePos;

        private Structures.Structure managingBuilding;
        private Structures.PieceTypes typeOfBuilding;

        private int selectedIndex = -1;

        public EconomyScreen(Player.MainPlayer player, StateManager stateManager, Structures.Structure econOwner, Structures.PieceTypes typeofBuilding)
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            this.player = player;
            this.stateManager = stateManager;
            this.managingBuilding = econOwner;
            this.typeOfBuilding = typeofBuilding;

            LoadContent();
        }

        public void LoadContent()
        {
            //Load Textures
            induTextures = new Texture2D[3];
            induTextures[0] = stateManager.Game.Content.Load<Texture2D>("Icons/Economy/Agriculture");
            induTextures[1] = stateManager.Game.Content.Load<Texture2D>("Icons/Economy/Automotive");
            induTextures[2] = stateManager.Game.Content.Load<Texture2D>("Icons/Economy/Computers");

            //Load Base Positions
            induBasePos = new Vector2[3];
            induBasePos[0] = new Vector2(stateManager.GraphicsDevice.Viewport.X + (stateManager.GraphicsDevice.Viewport.Width / 2), stateManager.GraphicsDevice.Viewport.Y + (stateManager.GraphicsDevice.Viewport.Height / 2 - stateManager.tile.Height));
            induBasePos[1] = new Vector2(stateManager.GraphicsDevice.Viewport.X + (stateManager.GraphicsDevice.Viewport.Width / 2), stateManager.GraphicsDevice.Viewport.Y + (stateManager.GraphicsDevice.Viewport.Height / 2));
            induBasePos[2] = new Vector2(stateManager.GraphicsDevice.Viewport.X + (stateManager.GraphicsDevice.Viewport.Width / 2) - stateManager.tile.Width, stateManager.GraphicsDevice.Viewport.Y + (stateManager.GraphicsDevice.Viewport.Height / 2) - (stateManager.tile.Height / 2));
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            bool isClose = true;

            if (input.CurrentMouseStates[0].LeftButton == ButtonState.Pressed && input.LastMouseStates[0].LeftButton == ButtonState.Released)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (x == 0)
                    {
                        if (new Rectangle((int)induBasePos[x].X, (int)induBasePos[x].Y - GetEconomyPercentage(x), stateManager.tile.Width, stateManager.tile.Height).Intersects(
                            new Rectangle(input.CurrentMouseStates[0].X, input.CurrentMouseStates[0].Y, 5, 5)))
                        {
                            selectedIndex = x;
                            isClose = false;
                        }
                    }
                    else if (x == 1)
                    {
                        if (new Rectangle((int)induBasePos[x].X, (int)induBasePos[x].Y + GetEconomyPercentage(x), stateManager.tile.Width, stateManager.tile.Height).Intersects(
                            new Rectangle(input.CurrentMouseStates[0].X, input.CurrentMouseStates[0].Y, 5, 5)))
                        {
                            selectedIndex = x;
                            isClose = false;
                        }
                    }
                    else
                    {
                        if (new Rectangle((int)induBasePos[x].X - GetEconomyPercentage(x), (int)induBasePos[x].Y, stateManager.tile.Width, stateManager.tile.Height).Intersects(
                            new Rectangle(input.CurrentMouseStates[0].X, input.CurrentMouseStates[0].Y, 5, 5)))
                        {
                            selectedIndex = x;
                            isClose = false;
                        }
                    }
                }

                if (isClose)
                    stateManager.RemoveScreen(this);
            }
            else if (input.CurrentMouseStates[0].LeftButton == ButtonState.Pressed && selectedIndex > -1)
            {
                if (selectedIndex == 0)
                    SetEconomyPercentage((int)MathHelper.Clamp(100 - (input.CurrentMouseStates[0].Y - (induBasePos[selectedIndex].Y - 100)), 0, 100));
                else if (selectedIndex == 1)
                    SetEconomyPercentage((int)MathHelper.Clamp((input.CurrentMouseStates[0].Y - (induBasePos[selectedIndex].Y - 100) - 100), 0, 100));
                else
                    SetEconomyPercentage((int)MathHelper.Clamp(100 - (input.CurrentMouseStates[0].X - (induBasePos[selectedIndex].X - 100)), 0, 100));
                isClose = false;
            }
            else if (input.CurrentMouseStates[0].LeftButton == ButtonState.Released && input.LastMouseStates[0].LeftButton == ButtonState.Released)
                selectedIndex = -1; //reset
        }

        private int GetEconomyPercentage(int index)
        {
            if (typeOfBuilding.Equals(Structures.PieceTypes.Factory))
                return (managingBuilding as Structures.Economy.Factory).economies[index];
            else if (typeOfBuilding.Equals(Structures.PieceTypes.Exporter))
                return (managingBuilding as Structures.Economy.Exporter).economies[index];
            else
                return (managingBuilding as Structures.Economy.Market).economies[index];
        }

        private void SetEconomyPercentage(int percentage)
        {
            if (typeOfBuilding.Equals(Structures.PieceTypes.Factory))
                (managingBuilding as Structures.Economy.Factory).economies[selectedIndex] = percentage;
            else if (typeOfBuilding.Equals(Structures.PieceTypes.Exporter))
                (managingBuilding as Structures.Economy.Exporter).economies[selectedIndex] = percentage;
            else
                (managingBuilding as Structures.Economy.Market).economies[selectedIndex] = percentage;


        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 addedPos;
            stateManager.SpriteBatch.Begin();

            for (int x = 0; x < 3; x++)
            {
                int perc = GetEconomyPercentage(x);
                if (x == 0)
                {
                    addedPos = new Vector2(0, -perc);
                    stateManager.DrawLine(stateManager.SpriteBatch, new Vector2(induBasePos[x].X + (stateManager.tile.Width / 2), induBasePos[x].Y + (stateManager.tile.Height / 2)), new Vector2(induBasePos[x].X + (stateManager.tile.Width / 2) + addedPos.X, induBasePos[x].Y + (stateManager.tile.Height / 2) + addedPos.Y), player.playerColor);
                    stateManager.SpriteBatch.DrawString(stateManager.Font, String.Format("{0}%", perc), new Vector2(induBasePos[x].X, induBasePos[x].Y - (stateManager.tile.Height / 2) + addedPos.Y), player.playerColor);
                }
                else if (x == 1)
                {
                    addedPos = new Vector2(0, perc);
                    stateManager.DrawLine(stateManager.SpriteBatch, induBasePos[x], induBasePos[x] + addedPos, player.playerColor);
                    stateManager.SpriteBatch.DrawString(stateManager.Font, String.Format("{0}%", perc), new Vector2(induBasePos[x].X, induBasePos[x].Y + stateManager.tile.Height + addedPos.Y), player.playerColor);
                }
                else
                {
                    addedPos = new Vector2(-perc, 0);
                    stateManager.DrawLine(stateManager.SpriteBatch, induBasePos[x], induBasePos[x] + addedPos, player.playerColor);
                    stateManager.SpriteBatch.DrawString(stateManager.Font, String.Format("{0}%", perc), new Vector2(induBasePos[x].X + addedPos.X, induBasePos[x].Y - (stateManager.tile.Height / 2)), player.playerColor);
                }

                stateManager.SpriteBatch.Draw(stateManager.tile, induBasePos[x] + addedPos, player.playerColor);
                stateManager.SpriteBatch.Draw(induTextures[x], new Rectangle((int)(induBasePos[x].X + addedPos.X), (int)(induBasePos[x].Y + addedPos.Y), stateManager.tile.Width, stateManager.tile.Height), player.playerColor);
            }


            stateManager.SpriteBatch.End();
        }
    }
}
