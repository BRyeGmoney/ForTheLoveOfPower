using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PenisPotato.Structures.Economy
{
    public class Exporter : Structure
    {
        public int[] economies;

        public Exporter() { }

        public Exporter(Vector2 pos, Color color, Texture2D pieceTex, int owner)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.pieceType = (byte)PieceTypes.Exporter;
            this.tileWidth = Convert.ToInt16(Resources.tileWidth);
            this.buildTime = 25;
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

        public override void Clicked(GameTime gameTime, Player.MainPlayer player)
        {
            player.ScreenManager.AddScreen(new StateSystem.Screens.EconomyScreen(player, player.ScreenManager, this, Structures.PieceTypes.Exporter), PlayerIndex.One);
        }

        public override void Update(GameTime gameTime, Player.Player player)
        {
            if (gameTime.TotalGameTime.Seconds - lastTime > 1)
            {
                player.Money++;
                lastTime = gameTime.TotalGameTime.Seconds;
            }
            base.Update(gameTime, player);
        }
    }
}
