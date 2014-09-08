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

        public EconomyScreen(Player.MainPlayer player, StateManager stateManager)
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            this.player = player;
            this.stateManager = stateManager;

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

                if (isClose)
                    stateManager.RemoveScreen(this);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            stateManager.SpriteBatch.Begin();

            for (int x = 0; x < 3; x++)
            {
                stateManager.SpriteBatch.Draw(stateManager.tile, induBasePos[x], player.playerColor);
                stateManager.SpriteBatch.Draw(induTextures[x], new Rectangle((int)induBasePos[x].X, (int)induBasePos[x].Y, stateManager.tile.Width, stateManager.tile.Height), player.playerColor);
            }


            stateManager.SpriteBatch.End();
        }
    }
}
