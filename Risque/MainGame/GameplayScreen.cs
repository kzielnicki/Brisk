#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields
        // define color pallete
        public static Color OCEAN = new Color(147 / 255f, 222 / 255f, 226 / 255f);
        public static Color BLUE = new Color(51 / 255f, 72 / 255f, 100 / 255f);
        public static Color YELLOW = new Color(151 / 255f, 142 / 255f, 31 / 255f);
        public static Color GREEN = new Color(67 / 255f, 125 / 255f, 75 / 255f);
        public static Color ORANGE = new Color(131 / 255f, 73 / 255f, 11 / 255f);
        public static Color GRAY = new Color(75 / 255f, 75 / 255f, 75 / 255f);
        int ROWS = 4;
        int COLS = 7;
        public double deployFreq = 10;
        public double baseInvDuration = 10;
        public double aiFreq;
        double invMusicEnd = -1000;
        int screenWidth, screenHeight;
        Vector2 offset;
        float scale = 1;

        ContentManager content;
        public SpriteFont gameFont;
        public SpriteFont gameFontSmall;
        public Texture2D whitePixel;
        public static Song bgMusic;
        public static Song invincibleMusic;
        public static Song victoryMusic;
        public static SoundEffect selectCountry;
        public static SoundEffect transferCountry;
        Texture2D country, arrow, arrowhead, crosshair;
        List<Texture2D> strengthWheel;
        UI.ProgressBar progressBar;
        public double totalGameTime = 0;
        double countdownTime;
        static int COUNTDOWN_LENGTH = 5;

        List<Player> players = new List<Player>();
        public static Player noone;
        List<Doodad> doodads = new List<Doodad>();
        public Dictionary<string, Country> territories;
        public List<Continent> continents;
        //Dictionary<string, List<Country>> continents;
        public List<Movement> movements = new List<Movement>();
        double lastDeployment = 0;
        double lastAI = 0;

        public Random random = new Random();

        enum GameState
        {
            Placement,
            Countdown,
            Playing,
            GameOver,
            Special,
        }
        GameState curGameState = GameState.Placement;
        Player curPlayer;
        int toPlace = 0;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.SmallGrid)
            {
                ROWS = 3;
                COLS = 5;
            }
            else if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.MediumGrid)
            {
                ROWS = 4;
                COLS = 7;
            }
            else if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.LargeGrid)
            {
                ROWS = 4;
                COLS = 9;
            }

            if (OptionsMenuScreen.currentSpeed == OptionsMenuScreen.Speed.Slow)
            {
                deployFreq = 40;
                Movement.frequency = 4;
            }
            else if (OptionsMenuScreen.currentSpeed == OptionsMenuScreen.Speed.Moderate)
            {
                deployFreq = 20;
                Movement.frequency = 2;
            }
            else if (OptionsMenuScreen.currentSpeed == OptionsMenuScreen.Speed.Fast)
            {
                deployFreq = 10;
                Movement.frequency = 1;
            }
            else if (OptionsMenuScreen.currentSpeed == OptionsMenuScreen.Speed.Frantic)
            {
                deployFreq = 3;
                Movement.frequency = 0.3;
            }

            if (OptionsMenuScreen.currentAI == OptionsMenuScreen.AIlevel.Easy)
            {
                aiFreq = 5;
            }
            else if (OptionsMenuScreen.currentAI == OptionsMenuScreen.AIlevel.Moderate)
            {
                aiFreq = 2;
            }
            else if (OptionsMenuScreen.currentAI == OptionsMenuScreen.AIlevel.Hard)
            {
                aiFreq = 0.8;
            }
            else if (OptionsMenuScreen.currentAI == OptionsMenuScreen.AIlevel.Impossible)
            {
                aiFreq = 0.1;
            }
            aiFreq *= 1.3; //decrease all AI difficulties a bit from original settings

            //if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.World)
            //{
            //    invDuration = deployFreq / 3f;
            //}
            //else
            //{
            //    invDuration = deployFreq;
            //}
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            whitePixel = content.Load<Texture2D>("whitepixel");
            gameFont = content.Load<SpriteFont>("gamefont");
            gameFontSmall = content.Load<SpriteFont>("gamefontsmall");
            country = content.Load<Texture2D>("country");
            arrow = content.Load<Texture2D>("arrow");
            arrowhead = content.Load<Texture2D>("arrowhead");
            crosshair = content.Load<Texture2D>("crosshair");

            selectCountry = content.Load<SoundEffect>("sound/select");
            transferCountry = content.Load<SoundEffect>("sound/boom");
            SoundEffect.MasterVolume = OptionsMenuScreen.sfxVolume / 100f;
            bgMusic = content.Load<Song>("sound/bgMusic");
            invincibleMusic = content.Load<Song>("sound/invincible");
            victoryMusic = content.Load<Song>("sound/victory");
            MediaPlayer.Volume = OptionsMenuScreen.bgVolume / 100f;
            MediaPlayer.Play(bgMusic);
            MediaPlayer.IsRepeating = true;

            strengthWheel = new List<Texture2D>();
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel1"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel2"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel3"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel4"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel5"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel6"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel7"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel8"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel9"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheel10"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB10"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB20"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB30"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB40"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB50"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB60"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB70"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB80"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB90"));
            strengthWheel.Add(content.Load<Texture2D>("strengthWheel/wheelB100"));

            //terrImages = new Dictionary<string,Texture2D>();
            //terrImages.Add("alaska", "territories/alaska2");
            //terrImages.Add("northwest", "territories/northwest");
            //terrImages.Add("westernUS", "territories/northwest-us");
            //terrImages.Add("greenland", "territories/greenland");
            //terrImages.Add("montreal", "territories/montreal");


            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            CreateMap();
            ScreenManager.Game.ResetElapsedTime();
        }


        public void CreateMap()
        {
            screenWidth = GameStateManagementGame.width;
            screenHeight = GameStateManagementGame.height;

            noone = new Player(this, PlayerIndex.One, GRAY, new Vector2(0, 0));

            if (PlayersMenuScreen.player1 == PlayersMenuScreen.PlayerType.Human)
                players.Add(new Player(this, PlayerIndex.One, BLUE, new Vector2(50, 10)));
            else if (PlayersMenuScreen.player1 == PlayersMenuScreen.PlayerType.AI)
                players.Add(new AI(this, PlayerIndex.One, BLUE, new Vector2(50, 10)));

            if (PlayersMenuScreen.player2 == PlayersMenuScreen.PlayerType.Human)
                players.Add(new Player(this, PlayerIndex.Two, YELLOW, new Vector2(930, 10)));
            else if (PlayersMenuScreen.player2 == PlayersMenuScreen.PlayerType.AI)
                players.Add(new AI(this, PlayerIndex.Two, YELLOW, new Vector2(930, 10)));

            if (PlayersMenuScreen.player3 == PlayersMenuScreen.PlayerType.Human)
                players.Add(new Player(this, PlayerIndex.Three, GREEN, new Vector2(50, 652)));
            if (PlayersMenuScreen.player3 == PlayersMenuScreen.PlayerType.AI)
                players.Add(new AI(this, PlayerIndex.Three, GREEN, new Vector2(50, 652)));

            if (PlayersMenuScreen.player4 == PlayersMenuScreen.PlayerType.Human)
                players.Add(new Player(this, PlayerIndex.Four, ORANGE, new Vector2(930, 652)));
            if (PlayersMenuScreen.player4 == PlayersMenuScreen.PlayerType.AI)
                players.Add(new AI(this, PlayerIndex.Four, ORANGE, new Vector2(930, 652)));

            curPlayer = players[random.Next(players.Count)];

            progressBar = new UI.ProgressBar(ScreenManager.Game, new Rectangle((screenWidth - 300)/2, 50, 300, 16));
            progressBar.maximum = (float)deployFreq;
            progressBar.fillColor = Color.White;
            progressBar.backgroundColor = Color.Black;


            if (OptionsMenuScreen.currentMap != OptionsMenuScreen.Map.World)
            {
                CreateGrid();
            }
            else
            {
                CreateWorld();
            }


            // we have to initialze the hover territory
            Dictionary<string, Country>.Enumerator e = territories.GetEnumerator();
            e.MoveNext();
            foreach (Player p in players)
            {
                p.hover = e.Current.Value;
            }

            //foreach (KeyValuePair<string, Country> c in territories)
            //{
            //    Player owner = c.Value.getOwner();
            //    bool frontLines = false;
            //    foreach (Country n in c.Value)
            //    {
            //        if (n.getOwner() != owner)
            //            frontLines = true;
            //    }
            //    if (frontLines)
            //        c.Value.setStrength(4);
            //}

            if (OptionsMenuScreen.currentPlacement == OptionsMenuScreen.Placement.Random)
                AssignRandomly();
            else
                AssignManually();

            pruneMovements();
        }

        public void CreateGrid()
        {
            float x = (screenWidth - country.Width * COLS) / 2;
            float y = (screenHeight - country.Height * ROWS) / 2;
            offset = new Vector2(x, y);
            territories = new Dictionary<string, Country>();

            for (int r = 0; r < ROWS; ++r)
            {
                for (int c = 0; c < COLS; ++c)
                {
                    x = country.Width / 2 + country.Width * c;
                    y = country.Height / 2 + country.Height * r;
                    Vector2 loc = new Vector2(x, y);
                    Country ctry = new Country(loc, country, noone, 4);
                    territories.Add(rcIdx(r, c), ctry);
                }
            }

            for (int r = 0; r < ROWS; ++r)
            {
                for (int c = 0; c < COLS; ++c)
                {
                    Country ctry, nbr;
                    territories.TryGetValue(rcIdx(r, c), out ctry);
                    if (c - 1 >= 0)
                    {
                        territories.TryGetValue(rcIdx(r, c - 1), out nbr);
                        ctry.addNeighbor(nbr);
                    }
                    if (c + 1 < COLS)
                    {
                        territories.TryGetValue(rcIdx(r, c + 1), out nbr);
                        ctry.addNeighbor(nbr);
                    }
                    if (r - 1 >= 0)
                    {
                        territories.TryGetValue(rcIdx(r - 1, c), out nbr);
                        ctry.addNeighbor(nbr);
                    }
                    if (r + 1 < ROWS)
                    {
                        territories.TryGetValue(rcIdx(r + 1, c), out nbr);
                        ctry.addNeighbor(nbr);
                    }
                }
            }
        }

        private void CreateWorld()
        {
            offset = new Vector2(0, 0);
            scale = 1.0f;
            territories = new Dictionary<string, Country>();
            doodads = new List<Doodad>();

            // import graphics
            Country c;
            //c = new Country(new Vector2(609.9492f, 358.1009f), new Vector2(0f, 0f), content.Load<Texture2D>("worldmap"), noone, 4);
            //territories.Add("worldmap", c);

            #region LoadDoodads
            doodads.Add(new Doodad(new Vector2(-14.69687f, 191.6566f), content.Load<Texture2D>("doodads/alaskaToKamchatka1"), "alaskaToKamchatka1", Color.White));
            doodads.Add(new Doodad(new Vector2(1266.268f, 191.0086f), content.Load<Texture2D>("doodads/alaskaToKamchatka2"), "alaskaToKamchatka2", Color.White));
            doodads.Add(new Doodad(new Vector2(516.4464f, 394.6735f), content.Load<Texture2D>("doodads/brazilToWestAfrica"), "brazilToWestAfrica", Color.White));
            doodads.Add(new Doodad(new Vector2(687.2501f, 292.322f), content.Load<Texture2D>("doodads/centralEuropeToEgypt"), "centralEuropeToEgypt", Color.White));
            doodads.Add(new Doodad(new Vector2(632.7992f, 273.3663f), content.Load<Texture2D>("doodads/centralEuropeToNorthAfrica"), "centralEuropeToNorthAfrica", Color.White));
            doodads.Add(new Doodad(new Vector2(980.6555f, 389.7321f), content.Load<Texture2D>("doodads/chinaToIndonesia"), "chinaToIndonesia", Color.White));
            doodads.Add(new Doodad(new Vector2(550.7408f, 193.8686f), content.Load<Texture2D>("doodads/greenlandToCentralEurope"), "greenlandToCentralEurope", Color.White));
            doodads.Add(new Doodad(new Vector2(1044.322f, 439.598f), content.Load<Texture2D>("doodads/indonesiaToEasternAustralia"), "indonesiaToEasternAustralia", Color.White));
            doodads.Add(new Doodad(new Vector2(1019.372f, 443.377f), content.Load<Texture2D>("doodads/indonesiaToWesternAustralia"), "indonesiaToWesternAustralia", Color.White));
            doodads.Add(new Doodad(new Vector2(424.5769f, 209.475f), content.Load<Texture2D>("doodads/montrealToGreenland"), "montrealToGreenland", Color.White));
            doodads.Add(new Doodad(new Vector2(635.3635f, 221.1855f), content.Load<Texture2D>("doodads/scandenaviaToCentralEurope"), "scandenaviaToCentralEurope", Color.White));
            doodads.Add(new Doodad(new Vector2(674.8361f, 216.3632f), content.Load<Texture2D>("doodads/scandenaviaToUkraine"), "scandenaviaToUkraine", Color.White));
            doodads.Add(new Doodad(new Vector2(614.2961f, 280.9735f), content.Load<Texture2D>("doodads/spainToNorthAfrica"), "spainToNorthAfrica", Color.White));
            doodads.Add(new Doodad(new Vector2(568.4507f, 516.1575f), content.Load<Texture2D>("doodads/compass"), "compass", Color.Lerp(OCEAN, Color.White, 0.3f)));
            doodads.Add(new Doodad(new Vector2(167.4364f, 497.0134f), content.Load<Texture2D>("doodads/briskBox"), "briskBox", Color.Lerp(OCEAN, Color.White, 0.3f)));
            //Color.Lerp(OCEAN, Color.White, 0.3f)
            //c = new Country(new Vector2(0f, 0f), content.Load<Texture2D>("doodads/briskBox"), noone, 4);
            //territories.Add("briskBox", c);

            #endregion

            #region InitializeCountries
            c = new Country(new Vector2(107.8508f, 200.828f), new Vector2(7.7f, -10.85f), content.Load<Texture2D>("territories/alaska"), noone, 4);
            territories.Add("alaska", c);
            c = new Country(new Vector2(222.2292f, 197.246f), new Vector2(-3.85f, 8.75f), content.Load<Texture2D>("territories/northwest"), noone, 4);
            territories.Add("northwest", c);
            c = new Country(new Vector2(343.4084f, 205.0364f), new Vector2(25.67739f, 27.60754f), content.Load<Texture2D>("territories/montreal"), noone, 4);
            territories.Add("montreal", c);
            c = new Country(new Vector2(457.0038f, 155.9649f), new Vector2(8.75f, -2.1f), content.Load<Texture2D>("territories/greenland"), noone, 4);
            territories.Add("greenland", c);
            c = new Country(new Vector2(237.4706f, 273.7477f), new Vector2(-1.75f, -2.45f), content.Load<Texture2D>("territories/westernUS"), noone, 4);
            territories.Add("westernUS", c);
            c = new Country(new Vector2(316.1926f, 296.1198f), new Vector2(-6.444972f, -11.69498f), content.Load<Texture2D>("territories/easternUS"), noone, 4);
            territories.Add("easternUS", c);
            c = new Country(new Vector2(280.1385f, 342.6657f), new Vector2(-12.95f, -7f), content.Load<Texture2D>("territories/mexico"), noone, 4);
            territories.Add("mexico", c);
            c = new Country(new Vector2(622.2657f, 313.593f), new Vector2(-2.594975f, 0.9050255f), content.Load<Texture2D>("territories/algeria"), noone, 4);
            territories.Add("algeria", c);
            c = new Country(new Vector2(389.8989f, 534.776f), new Vector2(6.897489f, -21.24749f), content.Load<Texture2D>("territories/argentina"), noone, 4);
            territories.Add("argentina", c);
            c = new Country(new Vector2(432.2558f, 456.1035f), new Vector2(9.432415f, -8.382415f), content.Load<Texture2D>("territories/brazil"), noone, 4);
            territories.Add("brazil", c);
            c = new Country(new Vector2(650.299f, 390.926f), new Vector2(8.569845f, -12.41985f), content.Load<Texture2D>("territories/congo"), noone, 4);
            territories.Add("congo", c);
            c = new Country(new Vector2(737.0989f, 384.7761f), new Vector2(-6.589952f, -6.589952f), content.Load<Texture2D>("territories/eastAfrica"), noone, 4);
            territories.Add("eastAfrica", c);
            c = new Country(new Vector2(688.9121f, 321.4395f), new Vector2(0f, 0f), content.Load<Texture2D>("territories/egypt"), noone, 4);
            territories.Add("egypt", c);
            c = new Country(new Vector2(587.9989f, 351.676f), new Vector2(-10.85f, 0.7f), content.Load<Texture2D>("territories/northAfrica"), noone, 4);
            territories.Add("northAfrica", c);
            c = new Country(new Vector2(717.1432f, 467.7557f), new Vector2(-16.1f, -1.75f), content.Load<Texture2D>("territories/southAfrica"), noone, 4);
            territories.Add("southAfrica", c);
            c = new Country(new Vector2(681.8353f, 246.2907f), new Vector2(-0.9801524f, -3.830152f), content.Load<Texture2D>("territories/centralEurope"), noone, 4);
            territories.Add("centralEurope", c);
            c = new Country(new Vector2(382.8569f, 419.208f), new Vector2(-17.58493f, -15.48492f), content.Load<Texture2D>("territories/venezuela"), noone, 4);
            territories.Add("venezuela", c);
            c = new Country(new Vector2(614.4059f, 246.7881f), new Vector2(3.955023f, 6.594975f), content.Load<Texture2D>("territories/westernEurope"), noone, 4);
            territories.Add("westernEurope", c);
            c = new Country(new Vector2(816.2734f, 277.6501f), new Vector2(5.587434f, -9.837434f), content.Load<Texture2D>("territories/afghanistan"), noone, 4);
            territories.Add("afghanistan", c);
            c = new Country(new Vector2(951.755f, 322.0817f), new Vector2(7.399749f, -7.919597f), content.Load<Texture2D>("territories/china"), noone, 4);
            territories.Add("china", c);
            c = new Country(new Vector2(1133.865f, 489.2401f), new Vector2(-41.7774f, 9.577386f), content.Load<Texture2D>("territories/easternAustralia"), noone, 4);
            territories.Add("easternAustralia", c);
            c = new Country(new Vector2(878.7529f, 328.2307f), new Vector2(-1.852512f, 0.9474875f), content.Load<Texture2D>("territories/india"), noone, 4);
            territories.Add("india", c);
            c = new Country(new Vector2(982.8348f, 403.5369f), new Vector2(14.39246f, 5.992463f), content.Load<Texture2D>("territories/indonesia"), noone, 4);
            territories.Add("indonesia", c);
            c = new Country(new Vector2(1135.609f, 193.1856f), new Vector2(-5.6f, -5.95f), content.Load<Texture2D>("territories/kamchatka"), noone, 4);
            territories.Add("kamchatka", c);
            c = new Country(new Vector2(765.4097f, 317.237f), new Vector2(-4.5897f, -8.039698f), content.Load<Texture2D>("territories/middleEast"), noone, 4);
            territories.Add("middleEast", c);
            c = new Country(new Vector2(1028.929f, 224.3549f), new Vector2(-5.684923f, -1.484924f), content.Load<Texture2D>("territories/moreSiberia"), noone, 4);
            territories.Add("moreSiberia", c);
            c = new Country(new Vector2(671.1532f, 194.2956f), new Vector2(-12.95f, 4.9f), content.Load<Texture2D>("territories/scandinavia"), noone, 4);
            territories.Add("scandinavia", c);
            c = new Country(new Vector2(907.4369f, 210.014f), new Vector2(28.39246f, 5.292463f), content.Load<Texture2D>("territories/siberia"), noone, 4);
            territories.Add("siberia", c);
            c = new Country(new Vector2(803.3606f, 208.4472f), new Vector2(-7.127388f, -3.977386f), content.Load<Texture2D>("territories/westRussia"), noone, 4);
            territories.Add("westRussia", c);
            c = new Country(new Vector2(1035.703f, 493.5189f), new Vector2(-6.939949f, 5.189949f), content.Load<Texture2D>("territories/westernAustralia"), noone, 4);
            territories.Add("westernAustralia", c);
            #endregion

            #region CreateContinents
            continents = new List<Continent>();
            Continent cont = new Continent("North America", 2);
            territories.TryGetValue("alaska", out c);
            cont.Add(c);
            territories.TryGetValue("northwest", out c);
            cont.Add(c);
            territories.TryGetValue("montreal", out c);
            cont.Add(c);
            territories.TryGetValue("greenland", out c);
            cont.Add(c);
            territories.TryGetValue("westernUS", out c);
            cont.Add(c);
            territories.TryGetValue("easternUS", out c);
            cont.Add(c);
            territories.TryGetValue("mexico", out c);
            cont.Add(c);
            continents.Add(cont);

            cont = new Continent("South America", 1);
            territories.TryGetValue("venezuela", out c);
            cont.Add(c);
            territories.TryGetValue("argentina", out c);
            cont.Add(c);
            territories.TryGetValue("brazil", out c);
            cont.Add(c);
            continents.Add(cont);

            cont = new Continent("Africa", 2);
            territories.TryGetValue("northAfrica", out c);
            cont.Add(c);
            territories.TryGetValue("congo", out c);
            cont.Add(c);
            territories.TryGetValue("southAfrica", out c);
            cont.Add(c);
            territories.TryGetValue("eastAfrica", out c);
            cont.Add(c);
            territories.TryGetValue("egypt", out c);
            cont.Add(c);
            territories.TryGetValue("algeria", out c);
            cont.Add(c);
            continents.Add(cont);

            cont = new Continent("Europe", 2);
            territories.TryGetValue("westernEurope", out c);
            cont.Add(c);
            territories.TryGetValue("centralEurope", out c);
            cont.Add(c);
            territories.TryGetValue("scandinavia", out c);
            cont.Add(c);
            continents.Add(cont);

            cont = new Continent("Asia", 3);
            territories.TryGetValue("westRussia", out c);
            cont.Add(c);
            territories.TryGetValue("middleEast", out c);
            cont.Add(c);
            territories.TryGetValue("afghanistan", out c);
            cont.Add(c);
            territories.TryGetValue("siberia", out c);
            cont.Add(c);
            territories.TryGetValue("moreSiberia", out c);
            cont.Add(c);
            territories.TryGetValue("kamchatka", out c);
            cont.Add(c);
            territories.TryGetValue("india", out c);
            cont.Add(c);
            territories.TryGetValue("china", out c);
            cont.Add(c);
            continents.Add(cont);

            cont = new Continent("Australia", 1);
            territories.TryGetValue("indonesia", out c);
            cont.Add(c);
            territories.TryGetValue("easternAustralia", out c);
            cont.Add(c);
            territories.TryGetValue("westernAustralia", out c);
            cont.Add(c);
            continents.Add(cont);
            #endregion

            // Adjacency lists
            Country n;

            #region NorthAmericaNeighbors
            territories.TryGetValue("alaska", out c);
            territories.TryGetValue("northwest", out n);
            c.addNeighbor(n);
            territories.TryGetValue("kamchatka", out n);
            c.addNeighbor(n);

            territories.TryGetValue("northwest", out c);
            territories.TryGetValue("alaska", out n);
            c.addNeighbor(n);
            territories.TryGetValue("montreal", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernUS", out n);
            c.addNeighbor(n);

            territories.TryGetValue("montreal", out c);
            territories.TryGetValue("northwest", out n);
            c.addNeighbor(n);
            territories.TryGetValue("greenland", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernUS", out n);
            c.addNeighbor(n);
            territories.TryGetValue("easternUS", out n);
            c.addNeighbor(n);

            territories.TryGetValue("greenland", out c);
            territories.TryGetValue("montreal", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernEurope", out n);
            c.addNeighbor(n);

            territories.TryGetValue("westernUS", out c);
            territories.TryGetValue("northwest", out n);
            c.addNeighbor(n);
            territories.TryGetValue("montreal", out n);
            c.addNeighbor(n);
            territories.TryGetValue("easternUS", out n);
            c.addNeighbor(n);
            territories.TryGetValue("mexico", out n);
            c.addNeighbor(n);

            territories.TryGetValue("easternUS", out c);
            territories.TryGetValue("montreal", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernUS", out n);
            c.addNeighbor(n);
            territories.TryGetValue("mexico", out n);
            c.addNeighbor(n);

            territories.TryGetValue("mexico", out c);
            territories.TryGetValue("easternUS", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernUS", out n);
            c.addNeighbor(n);
            territories.TryGetValue("venezuela", out n);
            c.addNeighbor(n);
            #endregion

            #region SouthAmericaNeighbors
            territories.TryGetValue("venezuela", out c);
            territories.TryGetValue("mexico", out n);
            c.addNeighbor(n);
            territories.TryGetValue("brazil", out n);
            c.addNeighbor(n);
            territories.TryGetValue("argentina", out n);
            c.addNeighbor(n);

            territories.TryGetValue("argentina", out c);
            territories.TryGetValue("venezuela", out n);
            c.addNeighbor(n);
            territories.TryGetValue("brazil", out n);
            c.addNeighbor(n);

            territories.TryGetValue("brazil", out c);
            territories.TryGetValue("venezuela", out n);
            c.addNeighbor(n);
            territories.TryGetValue("argentina", out n);
            c.addNeighbor(n);
            territories.TryGetValue("northAfrica", out n);
            c.addNeighbor(n);
            #endregion

            #region AfricaNeighbors
            territories.TryGetValue("northAfrica", out c);
            territories.TryGetValue("brazil", out n);
            c.addNeighbor(n);
            territories.TryGetValue("algeria", out n);
            c.addNeighbor(n);
            territories.TryGetValue("congo", out n);
            c.addNeighbor(n);

            territories.TryGetValue("congo", out c);
            territories.TryGetValue("northAfrica", out n);
            c.addNeighbor(n);
            territories.TryGetValue("algeria", out n);
            c.addNeighbor(n);
            territories.TryGetValue("egypt", out n);
            c.addNeighbor(n);
            territories.TryGetValue("eastAfrica", out n);
            c.addNeighbor(n);
            territories.TryGetValue("southAfrica", out n);
            c.addNeighbor(n);

            territories.TryGetValue("southAfrica", out c);
            territories.TryGetValue("eastAfrica", out n);
            c.addNeighbor(n);
            territories.TryGetValue("congo", out n);
            c.addNeighbor(n);

            territories.TryGetValue("eastAfrica", out c);
            territories.TryGetValue("southAfrica", out n);
            c.addNeighbor(n);
            territories.TryGetValue("congo", out n);
            c.addNeighbor(n);
            territories.TryGetValue("egypt", out n);
            c.addNeighbor(n);
            territories.TryGetValue("middleEast", out n);
            c.addNeighbor(n);

            territories.TryGetValue("egypt", out c);
            territories.TryGetValue("algeria", out n);
            c.addNeighbor(n);
            territories.TryGetValue("congo", out n);
            c.addNeighbor(n);
            territories.TryGetValue("eastAfrica", out n);
            c.addNeighbor(n);
            territories.TryGetValue("middleEast", out n);
            c.addNeighbor(n);
            territories.TryGetValue("centralEurope", out n);
            c.addNeighbor(n);

            territories.TryGetValue("algeria", out c);
            territories.TryGetValue("egypt", out n);
            c.addNeighbor(n);
            territories.TryGetValue("congo", out n);
            c.addNeighbor(n);
            territories.TryGetValue("northAfrica", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernEurope", out n);
            c.addNeighbor(n);
            territories.TryGetValue("centralEurope", out n);
            c.addNeighbor(n);
            #endregion

            #region EuropeNeighbors
            territories.TryGetValue("westernEurope", out c);
            territories.TryGetValue("algeria", out n);
            c.addNeighbor(n);
            territories.TryGetValue("centralEurope", out n);
            c.addNeighbor(n);
            territories.TryGetValue("scandinavia", out n);
            c.addNeighbor(n);

            territories.TryGetValue("centralEurope", out c);
            territories.TryGetValue("algeria", out n);
            c.addNeighbor(n);
            territories.TryGetValue("egypt", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernEurope", out n);
            c.addNeighbor(n);
            territories.TryGetValue("middleEast", out n);
            c.addNeighbor(n);
            territories.TryGetValue("scandinavia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westRussia", out n);
            c.addNeighbor(n);

            territories.TryGetValue("scandinavia", out c);
            territories.TryGetValue("centralEurope", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernEurope", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westRussia", out n);
            c.addNeighbor(n);
            #endregion

            #region AsiaNeighbors
            territories.TryGetValue("westRussia", out c);
            territories.TryGetValue("scandinavia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("centralEurope", out n);
            c.addNeighbor(n);
            territories.TryGetValue("afghanistan", out n);
            c.addNeighbor(n);
            territories.TryGetValue("siberia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("middleEast", out n);
            c.addNeighbor(n);

            territories.TryGetValue("middleEast", out c);
            territories.TryGetValue("centralEurope", out n);
            c.addNeighbor(n);
            territories.TryGetValue("egypt", out n);
            c.addNeighbor(n);
            territories.TryGetValue("eastAfrica", out n);
            c.addNeighbor(n);
            territories.TryGetValue("afghanistan", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westRussia", out n);
            c.addNeighbor(n);

            territories.TryGetValue("afghanistan", out c);
            territories.TryGetValue("westRussia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("middleEast", out n);
            c.addNeighbor(n);
            territories.TryGetValue("siberia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("india", out n);
            c.addNeighbor(n);

            territories.TryGetValue("siberia", out c);
            territories.TryGetValue("westRussia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("afghanistan", out n);
            c.addNeighbor(n);
            territories.TryGetValue("india", out n);
            c.addNeighbor(n);
            territories.TryGetValue("china", out n);
            c.addNeighbor(n);
            territories.TryGetValue("moreSiberia", out n);
            c.addNeighbor(n);

            territories.TryGetValue("moreSiberia", out c);
            territories.TryGetValue("siberia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("china", out n);
            c.addNeighbor(n);
            territories.TryGetValue("kamchatka", out n);
            c.addNeighbor(n);

            territories.TryGetValue("kamchatka", out c);
            territories.TryGetValue("moreSiberia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("alaska", out n);
            c.addNeighbor(n);

            territories.TryGetValue("india", out c);
            territories.TryGetValue("afghanistan", out n);
            c.addNeighbor(n);
            territories.TryGetValue("siberia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("china", out n);
            c.addNeighbor(n);

            territories.TryGetValue("china", out c);
            territories.TryGetValue("india", out n);
            c.addNeighbor(n);
            territories.TryGetValue("siberia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("moreSiberia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("indonesia", out n);
            c.addNeighbor(n);
            #endregion

            #region AustraliaNeighbors
            territories.TryGetValue("indonesia", out c);
            territories.TryGetValue("china", out n);
            c.addNeighbor(n);
            territories.TryGetValue("easternAustralia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernAustralia", out n);
            c.addNeighbor(n);

            territories.TryGetValue("easternAustralia", out c);
            territories.TryGetValue("indonesia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("westernAustralia", out n);
            c.addNeighbor(n);

            territories.TryGetValue("westernAustralia", out c);
            territories.TryGetValue("indonesia", out n);
            c.addNeighbor(n);
            territories.TryGetValue("easternAustralia", out n);
            c.addNeighbor(n);
            #endregion

        }

        public void AssignRandomly()
        {
            int terrPerPlayer = territories.Count / players.Count;
            int[] toPlace = new int[players.Count];
            for (int i = 0; i < players.Count; ++i)
                toPlace[i] = terrPerPlayer;

            //for (int r = 0; r < ROWS; ++r)
            //{
            //    for (int c = 0; c < COLS; ++c)
            //    {

            foreach (KeyValuePair<string, Country> c in territories)
            {
                int p = 0;
                ////DEBUG CODE FOR WORLD MAP
                //if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.World)
                //{
                //    if (terrPerPlayer != 0)
                //    {
                //        terrPerPlayer = 0;
                //    }
                //    else
                //        p = 1;
                //}
                ////END DEBUG CODE
                if (terrPerPlayer * players.Count > territories.Count)
                {
                    int rnd = random.Next(terrPerPlayer * players.Count - territories.Count);
                    int cutoff = 0;
                    for (int i = 0; i < players.Count; ++i)
                    {
                        cutoff += toPlace[i];
                        if (rnd < cutoff)
                        {
                            toPlace[i]--;
                            p = i;
                            break;
                        }
                    }
                }
                else
                {
                    do
                    {
                        p = random.Next(players.Count);
                    } while (toPlace[p] < 0);
                    toPlace[p]--;
                }

                //Country ctry;
                //territories.TryGetValue(rcIdx(r, c), out ctry);
                c.Value.setOwner(players[p]);
                //c.Value.setStrength(4);
            }
            curGameState = GameState.Countdown;
            countdownTime = COUNTDOWN_LENGTH;
        }


        public void AssignManually()
        {
            toPlace = territories.Count;
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            MediaPlayer.Stop();
            content.Unload();
        }


        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                if (curGameState == GameState.Placement)
                {
                    if (curPlayer.GetType() == typeof(AI))
                    {
                        ((AI)curPlayer).place();

                        int newIdx = (players.IndexOf(curPlayer) + 1) % players.Count;
                        curPlayer = players[newIdx];
                        if (--toPlace <= 0)
                        {
                            curGameState = GameState.Countdown;
                            countdownTime = COUNTDOWN_LENGTH;
                        }
                        pruneMovements();
                    }
                }
                else if (curGameState == GameState.Countdown)
                {
                    countdownTime -= gameTime.ElapsedGameTime.TotalSeconds;
                    if (countdownTime <= 0)
                        curGameState = GameState.Playing;
                }
                else if (curGameState == GameState.Playing)
                {
                    totalGameTime += gameTime.ElapsedGameTime.TotalSeconds;
                    // perform all active movements at their correct times
                    if (movements.Count > 0)
                    {
                        Movement m = movements[0];
                        if (m.lastRan + Movement.frequency <= totalGameTime)
                        {
                            movements.RemoveAt(0);
                            if (m.origin.hasStrength())
                            {
                                bool repeat = m.perform(totalGameTime);
                                if (repeat)
                                    movements.Add(m);
                                else
                                    m.origin.isActive = false;
                            }
                            else
                            {
                                m.origin.isActive = false;
                            }
                            pruneMovements();
                        }
                    }

                    // deploy new guys to contries every deployFreq
                    double deployProgress = totalGameTime - lastDeployment;
                    progressBar.value = (float)deployProgress;
                    progressBar.Update(gameTime);
                    if (deployProgress >= deployFreq)
                    {
                        lastDeployment = totalGameTime;
                        foreach (KeyValuePair<string, Country> c in territories)
                        {
                            c.Value.addStrength(1);
                        }

                        if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.World)
                        {
                            foreach (Continent cont in continents)
                            {
                                if (cont.owner != noone)
                                    cont.owner.captures += cont.value;
                            }

                            foreach (Player p in players)
                            {
                                p.captures++;
                            }
                        }
                    }

                    double aiProgress = totalGameTime - lastAI;
                    if (aiProgress >= aiFreq)
                    {
                        lastAI = totalGameTime;
                        foreach (Player p in players)
                        {
                            if (p.GetType() == typeof(AI))
                            {
                                ((AI)p).update();
                            }
                        }
                    }

                    // if someone was just invincible, we might have to switch up our music
                    if (invMusicEnd < totalGameTime && invMusicEnd + gameTime.ElapsedGameTime.TotalSeconds > totalGameTime)
                    {
                        MediaPlayer.Play(bgMusic);
                    }
                }
            }
        }

        // delete any movements which are canceled or no longer valid
        // also check if there is a winner
        public void pruneMovements()
        {
            // Note: there is a time/space trade-off here, current method is
            // very inefficient if number of movements is large, no big deal for
            // now, but could move to dedicated country=>movement dictionary

            for (int i = 0; i < movements.Count; ++i)
            {
                Movement m = movements[i];
                if (m.dest.changedHands == true)
                {
                    if (isFrontLines(m.dest) && !isFrontLines(m.origin))
                    {
                        m.myType = Movement.Type.Transfer;
                    }
                    else
                    {
                        m.origin.isActive = false;
                        movements.RemoveAt(i--);
                    }
                }
                else if (m.origin.cancelOutgoing == true || m.origin.changedHands == true)
                {
                    m.origin.isActive = false;
                    movements.RemoveAt(i--);
                }
                else if (m.myType == Movement.Type.Transfer && !isValidPath(m.origin, m.dest))
                {
                    m.origin.isActive = false;
                    movements.RemoveAt(i--);
                }
            }

            Player owner;
            if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.World)
            {
                foreach (Continent cont in continents)
                {
                    owner = cont[0].getOwner();
                    bool controlled = true;
                    foreach (Country c in cont)
                    {
                        if (c.getOwner() != owner)
                        {
                            controlled = false;
                            break;
                        }
                    }

                    // if the continent changed hands since last time, we need to update things
                    if (controlled != cont[0].contIsControlled)
                    {
                        foreach (Country c in cont)
                        {
                            c.contIsControlled = controlled;
                        }
                        if (controlled)
                            cont.owner = cont[0].getOwner();
                        else
                            cont.owner = noone;
                    }

                }
            }

            if (curGameState == GameState.Playing)
            {
                Dictionary<string, Country>.Enumerator e = territories.GetEnumerator();
                e.MoveNext();
                owner = e.Current.Value.getOwner();
                bool winner = true;
                foreach (KeyValuePair<string, Country> c in territories)
                {
                    c.Value.changedHands = false;
                    c.Value.cancelOutgoing = false;
                    if (c.Value.getOwner() != owner)
                    {
                        winner = false;
                    }
                }

                if (winner)
                {
                    ScreenManager.AddScreen(new GameOverScreen("Player " + owner.PI.ToString()), null);
                    MediaPlayer.IsRepeating = false;
                    MediaPlayer.Play(victoryMusic);
                }
            }
        }


        // DEBUG CODE -- REMOVE LATER
        int mode = 0;
        // END DEBUG

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            //for(int playerIndex = 0; playerIndex < Math.Min(players.Count, InputState.MaxInputs);  ++playerIndex) {
            foreach(Player cP in players) {

                KeyboardState keyboardState = input.CurrentKeyboardStates[(int)cP.PI];
                GamePadState gamePadState = input.CurrentGamePadStates[(int)cP.PI];

                // The game pauses either if the user presses the pause button, or if
                // they unplug the active gamepad. This requires us to keep track of
                // whether a gamepad was ever plugged in, because we don't want to pause
                // on PC if they are playing with a keyboard and have no gamepad at all!
                bool gamePadDisconnected = !gamePadState.IsConnected && input.GamePadWasConnected[(int)cP.PI];

                if (input.IsPauseGame(cP.PI) || gamePadDisconnected)
                {
                    ScreenManager.AddScreen(new PauseMenuScreen(), cP.PI);
                }
                else if (curGameState == GameState.Placement)
                {
                    HandlePlacementInputButtons(input, cP);
                }
                else if (curGameState == GameState.Playing)
                {
                    HandleGameplayInputButtons(input, cP);
                }
                
                // Otherwise move the player position.
                if (true)
                {
                    Vector2 movement = Vector2.Zero;
                    if (true)//cP.PI == PlayerIndex.One)
                    {
                        if (keyboardState.IsKeyDown(Keys.Left))
                        {
                            movement.X--;
                        }

                        if (keyboardState.IsKeyDown(Keys.Right))
                        {
                            movement.X++;
                        }

                        if (keyboardState.IsKeyDown(Keys.Up))
                        {
                            movement.Y--;
                        }

                        if (keyboardState.IsKeyDown(Keys.Down))
                        {
                            movement.Y++;
                        }
                    }

                    Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                    movement.X += thumbstick.X;
                    movement.Y -= thumbstick.Y;

                    if (movement.Length() > 1)
                        movement.Normalize();

                    cP.pos += movement * 10;
                    //if (cP.pos.X < offset.X)
                    //    cP.pos.X = offset.X;
                    //if (cP.pos.Y < offset.Y)
                    //    cP.pos.Y = offset.Y;
                    //if (cP.pos.X > COLS * country.Width + offset.X - 1)
                    //    cP.pos.X = COLS * country.Width + offset.X - 1;
                    //if (cP.pos.Y > ROWS * country.Height + offset.Y - 1)
                    //    cP.pos.Y = ROWS * country.Height + offset.Y - 1;
                    if (cP.pos.X < 0)
                    {
                        if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.World)
                            cP.pos.X = screenWidth;
                        else
                            cP.pos.X = 0;
                    }
                    if (cP.pos.Y < 0)
                        cP.pos.Y = 0;
                    if (cP.pos.X > screenWidth)
                    {
                        if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.World)
                            cP.pos.X = 0;
                        else
                            cP.pos.X = screenWidth;
                    }
                    if (cP.pos.Y > screenHeight)
                        cP.pos.Y = screenHeight;

                    cP.hover = closestCountry(cP.pos);

                    // debug code for organizing world map
                    if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.World)// && cP.PI == PlayerIndex.Two)
                    {
                        if (keyboardState.IsKeyDown(Keys.LeftControl))
                        {
                            offset += movement * 10;
                        }
                        else if (cP.selected == true)
                        {
                            if (keyboardState.IsKeyDown(Keys.LeftShift))
                                movement *= 10 / scale;

                            if (mode == 1)
                            {
                                Vector2 pos = cP.origin.getLocation() + 0.2f*movement;
                                cP.origin.setLocation(pos);
                            }
                            else if(mode == 2)
                            {
                                Vector2 pos = cP.origin.getLabelOffset() + movement;
                                cP.origin.setLabelOffset(pos);
                            }
                        }

                        PlayerIndex output;
                        if (input.IsNewKeyPress(Keys.P, cP.PI, out output))
                        {
                            Console.Out.WriteLine("\n\nTerr. Locations:\n");
                            float tempRescale = 1;
                            foreach (KeyValuePair<string, Country> c in territories)
                            {

                                //Country c = new Country(new Vector2(320.448f, 500.0194f), new Vector2(0, 0), content.Load<Texture2D>("territories/alaska"), noone, 4);
                                //territories.Add("alaska", c);
                                Vector2 loc = c.Value.getLocation() * tempRescale + offset / scale;
                                Vector2 label = c.Value.getLabelOffset() * tempRescale;
                                Console.Out.WriteLine("c = new Country(new Vector2(" + loc.X + "f, " + loc.Y + "f), new Vector2(" + label.X + "f, " + label.Y + "f), content.Load<Texture2D>(\"territories/"+c.Key+"\"), noone, 4);");
                                Console.Out.WriteLine("territories.Add(\""+c.Key+"\", c);");
                                //Console.Out.WriteLine(c.Key+": "+(loc));
                            }
                            foreach (Doodad d in doodads)
                            {
                                Vector2 loc = d.location * tempRescale + offset / scale;
                                string name = d.image.Name;
                                Console.Out.WriteLine("doodads.Add(new Doodad(new Vector2(" + loc.X + "f, " + loc.Y + "f), content.Load<Texture2D>(\"doodads/" + name + "\"), \""+name+"\", Color.White));");
                            }
                        }
                        if (input.IsNewKeyPress(Keys.S, cP.PI, out output))
                            mode = (mode + 1) % 3;
                        if (input.IsNewKeyPress(Keys.Add, cP.PI, out output))
                            scale *= 2f;
                        if (input.IsNewKeyPress(Keys.Subtract, cP.PI, out output))
                            scale *= 0.5f;
                        if (input.IsNewKeyPress(Keys.R, cP.PI, out output))
                            offset = new Vector2(0,0);

                    }
                }
            }
        }

        private bool HandlePlacementInputButtons(InputState input, Player cP)
        {
            PlayerIndex output;
            if (cP == curPlayer && cP.hover.getOwner() == noone && input.IsSelect(cP.PI, out output))
            {
                cP.hover.setOwner(cP);
                int newIdx = (players.IndexOf(curPlayer) + 1)%players.Count;
                curPlayer = players[newIdx];
                if (--toPlace <= 0)
                {
                    curGameState = GameState.Countdown;
                    countdownTime = COUNTDOWN_LENGTH;
                }
                //Console.Out.WriteLine(toPlace);
                pruneMovements();
                selectCountry.Play();
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool HandleGameplayInputButtons(InputState input, Player cP)
        {
            PlayerIndex output;
            if (input.IsSelect(cP.PI, out output))
            {
                if (cP.selected == false && cP.hover.getOwner() == cP && cP.hover.canAddMove())
                {
                    cP.selected = true;
                    cP.origin = cP.hover;
                }
                else if (cP.selected == true)
                {
                    cP.selected = false;
                    if (cP.target == Player.Tgt.Self && cP.hover != cP.origin && cP.origin.canAddMove())
                    {
                        //cP.origin.transfer(cP.dest);
                        cP.dest = cP.hover;
                        movements.Add(new Movement(cP, cP.origin, cP.dest, Movement.Type.Transfer));
                    }
                    if (cP.target == Player.Tgt.Enemy && cP.origin.canAddMove())
                    {
                        //cP.origin.attack(cP.dest);
                        cP.dest = cP.hover;
                        movements.Add(new Movement(cP, cP.origin, cP.dest, Movement.Type.Attack));
                    }

                }
            }
            else if (input.IsMenuCancel(cP.PI, out output))
            {
                if (cP.selected == true)
                {
                    cP.selected = false;
                }
                else
                {
                    cP.hover.cancelOutgoing = true;
                    pruneMovements();
                }
            }
            else if (input.IsScrollLeft(cP.PI))
            {
                cP.decPower();
            }
            else if (input.IsScrollRight(cP.PI))
            {
                cP.incPower();
            }
            else if (input.IsUsePower(cP.PI))
            {
                if (cP.usePower())
                {
                    if (cP.curPower == Player.Power.Transfer)
                        transfer(cP);
                    else if (cP.curPower == Player.Power.Reinforce)
                        reinforce(cP);
                    else if (cP.curPower == Player.Power.Invincible)
                        invincible(cP);
                    else if (cP.curPower == Player.Power.Defect)
                        defect(cP, cP.hover);
                }
            }
            else
            {
                return false;
            }
            return true;
        }            

        public void transfer(Player p)
        {
            int excess=0;
            List<Country> frontLines = new List<Country>();
            foreach (KeyValuePair<string, Country> c in territories)
            {
                if (c.Value.getOwner() == p)
                {
                    if (isFrontLines(c.Value))
                    {
                        frontLines.Add(c.Value);
                    }
                    else
                    {
                        excess += c.Value.getStrength() - 1;
                        c.Value.setStrength(1);
                    }
                }
            }

            int perEach = excess / frontLines.Count;
            excess -= perEach * frontLines.Count;
            foreach (Country c in frontLines)
            {
                c.addStrength(perEach);
                if (excess-- > 0)
                    c.addStrength(1);
            }
        }

        public void reinforce(Player p)
        {
            foreach (KeyValuePair<string, Country> c in territories)
            {
                if (c.Value.getOwner() == p)
                    c.Value.addStrength(1);
            }
        }

        public void invincible(Player p)
        {
            double invDuration = getInvDuration(p);
            p.becomeInvincible(invDuration);
            invMusicEnd = Math.Max(invMusicEnd, totalGameTime + invDuration);
            MediaPlayer.Play(invincibleMusic);
        }

        public void defect(Player p, Country c)
        {
            c.setOwner(p);
            c.changedHands = true;
            pruneMovements();
        }

        public Country closestCountry(Vector2 pos)
        {
            // first see if there is a country located at pos
            Point myPos = new Point((int)pos.X, (int)pos.Y);
            foreach (KeyValuePair<string, Country> c in territories)
            {
                Texture2D img = c.Value.getImage();
                Point imgPos = new Point((int)c.Value.getLocation().X, (int)c.Value.getLocation().Y);
                Rectangle imgBounds = new Rectangle(imgPos.X - img.Width/2,  imgPos.Y - img.Height/2, img.Width, img.Height);
                if (imgBounds.Contains(myPos) && c.Key != "worldmap")
                {
                    uint[] bits = new uint[img.Width * img.Height];
                    img.GetData<uint>(bits);
                    if (((bits[(myPos.X - imgBounds.X) + (myPos.Y - imgBounds.Y) * img.Width] & 0xFF000000) >> 24) > 20)
                        return c.Value;
                }
            }

            // otherwise, choose the country that is closest to pos
            float min = float.PositiveInfinity;
            Country closest = null;

            foreach (KeyValuePair<string, Country> c in territories)
            {
                float d = Vector2.Distance(pos, c.Value.getScaledLoc(scale, offset));
                if (d < min)
                {
                    min = d;
                    closest = c.Value;
                }
            }

            return closest;
        }

        public double getInvDuration(Player p)
        {
            double factor = 1.0;
            if(OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.World) {
            foreach(Continent cont in continents){
                if (cont.owner == p)
                    factor -= cont.value / 10f;
                }
            }

            factor = Math.Max(factor, 0.3);
            return baseInvDuration * factor;
        }

        public bool isValidPath(Country start, Country end)
        {
            // use a BFS to determine if there is a valid path from the start to the end
            Dictionary<Country, Country> visits = new Dictionary<Country, Country>();

            Queue<Country> queue = new Queue<Country>();
            queue.Enqueue(start);
            visits.Add(start, start);
            bool found = false;
            while (queue.Count != 0)
            {
                Country c = queue.Dequeue();
                if (c != end)
                {
                    foreach (Country next in c)
                    {
                        if (!visits.ContainsKey(next) && c.getOwner() == next.getOwner())
                        {
                            queue.Enqueue(next);
                            visits.Add(next, c);
                        }
                    }
                }
                else
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        public bool isFrontLines(Country c)
        {
            bool isFrontLines = false;
            foreach (Country n in c)
            {
                if (n.getOwner() != c.getOwner())
                {
                    isFrontLines = true;
                    break;
                }
            }
            return isFrontLines;
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, OCEAN, 0, 0);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();
            if (curGameState == GameState.Playing)
                progressBar.Draw(spriteBatch);
            else if(curGameState == GameState.Placement)
            {
                spriteBatch.DrawString(gameFont, "Player " + curPlayer.PI + ", Go!", progressBar.getPos(), curPlayer.getDarkColor());
            }
            else if (curGameState == GameState.Countdown)
            {
                spriteBatch.DrawString(gameFont, "Start In "+(int)(Math.Ceiling(countdownTime)), progressBar.getPos(), Color.Black);
            }

            foreach (Player p in players)
                p.Draw(spriteBatch);
            spriteBatch.End();

            DrawTerrs();

            // draw the active movements
            foreach (Movement m in movements)
            {
                Color selectColor = new Color(Color.Lerp(Color.White, m.origin.getOwner().getLightColor(), .7f), 0.7f);
                Color attackColor = new Color(Color.Lerp(Color.White, m.origin.getOwner().getLightColor(), .7f), 0.7f);

                if(m.myType == Movement.Type.Transfer)
                    DrawArrow(m.origin.getScaledLabelLoc(scale, offset), m.dest.getScaledLabelLoc(scale, offset), selectColor);
                else
                    DrawArrow(m.origin.getScaledLabelLoc(scale, offset), m.dest.getScaledLabelLoc(scale, offset), attackColor);
            }

            // draw current position of player crosshair or planned movement
            foreach (Player p in players)
            {
                if (p.GetType() == typeof(Player))
                {
                    Color selectColor = new Color(Color.Lerp(Color.White, p.getLightColor(), .7f), 0.7f);
                    Color attackColor = new Color(Color.Lerp(Color.White, p.getLightColor(), .7f), 0.7f);
                    if (p.selected == true)
                    {
                        if (!isValidPath(p.origin, p.hover))
                        {
                            bool enemyFound = false;
                            foreach (Country c in p.origin)
                            {
                                if (c == p.hover)
                                {
                                    p.target = Player.Tgt.Enemy;
                                    DrawArrow(p.origin.getScaledLabelLoc(scale, offset), p.pos, attackColor);
                                    enemyFound = true;
                                }
                            }
                            if (enemyFound == false)
                            {
                                p.target = Player.Tgt.Invalid;
                                DrawCrosshair(p.pos, attackColor);
                            }
                        }
                        else
                        {
                            p.target = Player.Tgt.Self;
                            DrawArrow(p.origin.getScaledLabelLoc(scale, offset), p.pos, selectColor);

                        }
                    }
                    else
                    {
                        DrawCrosshair(p.pos, selectColor);
                    }
                }
            }

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }

        private Matrix GetMatrix(ref Vector2 origin, float radians)
        {
            // Translate sprites to center around screen (0,0), rotate them, and
            // translate them back to their original positions
            Vector3 matrixorigin = new Vector3(origin, 0);
            return Matrix.CreateTranslation(-matrixorigin) *
                Matrix.CreateRotationZ(radians) *
                Matrix.CreateTranslation(matrixorigin);
        }

        void DrawArrow(Vector2 start, Vector2 end, Color arrowColor)
        {
            // if the around the world distance is shorter, draw the reverse arrow
            float forwardDist = Math.Abs(end.X - start.X);
            float reverseDist = screenWidth - forwardDist;
            if (forwardDist <= reverseDist)
                DrawForwardArrow(start, end, arrowColor);
            else
                DrawReverseArrow(start, end, arrowColor);
        }

        void DrawForwardArrow(Vector2 start, Vector2 end, Color arrowColor)
        {
            Vector2 dif = end - start;
            float multiple = (dif.Length() - arrowhead.Height) / arrow.Height;

            float angle = (float)Math.Atan2(dif.X, dif.Y);
            Matrix rotationMatrix = GetMatrix(ref start, -angle);

            SpriteBatch arrowBatch = new SpriteBatch(ScreenManager.GraphicsDevice);
            arrowBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None, rotationMatrix);
            arrowBatch.Draw(arrow, start + new Vector2(-arrow.Width / 2, 0), null, arrowColor, 0f, Vector2.Zero, new Vector2(1, multiple), SpriteEffects.None, 0f);
            arrowBatch.Draw(arrowhead, start + new Vector2(-arrowhead.Width / 2, 20 * multiple), arrowColor);
            arrowBatch.End();
        }

        void DrawReverseArrow(Vector2 start, Vector2 end, Color arrowColor)
        {
            float deltaX = screenWidth - Math.Abs(end.X - start.X);
            float slope = (end.Y - start.Y) / (deltaX);
            float midHeight;
            Vector2 midPointA, midPointB;
            // we need to keep track of which point is to the left so we can draw the
            // arrow to the correct side of the screen
            if (start.X < end.X)
            {
                midHeight = start.Y + slope * start.X;
                midPointA = new Vector2(0, midHeight);
                midPointB = new Vector2(screenWidth, midHeight);
            }
            else
            {
                midHeight = end.Y - slope * end.X;
                midPointA = new Vector2(screenWidth, midHeight);
                midPointB = new Vector2(0, midHeight);
            }
            DrawForwardArrow(start, midPointA, arrowColor);
            DrawForwardArrow(midPointB, end, arrowColor);
        }

        void DrawCrosshair(Vector2 loc, Color c)
        {
            SpriteBatch crosshairBatch = new SpriteBatch(ScreenManager.GraphicsDevice);
            crosshairBatch.Begin();
            Vector2 crosshairDim = new Vector2(crosshair.Width, crosshair.Height);
            Vector2 posCentered = loc - crosshairDim / 2;
            crosshairBatch.Draw(crosshair, posCentered, c);
            crosshairBatch.End();
        }

        void DrawTerrs()
        {
            Matrix scaleMatrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(offset.X,offset.Y,0);
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None, scaleMatrix);
            foreach (Doodad d in doodads)
            {
                Vector2 pos = d.location - new Vector2(d.image.Width / 2f, d.image.Height / 2f);
                spriteBatch.Draw(d.image, pos, d.color);
            }
            foreach (KeyValuePair<string,Country> cKey in territories)
            {
                Country c = cKey.Value;
                Vector2 pos = c.getLocation();
                Texture2D image = c.getImage();
                pos.X -= image.Width / 2f;
                pos.Y -= image.Height / 2f;
                Color color = c.getColor(isEmphasized(c));
                if (isHighlighted(c))
                    color = new Color(color, 0.7f);
                //color = Color.White;
                spriteBatch.Draw(image, pos, color);
                //Rectangle dest = new Rectangle((int)Math.Round(pos.X*scale), (int)Math.Round(pos.Y*scale), (int)Math.Round(image.Width * scale), (int)Math.Round(image.Height * scale));
                //spriteBatch.Draw(image, dest, color);
                
                //String label = Convert.ToString(c.getStrength());
                //Vector2 stringDim = gameFont.MeasureString(label);
                //Vector2 posCentered = c.getLocation() + c.getLabelOffset() - stringDim / 2;
                //spriteBatch.DrawString(gameFont, label, posCentered, Color.Black);

                Vector2 stDim = new Vector2(strengthWheel[0].Width, strengthWheel[0].Height);
                Vector2 posCentered = c.getLocation() + c.getLabelOffset() - stDim / 2;
                int smallStrength = c.getStrength();
                int largeStrength = 0;
                color = Color.Lerp(c.getColor(true), Color.White, 0.4f);
                while(smallStrength > 10) {
                    smallStrength -= 10;
                    largeStrength += 1;
                }
                if (smallStrength > 0)
                {
                    Texture2D strengthSmallImg = strengthWheel[smallStrength - 1];
                    spriteBatch.Draw(strengthSmallImg, posCentered, color);
                }
                if (largeStrength > 0)
                {
                    Texture2D largeSmallImg = strengthWheel[Math.Min(largeStrength + 9,19)];
                    spriteBatch.Draw(largeSmallImg, posCentered, Color.White);
                }
                    
            }
            spriteBatch.End();
            //foreach (KeyValuePair<string, Country> cKey in territories)
            //{
            //    Country c = cKey.Value;
            //    foreach (Country n in c)
            //    {
            //        //int r = random.Next(5);
            //        //if(r == 1)
            //            DrawArrow(c.getScaledLabelLoc(scale, offset), n.getScaledLabelLoc(scale, offset), Color.BlanchedAlmond);
            //    }
            //}
        }

        bool isHighlighted(Country c)
        {
            foreach (Player p in players)
            {
                if(c == p.hover)
                    return true;
            }
            return false;
        }

        bool isEmphasized(Country c)
        {
            if (c.getOwner() == noone)
                return true;
            if (OptionsMenuScreen.currentMap == OptionsMenuScreen.Map.World)
            {
                return c.contIsControlled;
            }
            return true;
        }

        string rcIdx(int r, int c)
        {
            return (c + r * COLS).ToString();
        }

        #endregion
    }
}
