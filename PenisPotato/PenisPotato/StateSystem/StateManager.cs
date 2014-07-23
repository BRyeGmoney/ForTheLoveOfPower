using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PenisPotato.StateSystem
{
    public class StateManager : DrawableGameComponent
    {

        #region Fields

        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> screensToUpdate = new List<GameScreen>();

        InputState input = new InputState();
        //BloomComponent bloom;

        SpriteBatch spriteBatch;
        SpriteFont font;

        Texture2D blankTexture;
        public Texture2D tile;

        public List<Screens.BuildMenuItem> buildItems;
        public List<Texture2D> textureRepo;

        bool isInitialized;

        bool traceEnabled;

        #endregion

        #region Properties


        /// <summary>
        /// A default SpriteBatch shared by all the screens. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        /// <summary>
        /// A default font shared by all the screens. This saves
        /// each screen having to bother loading their own local copy.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
        }


        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled
        {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public StateManager(Game game)
            : base(game)
        {
            //bloom = new BloomComponent(game);
            //game.Components.Add(bloom);
            //bloom.Settings = new BloomSettings(null, 0.15f, 2, 1.25f, 1, 1.5f, 1);
        }


        /// <summary>
        /// Initializes the screen manager component.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            isInitialized = true;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            ContentManager content = Game.Content;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = content.Load<SpriteFont>("Fonts/debugFont");
            blankTexture = content.Load<Texture2D>("Textures/Misc/blank");
            tile = content.Load<Texture2D>("Textures/Map/square");

            SetupBuildMenuItems(content);
            SetupTextureRepository(content);

            // Tell each of the screens to load their content.
            foreach (GameScreen screen in screens)
            {
                screen.LoadContent();
            }
        }

        private void SetupBuildMenuItems(ContentManager content)
        {
            buildItems = new List<Screens.BuildMenuItem>();

            //Main 0-2
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(GraphicsDevice.Viewport.X + (GraphicsDevice.Viewport.Width / 2), GraphicsDevice.Viewport.Y + (GraphicsDevice.Viewport.Height / 2 - tile.Height)), content.Load<Texture2D>("Textures/Misc/Menu_Manipulation"), tile, Screens.BuildMenuStates.Main, Screens.BuildMenuStates.Manipulation , Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(GraphicsDevice.Viewport.X + (GraphicsDevice.Viewport.Width / 2), GraphicsDevice.Viewport.Y + (GraphicsDevice.Viewport.Height / 2)), content.Load<Texture2D>("Textures/Misc/Menu_Military"), tile, Screens.BuildMenuStates.Main, Screens.BuildMenuStates.Military, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(GraphicsDevice.Viewport.X + (GraphicsDevice.Viewport.Width / 2) - tile.Width, GraphicsDevice.Viewport.Y + (GraphicsDevice.Viewport.Height / 2) - (tile.Height / 2)), content.Load<Texture2D>("Textures/Misc/Menu_Economy"), tile, Screens.BuildMenuStates.Main, Screens.BuildMenuStates.Economical, Color.Aqua));

            //Manipulation 3-5
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(buildItems[0].position.X - (tile.Width / 2), buildItems[0].position.Y - tile.Height), content.Load<Texture2D>("Textures/Structures/Manipulation/Structure_Contractor"), tile, Screens.BuildMenuStates.Manipulation, Screens.BuildMenuStates.MilitaryContractor, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(buildItems[0].position.X + (tile.Width / 2), buildItems[0].position.Y - tile.Height), content.Load<Texture2D>("Textures/Structures/Manipulation/Structure_LabourCamp"), tile, Screens.BuildMenuStates.Manipulation, Screens.BuildMenuStates.LabourCamp, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(buildItems[0].position.X + tile.Width, buildItems[0].position.Y), content.Load<Texture2D>("Textures/Structures/Manipulation/Structure_Propaganda"), tile, Screens.BuildMenuStates.Manipulation, Screens.BuildMenuStates.Propaganda, Color.Aqua));
             
            //Economy 6-8
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(buildItems[2].position.X - (tile.Width / 2), buildItems[2].position.Y - tile.Height), content.Load<Texture2D>("Textures/Structures/Economy/Structure_Factory"), tile, Screens.BuildMenuStates.Economical, Screens.BuildMenuStates.Factory, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(buildItems[2].position.X - tile.Width, buildItems[2].position.Y), content.Load<Texture2D>("Textures/Structures/Economy/Structure_Market"), tile, Screens.BuildMenuStates.Economical, Screens.BuildMenuStates.Market, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(buildItems[2].position.X - (tile.Width / 2), buildItems[2].position.Y + tile.Height), content.Load<Texture2D>("Textures/Structures/Economy/Structure_Exporter"), tile, Screens.BuildMenuStates.Economical, Screens.BuildMenuStates.Exporter, Color.Aqua));

            //Military 9-11
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(buildItems[1].position.X + tile.Width, buildItems[1].position.Y), content.Load<Texture2D>("Textures/Structures/Military/Structure_Barracks"), tile, Screens.BuildMenuStates.Military, Screens.BuildMenuStates.Baracks , Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(buildItems[1].position.X - (tile.Width / 2), buildItems[1].position.Y + tile.Height), content.Load<Texture2D>("Textures/Structures/Military/Structure_Airport"), tile, Screens.BuildMenuStates.Military, Screens.BuildMenuStates.AirField, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(buildItems[1].position.X + (tile.Width / 2), buildItems[1].position.Y + tile.Height), content.Load<Texture2D>("Textures/Structures/Military/Structure_TankDepot"), tile, Screens.BuildMenuStates.Military, Screens.BuildMenuStates.TankDepot , Color.Aqua));

            //Units 12-14
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(-1000, 0), content.Load<Texture2D>("Textures/Units/Military/Unit_Infantry"), tile, Screens.BuildMenuStates.Military, Screens.BuildMenuStates.Infantry, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(-1000, 0), content.Load<Texture2D>("Textures/Units/Military/Unit_Tank"), tile, Screens.BuildMenuStates.Military, Screens.BuildMenuStates.Tank, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(-1000, 0), content.Load<Texture2D>("Textures/Units/Military/Unit_Plane"), tile, Screens.BuildMenuStates.Military, Screens.BuildMenuStates.Jet, Color.Aqua));
        
            //Settlements 15-18
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(GraphicsDevice.Viewport.X + (GraphicsDevice.Viewport.Width / 2) - (tile.Width / 2), GraphicsDevice.Viewport.Y + (GraphicsDevice.Viewport.Height / 2) - (tile.Height / 2)), content.Load<Texture2D>("Textures/Structures/Civil/Structure_Settlement"), tile, Screens.BuildMenuStates.Settlement, Screens.BuildMenuStates.Settlement, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(-1000, 0), content.Load<Texture2D>("Textures/Structures/Civil/Structure_TownHall"), tile, Screens.BuildMenuStates.Settlement, Screens.BuildMenuStates.TownHall, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(-1000, 0), content.Load<Texture2D>("Textures/Structures/Civil/Structure_CityHall"), tile, Screens.BuildMenuStates.Settlement, Screens.BuildMenuStates.CityHall, Color.Aqua));
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(-1000, 0), content.Load<Texture2D>("Textures/Structures/Civil/Structure_Capitol"), tile, Screens.BuildMenuStates.Settlement, Screens.BuildMenuStates.Capitol, Color.Aqua));

            //Dictator 19
            buildItems.Add(new Screens.BuildMenuItem(new Vector2(-1000, 0), content.Load<Texture2D>("Textures/Units/Unit_Dictator"), tile, Screens.BuildMenuStates.Military, Screens.BuildMenuStates.Dictator, Color.Aqua));
        }

        private void SetupTextureRepository(ContentManager content)
        {
            textureRepo = new List<Texture2D>();
            textureRepo.Add(content.Load<Texture2D>("Icons//Modifiers//minusMoney"));
            textureRepo.Add(content.Load<Texture2D>("Icons//Modifiers//plusMoney"));
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Tell each of the screens to unload their content.
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Read the keyboard and gamepad.
            input.Update();

            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screensToUpdate.Clear();

            foreach (GameScreen screen in screens)
                screensToUpdate.Add(screen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = screensToUpdate[screensToUpdate.Count - 1];

                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(gameTime, input);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            // Print debug trace?
            if (traceEnabled)
                TraceScreens();
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (GameScreen screen in screens)
                screenNames.Add(screen.GetType().Name);

            //Debug.WriteLine(string.Join(", ", screenNames.ToArray()));
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            foreach (GameScreen screen in screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;
                
                screen.Draw(gameTime);
            }
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
        {
            screen.ControllingPlayer = controllingPlayer;
            screen.ScreenManager = this;
            screen.IsExiting = false;

            // If we have a graphics device, tell the screen to load content.
            if (isInitialized)
            {
                screen.LoadContent();
            }

            screens.Add(screen);
        }


        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (isInitialized)
            {
                screen.UnloadContent();
            }

            screens.Remove(screen);
            screensToUpdate.Remove(screen);
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(float alpha)
        {
            Viewport viewport = GraphicsDevice.Viewport;

            spriteBatch.Begin();

            spriteBatch.Draw(blankTexture,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             Color.Black * alpha);

            spriteBatch.End();
        }


        #endregion


    }

    public enum TextureRepoOrder
    {
        minusMoney = 0,
        plusMoney = 1,

    }

    public enum BuildItems
    {
        manipMenu = 0,
        militaryMenu = 1,
        economyMenu = 2,
        contractor = 3,
        labourCamp = 4,
        propaganda = 5,
        factory = 6,
        market = 7,
        exporter = 8,
        barracks = 9,
        airfield = 10,
        tankDepot = 11,
        infantry = 12,
        tank = 13,
        plane = 14,
        settlement = 15,
        townHall = 16,
        cityHall = 17,
        capitol = 18,
        dictator = 19,
    }
}
