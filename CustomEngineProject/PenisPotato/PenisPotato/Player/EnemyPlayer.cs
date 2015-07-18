using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace PenisPotato.Player
{
    public class EnemyPlayer : Player
    {
        /*private StateSystem.StateManager stateManager;
        private StateSystem.Screens.GameplayScreen masterState;

        public Int32 Money { get { return money; } set { money = value; } }
        public Color playerColor;
        public List<Structures.Structure> playerStructures;
        public List<Units.Unit> playerUnits;
        public List<Vector2> buildingTiles;*/

        //int tileWidth = 128;
        //int tileHeight = 128;
        //private int money = 0;


        public EnemyPlayer()
        {
            playerColor = Color.CadetBlue;
            InitializeEnemy();
        }

        public EnemyPlayer(StateSystem.StateManager stateManager, StateSystem.Screens.GameplayScreen master, Color playerColor)
        {
            this.ScreenManager = stateManager;
            this.masterState = master;
            this.Content = masterState.playerOne.Content;
            this.playerColor = playerColor;

            LoadContent();

            InitializeEnemy();
        }

        private void InitializeEnemy()
        {
            this.playerStructures = new List<Structures.Structure>();
            this.playerSettlements = new List<Structures.Civil.Settlement>();
            this.playerUnits = new List<Units.Unit>();
            this.dupeBuildingTiles = new List<Vector2>();
            this.buildingTiles = new List<Vector2>();

            playerUnits.Add(new Units.Misc.Dictator(new Vector2(6, 6), playerColor, ScreenManager.buildItems[(int)StateSystem.BuildItems.dictator].menuItem, GetNextUnitId()));
            playerUnits.Add(new Units.Infantry(new Vector2(4, 4), playerColor, 20, ScreenManager.buildItems[(int)StateSystem.BuildItems.infantry].menuItem, GetNextUnitId()));
            playerUnits.Add(new Units.Tank(new Vector2(5, 7), playerColor, 20, ScreenManager.buildItems[(int)StateSystem.BuildItems.tank].menuItem, GetNextUnitId()));
            playerUnits.Add(new Units.Jet(new Vector2(6, 4), playerColor, 20, ScreenManager.buildItems[(int)StateSystem.BuildItems.plane].menuItem, GetNextUnitId()));
            playerSettlements.Add(new Structures.Civil.Settlement(new Vector2(4, 5), playerColor, ScreenManager.buildItems[(int)StateSystem.BuildItems.settlement].menuItem));
            playerStructures.Add(playerSettlements.Last());
        }

        /*public override void Update(GameTime gameTime)
        {
            foreach (Structures.Structure structure in playerStructures)
                structure.Update(gameTime, this);
            playerUnits.ForEach(pU => pU.Update(gameTime, this));
        }*/

        /*public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (Structures.Structure structure in playerStructures)
                structure.Draw(spriteBatch);

            foreach (Units.Unit unit in playerUnits)
                unit.Draw(spriteBatch, ScreenManager.Font, ScreenManager);
        }*/
    }
}
