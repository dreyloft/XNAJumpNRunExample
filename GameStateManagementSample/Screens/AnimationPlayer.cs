using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ratatöskrs_Great_Adventure.Screens
{
    //Controls playback of an Animation
    struct AnimationPlayer
    {
        //Gets the animation which is currently playing
        public Animation Animation
        {
            get { return animation; }
        }
        Animation animation;


        //Gets the index of the current frame in the animation
        public int FrameIndex
        {
            get { return frameIndex; }
        }
        int frameIndex;


        //The amount of time in seconds that the current frame has been shown for
        private float time;


        //Gets a texture origin at the bottom center of each frame
        public Vector2 Origin
        {
            get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); }
        }


        //Begins or continues playback of an animation
        public void PlayAnimation(Animation animation)
        {
            //If this animation is already running, do not restart it
            if (Animation == animation)
                return;

            //Start the new animation
            this.animation = animation;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        #region draw

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects, Color color)
        {
            if (Animation == null)
                throw new NotSupportedException("Animation Player - No animation is currently playing.");


            //Process passing time
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (time > Animation.FrameTime)
            {
                time -= Animation.FrameTime;

                //Advance the frame index; looping or clamping as appropriate
                if (Animation.IsLooping)
                {
                    frameIndex = (frameIndex + 1) % Animation.FrameCount;
                }
                else
                {
                    frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                }
            }
            //Calculate the source rectangle of the current frame
            Rectangle source = new Rectangle(FrameIndex * Animation.Texture.Height, 0, Animation.Texture.Height, Animation.Texture.Height);

            spriteBatch.Draw(Animation.Texture, position, source, color, 0.0f, Origin, 1.0f, spriteEffects, 0.0f);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
        {
            Draw(gameTime, spriteBatch, position, spriteEffects, Color.White);
        }
        
        #endregion
    }
}
