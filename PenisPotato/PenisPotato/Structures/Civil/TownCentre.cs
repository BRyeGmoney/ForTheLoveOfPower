using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PenisPotato.Structures.Civil
{
    class TownCentre : Structure
    {
        public TownCentre() {}

        public TownCentre(Vector2 pos, Color color, Texture2D pieceTex)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.pieceType = (byte)PieceTypes.TownHall;
            this.tileWidth = pieceTex.Width;
            this.buildTime = 15;
        }

        public override void LoadContent(ContentManager Content)
        {
            this.pieceTexture = Content.Load<Texture2D>("Textures/Structures/Civil/TownCentre");
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
