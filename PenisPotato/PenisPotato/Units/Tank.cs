using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PenisPotato.Units
{
    class Tank : Unit
    {
        public Tank() { }

        public Tank(Vector2 pos, Color color, int numUnits, Texture2D pieceTex)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.movementPoints = new List<Vector2>();
            this.unitSpeed = 1.5f;
            this.numUnits = numUnits;
            this.tileWidth = Convert.ToInt16(Resources.tileWidth);
            this.unitType = (byte)UnitType.Tank;
            this.goodAgainst = UnitType.Jet;
            animPlayer = new Graphics.Animation.AnimationPlayer();
        }
    }
}
