using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Ratatöskrs_Great_Adventure.Screens
{
    class Nut
    {
        #region Variables etc.

        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;

        public readonly Color Color;
        public readonly int PointValue;

        private Vector2 basePosition;
        private float bounce;

        public bool IsPowerUp { get; private set; }

        public Level Level
        {
            get { return level; }
        }
        Level level;

        #endregion

        #region Position and Boundingcircle

        //Gets the current position of this gem in world space
        public Vector2 Position
        {
            get
            {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }


        //Gets a circle which bounds this gem in world space
        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }
        
        #endregion
        
        #region set Nuts in the level and give them values

        public Nut(Level level, Vector2 position, bool isPowerUp)
        {
            this.level = level;
            this.basePosition = position;

            IsPowerUp = isPowerUp;
            if (IsPowerUp)
            {
                Color = Color.White;
            }
            else
            {
                PointValue = 1;
                Color = Color.White;
            }

            LoadContent();
        }

        #endregion

        #region Load exture and sound.

        public void LoadContent()
        {
            if (IsPowerUp)
            {
                texture = Level.Content.Load<Texture2D>("Sprites/Power");
            }
            else
            {
                texture = Level.Content.Load<Texture2D>("Sprites/Nut");
            }

            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/Nut");
        }

        #endregion

        #region Update

        //Bounces up and down in the air to entice players to collect them
        public void Update(GameTime gameTime)
        {
            //Bounce control constants
            const float BounceHeight = 0.18f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }

        #endregion

        #region Collected

        public void OnCollected(Player collectedBy)
        {
            collectedSound.Play();
            if (IsPowerUp)
                collectedBy.PowerUp();
        }

        #endregion

        #region Draw the Nuts

        //Draws a nut in the appropriate color
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

        #endregion
    }
}
