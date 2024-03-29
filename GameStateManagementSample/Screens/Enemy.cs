using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Ratatöskrs_Great_Adventure.Screens
{
    //Facing direction along the X axis
    enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    class Enemy
    {
        public Level Level
        {
            get { return level; }
        }
        Level level;


        public bool Life
        {
            get { return life; }
            set { life = value; }
        }
        bool life;


        //Position in world space of the bottom center of this enemy
        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        private Rectangle localBounds;


        //Gets a rectangle which bounds this enemy in world space
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }


        //Animations
        private Animation dieAnimation;
        private Animation runAnimation;
        private Animation idleAnimation;
        private AnimationPlayer sprite;

        private SoundEffect killedSound;


        //The direction this enemy is facing and moving along the X axis
        private FaceDirection direction = FaceDirection.Left;


        //How long this enemy has been waiting before turning around
        private float waitTime;


        //How long to wait before turning around
        private const float MaxWaitTime = 0.5f;


        //The speed at which this enemy moves along the X axis
        private const float MoveSpeed = 64.0f;


        //Constructs a new Enemy
        public Enemy(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.position = position;
            this.Life = true;

            LoadContent(spriteSet);
        }


        //Loads a particular enemy sprite sheet and sounds
        public void LoadContent(string spriteSet)
        {
            //Load animations
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.07f, false);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            sprite.PlayAnimation(idleAnimation);

            killedSound = Level.Content.Load<SoundEffect>("Sounds/Killed");


            //Calculate bounds within texture size
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }


        //Paces back and forth along a platform, waiting at either end
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!Life)
                return;


            //Calculate tile position based on the side we are walking towards
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0)
            {
                //Wait for some amount of time
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    //Then turn around
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                //If we are about to run into a wall or off a cliff, start waiting
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    //Move in the current direction
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                }
            }
        }


        //Draws the animated enemy
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Life)
            {
                sprite.PlayAnimation(dieAnimation);
            }
            else if (!Level.Player.Life ||
                      Level.ReachedExit ||
                      Level.TimeRemaining == TimeSpan.Zero ||
                      waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }


            //Draw facing the way the enemy is movings
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }


        public void OnKilled(Player killedBy)
        {
            Life = false;
            killedSound.Play();
        }        
    }
}
