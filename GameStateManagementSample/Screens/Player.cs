using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;



namespace Ratatöskrs_Great_Adventure.Screens
{
    class Player
    {
        #region Vaiables Player

        Fire[] stones;
        
        //Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;


        //Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect enemykilledSound;

        //player status
        private float movement;
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        private float previousBottom;

        public Vector2 PosOfPlayer
        {
            get { return posOfPlayer; }
            set { posOfPlayer = value; }
        }
        Vector2 posOfPlayer;


        public Level Level
        {
            get { return level; }
        }
        Level level;


        public bool Life
        {
            get { return life; }
        }
        bool life;

        
        //Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        //speed of Player
        public Vector2 Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        Vector2 speed;

        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        //"Bounding Box"
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        #endregion

        #region Variables PowerUp

        //sound
        private SoundEffect powerUpSound;

        // Powerup state        
        private const float MaxPowerUpTime = 6.5f;
        private float powerUpTime;
        public bool IsPoweredUp
        {
            get { return powerUpTime > 0.0f; }
        }
        //colorchange when powered up
        private readonly Color[] poweredUpColors = {
                               Color.Black,
                               Color.White,
                                                    };
  
        #endregion

        #region constants

        // movement constants 
        
        //x movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        //y movement
        private const float MaxJumpTime = 0.52f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        //Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;

        #endregion

        #region Constructor

        //new Player constr.
        public Player(Level level, Vector2 position)
        {
            this.level = level;
            LoadContent();
            Reset(position);
        }

        #endregion

        #region LoadContent

        public void LoadContent()
        {
            //Animation Sprites
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);
            
            //BoundingFrame Calc orientated on Idle Animation Texture Size 
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
            
            //Load Sounds
            enemykilledSound = Level.Content.Load<SoundEffect>("Sounds/Killed");
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/Jump");
            powerUpSound = Level.Content.Load<SoundEffect>("Sounds/PowerUp");

            //only 1 stone at the same time
            stones = new Fire[1];
            for (int i = 0; i < 1; i++)
            {
                stones[i] = new Fire(Level.Content.Load<Texture2D>("Sprites/Stone"));
            }
        }

        #endregion 

        #region reset player after killed

        //back to life 
        public void Reset(Vector2 position)
        {
            Position = position;
            Speed = Vector2.Zero;
            life = true;
            powerUpTime = 0.0f;
            sprite.PlayAnimation(idleAnimation);
        }

        #endregion

        #region powerup

        public void PowerUp()
        {
            powerUpTime = MaxPowerUpTime;
            powerUpSound.Play();
        }

        #endregion

        #region Update Player

        public void Update(GameTime gameTime, TouchCollection touchState, AccelerometerState accelState, DisplayOrientation orientation)
        {
            GetInput(touchState, accelState, orientation);
            UpdateStones();
            
            ApplyPhysics(gameTime);

            if (IsPoweredUp)
                powerUpTime = Math.Max(0.0f, powerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);

            if (Life && IsOnGround)
            {
                if (Math.Abs(Speed.X) - 0.02f > 0)
                {
                    sprite.PlayAnimation(runAnimation);
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                }
            }

            //Clear input
            movement = 0.0f;
            isJumping = false;
        }

        #endregion

        #region Stone is fireing

        // Fire a stone 
        private void FireStone()
        {
            foreach (Fire stone in stones)
            {
                //Find a bullet that isn't alive
                if (!stone.collision)
                {
                    //And set it to alive.
                    stone.collision = true;
                    
                    //Facing right
                    if (flip == SpriteEffects.FlipHorizontally) 
                    {
                        stone.position = new Vector2(position.X + 5, position.Y - 60);
                        stone.velocity = new Vector2(15, 7);
                    }
                    //Facing left
                    else 
                    {
                        stone.position = new Vector2(position.X - 5, position.Y - 60);
                        stone.velocity = new Vector2(-15, 7);
                    }

                    return;
                }
            }
        }

        #endregion

        #region Update Stones

        private void UpdateStones()
        {
            //Check all of our bullets
            foreach (Fire stone in stones)
            {
                //Only update them if they're alive
                if (stone.collision)
                {
                    //Move our stone based on it's velocity
                    stone.position += stone.velocity;
                    //Rectangle the size of the screen so bullets that fly off screen are deleted
                    Rectangle screenRect = new Rectangle(0, -200, level.Width * Tile.Width, 680);
                    if (!screenRect.Contains(new Point(
                        (int)stone.position.X,
                        (int)stone.position.Y)))
                    {
                        stone.collision = false;
                        continue;
                    }
                    //Collision rectangle for each bullet -Will also be used for collisions with enemies
                    Rectangle stoneRect = new Rectangle(
                        (int)stone.position.X - stone.sprite.Width * 2,
                        (int)stone.position.Y - stone.sprite.Height * 2,
                        stone.sprite.Width * 4,
                        stone.sprite.Height * 4);

                    //Check for collisions with the enemies
                    foreach (Enemy enemy in level.enemies)
                    {
                        if (stoneRect.Intersects(enemy.BoundingRectangle) && enemy.Life)
                        {
                            enemykilledSound.Play();
                            enemy.Life = false;
                            stone.collision = false;
                        }
                    }


                    Rectangle bounds = new Rectangle(
                        stoneRect.Center.X - 6,
                        stoneRect.Center.Y - 6,
                        stoneRect.Width / 4,
                        stoneRect.Height / 4);
                    int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
                    int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
                    int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
                    int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;
                    // For each potentially colliding tile
                    for (int y = topTile; y <= bottomTile; ++y)
                    {
                        for (int x = leftTile; x <= rightTile; ++x)
                        {
                            TileCollision collision = Level.GetCollision(x, y);
                            //If we collide with an Impassable or Platform tile then delete our bullet
                            if (collision == TileCollision.Impassable ||
                                collision == TileCollision.Platform)
                            {
                                if (stoneRect.Intersects(bounds))
                                    stone.collision = false;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Input

        //Gets player horizontal movement and jump commands from input
        private void GetInput(
            TouchCollection touchState,
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            //Shoot = RightTrigger
            if (touchState.FireButtonTouch())
            {
                FireStone();
            }

            //Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                //set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                //if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }            

            //Check if the player wants to jump
            isJumping =  touchState.JumpButtonTouch();
        }

        #endregion

        #region physics

        //Updates the player's velocity and position based on input, gravity, etc.
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;


            speed.X += movement * MoveAcceleration * elapsed;
            speed.Y = MathHelper.Clamp(speed.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            speed.Y = DoJump(speed.Y, gameTime);

            if (IsOnGround)
                speed.X *= GroundDragFactor;
            else
                speed.X *= AirDragFactor;

            speed.X = MathHelper.Clamp(speed.X, -MaxMoveSpeed, MaxMoveSpeed);


            //Apply velocity
            Position += speed * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));


            //If the player is now colliding with the level, separate them.
            HandleCollisions();


            //If the collision stopped us from moving, reset the velocity to zero
            if (Position.X == previousPosition.X)
                speed.X = 0;

            if (Position.Y == previousPosition.Y)
                speed.Y = 0;
        }

        #endregion

        #region jump

        //Calculates the Y velocity
        private float DoJump(float velocityY, GameTime gameTime)
        {
            //If the player wants to jump
            if (isJumping)
            {
                //Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(jumpAnimation);
                }


                //If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    //Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                //Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        #endregion

        #region CollisionHandler

        private void HandleCollisions()
        {
            //Get the player's bounding rectangle and find neighboring tiles
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            //Reset flag to search for ground collision
            isOnGround = false;


            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        //Determine collision depth (with direction) and magnitude
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            //Resolve the collision along the shallow axis
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                //If we crossed the top of a tile, we are on the ground
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                //Ignore platforms, unless we are on the ground
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    //Perform further collisions with the new bounds
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);
                                    bounds = BoundingRectangle;
                                }
                            }
                            //Ignore platforms
                            else if (collision == TileCollision.Impassable)
                            {
                                //Resolve the collision along the X axis
                                Position = new Vector2(Position.X + depth.X, Position.Y);
                                //Perform further collisions with the new bounds
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }
            //Save the new bounds bottom
            previousBottom = bounds.Bottom;
        }

        #endregion

        #region Player Killed

        //Called when the player has been killed
        public void OnKilled(Enemy killedBy)
        {
            life = false;
            GameplayScreen.score = 0;

            //kiled by enemy
            if (killedBy != null)
                killedSound.Play();
            else //killed by falling down
                killedSound.Play();

            GameplayScreen.numberoflifes--;
            sprite.PlayAnimation(dieAnimation);
        }

        #endregion

        #region player reached exit

        //Called when this player reaches the "room exit"
        public void OnReachedExit()
        {
            sprite.PlayAnimation(idleAnimation);
        }
        
        //Called when this player reaches the level's end
        public void LevelComplete()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        #endregion

        #region Draw

        //Draws the animated player
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //Flip the sprite to face the way we are moving
            if (Speed.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Speed.X < 0)
                flip = SpriteEffects.None;


            //Draw the bullets
            foreach (Fire bullet in stones)
            {
                if (bullet.collision)
                {
                    spriteBatch.Draw(bullet.sprite,
                        bullet.position, Color.White);
                }
            }


            // player flicker                                                                                            
            Color color;
            if (IsPoweredUp)
            {
                float t = ((float)gameTime.TotalGameTime.TotalSeconds + powerUpTime / MaxPowerUpTime) * 20.0f;
                int colorIndex = (int)t % poweredUpColors.Length;
                color = poweredUpColors[colorIndex];
            }
            else
            {
                color = Color.White;
            }
            sprite.Draw(gameTime, spriteBatch, Position, flip, color);
        }

        #endregion
        
    }
}

