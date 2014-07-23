using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PenisPotato.Units
{
    class Jet : Unit
    {
        public Jet() { }

        public Jet(Vector2 pos, Color color, Texture2D pieceTex)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.movementPoints = new List<Vector2>();
            this.unitSpeed = 0.5f;
            this.tileWidth = pieceTex.Width;
            this.unitType = UnitType.Jet;
            this.goodAgainst = UnitType.Infantry;
        }
    }
}
