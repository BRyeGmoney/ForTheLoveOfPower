using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PenisPotato
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        StateSystem.StateManager screenManager;

        //Major Components
        /*Camera camera;

        //Map
        Texture2D tile;
        //Texture2D selectedTile;
        int tileWidth = 32;
        int tileHeight = 32;
        //Vector2 selectedTilePos = Vector2.Zero;

        //Game input
        //Vector2 previousPosition = Vector2.Zero;
        //int previousScroll = 0;
        float zoomIncrement = 1.0f;

        //Text
        //SpriteFont unitPos;
        //Vector2 textPos;
        //String unitPosString;

        //Players
        Player.Player playerOne;*/

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = 480;
            graphics.PreferredBackBufferWidth = 340;
            this.IsMouseVisible = true;

            Content.RootDirectory = "Content";

            // Create the screen manager component.
            screenManager = new StateSystem.StateManager(this);
            screenManager.audioManager = new Audio.AudioManager(this);
            
            Components.Add(screenManager);
            Components.Add(screenManager.audioManager);

            // Activate the first screens.
            //screenManager.AddScreen(new StateSystem.Screens.BackgroundScreen(), null, false);
            //screenManager.AddScreen(new StateSystem.Screens.TitleScreen(), null, false);
            screenManager.AddScreen(new StateSystem.Screens.MainMenuScreen(), null, false);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //screenManager.audioManager.StartPlaylist(0);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);
        }

        
    }
}
