using System;
using Microsoft.Xna.Framework;

namespace Ratatöskrs_Great_Adventure.Screens
{
    //Represents a 2D circle
    struct Circle
    {
        public Vector2 Center;
        public float Radius;

        //Constructs a new circle
        public Circle(Vector2 position, float radius)
        {
            Center = position;
            Radius = radius;
        }

        //Determines if a circle intersects a rectangle
        public bool Intersects(Rectangle rectangle)
        {
            Vector2 v = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right),
                                    MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));

            Vector2 direction = Center - v;
            float distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < Radius * Radius));
        }
    }
}
