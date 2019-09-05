using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PenisPotato.Units.Misc
{
    public class Dictator : Unit
    {
        public Dictator() { }

        public Dictator(Vector2 pos, Color color, Texture2D pieceTex, int unitId)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.unitType = (byte)UnitType.Dictator;
            this.movementPoints = new List<Vector2>();
            this.unitSpeed = 2.0f;
            this.tileWidth = Convert.ToInt16(Resources.tileWidth);
            this.unitID = unitId;
        }

        public override void LoadContent(ContentManager Content)
        {
            this.pieceTexture = Content.Load<Texture2D>("Textures/Units/Unit_Dictator");
        }

        public override void Update(GameTime gameTime, Player.Player player)
        {

            base.Update(gameTime, player);
        }
    }
}
