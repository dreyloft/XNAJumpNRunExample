using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Ratatöskrs_Great_Adventure.Screens
{
    class ControlsMenuScreen : MenuScreen
    {
        MenuEntry controlsMenuEntry;
        ContentManager content;
        Texture2D ctrl;
        Texture2D controlsTexture;
        Texture2D backgroundTexture;

        public ControlsMenuScreen()
            : base("Controls")
        {
            controlsMenuEntry = new MenuEntry(string.Empty);
            MenuEntries.Add(controlsMenuEntry);        
        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            ctrl = content.Load<Texture2D>("ctrl");
            controlsTexture = content.Load<Texture2D>("controls");
            backgroundTexture = content.Load<Texture2D>("background");
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);
            spriteBatch.Draw(backgroundTexture, new Vector2(0, 0), Color.White);
            spriteBatch.Draw(ctrl, new Vector2(270 - 134/2, 80 - 28 / 2), Color.White);
            spriteBatch.Draw(controlsTexture, new Vector2(67, 114), Color.White);

            spriteBatch.End();
        }
    }
}
