using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectPenguin
{
    class LoadingScreenClass
    {
        TimeSpan timePast, totalTimePast;
        Texture2D[] loadingTex;
        const int size = 80;
        bool drawCircle = true;
        int loadingTime = 2000;
        bool isBeforeTitleScreen;

        public LoadingScreenClass(ContentManager content, int? loadingTime, bool isBeforeTitleScreen)
        {
            loadingTex = new Texture2D[17];
            this.loadingTime = (loadingTime == null) ? this.loadingTime : (int)loadingTime;
            this.isBeforeTitleScreen = isBeforeTitleScreen;

            for (int i = 0; i < loadingTex.Length; i++)
                loadingTex[i] = content.Load<Texture2D>("Images/LoadingTextures/" + i);
        }

        public void Update(GameTime gameTime, Game1 game1)
        {
            timePast += gameTime.ElapsedGameTime;
            totalTimePast += gameTime.ElapsedGameTime;


            if (totalTimePast.TotalMilliseconds >= loadingTime)
            {
                if (!isBeforeTitleScreen) game1.ChangeGameState(GameState.Playing);

                drawCircle = false;
            }
            if ((int)(timePast.TotalMilliseconds / 20) >= loadingTex.Length)
                timePast = TimeSpan.Zero;
        }

        public void Draw(GraphicsDeviceManager graphics, SpriteBatch sprite)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            if (drawCircle)
            {
                Texture2D current = loadingTex[(int)(timePast.TotalMilliseconds / 20)];

                Rectangle rect = new Rectangle(graphics.PreferredBackBufferWidth - size - 50, graphics.PreferredBackBufferHeight - size - 50, size, size);

                sprite.Begin();
                sprite.Draw(current, rect, Color.White);
                sprite.End();

                graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
        }
    }
}
