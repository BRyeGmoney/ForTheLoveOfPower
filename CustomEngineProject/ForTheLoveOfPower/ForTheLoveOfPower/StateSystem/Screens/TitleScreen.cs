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
    public class TitleScreen : GameScreen
    {
        GraphicsDevice graphics;
        ContentManager content;
        SpriteBatch spriteBatch;
        BloomComponent bloom;

        Texture2D powerBtnTex;
        private float modifierTimer;
        private TitleScreenState currentState;

        static Random random = new Random();

        private Rectangle powerBtnRect;
        int counter;
        bool finalFlicker = false;

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");
            graphics = ScreenManager.GraphicsDevice;
            bloom = new BloomComponent(ScreenManager.Game);

            ScreenManager.Game.Components.Add(bloom);
            //bloom.Settings = new BloomSettings(null, 0.15f, 2, 1.75f, 2.5f, 1.5f, 1.4f);
            bloom.Settings = new BloomSettings(null, 0.15f, 2, 2.75f, 2f, 1.5f, 1.4f);
            spriteBatch = ScreenManager.SpriteBatch;

            powerBtnTex = content.Load<Texture2D>("Icons/powerBtn");
            powerBtnRect = new Rectangle(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2), ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2), 100, 40);

            currentState = TitleScreenState.FlashingPower;
        }

        public Double RandomNormal(double mean, double stdDev)
        {
            double r1 = random.NextDouble();
            double r2 = random.NextDouble();
            return mean + (stdDev * (Math.Sqrt(-2 * Math.Log(r1)) * Math.Cos(6.28 * r2)));
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, false, false);

            

            if (currentState.Equals(TitleScreenState.FlashingPower))
            {
                modifierTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (modifierTimer > 1.5)
                    modifierTimer = 0;
            }
            else if (currentState.Equals(TitleScreenState.TitlePresentation))
            {
                modifierTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (modifierTimer > 1)
                {
                    modifierTimer = 0;
                    counter++;
                }

                if (counter > 6)
                {
                    currentState = TitleScreenState.RuleAllLoad;
                    modifierTimer = 0;
                }
            }
            else if (currentState.Equals(TitleScreenState.RuleAllLoad))
            {
                modifierTimer += 25 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                modifierTimer = Math.Min(modifierTimer, 100);

                if (modifierTimer >= 100)
                {
                    ScreenManager.AddScreen(new StateSystem.Screens.MainMenuScreen(), null, false);
                    currentState = TitleScreenState.Welcome;
                }
            }

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
            //int playerIndex = (int)ControllingPlayer.Value;
            if (currentState.Equals(TitleScreenState.FlashingPower))
            {
                MouseState mState = input.CurrentMouseStates[0];
                if (mState.LeftButton == ButtonState.Pressed && powerBtnRect.Contains(new Rectangle(mState.X, mState.Y, 1, 1)))
                {
                    currentState = TitleScreenState.TitlePresentation;
                    modifierTimer = 0;
                }
            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            bloom.BeginDraw();
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

            if (currentState.Equals(TitleScreenState.FlashingPower))
                DrawFlashingPowerState();
            else if (currentState.Equals(TitleScreenState.TitlePresentation))
                DrawTitlePresentation();
            else if (currentState.Equals(TitleScreenState.RuleAllLoad))
                DrawRuleAllLoad();
            else if (currentState.Equals(TitleScreenState.Welcome))
                DrawWelcome();

            spriteBatch.End();
        }

        private void DrawFlashingPowerState()
        {
            //spriteBatch.Draw(powerBtnTex, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2), ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2)), new Color(238, 50, 51));
            //spriteBatch.DrawString(ScreenManager.Font, "POWER", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2) + 10, ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2) - 3), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1);
            spriteBatch.Draw(powerBtnTex, powerBtnRect, new Color(238, 50, 51) * (1 - (modifierTimer / 3)));
            spriteBatch.DrawString(ScreenManager.Font, "POWER", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2) + 15, ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2)), new Color(238, 50, 51) * (1 - (modifierTimer / 3)), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1);
        }

        private void DrawTitlePresentation()
        {
            if (counter < 5)
            {
                if (counter < 1)
                {
                    spriteBatch.DrawString(ScreenManager.Font, "POWER", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2) + 15, ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2)), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1);
                    spriteBatch.DrawString(ScreenManager.Font, "LOVE", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51) * Math.Min(1, modifierTimer), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.DrawString(ScreenManager.Font, "POWER", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2) + 15, ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2)), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1);
                    spriteBatch.DrawString(ScreenManager.Font, "LOVE", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                }

                if (counter > 0 && counter < 2)
                {
                    spriteBatch.DrawString(ScreenManager.Font, "Absurd-O-Matic Games", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 130, 40), Color.AntiqueWhite * Math.Min(1, modifierTimer), 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(ScreenManager.Font, "Presents", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 40, 80), Color.AntiqueWhite * Math.Min(1, modifierTimer), 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                }
                else if (counter >= 2)
                {
                    spriteBatch.DrawString(ScreenManager.Font, "Absurd-O-Matic Games", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 130, 40), Color.AntiqueWhite, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(ScreenManager.Font, "Presents", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 40, 80), Color.AntiqueWhite, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                }

                if (counter < 4 && counter > 2)
                {
                    //spriteBatch.DrawString(ScreenManager.Font, "POWER", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2) + 15, ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2)), new Color(238, 50, 51) * Math.Min(1, modifierTimer), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1);
                    spriteBatch.DrawString(ScreenManager.Font, "FOR THE      OF", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width) + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51) * (float)(RandomNormal(0.5, 0.5)), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                    //spriteBatch.DrawString(ScreenManager.Font, "LOVE", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51) * Math.Min(1, modifierTimer), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                }
                else if (counter > 3)
                {
                    //spriteBatch.DrawString(ScreenManager.Font, "POWER", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2) + 15, ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2)), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1);
                    spriteBatch.DrawString(ScreenManager.Font, "FOR THE      OF", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width) + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                    //spriteBatch.DrawString(ScreenManager.Font, "LOVE", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                }
            }
            else
            {
                if (modifierTimer < 0.5 && !finalFlicker)
                {
                    float r = (float)(RandomNormal(0.5, 0.5));
                    spriteBatch.DrawString(ScreenManager.Font, "Absurd-O-Matic Games", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 130, 40), Color.AntiqueWhite * r, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(ScreenManager.Font, "Presents", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 40, 80), Color.AntiqueWhite * r, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(ScreenManager.Font, "FOR THE      OF", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width) + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51) * r, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(ScreenManager.Font, "POWER", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2) + 15, ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2)), new Color(238, 50, 51) * r, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1);
                    spriteBatch.DrawString(ScreenManager.Font, "LOVE", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51) * r, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                }
                else if (modifierTimer < 0.7 && !finalFlicker)
                {
                    spriteBatch.DrawString(ScreenManager.Font, "Absurd-O-Matic Games", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 130, 40), Color.AntiqueWhite, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(ScreenManager.Font, "Presents", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 40, 80), Color.AntiqueWhite, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(ScreenManager.Font, "FOR THE      OF", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width) + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                    spriteBatch.DrawString(ScreenManager.Font, "POWER", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - (powerBtnTex.Width / 2) + 15, ScreenManager.GraphicsDevice.Viewport.Height / 2 + (powerBtnTex.Height / 2)), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1);
                    spriteBatch.DrawString(ScreenManager.Font, "LOVE", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 + 5, ScreenManager.GraphicsDevice.Viewport.Height / 2 - (powerBtnTex.Height / 2)), new Color(238, 50, 51), 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                    finalFlicker = true;
                }
            }
        }

        private void DrawRuleAllLoad()
        {
            Vector2 startPos = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 40, ScreenManager.GraphicsDevice.Viewport.Height / 2 - 5);
            spriteBatch.DrawString(ScreenManager.Font, "LOADING", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - 31, ScreenManager.GraphicsDevice.Viewport.Height / 2 + 5), new Color(57, 255, 20), 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1);
            spriteBatch.DrawLine(startPos, new Vector2(startPos.X + ((ScreenManager.GraphicsDevice.Viewport.Width / 2 + 40 - startPos.X) * (modifierTimer / 100)), ScreenManager.GraphicsDevice.Viewport.Height / 2 - 5), new Color(57, 255, 0) * 0.9f, 10);
        }

        private void DrawWelcome()
        {
        }
    }

    public enum TitleScreenState
    {
        FlashingPower = 0,
        TitlePresentation = 1,
        RuleAllLoad = 2,
        Welcome = 3,
    }
}
