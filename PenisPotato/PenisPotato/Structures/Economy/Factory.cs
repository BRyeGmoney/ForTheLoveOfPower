using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PenisPotato.Structures.Economy
{
    public class Factory : Structure
    {
        public int[] economies;

        public Factory() { }

        public Factory(Vector2 pos, Color color, Texture2D pieceTex, int owner)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.pieceType = (byte)PieceTypes.Factory;
            this.tileWidth = Convert.ToInt16(Resources.tileWidth);
            this.buildTime = 25;
            this.modifierIndex = (int)StateSystem.TextureRepoOrder.plusMoney;
            this.settlementOwnerIndex = owner;
            SetEconomy();
        }

        private void SetEconomy()
        {
            economies = new int[3];
            economies[0] = 100;
            economies[1] = 0;
            economies[2] = 0;
        }

        public override void LoadContent(ContentManager Content)
        {
            this.pieceTexture = Content.Load<Texture2D>("Textures/Structures/Economy/Factory");
        }

        public override void Clicked(GameTime gameTime, Player.MainPlayer player)
        {
            player.ScreenManager.AddScreen(new StateSystem.Screens.EconomyScreen(player, player.ScreenManager, this, Structures.PieceTypes.Factory), PlayerIndex.One);
        }

        public override void Update(GameTime gameTime, Player.Player player)
        {
            lastTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (built >= 100 && lastTime > 2f)
            {
                player.Money++;
                displayModifier = true;
                lastTime = 0.0f;
            }
            base.Update(gameTime, player);
        }
    }
}
