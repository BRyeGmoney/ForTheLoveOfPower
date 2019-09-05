using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PenisPotato.Structures.Economy
{
    public class Factory : Structure
    {
        public int[] economies;

        public Factory() { }

        public Factory(Vector2 pos, Color color, Texture2D pieceTex, int owner)
        {
            this.piecePosition = pos;
            this.playerColor = color;
            this.pieceTexture = pieceTex;
            this.pieceType = (byte)PieceTypes.Factory;
            this.tileWidth = Convert.ToInt16(Resources.tileWidth);
            this.buildTime = 25;
            this.modifierIndex = (int)StateSystem.TextureRepoOrder.plusMoney;
            this.settlementOwnerIndex = owner;
        }

        private void SetEconomy(int ind0, int ind1, int ind2)
        {
            economies = new int[3];
            economies[0] = ind0;
            economies[1] = ind1;
            economies[2] = ind2;
        }

        public override void LoadContent(ContentManager Content)
        {
            this.pieceTexture = Content.Load<Texture2D>("Textures/Structures/Economy/Factory");
        }

        public override void Clicked(GameTime gameTime, Player.MainPlayer player)
        {
            if (built >= 100)
            {
                int[] minEco = new int[3] { player.playerSettlements[settlementOwnerIndex].economyAvailable[0, 0] - player.playerSettlements[settlementOwnerIndex].economyAvailable[1, 0], 
                    player.playerSettlements[settlementOwnerIndex].economyAvailable[0, 1] - player.playerSettlements[settlementOwnerIndex].economyAvailable[1, 1], 
                    player.playerSettlements[settlementOwnerIndex].economyAvailable[0, 2] - player.playerSettlements[settlementOwnerIndex].economyAvailable[1, 2] };

                player.ScreenManager.AddScreen(new StateSystem.Screens.EconomyScreen(player, player.ScreenManager, this, Structures.PieceTypes.Factory, minEco), PlayerIndex.One, false);
            }
        }

        public void DoneModifyingEconomy(Player.Player player)
        {
            player.playerSettlements[settlementOwnerIndex].ReCalcFactoryEconomies();
            doneModifying = false;   
        }

        public override void Update(GameTime gameTime, Player.Player player)
        {
            lastTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (built == 100)
            {
                SetEconomy(10, 0, 0);
                doneModifying = true;
            }

            if (built >= 100)
            {
                if (doneModifying)
                    DoneModifyingEconomy(player);

                if (lastTime > 2f)
                {
                    //player.Money++;
                    displayModifier = true;
                    lastTime = 0.0f;
                }
            }
            base.Update(gameTime, player);
        }
    }
}
