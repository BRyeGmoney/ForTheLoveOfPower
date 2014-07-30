using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PenisPotato.Structures.Military
{
    class Barracks : Structure
    {

        public Barracks() { }

        public Barracks(Vector2 pos, Color color, Texture2D pieceTex)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.pieceType = (byte)PieceTypes.Barracks;
            this.tileWidth = Convert.ToInt16(Resources.tileWidth);
            this.buildTime = 25;
        }

        public override void Clicked(GameTime gameTime, Player.Player player)
        {
            if (built >= 100)
            {
                Units.Unit unitInSpace = player.playerUnits.Find(pu => pu.piecePosition.Equals(this.piecePosition));

                if (unitInSpace == null)
                    player.playerUnits.Add(new Units.Infantry(this.piecePosition, player.playerColor, player.ScreenManager.buildItems[12].menuItem));
                else if (unitInSpace.GetType() == typeof(Units.Infantry))
                    unitInSpace.AddUnits(1);
            }
        }
    }
}
