using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PenisPotato.Structures.Manipulation
{
    class MilitaryContractor : Structure
    {
        public MilitaryContractor() { }

        public MilitaryContractor(Vector2 pos, Color color, Texture2D pieceTex)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.pieceType = (byte)PieceTypes.Factory;
            this.tileWidth = pieceTex.Width;
            this.buildTime = 25;
        }
    }
}
