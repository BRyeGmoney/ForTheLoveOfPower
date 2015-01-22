#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;
using PenisPotato.StateSystem.Screens;
using System.Threading;
using Lidgren.Network;
#endregion

namespace PenisPotato.StateSystem.Networking
{
    /// <summary>
    /// This menu screen lets the user choose whether to create a new
    /// network session, or search for an existing session to join.
    /// </summary>
    class CreateOrFindSessionScreen : StateSystem.Screens.MenuScreen
    {
        // Server object
        GameServer gameServer;

        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public CreateOrFindSessionScreen()
            : base("Create Or Find Session")
        {
            // Create our menu entries.
            MenuEntry createSessionMenuEntry = new MenuEntry(Resources.CreateSession);
            MenuEntry findSessionsMenuEntry = new MenuEntry(Resources.FindLocalSessions);
            MenuEntry findIPSessionMenuEntry = new MenuEntry(Resources.FindByIP);
            MenuEntry backMenuEntry = new MenuEntry(Resources.Back);

            // Hook up menu event handlers.
            createSessionMenuEntry.Selected += CreateSessionMenuEntrySelected;
            findSessionsMenuEntry.Selected += FindSessionsMenuEntrySelected;
            findIPSessionMenuEntry.Selected += FindIPSessionsMenuEntrySelected;
            backMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(createSessionMenuEntry);
            MenuEntries.Add(findSessionsMenuEntry);
            MenuEntries.Add(findIPSessionMenuEntry);
            MenuEntries.Add(backMenuEntry);
        }
        #endregion

        #region Event Handlers


        /// <summary>
        /// Event handler for when the Create Session menu entry is selected.
        /// </summary>
        void CreateSessionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            int retries = 0;

            gameServer = new GameServer();
            gameServer.Run();

            while (!gameServer.IsRunning() && retries <= 50)
            {
                Thread.Sleep(500);
                retries++;
            }

            if (retries <= 50)
                ScreenManager.AddScreen(new LobbyScreen(gameServer, ScreenManager), PlayerIndex.One, false);
        }


        /// <summary>
        /// Event handler for when the asynchronous create network session
        /// operation has completed.
        /// </summary>
        void CreateSessionOperationCompleted(object sender,
                                             OperationCompletedEventArgs e)
        {
            //and toss the user into a lobby if succesful
        }


        /// <summary>
        /// Event handler for when the Find Sessions menu entry is selected.
        /// </summary>
        void FindSessionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new LobbyScreen("", ScreenManager), PlayerIndex.One, false);
        }


        /// <summary>
        /// Event handler for when the find by ip menu entry is selected.
        /// </summary>
        void FindIPSessionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Enter the IP you \nwish to connect to:";

            MessageBoxScreen inputIPMessageBox = new MessageBoxScreen(message);

            inputIPMessageBox.Accepted += InputIPMessageBoxAccepted;

            ScreenManager.AddScreen(inputIPMessageBox, ControllingPlayer, false);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void InputIPMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            MessageBoxScreen msg = sender as MessageBoxScreen;
            ScreenManager.AddScreen(new LobbyScreen(msg.IPAddress, ScreenManager), PlayerIndex.One, false);
        }


        #endregion
    }
}
