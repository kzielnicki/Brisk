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
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry playersMenuEntry;
        MenuEntry placementMenuEntry;
        MenuEntry mapMenuEntry;
        MenuEntry speedMenuEntry;
        MenuEntry AIMenuEntry;
        MenuEntry bgVolumeEntry;
        MenuEntry sfxVolumeEntry;

        public enum Placement
        {
            Random,
            Manual,
        }

        public enum Map
        {
            SmallGrid,
            MediumGrid,
            LargeGrid,
            World,
        }

        public enum Speed
        {
            Slow,
            Moderate,
            Fast,
            Frantic,
        }

        public enum AIlevel
        {
            Easy,
            Moderate,
            Hard,
            Impossible,
        }

        public static int players = 2;
        public static Placement currentPlacement = Placement.Random;
        public static Map currentMap = Map.World;
        public static Speed currentSpeed = Speed.Moderate;
        public static AIlevel currentAI = AIlevel.Moderate;
        public static int bgVolume = 0;
        public static int sfxVolume = 100;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options", new Vector2(450, 510))
        {
            // Create our menu entries.
            playersMenuEntry = new MenuEntry(string.Empty);
            placementMenuEntry = new MenuEntry(string.Empty);
            mapMenuEntry = new MenuEntry(string.Empty);
            speedMenuEntry = new MenuEntry(string.Empty);
            AIMenuEntry = new MenuEntry(string.Empty);
            bgVolumeEntry = new MenuEntry(string.Empty);
            sfxVolumeEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();

            MenuEntry backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            playersMenuEntry.Selected += PlayersMenuEntrySelected;
            placementMenuEntry.Selected += PlacementMenuEntrySelected;
            mapMenuEntry.Selected += MapMenuEntrySelected;
            speedMenuEntry.Selected += SpeedMenuEntrySelected;
            AIMenuEntry.Selected += AIMenuEntrySelected;
            bgVolumeEntry.Selected += BgVolumeMenuEntrySelected;
            sfxVolumeEntry.Selected += SfxVolumeMenuEntrySelected;
            backMenuEntry.Selected += OnCancel;
            
            // Add entries to the menu.
            //MenuEntries.Add(playersMenuEntry);
            MenuEntries.Add(placementMenuEntry);
            //MenuEntries.Add(mapMenuEntry);
            MenuEntries.Add(speedMenuEntry);
            MenuEntries.Add(AIMenuEntry);
            MenuEntries.Add(bgVolumeEntry);
            MenuEntries.Add(sfxVolumeEntry);
            MenuEntries.Add(backMenuEntry);
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            //languageMenuEntry.Text = "Language: " + languages[currentLanguage];
            //frobnicateMenuEntry.Text = "Frobnicate: " + (frobnicate ? "on" : "off");
            playersMenuEntry.Text = "Players: " + players;
            placementMenuEntry.Text = "Placement Mode: " + currentPlacement;
            mapMenuEntry.Text = "Map Mode: " + currentMap;
            speedMenuEntry.Text = "Speed: " + currentSpeed;
            AIMenuEntry.Text = "AI Level: " + currentAI;
            bgVolumeEntry.Text = "Music Volume: " + bgVolume + "%";
            sfxVolumeEntry.Text = "Sound Volume: " + sfxVolume + "%";
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Players menu entry is selected.
        /// </summary>
        void PlayersMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            players++;
            if (players > 4)
                players = 1;

            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Placement menu entry is selected.
        /// </summary>
        void PlacementMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentPlacement++;

            if (currentPlacement > Placement.Manual)
                currentPlacement = 0;

            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Map menu entry is selected.
        /// </summary>
        void MapMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentMap++;

            if (currentMap > Map.World)
                currentMap = 0;

            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Speed menu entry is selected.
        /// </summary>
        void SpeedMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentSpeed++;

            if (currentSpeed > Speed.Frantic)
                currentSpeed = 0;

            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the AI menu entry is selected.
        /// </summary>
        void AIMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentAI++;

            if (currentAI > AIlevel.Impossible)
                currentAI = 0;

            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Players menu entry is selected.
        /// </summary>
        void BgVolumeMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            bgVolume += 10;
            if (bgVolume > 100)
                bgVolume = 0;

            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Players menu entry is selected.
        /// </summary>
        void SfxVolumeMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            sfxVolume += 10;
            if (sfxVolume > 100)
                sfxVolume = 0;

            SetMenuEntryText();
        }


        #endregion
    }
}
