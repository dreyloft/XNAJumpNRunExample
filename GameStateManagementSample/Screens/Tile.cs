using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ratatöskrs_Great_Adventure.Screens
{
    #region behavior

    //collision detection and response behavior of tile
    enum TileCollision
    {
        Passable = 0,
        Impassable = 1,
        Platform = 2,
    }

    #endregion

    #region tile

    //appearance and collision behavior of tile
    struct Tile
    {
        public Texture2D Texture;
        public TileCollision Collision;

        public const int Width = 48;
        public const int Height = Width;

        public static readonly Vector2 Size = new Vector2(Width, Height);


        //Constructs a new tile
        public Tile(Texture2D texture, TileCollision collision)
        {
            Texture = texture;
            Collision = collision;
        }
    }
    #endregion
}
