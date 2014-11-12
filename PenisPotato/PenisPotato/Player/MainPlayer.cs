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
    public class MainPlayer : Player
    {
        public Vector2 selectedTilePos = Vector2.Zero;
        private Menu.Menu playerMenu;

        //Game input
        float zoomIncrement = 0.1f;
        Vector2 cameraMovement = Vector2.Zero;

        //Text
        public String MoneyString { get; set; }

        public MainPlayer() { LoadContent(); }

        /*public MainPlayer(ContentManager content, GraphicsDevice graphics, StateSystem.StateManager stateManager, StateSystem.Screens.MultiplayerGameplayScreen master, NetworkPlayer netPlayer, Color color)
        {
            this.graphics = graphics;
            this.Content = content;
            this.ScreenManager = stateManager;
            //this.masterState = master;
            this.netPlayer = netPlayer;
            this.playerColor = color;
            if (netPlayer != null)
            {
                netPlayer.InitGamePlayer(true, masterState);
            }
            playerStructures = new List<Structures.Structure>();
            playerSettlements = new List<Structures.Civil.Settlement>();
            playerUnits = new List<Units.Unit>();
            buildingTiles = new List<Vector2>();
            dupeBuildingTiles = new List<Vector2>();
            movementTiles = new List<Vector2>();

            LoadContent();
        }*/

        public MainPlayer(ContentManager content, GraphicsDevice graphics, StateSystem.StateManager stateManager, StateSystem.Screens.GameplayScreen master, NetworkPlayer netPlayer, Color color)
        {
            this.graphics = graphics;
            this.Content = content;
            this.ScreenManager = stateManager;
            this.masterState = master;
            this.netPlayer = netPlayer;
            this.playerColor = color;
            
            playerStructures = new List<Structures.Structure>();
            playerSettlements = new List<Structures.Civil.Settlement>();
            playerUnits = new List<Units.Unit>();
            buildingTiles = new List<Vector2>();
            dupeBuildingTiles = new List<Vector2>();
            movementTiles = new List<Vector2>();
            //Combat List
            combat = new List<Units.Combat>();
            if (netPlayer != null)
            {
                netPlayer.InitGamePlayer(true, masterState);
                this.netPlayer.combat = this.combat;
            }
            

            LoadContent();
        }

        public override void LoadContent()
        {
            base.LoadContent();
            playerMenu = new Menu.Menu(Content.Load<Texture2D>("Textures/Map/square"));
        }

        public override void Update(GameTime gameTime)
        {
            //base.Update(gameTime);
            MoneyString = String.Format("Money: {0}", money);
            UpdateStructures(gameTime);
            playerUnits.ForEach(pU => {
                pU.Update(gameTime, this);

                if (pU.numUnits < 1 || (pU.numUnits < 1 && netPlayer != null && !netPlayer.unitsToUpdate.Contains(pU)))
                    playerUnits.Remove(pU);
            });

            if (netPlayer != null)
            {
                netPlayer.Update(gameTime, this);
            }
            prevUnits = playerUnits.Count;
        }

        public override void UpdateStructures(GameTime gameTime)
        {
            int diff;

            base.UpdateStructures(gameTime);

            diff = playerStructures.Count - prevStructures;
            if (netPlayer != null && diff > 0)
                SendStructureInfo(diff);

            prevStructures = playerStructures.Count;
        }

        public void UpdateInput(GameTime gameTime, StateSystem.InputState input, Camera camera)
        {
            if (input.IsPauseGame(PlayerIndex.One))
                ScreenManager.AddScreen(new StateSystem.Screens.PauseMenuScreen(), PlayerIndex.One);
            else
            {
                // Adjust zoom if the mouse wheel has moved
                if (input.CurrentMouseStates[0].ScrollWheelValue > input.LastMouseStates[0].ScrollWheelValue)
                    camera.Zoom += zoomIncrement;
                else if (input.CurrentMouseStates[0].ScrollWheelValue < input.LastMouseStates[0].ScrollWheelValue)
                    camera.Zoom -= zoomIncrement;

                if (input.CurrentMouseStates[0].RightButton == ButtonState.Pressed)
                    cameraMovement = (new Vector2(input.LastMouseStates[0].X, input.LastMouseStates[0].Y) - new Vector2(input.CurrentMouseStates[0].X, input.CurrentMouseStates[0].Y)) / camera.Zoom;

                //Initial press onto the screen
                if (input.CurrentMouseStates[0].LeftButton == ButtonState.Pressed && input.LastMouseStates[0].LeftButton == ButtonState.Released)
                {
                    selectedTilePos = GetMouseStateRelative(input.CurrentMouseStates[0], camera);
                    DetermineSelectedTile(camera);

                    playerUnits.ForEach(pS =>
                    {
                        if (pS.piecePosition.Equals(selectedTilePos))
                            navigatingUnit = pS;
                    });

                    if (navigatingUnit != null && !movementTiles.Contains(selectedTilePos) && !navigatingUnit.piecePosition.Equals(selectedTilePos))
                    {
                        navigatingUnit.movementPoints.Clear();
                        movementTiles.Add(selectedTilePos);
                    }
                }
                //continuing press onto the screen
                else if (input.CurrentMouseStates[0].LeftButton == ButtonState.Pressed && input.LastMouseStates[0].LeftButton == ButtonState.Pressed)
                {
                    selectedTilePos = GetMouseStateRelative(input.CurrentMouseStates[0], camera);
                    DetermineSelectedTile(camera);

                    if (navigatingUnit != null && !movementTiles.Contains(selectedTilePos) && !navigatingUnit.piecePosition.Equals(selectedTilePos))
                        movementTiles.Add(selectedTilePos);
                }
                //release of press onto the screen
                if (input.CurrentMouseStates[0].LeftButton == ButtonState.Released && input.LastMouseStates[0].LeftButton == ButtonState.Pressed)
                {
                    //Dump the tiles we've highlighted for moving into the navigating unit's repertoire.
                    if (navigatingUnit != null && movementTiles.Count > 0)
                    {
                        //Clear the movement points because the new set of movement points is being sent to the navigating unit
                        navigatingUnit.movementPoints.AddRange(movementTiles);

                        //kill the reference to the navigating unit and clear movement points.
                        navigatingUnit = null;
                        movementTiles.Clear();
                    }
                    else
                    {
                        //If there's no buildings yet then we have to create an mvp
                        if (playerUnits.Count < 1)
                        {
                            playerUnits.Add(new Units.Misc.Dictator(selectedTilePos, playerColor, dictatorTex));
                            //err i know this is awkward placement for this but whatever
                            if (netPlayer != null)
                                netPlayer.unitsToSend.Enqueue(playerUnits[playerUnits.Count - 1]);
                        }
                        else
                        {

                            playerStructures.ForEach(pS =>
                            {
                                if (pS.piecePosition.Equals(selectedTilePos))
                                {
                                    pS.Clicked(gameTime, this);
                                    performedAction = true;
                                }
                            });

                            playerUnits.ForEach(pU =>
                            {
                                if (!performedAction && pU.piecePosition.Equals(selectedTilePos) && pU.canBuild)
                                {
                                    ScreenManager.AddScreen(new StateSystem.Screens.BuildMenuScreen(ScreenManager, this, true), PlayerIndex.One);
                                    performedAction = true;
                                }
                            });

                            //Otherwise, lets do this shit.
                            if (!performedAction && buildingTiles.Contains(selectedTilePos))
                                ScreenManager.AddScreen(new StateSystem.Screens.BuildMenuScreen(ScreenManager, this, false), PlayerIndex.One);
                        }
                    }
                }
                else if (input.CurrentMouseStates[0].LeftButton == ButtonState.Released && input.LastMouseStates[0].LeftButton == ButtonState.Released)
                {
                    navigatingUnit = null;
                    movementTiles.Clear();
                }
                performedAction = false;
            }

            if (Math.Abs(cameraMovement.X) > 0.1 || Math.Abs(cameraMovement.Y) > 0.1)
            {
                camera.Pos += cameraMovement;
                cameraMovement *= 0.9f;
            }
        }

        private Vector2 GetMouseStateRelative(MouseState mState, Camera camera)
        {
            float MouseWorldX = (camera.Pos.X - ((float)(graphics.Viewport.Width * 0.5) / camera.Zoom)) + (mState.X / camera.Zoom);
            float MouseWorldY = (camera.Pos.Y - ((float)(graphics.Viewport.Height * 0.5) / camera.Zoom)) + (mState.Y / camera.Zoom);

            return new Vector2(MouseWorldX, MouseWorldY);
        }

        private void DetermineSelectedTile(Camera camera)
        {
            selectedTilePos.X = ToRoundX(selectedTilePos.X / tileWidth);
            selectedTilePos.Y = ToRoundY((selectedTilePos.Y + (Math.Abs(selectedTilePos.X % 2) * (tileWidth / 2))) / tileHeight);
        }

        private bool CanBuild()
        {
            foreach (Vector2 position in buildingTiles)
            {
                if (position.Equals(selectedTilePos))
                    return false;
            }

            return true;
        }


    }
}
