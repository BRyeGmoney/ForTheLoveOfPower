using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PenisPotato.StateSystem.Screens
{
    public class BuildMenuScreen : GameScreen
    {
        public ContentManager content;
        public Player.MainPlayer player;
        public StateManager stateManager;
        public BuildMenuStates buildState;
        private List<BuildMenuItem> buildItems;

        public BuildMenuScreen(StateManager stateManage, Player.MainPlayer player, bool isSettlement)
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            if (isSettlement)
                buildState = BuildMenuStates.Settlement;
            else
                buildState = BuildMenuStates.Main;

            this.stateManager = stateManage;
            this.player = player;
            this.buildItems = this.stateManager.buildItems;
        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            base.LoadContent();
        }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            //MouseState mState = Mouse.GetState();
            BuildMenuItem buildItem;
            bool isClose = true;

            if (input.CurrentMouseStates[0].LeftButton == ButtonState.Pressed && input.LastMouseStates[0].LeftButton == ButtonState.Released)
            {
                if (buildState == BuildMenuStates.Settlement)
                {
                    buildItem = buildItems.Find(bi => bi.specificState.Equals(buildState));
                    if (buildItem.position.Contains(input.CurrentMouseStates[0].X, input.CurrentMouseStates[0].Y))
                    {
                        buildItem.PerformFunction(player, this);
                        isClose = false;
                    }
                }
                else
                {
                    buildItems.ForEach(bi =>
                    {
                        if (bi.position.Contains(input.CurrentMouseStates[0].X, input.CurrentMouseStates[0].Y))
                        {
                            if (bi.state.Equals(BuildMenuStates.Main))
                            {
                                bi.PerformFunction(this);
                                isClose = false;
                            }
                            else if (bi.state.Equals(buildState))
                            {
                                bi.PerformFunction(player, this);
                                isClose = false;
                            }
                        }
                    });
                }

                if (isClose.Equals(true))
                    stateManager.RemoveScreen(this);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            stateManager.SpriteBatch.Begin();

            //Goes through the list of build items and draws whatever the current selection of menu items is.
            buildItems.ForEach(bi => {
                if (bi.state.Equals(buildState) || (bi.state.Equals(BuildMenuStates.Main) && !buildState.Equals(BuildMenuStates.Settlement)))
                    bi.Draw(stateManager.SpriteBatch);
            });
            stateManager.SpriteBatch.End();
        }
    }

    public enum BuildMenuStates
    {
        Main,
        Civil,
        Military,
        Economical,
        Manipulation,
        Settlement,
        TownHall,
        CityHall,
        Capitol,
        Factory,
        Market,
        Exporter,
        GovernmentOffices,
        MilitaryContractor,
        LabourCamp,
        Propaganda,
        Baracks,
        AirField,
        TankDepot,
        Infantry,
        Tank,
        Jet,
        Dictator,
    }

    public class BuildMenuItem
    {
        public Rectangle position;
        public BuildMenuStates state;
        public BuildMenuStates specificState;
        public Texture2D menuItem;
        private Texture2D tile;
        private Color color;

        public BuildMenuItem(Vector2 pos, Texture2D menuTex, Texture2D tile, BuildMenuStates state, BuildMenuStates specificState, Color color)
        {
            this.position = new Rectangle((int)pos.X, (int)pos.Y, tile.Width, tile.Height);
            this.menuItem = menuTex;
            this.tile = tile;
            this.state = state;
            this.specificState = specificState;
            this.color = color;
        }

        public void PerformFunction(Player.MainPlayer player, BuildMenuScreen bms)
        {
            if (specificState.Equals(BuildMenuStates.Settlement))
            {
                player.playerSettlements.Add(new Structures.Civil.Settlement(player.selectedTilePos, player.playerColor, this.menuItem));
                player.playerStructures.Add(player.playerSettlements.Last());
            } 
            else
            {
                List<Vector2> selectedposition = new List<Vector2>();
                selectedposition.Add(player.selectedTilePos);

                Structures.Civil.Settlement settlement = player.playerSettlements.Find(pS =>
                    pS.GetAllTilesBelongingToSettlement().Intersect(selectedposition).Count() > 0);
                int indexOfOwner = player.playerSettlements.IndexOf(settlement);

                if (specificState.Equals(BuildMenuStates.Factory))
                    settlement.settlementProperties.Add(new Structures.Economy.Factory(player.selectedTilePos, player.playerColor, this.menuItem, indexOfOwner));
                else if (specificState.Equals(BuildMenuStates.Market))
                    settlement.settlementProperties.Add(new Structures.Economy.Market(player.selectedTilePos, player.playerColor, this.menuItem, indexOfOwner));
                else if (specificState.Equals(BuildMenuStates.Exporter))
                    settlement.settlementProperties.Add(new Structures.Economy.Exporter(player.selectedTilePos, player.playerColor, this.menuItem, indexOfOwner));
                else if (specificState.Equals(BuildMenuStates.Baracks))
                    settlement.settlementProperties.Add(new Structures.Military.Barracks(player.selectedTilePos, player.playerColor, this.menuItem, indexOfOwner));
                else if (specificState.Equals(BuildMenuStates.AirField))
                    settlement.settlementProperties.Add(new Structures.Military.AirBase(player.selectedTilePos, player.playerColor, this.menuItem, indexOfOwner));
                else if (specificState.Equals(BuildMenuStates.TankDepot))
                    settlement.settlementProperties.Add(new Structures.Military.TankDepot(player.selectedTilePos, player.playerColor, this.menuItem, indexOfOwner));
                else if (specificState.Equals(BuildMenuStates.LabourCamp))
                    settlement.settlementProperties.Add(new Structures.Manipulation.LabourCamp(player.selectedTilePos, player.playerColor, this.menuItem, indexOfOwner));
                else if (specificState.Equals(BuildMenuStates.MilitaryContractor))
                    settlement.settlementProperties.Add(new Structures.Manipulation.MilitaryContractor(player.selectedTilePos, player.playerColor, this.menuItem, indexOfOwner));
                else if (specificState.Equals(BuildMenuStates.Propaganda))
                    settlement.settlementProperties.Add(new Structures.Manipulation.Propaganda(player.selectedTilePos, player.playerColor, this.menuItem, indexOfOwner));

                player.playerStructures.Add(settlement.settlementProperties.Last());
            }
            
            bms.stateManager.RemoveScreen(bms);
        }

        public void PerformFunction(BuildMenuScreen bms)
        {
            bms.buildState = this.specificState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tile, position, color);
            spriteBatch.Draw(menuItem, position, color);
        }
    }
}
