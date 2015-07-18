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
    public class MultiplayerGameplayScreen : GameScreen
    {
        #region Fields

        GraphicsDevice graphics;
        ContentManager content;
        SpriteBatch spriteBatch;
        BloomComponent bloom;
        Camera camera;

        //Map
        Texture2D tile;
        Texture2D selectedTile;
        int tileWidth = 256;

        private float pauseAlpha;

        //Players
        Player.MainPlayer playerOne;
        Player.NetworkPlayer netPlayer;
        public List<Player.Player> players;

        public List<Units.Combat> combat;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MultiplayerGameplayScreen(Player.NetworkPlayer nP)
        {
            players = new List<Player.Player>();

            netPlayer = nP;
            nP.peers.Remove(netPlayer);
            players.AddRange(nP.peers);

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");
            graphics = ScreenManager.GraphicsDevice;
            bloom = new BloomComponent(ScreenManager.Game);
            tile = content.Load<Texture2D>("Textures/Map/square2");

            camera = new Camera(graphics.Viewport, 10000000, 10000000, 0.4f);
            ScreenManager.Game.Components.Add(bloom);
            bloom.Settings = new BloomSettings(null, 0.15f, 2, 1.25f, 1, 1.5f, 1);
            spriteBatch = ScreenManager.SpriteBatch;

            //Players
            //playerOne = new Player.MainPlayer(content, graphics, ScreenManager, this, netPlayer, Color.PaleVioletRed);

            players.ForEach(pS => {
                if (pS.GetType().Equals(typeof(Player.NetworkPlayer)))
                {
                    (pS as Player.NetworkPlayer).InitGamePlayer(false, null);
                    pS.ScreenManager = ScreenManager;
                }
            });

            //Combat List
            combat = new List<Units.Combat>();

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            //Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            playerOne.Update(gameTime);

            combat.ForEach(fight => {
                fight.Update(gameTime);
            });
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            if (pauseAlpha == 0)
                playerOne.UpdateInput(gameTime, input,camera);
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            int startx = (int)(camera.Pos.X / tileWidth);
            int starty = (int)(camera.Pos.Y / tileWidth);

            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);
            bloom.BeginDraw();
            spriteBatch.Begin(SpriteSortMode.Immediate,
                    null, SamplerState.PointClamp, null, null, null,
                    camera.GetTransformation());

            //Only draw a chunk of tiles around the camera. This way we can draw an "infinite" amount of tiles without bogging down the cpu with endless
            //tiles that we're not even looking at.
            for (int y = starty - 15; y < starty + 15; y += 5)
            {
                for (int x = startx - 15; x < startx + 15; x += 5)
                    DrawChunk(spriteBatch, x, y);
            }

            playerOne.buildingTiles.ForEach(pS => {
                spriteBatch.Draw(tile, new Rectangle((int)(pS.X * tileWidth), (int)((pS.Y * tileWidth) - Math.Abs(pS.X % 2) * (tileWidth / 2)), tileWidth, tileWidth), playerOne.playerColor);
            });
            playerOne.movementTiles.ForEach(pS => {
                spriteBatch.Draw(tile, new Rectangle((int)(pS.X * tileWidth), (int)((pS.Y * tileWidth) - Math.Abs(pS.X % 2) * (tileWidth / 2)), tileWidth, tileWidth), playerOne.playerColor);
            });


            players.ForEach(player =>{
                player.ActionDraw(spriteBatch, gameTime, camera);
            });
            playerOne.ActionDraw(spriteBatch, gameTime, camera);
            spriteBatch.End();

            //Hud spritebatch
            if (playerOne.MoneyString != null)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(ScreenManager.Font, playerOne.MoneyString, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }

        private void DrawChunk(SpriteBatch spriteBatch, int startx, int starty)
        {
            Rectangle rect;

            for (int y = starty; y <= starty + 5; y++)
            {
                for (int x = startx; x <= startx + 5; x++)
                {
                    rect = new Rectangle(x * tileWidth, (y * tileWidth) - Math.Abs(x % 2) * (tileWidth / 2), tileWidth, tileWidth);
                    spriteBatch.Draw(tile, rect, Color.Cyan);
                }
            }
        }

        private void CreateRectangleTexture()
        {
            selectedTile = new Texture2D(graphics, 1, 1, false, SurfaceFormat.Color);
            selectedTile.SetData<Color>(new Color[] { Color.White });
        }

        #endregion
    }
}