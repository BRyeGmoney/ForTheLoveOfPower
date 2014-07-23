using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PenisPotato.Structures.Military
{
    class TankDepot : Structure
    {
        public TankDepot() { }

        public TankDepot(Vector2 pos, Color color, Texture2D pieceTex)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.pieceType = (byte)PieceTypes.TankDepot;
            this.tileWidth = pieceTex.Width;
            this.buildTime = 20;
        }

        public override void Clicked(GameTime gameTime, Player.Player player)
        {
            if (built >= 100)
            {
                Units.Unit unitInSpace = player.playerUnits.Find(pu => pu.piecePosition.Equals(this.piecePosition));

                if (unitInSpace == null)
                    player.playerUnits.Add(new Units.Tank(this.piecePosition, player.playerColor, player.ScreenManager.buildItems[13].menuItem));
                else if (unitInSpace.GetType() == typeof(Units.Tank))
                    unitInSpace.AddUnits(1);
            }
        }
    }
}
