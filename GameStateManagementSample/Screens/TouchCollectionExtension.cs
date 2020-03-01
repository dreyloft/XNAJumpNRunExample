using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;


namespace Ratatöskrs_Great_Adventure.Screens
{
    public static class TouchCollectionExtensions
    {
        public static bool fired;

        public static bool FireButtonTouch(this TouchCollection touchState)
        {
            foreach (TouchLocation location in touchState)
            {
                if (location.Position.X > 400 && location.State == TouchLocationState.Moved)
                {
                    return fired = true;
                }
            }
            return fired = false;
        }

        public static bool JumpButtonTouch(this TouchCollection touchState)
        {
            foreach (TouchLocation location in touchState)
            {
                if (location.Position.X < 400 && location.State == TouchLocationState.Moved)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
