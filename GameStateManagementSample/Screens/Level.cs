using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

namespace Ratatöskrs_Great_Adventure.Screens
{
    class Level
    {
        #region Variables
           
        //Physical structure of the level
        private Tile[,] tiles;
        private Layer[] layers;

        //Random for solid Blocks
        private Random random = new Random(1000);
        private float cameraPosition;

        private List<Nut> nuts = new List<Nut>();
        public List<Enemy> enemies = new List<Enemy>();

        //dif. Variables
        private bool Complete;

        //Sound
        private SoundEffect winSound;

        //The layer which entities are drawn on top of
        private const int EntityLayer = 2;
        
        //Entities in the level
        public Player Player
        {
            get { return player; }
        }
        Player player;
                
        //"Special locations"
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        #endregion

        #region "Variables from other classes"

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public bool LevelComplete
        {
            get { return levelComplete; }
            set { levelComplete = value; }
        }
        bool levelComplete;

        public bool LastLevel
        {
            get { return lastLevel; }
            set { lastLevel = value; }
        }
        bool lastLevel;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
            set { timeRemaining = value; }
        }
        TimeSpan timeRemaining;

        //private const int PointsPerSecond = 5;
        
        //Level content    
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        #endregion 

        #region Loading

        //Constructs a new level
        public Level(ContentManager manager, Stream fileStream, int levelIndex)
        {
            //Create a new content manager to load content used just by this level
            this.content = manager;
            
            LoadTiles(fileStream);

            //Load background layer textures
            layers = new Layer[3];
            layers[0] = new Layer(content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(content, "Backgrounds/Layer1", 0.5f);
            layers[2] = new Layer(content, "Backgrounds/Layer2", 0.8f);

            //Load sounds
            winSound = Content.Load<SoundEffect>("Sounds/Win");
        }


        private void LoadTiles(Stream fileStream)
        {
            //Load the level and ensure all of the lines are the same length
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("line {0} is different", lines.Count));
                    line = reader.ReadLine();
                }
            }


            //Allocate the tile grid
            tiles = new Tile[width, lines.Count];

            //Loop over every tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    //to load each tile
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            //Verify that the level has a beginning and an end
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if ((exit == InvalidPosition) && (Complete == false))
                throw new NotSupportedException("A level must have an exit.");
        }

        #region Tiles

        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                    //Player start
                case '1':
                    return LoadStartTile(x, y);
                   
                //Exit
                case 'X':
                    LoadExit2Tile(x, y);
                    return LoadExit2Tile(x, y);

                //Complete
                case 'Y':
                    LoadExitTile(x, y);
                    Complete = true;
                    return LoadCompleteTile(x, y);

                // Nut
                case 'G':
                    return LoadNutTile(x, y, false);

                //PowerUp
                case 'P':
                    return LoadNutTile(x, y, true);

                //Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                //Mouse
                case 'A':
                    return LoadEnemyTile(x, y, "MonsterA");
                //hedgehog
                case 'B':
                    return LoadEnemyTile(x, y, "MonsterB");
                //Bird
                case 'C':
                    return LoadEnemyTile(x, y, "MonsterC");
                //Ant
                case 'D':
                    return LoadEnemyTile(x, y, "MonsterD");


                //leaf left
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                //leaf right
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);
                                    
                //massive block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                //Beta End Tile
                case '!':
                    return LoadVarietyTile("BetaEnd", 1, TileCollision.Passable);
                        
                //fault
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        #endregion 

        //Creates a new tile
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }

        //Loads a tile with a random appearance
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }

        //Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadBetaEndTile(int x, int y)
        {
            return LoadTile("BetaEndTile", TileCollision.Passable);
        }

        //Remembers the location of the level's exit
        private Tile LoadCompleteTile(int x, int y)
        {
            exit = GetBounds(x, y).Center;
            lastLevel = true;

            return LoadTile("Complete", TileCollision.Passable);
        }

        private Tile BeatExitTile(int x, int y)
        {
            exit = GetBounds(x, y).Center;
            lastLevel = true;

            return LoadTile("Complete", TileCollision.Passable);
        }
        
        //Remembers the location of the level's exit
        private Tile LoadExitTile(int x, int y)
        {
            return LoadTile("Exit", TileCollision.Passable);
        }

        //Remembers the location of the level's exit
        private Tile LoadExit2Tile(int x, int y)
        {
            exit = GetBounds(x, y).Center;

            return LoadTile("Exit2", TileCollision.Passable);
        }

        //Instantiates an enemy and puts him in the level
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }

        //Instantiates a nut and puts it in the level
        private Tile LoadNutTile(int x, int y, bool isPowerUp)
        {
            Point position = GetBounds(x, y).Center;
            nuts.Add(new Nut(this, new Vector2(position.X, position.Y), isPowerUp));

            return new Tile(null, TileCollision.Passable);
        }

        #endregion

        #region Bounds and collision

        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;

            //Allow jumping past the level top and falling through the bottom
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public int Width
        {
            get { return tiles.GetLength(0); }
        }


        public int Height
        {
            get { return tiles.GetLength(1); }
        }
        #endregion

        #region Update
        public void Update(
            GameTime gameTime,
            TouchCollection touchState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            //Pause while the player is dead or time is expired
            if (!Player.Life || TimeRemaining == TimeSpan.Zero)
            {
                //Still want to perform physics on the player
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit && touchState.FireButtonTouch())
            {
                //timeRemaining = TimeSpan.Zero;
            }
            else if (LevelComplete)
            {
                //timeRemaining = TimeSpan.Zero;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, touchState, accelState, orientation);
                UpdateNuts(gameTime);

                //Falling off the bottom of the level kills the player
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                {
                    OnPlayerKilled(null);
                    GameplayScreen.score = 0;
                }

                UpdateEnemies(gameTime);

                if (Player.Life &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
                else if (Player.Life &&
                  Player.IsOnGround &&
                  !Player.BoundingRectangle.Contains(exit))
                {
                    reachedExit = false;
                }
            }
        }



        //Animates each nut and checks to allows the player to collect them
        private void UpdateNuts(GameTime gameTime)
        {
            for (int i = 0; i < nuts.Count; ++i)
            {
                Nut nut = nuts[i];

                nut.Update(gameTime);

                if (nut.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    nuts.RemoveAt(i--);
                    OnNutCollected(nut, Player);
                }
            }
        }

        //Animates each enemy and allow them to kill the player
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                //Player Killed by Enemy or Powerup and Enemy gets Killed
                if (enemy.Life && enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (Player.IsPoweredUp)
                    {
                        OnEnemyKilled(enemy, Player);
                    }
                    else
                    {
                        OnPlayerKilled(enemy);
                    }
                }
            }
        }


        private void OnEnemyKilled(Enemy enemy, Player killedBy)
        {
            enemy.OnKilled(killedBy);
        }


        private void OnNutCollected(Nut nut, Player collectedBy)
        {
            GameplayScreen.score += nut.PointValue;
            nut.OnCollected(collectedBy);
        }


        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }


        //Called when the player reaches the level's exit
        private void OnExitReached()
        {
            Player.OnReachedExit();
            reachedExit = true;
        }


        //Called when the player reaches the level's exit
        private void LevelColmplete()
        {
            Player.OnReachedExit();
            winSound.Play();
            reachedExit = true;
        }

        //Restores the player to the starting point to try the level again
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion        

        #region Draw

        //Draw everything in the level from background to foreground
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, 0.0f, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default,
                                RasterizerState.CullCounterClockwise, null, cameraTransform);

            DrawTiles(spriteBatch);

            foreach (Nut nut in nuts)
                nut.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition);
            spriteBatch.End();
        }

        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.45f;

            //Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            //Calculate how far to scroll when the player is near the edges of the screen
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            //Update the camera position, but prevent scrolling off the ends of the level
            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
        }


        //Draws each tile in the level
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            //Calculate the visible range of tiles
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = 48 + left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);

            //For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    //If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        //Draw it in screen space
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        #endregion
    }
}
