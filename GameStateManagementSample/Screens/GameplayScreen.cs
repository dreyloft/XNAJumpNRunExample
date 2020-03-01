using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System.Threading;
using Microsoft.Xna.Framework.Content;


namespace Ratatöskrs_Great_Adventure.Screens
{
    class GameplayScreen : GameScreen
    {
        #region Variables
        
        ContentManager content;

        // Textures
        private SpriteFont hudFont;
        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;
        private Texture2D buttonNormal;
        private Texture2D buttonPressed;
        private Texture2D buttonNormalFire;
        private Texture2D buttonPressedFire;
        private Texture2D startScreen;
        private SpriteBatch spriteBatch;

        // Sounds
        private SoundEffect door;
        private SoundEffect win;

              
        private Level level;
        private int tempScore;
        private bool wasContinuePressed;


        //When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);
        private TouchCollection touchState;
        private AccelerometerState accelerometerState;
        public GraphicsDeviceManager graphics { get; set; }
        public ContentManager manager { get; set; }

        public static int numberoflifes = 3;
        private const int numberOfLevels = 8;

        public static int levelIndex = -1;
        public int LevelIndex
        {
            get { return levelIndex; }
            set { levelIndex = value; }
        }

        public static int score;
        public int Score
        {
            get { return score; }
            set { score = value; }
        }
        

        public int NumberOfLifes
        {
            get { return numberoflifes; }
            set { numberoflifes = value; }
        }
        

        public TimeSpan ActualTimeRemaining
        {
            get { return actualTimeRemaining; }
            set { ActualTimeRemaining = value; }
        }
        public TimeSpan actualTimeRemaining;

        #endregion

        #region FadeIn and Acc ini

        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            if (numberoflifes == 0)
                numberoflifes = 3;


            if (Accelerometer.isInitialized == false)
                Accelerometer.Initialize();            
        }

        #endregion

        #region Load Content and play music

        public override void LoadContent()
        {
            content = new ContentManager(ScreenManager.Game.Services, "Content");
            
            //Load font
            hudFont = content.Load<SpriteFont>("Fonts/Hud");
            
            //Load Textures
            startScreen = content.Load<Texture2D>("Background");
            winOverlay = content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = content.Load<Texture2D>("Overlays/you_died");
            buttonNormal = content.Load<Texture2D>("Sprites/ButtonNormal");
            buttonPressed = content.Load<Texture2D>("Sprites/ButtonPressed");
            buttonNormalFire = content.Load<Texture2D>("Sprites/ButtonNormal");
            buttonPressedFire = content.Load<Texture2D>("Sprites/ButtonPressed");

            //Load Sounds
            door = content.Load<SoundEffect>("Sounds/Door");
            win = content.Load<SoundEffect>("Sounds/Win");

            // Load Music (copyrights?)
            //try
            //{                
            //    MediaPlayer.IsRepeating = true;
            //    MediaPlayer.Play(content.Load<Song>("Sounds/Music"));
            //}
            //catch { }

            LoadNextLevel();

            ScreenManager.Game.ResetElapsedTime();
        }

        #endregion

        #region unload Content

        public override void UnloadContent()
        {
            content.Unload();
        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            {        
                //Handle polling for our input and handling high-level input
                HandleInput();

                if(level.TimeRemaining != TimeSpan.Zero)
                    actualTimeRemaining = level.TimeRemaining;
                                
                //update our level, passing down the GameTime along with all of our input states
                level.Update(gameTime, touchState,
                    accelerometerState, TouchPanel.DisplayOrientation);                          // Marker

                base.Update(gameTime, false, false);
            }
        }

        #endregion

        #region HandleInput

        private void HandleInput()
        {
            //get all of our input states
            touchState = TouchPanel.GetState();
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);
            accelerometerState = Accelerometer.GetState();

            if ((gamepadState.Buttons.Back == ButtonState.Pressed) || (NumberOfLifes == 0 && wasContinuePressed))
            {
                levelIndex = -1;
                //MediaPlayer.Stop();
                LoadingScreen.Load(ScreenManager, false, ControllingPlayer, new BackgroundScreen(), new MainMenuScreen());
            }

            bool continuePressed = touchState.JumpButtonTouch() || touchState.FireButtonTouch();
            
            //Perform the appropriate action to advance the game and to get the player back to playing
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.Life)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }
            wasContinuePressed = continuePressed;


            if ((level.ReachedExit) && (level.LastLevel) && (touchState.FireButtonTouch()))
            {
                level.TimeRemaining = TimeSpan.Zero;
            }

            if ((level.ReachedExit == true) && (level.LastLevel == false) && (touchState.FireButtonTouch()))
            {
                LoadNextLevel();
            }
        }

        #endregion

        #region Load the next level or Reload level

        private void LoadNextLevel()
        {
            if (numberOfLevels-1 == levelIndex)
            {
                levelIndex = -1;
                //MediaPlayer.Stop();
                LoadingScreen.Load(ScreenManager, false, ControllingPlayer, new BackgroundScreen(), new MainMenuScreen());
            }

            //move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            if (level != null)
            {
                if (level.LastLevel)
                {
                    win.Play();
                }
                else
                {
                    door.Play();
                }
            }


            //Load the level
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(content, fileStream, levelIndex);


            // New level or New Room
            switch (levelIndex)
            {
                // Level 1 is new Level
                case 0:
                    level.TimeRemaining = TimeSpan.FromMinutes(5.0);
                    break;

                // Level 1 Room
                case 1:
                    level.TimeRemaining = actualTimeRemaining;
                    break;

                // Level 2 new Level
                case 2:
                    level.TimeRemaining = TimeSpan.FromMinutes(7.0);
                    break;

                // Level 2 new Room
                case 3:
                    level.TimeRemaining = actualTimeRemaining;
                    break;

                // Level 2 new Room
                case 4:
                    level.TimeRemaining = actualTimeRemaining;
                    break;

                // Level 3 new Room
                case 5:
                    level.TimeRemaining = TimeSpan.FromMinutes(4.0);
                    break;

                // Level 2 new Room
                case 6:
                    level.TimeRemaining = actualTimeRemaining;
                    break;

                // Beta End Level
                case 7:
                    level.TimeRemaining = TimeSpan.FromMinutes(15.0);
                    break;

                default:
                    level.TimeRemaining = TimeSpan.FromMinutes(55.0);
                    break;
            }
        }


        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        #endregion

        #region Draw Level and Hud with Strings

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);

            spriteBatch = ScreenManager.SpriteBatch;
            
            level.Draw(gameTime, spriteBatch);
            DrawHud();
            
            base.Draw(gameTime);

            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }

        private void DrawHud()
        {

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            Rectangle titleSafeArea = ScreenManager.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                 titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            string timeString = "  Time:  " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;

            spriteBatch.Begin();
            
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.White;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            //Draw scores etc.
            float timeHeight = hudFont.MeasureString(timeString).Y;
            DrawShadowedString(hudFont, "  Nuts:  " + Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.0f * 1), Color.White);
            DrawShadowedString(hudFont, "  Lifes: " + NumberOfLifes.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.0f * 2), Color.White);
            
            //Determine the status overlay message to show
            Texture2D status = null;


            if (level.TimeRemaining == TimeSpan.Zero)
            {                
                //if time is over win level or die
                if (level.ReachedExit && level.LastLevel)
                {                        
                    status = winOverlay;
                }
                else
                {
                    status = diedOverlay;
                }
            }
            else if (!level.Player.Life)
            {
                status = diedOverlay;
            }

            if (Score > 99)
            {
                NumberOfLifes++;
                tempScore = Score - 100;
                Score = tempScore;
            }

            if (NumberOfLifes < 0)
                NumberOfLifes = 0;

            if (NumberOfLifes == 0)
            {
                status = loseOverlay;
            }

            if (status != null)
            {
                //Draw status message
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            spriteBatch.End();
        }

        #endregion 
    }
}