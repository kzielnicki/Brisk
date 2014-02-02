#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class PlayersMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry player1MenuEntry;
        MenuEntry player2MenuEntry;
        MenuEntry player3MenuEntry;
        MenuEntry player4MenuEntry;

        public enum PlayerType
        {
            None,
            Human,
            AI,
        }

        public static PlayerType player1 = PlayerType.Human;
        public static PlayerType player2 = PlayerType.Human;
        public static PlayerType player3 = PlayerType.None;
        public static PlayerType player4 = PlayerType.None;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayersMenuScreen()
            : base("Players", new Vector2(450, 510))
        {
            // Create our menu entries.
            player1MenuEntry = new MenuEntry(string.Empty);
            player2MenuEntry = new MenuEntry(string.Empty);
            player3MenuEntry = new MenuEntry(string.Empty);
            player4MenuEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();

            MenuEntry backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            player1MenuEntry.Selected += Player1MenuEntrySelected;
            player2MenuEntry.Selected += Player2MenuEntrySelected;
            player3MenuEntry.Selected += Player3MenuEntrySelected;
            player4MenuEntry.Selected += Player4MenuEntrySelected;
            backMenuEntry.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(player1MenuEntry);
            MenuEntries.Add(player2MenuEntry);
            MenuEntries.Add(player3MenuEntry);
            MenuEntries.Add(player4MenuEntry);
            MenuEntries.Add(backMenuEntry);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            player1MenuEntry.Text = "Player 1: " + player1;
            player2MenuEntry.Text = "Player 2: " + player2;
            player3MenuEntry.Text = "Player 3: " + player3;
            player4MenuEntry.Text = "Player 4: " + player4;
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Players menu entry is selected.
        /// </summary>
        void Player1MenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            player1++;
            if (player1 > PlayerType.AI)
                player1 = 0;

            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Players menu entry is selected.
        /// </summary>
        void Player2MenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            player2++;
            if (player2 > PlayerType.AI)
                player2 = 0;

            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Players menu entry is selected.
        /// </summary>
        void Player3MenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            player3++;
            if (player3 > PlayerType.AI)
                player3 = 0;

            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Players menu entry is selected.
        /// </summary>
        void Player4MenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            player4++;
            if (player4 > PlayerType.AI)
                player4 = 0;

            SetMenuEntryText();
        }



        #endregion
    }
}
