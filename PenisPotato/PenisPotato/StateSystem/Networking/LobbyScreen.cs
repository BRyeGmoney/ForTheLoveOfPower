#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using PenisPotato.StateSystem.Screens;
using Lidgren.Network;
#endregion

namespace PenisPotato.StateSystem.Networking
{
    /// <summary>
    /// The lobby screen provides a place for gamers to congregate before starting
    /// the actual gameplay. It displays a list of all the gamers in the session,
    /// and indicates which ones are currently talking. Each gamer can press a button
    /// to mark themselves as ready: gameplay will begin after everyone has done this.
    /// </summary>
    class LobbyScreen : GameScreen
    {
        #region Fields

        GameServer networkSession;
        //NetClient netClient;
        Player.NetworkPlayer netPlayer;

        Texture2D isReadyTexture;
        //Texture2D hasVoiceTexture;
        //Texture2D isTalkingTexture;
        //Texture2D voiceMutedTexture;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new lobby screen.
        /// </summary>
        public LobbyScreen(GameServer gameServer)
        {
            this.networkSession = gameServer;
            //this.networkSession.stateManager = ScreenManager;
            netPlayer = new Player.NetworkPlayer(true, "");

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public LobbyScreen(String ipaddress)
        {
            //this.netClient = netClient;
            netPlayer = new Player.NetworkPlayer(false, ipaddress);

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Loads graphics content used by the lobby screen.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            isReadyTexture = content.Load<Texture2D>("Icons/Networking/chat_ready");
            //hasVoiceTexture = content.Load<Texture2D>("chat_able");
            //isTalkingTexture = content.Load<Texture2D>("chat_talking");
            //voiceMutedTexture = content.Load<Texture2D>("chat_mute");
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the lobby screen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            netPlayer.Update(gameTime);

            if (netPlayer.peers.Count > 0)
            {
                bool allReady = true;
                foreach (Player.NetworkPlayer nP in netPlayer.peers)
                {
                    allReady = nP.IsReady;
                }

                if (allReady)
                    LoadingScreen.Load(ScreenManager, true, PlayerIndex.One,
                                  new MultiplayerGameplayScreen(netPlayer));
            }

            if (!IsExiting)
            {
                /*if (networkSession.SessionState == NetworkSessionState.Playing)
                {
                    // Check if we should leave the lobby and begin gameplay.
                    // We pass null as the controlling player, because the networked
                    // gameplay screen accepts input from any local players who
                    // are in the session, not just a single controlling player.
                    LoadingScreen.Load(ScreenManager, true, null,
                                       new MultiplayerGameplayScreen());
                }
                else if (networkSession.IsHost && networkSession.IsEveryoneReady)
                {
                    // The host checks whether everyone has marked themselves
                    // as ready, and starts the game in response.
                    networkSession.StartGame();
                }*/
            }
        }


        /// <summary>
        /// Handles user input for all the local gamers in the session. Unlike most
        /// screens, which use the InputState class to combine input data from all
        /// gamepads, the lobby needs to individually mark specific players as ready,
        /// so it loops over all the local gamers and reads their inputs individually.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.D))
                netPlayer.client.Disconnect("Bye");
            if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.R) && !input.LastKeyboardStates[0].IsKeyDown(Keys.R))
                SendReadyStatus();

            
            /*foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                PlayerIndex playerIndex = gamer.SignedInGamer.PlayerIndex;

                PlayerIndex unwantedOutput;

                if (input.IsMenuSelect(playerIndex, out unwantedOutput))
                {
                    HandleMenuSelect(gamer);
                }
                else if (input.IsMenuCancel(playerIndex, out unwantedOutput))
                {
                    HandleMenuCancel(gamer);
                }
            }*/
        }

        private void SendReadyStatus()
        {
            NetOutgoingMessage outmsg = netPlayer.client.CreateMessage();
            outmsg.Write((byte)Player.PacketType.READY);
            outmsg.Write(netPlayer.uniqueIdentifer);
            netPlayer.client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Handle MenuSelect inputs by marking ourselves as ready.
        /// </summary>
        void HandleMenuSelect(LocalNetworkGamer gamer)
        {
            if (!gamer.IsReady)
            {
                gamer.IsReady = true;
            }
            else if (gamer.IsHost)
            {
                // The host has an option to force starting the game, even if not
                // everyone has marked themselves ready. If they press select twice
                // in a row, the first time marks the host ready, then the second
                // time we ask if they want to force start.
                MessageBoxScreen messageBox = new MessageBoxScreen(
                                                    Resources.ConfirmForceStartGame);

                messageBox.Accepted += ConfirmStartGameMessageBoxAccepted;

                ScreenManager.AddScreen(messageBox, PlayerIndex.One);
            }
        }


        /// <summary>
        /// Event handler for when the host selects ok on the "are you sure
        /// you want to start even though not everyone is ready" message box.
        /// </summary>
        void ConfirmStartGameMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            /*if (networkSession.SessionState == NetworkSessionState.Lobby)
            {
                networkSession.StartGame();
            }*/
        }


        /// <summary>
        /// Handle MenuCancel inputs by clearing our ready status, or if it is
        /// already clear, prompting if the user wants to leave the session.
        /// </summary>
        void HandleMenuCancel(LocalNetworkGamer gamer)
        {
            if (gamer.IsReady)
            {
                gamer.IsReady = false;
            }
            else
            {
                PlayerIndex playerIndex = gamer.SignedInGamer.PlayerIndex;

                NetworkSessionComponent.LeaveSession(ScreenManager, playerIndex);
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the lobby screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Vector2 position = new Vector2(100, 150);

            // Make the lobby slide into place during transitions.
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            if (ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            spriteBatch.Begin();

            // Draw all the gamers in the session.
            int gamerCount = 0;

            // Draw the screen title.
            string title = Resources.Lobby;

            Vector2 titlePosition = new Vector2(0, 0);
            Vector2 titleOrigin = new Vector2(0,0);//font.MeasureString(title) / 2;
            Color titleColor = new Color(192, 192, 192) * TransitionAlpha;
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, title, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            foreach (Player.NetworkPlayer nP in netPlayer.peers)
            {
                gamerCount++;
                spriteBatch.DrawString(font, String.Format("{0} #{1}", nP.playerName, gamerCount), new Vector2(titlePosition.X + 32, titlePosition.Y + (gamerCount * 50)), nP.playerColor, 0,
                                        titleOrigin, titleScale, SpriteEffects.None, 0);
                if (nP.IsReady)
                    spriteBatch.Draw(isReadyTexture, new Rectangle((int)titlePosition.X, (int)titlePosition.Y + (gamerCount * 50), 32, 32),
                                 Color.Lime * TransitionAlpha);
            }

            spriteBatch.End();
        }


        /// <summary>
        /// Helper draws the gamertag and status icons for a single NetworkGamer.
        /// </summary>
        void DrawGamer(Player.NetworkPlayer player, Vector2 position)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            Vector2 iconWidth = new Vector2(34, 0);
            Vector2 iconOffset = new Vector2(0, 12);

            Vector2 iconPosition = position + iconOffset;

            // Draw the "is ready" icon.
            if (player.IsReady)
            {
                spriteBatch.Draw(isReadyTexture, iconPosition,
                                 Color.Lime * TransitionAlpha);
            }

            iconPosition += iconWidth;

            // Draw the "is muted", "is talking", or "has voice" icon.
            /*if (gamer.IsMutedByLocalUser)
            {
                //spriteBatch.Draw(voiceMutedTexture, iconPosition,
                  //               Color.Red * TransitionAlpha);
            }
            else if (gamer.IsTalking)
            {
                //spriteBatch.Draw(isTalkingTexture, iconPosition,
                                 //Color.Yellow * TransitionAlpha);
            }
            else if (gamer.HasVoice)
            {
                //spriteBatch.Draw(hasVoiceTexture, iconPosition,
                            //     Color.White * TransitionAlpha);
            }*/

            // Draw the gamertag, normally in white, but yellow for local players.
            string text = player.playerName;

            if (player.IsHost)
                text += Resources.HostSuffix;

            Color color = (player.IsHost) ? Color.Yellow : Color.White;

            spriteBatch.DrawString(font, text, position + iconWidth * 2,
                                   color * TransitionAlpha);
        }


        #endregion
    }
}

