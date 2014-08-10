using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PenisPotato.Structures.Military
{
    class AirBase : Structure
    {
        public AirBase() {} 

        public AirBase(Vector2 pos, Color color, Texture2D pieceTex)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.pieceType = (byte)PieceTypes.AirBase;
            this.tileWidth = Convert.ToInt16(Resources.tileWidth);
            this.buildTime = 15;
        }

        public override void Clicked(GameTime gameTime, Player.Player player)
        {
            if (built >= 100)
            {
                Units.Unit unitInSpace = player.playerUnits.Find(pu => pu.piecePosition.Equals(this.piecePosition));

                if (unitInSpace == null)
                {
                    player.playerUnits.Add(new Units.Jet(this.piecePosition, player.playerColor, 1, player.ScreenManager.buildItems[14].menuItem));
                    if (player.netPlayer != null)
                        player.netPlayer.unitsToSend.Enqueue(player.playerUnits[player.playerUnits.Count - 1]);
                }
                else if (unitInSpace.GetType() == typeof(Units.Jet))
                {
                    unitInSpace.AddUnits(1);
                    if (player.netPlayer != null)
                        player.netPlayer.unitsToUpdate.Enqueue(unitInSpace);
                }

            }
        }
    }
}
