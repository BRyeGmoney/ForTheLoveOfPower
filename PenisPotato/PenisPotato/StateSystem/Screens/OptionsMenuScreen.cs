using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        string[] colors = { "Red", "Green", "Blue" };
        int currentColor;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {
            // Create our menu entries.
            nameTypeMenuEntry = new MenuEntry(string.Empty);
            colorChoiceMenuEntry = new MenuEntry(string.Empty);

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
            colorChoiceMenuEntry.Text = "Color: " + colors[currentColor];
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
            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Language menu entry is selected.
        /// </summary>
        void ColorChoiceEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentColor = (currentColor + 1) % colors.Length;

            SetMenuEntryText();
        }


        #endregion
    }
}

