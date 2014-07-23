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
    public class Player
    {
        public ContentManager Content;
        public GraphicsDevice graphics;
        public StateSystem.StateManager ScreenManager;
        public StateSystem.Screens.GameplayScreen masterState;
       
        public String playerName;
        public NetworkPlayer netPlayer;

        public Int32 Money { get { return money; } set { money = value; } }
        public Color playerColor;
        public List<Structures.Structure> playerStructures;
        public List<Structures.Civil.Settlement> playerSettlements;
        public List<Units.Unit> playerUnits;
        public List<Vector2> buildingTiles;
        public List<Vector2> movementTiles;
        public Units.Unit navigatingUnit;

        public Texture2D dictatorTex;
        public int tileWidth = 256;
        public int tileHeight = 256;
        
        public int prevStructures;
        public int prevUnits;
        public bool performedAction = false;

        public int money = 0;

        public String PlayerName { get { return playerName; } set { playerName = value; } }

        public Player() { }

        /*public Player(ContentManager content, GraphicsDevice graphics, StateSystem.StateManager stateManager, StateSystem.Screens.MultiplayerGameplayScreen master, NetworkPlayer netPlayer, Color color)
        {
            this.graphics = graphics;
            this.Content = content;
            this.ScreenManager = stateManager;
            //this.masterState = master;
            this.netPlayer = netPlayer;
            this.playerColor = color;
            if (netPlayer != null)
            {
                netPlayer.InitGamePlayer(true);
                netPlayer.stateManager = ScreenManager;
            }
            playerStructures = new List<Structures.Structure>();
            playerSettlements = new List<Structures.Civil.Settlement>();
            playerUnits = new List<Units.Unit>();
            buildingTiles = new List<Vector2>();
            movementTiles = new List<Vector2>();

            LoadContent();
        }

        public Player(ContentManager content, GraphicsDevice graphics, StateSystem.StateManager stateManager, StateSystem.Screens.GameplayScreen master, NetworkPlayer netPlayer, Color color)
        {
            this.graphics = graphics;
            this.Content = content;
            this.ScreenManager = stateManager;
            this.masterState = master;
            this.netPlayer = netPlayer;
            this.playerColor = color;
            if (netPlayer != null)
            {
                netPlayer.InitGamePlayer(true);
                netPlayer.stateManager = ScreenManager;
            }
            playerStructures = new List<Structures.Structure>();
            playerSettlements = new List<Structures.Civil.Settlement>();
            playerUnits = new List<Units.Unit>();
            buildingTiles = new List<Vector2>();
            movementTiles = new List<Vector2>();

            LoadContent();
        }*/

        public virtual void LoadContent()
        {
            dictatorTex = Content.Load<Texture2D>("Textures/Units/Unit_Dictator");
        }

        public virtual void Update(GameTime gameTime)
        {
            UpdateStructures(gameTime);
            playerUnits.ForEach(pU => pU.Update(gameTime, this));
        }

        public virtual void UpdateStructures(GameTime gameTime)
        {
            playerStructures.ForEach(pS => pS.Update(gameTime, this));
        }

        public void SendStructureInfo(int diff)
        {
            for (int x = diff; x > 0; x--)
                netPlayer.structuresToSend.Enqueue(playerStructures[playerStructures.Count - diff]);
        }

        public int ToRoundX(float num)
        {
            if (num < 0)
                return (int)Math.Floor(num);
            else
                return (int)num;
        }

        public int ToRoundY(float num)
        {
            if (num < 0)
                return (int)Math.Floor(num);
            else
                return (int)num;
        }

        

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime, Camera camera)
        {
            float minboundsx = (camera.Pos.X * camera.Zoom) - camera._viewportWidth, maxboundsx = (camera.Pos.X * camera.Zoom) + camera._viewportWidth;
            float minboundsy = (camera.Pos.Y * camera.Zoom) - camera._viewportHeight, maxboundsy = (camera.Pos.Y * camera.Zoom) + camera._viewportHeight;
            float modTileWidth = tileWidth * camera.Zoom;

            //camera.numStructsDrawn = 0; camera.numUnitsDrawn = 0;

            foreach (Units.Unit unit in playerUnits)
                if (isInRange(unit.piecePosition.X * modTileWidth, minboundsx - modTileWidth, maxboundsx) && isInRange(unit.piecePosition.Y * modTileWidth, minboundsy - modTileWidth, maxboundsy))
                    unit.Draw(spriteBatch, ScreenManager.Font, ScreenManager);

            foreach (Structures.Structure structure in playerStructures)
                if (isInRange(structure.piecePosition.X * modTileWidth, minboundsx - modTileWidth, maxboundsx) && isInRange(structure.piecePosition.Y * modTileWidth, minboundsy - modTileWidth, maxboundsy))
                    structure.Draw(spriteBatch, gameTime, ScreenManager);
        }

        private bool isInRange(float value, float min, float max)
        {
            return value > min && value < max;
        }
    }
}
