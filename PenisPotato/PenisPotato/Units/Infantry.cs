﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PenisPotato.Units
{
    class Infantry : Unit
    {
        public Infantry() { }

        public Infantry(Vector2 pos, Color color, Texture2D pieceTex)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.movementPoints = new List<Vector2>();
            this.unitSpeed = 3.0f;
            this.tileWidth = pieceTex.Width;
            this.unitType = UnitType.Infantry;
            this.goodAgainst = UnitType.Tank;
        }
    }
}
