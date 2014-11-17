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
            SetEconomy(10, 0, 0);
        }

        private void SetEconomy(int ind0, int ind1, int ind2)
        {
            economies = new int[3];
            economies[0] = ind0;
            economies[1] = ind1;
            economies[2] = ind2;
        }

        public override void Clicked(GameTime gameTime, Player.MainPlayer player)
        {
            if (built >= 100)
            {
                int[] availEco = new int[3] { player.playerSettlements[settlementOwnerIndex].economyAvailable[0, 0] - player.playerSettlements[settlementOwnerIndex].economyAvailable[1, 0], 
                player.playerSettlements[settlementOwnerIndex].economyAvailable[0, 1] - player.playerSettlements[settlementOwnerIndex].economyAvailable[1, 1], 
                player.playerSettlements[settlementOwnerIndex].economyAvailable[0, 2] - player.playerSettlements[settlementOwnerIndex].economyAvailable[1, 2] };
                player.ScreenManager.AddScreen(new StateSystem.Screens.EconomyScreen(player, player.ScreenManager, this, Structures.PieceTypes.Exporter, availEco), PlayerIndex.One);
            }
        }

        public void DoneModifyingEconomy(Player.Player player)
        {
            player.playerSettlements[settlementOwnerIndex].ReCalcExporterEconomies();
            doneModifying = false;  
        }

        public override void Update(GameTime gameTime, Player.Player player)
        {
            lastTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (built == 100)
            {
                SetEconomy(3, 0, 0);
                doneModifying = true;
            }

            if (built >= 100)
            {
                if (doneModifying)
                    DoneModifyingEconomy(player);

                if (lastTime > 2f)
                {
                    //player.Money++;
                    //displayModifier = true;
                    lastTime = 0.0f;
                }
            }
            base.Update(gameTime, player);
        }
    }
}
