using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Ratatöskrs_Great_Adventure.Screens
{
    class Fire
    {
        public Texture2D sprite;
        public Vector2 position;
        public Vector2 velocity;
        public bool collision;

        public Rectangle rectangle
        {
            get
            {
                int left = (int)position.X;
                int width = sprite.Width;
                int top = (int)position.Y;
                int height = sprite.Height;
                return new Rectangle(left, top, width, height);
            }
        }

        public Fire(Texture2D loadedTexture)
        {
            position = Vector2.Zero;
            sprite = loadedTexture;
            velocity = Vector2.Zero;
            collision = false;
        }
    }
}
