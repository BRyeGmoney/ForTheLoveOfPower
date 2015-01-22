using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PenisPotato.Graphics;
namespace PenisPotato.StateSystem.Screens
{
    class MainMenuScreen : GameScreen
    {
        public ContentManager content;
        private List<MenuItem> menuItems;
        BloomComponent bloom;
        Camera camera;

        public MainMenuScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            camera = new Camera(ScreenManager.GraphicsDevice.Viewport, 10000000, 10000000, 1f);
            camera.Pos = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2);
            bloom = new BloomComponent(ScreenManager.Game);
            ScreenManager.Game.Components.Add(bloom);
            bloom.Settings = new BloomSettings(null, 0.15f, 2, 1.25f, 1, 1.5f, 1);
            Vector2 origin = new Vector2(ScreenManager.GraphicsDevice.Viewport.X + (ScreenManager.GraphicsDevice.Viewport.Width / 2), ScreenManager.GraphicsDevice.Viewport.Y + (ScreenManager.GraphicsDevice.Viewport.Height / 2));

            menuItems = new List<MenuItem>();
            menuItems.Add(new Screens.MenuItem(new Vector2(origin.X - (ScreenManager.tile.Width / 2), origin.Y - ScreenManager.tile.Height), content.Load<Texture2D>("Textures/Misc/Menu_Manipulation"), ScreenManager.tile, MenuItemStates.SinglePlayer, Color.Crimson));
            menuItems.Add(new Screens.MenuItem(new Vector2(origin.X - ScreenManager.tile.Width, origin.Y), content.Load<Texture2D>("Textures/Misc/Menu_Manipulation"), ScreenManager.tile, MenuItemStates.Multiplayer,Color.Magenta));
            menuItems.Add(new Screens.MenuItem(new Vector2(origin.X - (ScreenManager.tile.Width / 2), origin.Y + ScreenManager.tile.Height), content.Load<Texture2D>("Textures/Misc/Menu_Manipulation"), ScreenManager.tile, MenuItemStates.Options, Color.Orange));

            base.LoadContent();
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input.CurrentMouseStates[0].LeftButton == ButtonState.Pressed && input.LastMouseStates[0].LeftButton == ButtonState.Released)
            {
                menuItems.ForEach(mi =>
                    {
                        if (mi.position.Contains(input.CurrentMouseStates[0].X, input.CurrentMouseStates[0].Y))
                        {
                            mi.PerformFunction(ScreenManager);
                        }
                    });
            }
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            bloom.BeginDraw();
            ScreenManager.SpriteBatch.Begin();
            //ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate,
            //        null, SamplerState.PointClamp, null, null, null,
            //        camera.GetTransformation());

            ScreenManager.SpriteBatch.FillRectangle(new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height), Color.Black);

            menuItems.ForEach(mI =>
                {
                    mI.Draw(ScreenManager.SpriteBatch);
                });

            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "Single Player", new Vector2(menuItems[0].position.X + 64, menuItems[0].position.Y + (64 / 5)), Color.Crimson);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "Multiplayer", new Vector2(menuItems[1].position.X + 64, menuItems[1].position.Y + (64 / 5)), Color.Magenta);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, "Options", new Vector2(menuItems[2].position.X + 64, menuItems[2].position.Y + (64 / 5)), Color.Orange);
            ScreenManager.SpriteBatch.End();
        }
    }

    public enum MenuItemStates
    {
        SinglePlayer,
        Multiplayer,
        Options,
    }

    public class MenuItem
    {
        public Rectangle position;
        public MenuItemStates curMenuItemState;
        public Texture2D menuItem;
        private Texture2D tile;
        private Color color;

        public MenuItem(Vector2 pos, Texture2D menuTex, Texture2D tile, MenuItemStates curState, Color color)
        {
            this.position = new Rectangle((int)pos.X, (int)pos.Y, tile.Width, tile.Height);
            this.menuItem = menuTex;
            this.tile = tile;
            this.curMenuItemState = curState;
            this.color = color;
        }

        public void PerformFunction(StateManager ScreenManager)
        {
            if (this.curMenuItemState.Equals(MenuItemStates.SinglePlayer))
                PlayGameMenuEntrySelected(ScreenManager, PlayerIndex.One);
            else if (this.curMenuItemState.Equals(MenuItemStates.Multiplayer))
                MultiPlayGameMenuEntrySelected(ScreenManager, PlayerIndex.One);
            else
                OptionsMenuEntrySelected(ScreenManager, PlayerIndex.One);
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        public void PlayGameMenuEntrySelected(StateManager ScreenManager, PlayerIndex e)
        {
            LoadingScreen.Load(ScreenManager, true, e,
                               new GameplayScreen());
        }

        void MultiPlayGameMenuEntrySelected(StateManager ScreenManager, PlayerIndex e)
        {
            ScreenManager.AddScreen(new Networking.CreateOrFindSessionScreen(), e, true);
        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(StateManager ScreenManager, PlayerIndex e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(ScreenManager), e, true);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tile, position, color);
            spriteBatch.Draw(menuItem, position, color);
        }
    }
}
