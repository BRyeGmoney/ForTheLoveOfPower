using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PenisPotato.StateSystem.Screens
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry nameTypeMenuEntry;
        MenuEntry colorChoiceMenuEntry;

        string name = "";
        Color[] colors = new Color[4] { Color.PaleVioletRed, Color.PaleTurquoise, Color.PaleGreen, Color.PaleGoldenrod };
        int currentColor;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen(StateManager screenManager)
            : base("Options")
        {
            name = screenManager.mpPlayerInfo.playerName;
            currentColor = colors.ToList().IndexOf(screenManager.mpPlayerInfo.playerColorChoice[0]);
            // Create our menu entries.
            nameTypeMenuEntry = new MenuEntry(name);
            colorChoiceMenuEntry = new MenuEntry(colors[currentColor].ToString());

            SetMenuEntryText();

            MenuEntry back = new MenuEntry("Back");
            
            // Hook up menu event handlers.
            nameTypeMenuEntry.Selected += NameTypeEntrySelected;
            colorChoiceMenuEntry.Selected += ColorChoiceEntrySelected;
            back.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(nameTypeMenuEntry);
            MenuEntries.Add(colorChoiceMenuEntry);
            MenuEntries.Add(back);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            nameTypeMenuEntry.Text = "Multiplayer Name: " + name;
            colorChoiceMenuEntry.Text = "Color: " + colors[currentColor].ToString();
            colorChoiceMenuEntry.playerColor = colors[currentColor];
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Ungulate menu entry is selected.
        /// </summary>
        void NameTypeEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Enter name:";

            MessageBoxScreen nameTypeMessageBox = new MessageBoxScreen(message);

            nameTypeMessageBox.Accepted += NameTypeMessageBoxAccepted;

            ScreenManager.AddScreen(nameTypeMessageBox, ControllingPlayer);
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void NameTypeMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            MessageBoxScreen msg = sender as MessageBoxScreen;
            name = msg.IPAddress;
            ScreenManager.mpPlayerInfo.playerName = name;
            SerializationHelper.Save(ScreenManager.mpPlayerInfo);
            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Language menu entry is selected.
        /// </summary>
        void ColorChoiceEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentColor = (currentColor + 1) % colors.Length;
            ScreenManager.mpPlayerInfo.playerColorChoice[0] = colors[currentColor];
            SerializationHelper.Save(ScreenManager.mpPlayerInfo);
            SetMenuEntryText();
        }


        #endregion
    }
}

