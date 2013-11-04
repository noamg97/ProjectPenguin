using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectPenguin
{
    class TitleScreenClass
    {
        Texture2D Title;
        TimeSpan timePressed = new TimeSpan(0, 0, 50, 50, 0);

        const int LoadingTime = 1100;
        const int TimeBeforeUserCanContinue = 300;
        const int TimeToWaitAfterPressed = 1400;
        TimeSpan DummyTimeSpan = new TimeSpan(0, 0, 50, 50, 0);
        Vector2 ResOffset;

        int PressAlpha = 0;
        bool isAdding = false;
        TimeSpan counter = TimeSpan.Zero;
        Texture2D blackTex;

        LoadingScreenClass loading;
        bool isGoingToMenu = false;
        TimeSpan logoAnimation = TimeSpan.Zero;
        Texture2D logo;
        const int logoFadeInTime = 600;
        const int logoMoveTime = 1000;
        const int timeBeforeMoving = 1500;
        const int timeAfterAnimation = 1500;
        public static Vector2 logoOrigin = new Vector2(960, 540);
        DynamicAnimation logoAnim;

        public static Vector4 endLogoPositionSize;



        public TitleScreenClass(ContentManager content, GraphicsDevice device, Vector2 Screen)
        {
            Title = content.Load<Texture2D>("Images/blackSmaller");
            logo = content.Load<Texture2D>("Images/Logo");

            ResOffset = new Vector2(Screen.X / 1920, Screen.Y / 1080);

            endLogoPositionSize = new Vector4(new Vector2(Screen.X / 2, 200 * ResOffset.Y), ((ResOffset.X + ResOffset.Y) / 2) * 0.5f, ((ResOffset.X + ResOffset.Y) / 2) * 0.5f);

            logoAnim = new DynamicAnimation(new Vector2[] { Screen / 2, ResOffset }, new Vector2[] { new Vector2(endLogoPositionSize.X, endLogoPositionSize.Y), new Vector2(endLogoPositionSize.Z, endLogoPositionSize.W) }, TimeSpan.FromMilliseconds(logoMoveTime), false);

            loading = new LoadingScreenClass(content, LoadingTime, true);
            blackTex = new Texture2D(device, 1, 1);
        }

        public void Update(GameTime gameTime, Game1 game1, MouseState mouseState, KeyboardState keyboardState)
        {
            if (gameTime.TotalGameTime.TotalMilliseconds >= LoadingTime - 500)
            {
                if (counter < TimeSpan.FromMilliseconds(1000)) counter += gameTime.ElapsedGameTime;
                if (counter > TimeSpan.FromMilliseconds(1000))
                {
                    if (isAdding) PressAlpha += (int)gameTime.ElapsedGameTime.TotalMilliseconds / 2;
                    else PressAlpha -= (int)gameTime.ElapsedGameTime.TotalMilliseconds / 2;

                    if (PressAlpha > 255 | PressAlpha < 0)
                    {
                        if (!isAdding) counter = TimeSpan.Zero;
                        isAdding = !isAdding;
                    }
                }
            }
            if (gameTime.TotalGameTime.TotalMilliseconds < LoadingTime) loading.Update(gameTime, game1);

            if (!(timePressed.TotalMilliseconds < DummyTimeSpan.TotalMilliseconds) && gameTime.TotalGameTime.TotalMilliseconds >= LoadingTime + TimeBeforeUserCanContinue && (keyboardState.GetPressedKeys().Length > 0 || mouseState.LeftButton == ButtonState.Pressed))
            {
                timePressed = gameTime.TotalGameTime;
                Game1.PlaySound("back");
            }
            if (gameTime.TotalGameTime.TotalMilliseconds - timePressed.TotalMilliseconds >= TimeToWaitAfterPressed)
                isGoingToMenu = true;

            if (isGoingToMenu)
            {
                logoAnimation += gameTime.ElapsedGameTime;
                if (logoAnimation.TotalMilliseconds >= timeBeforeMoving + logoMoveTime + timeAfterAnimation) game1.ChangeGameState(GameState.Menu); // animation finished
            }
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, GameTime gameTime, FontLoader fontLoader)
        {
            if (gameTime.TotalGameTime.TotalMilliseconds <= LoadingTime + TimeBeforeUserCanContinue)
                loading.Draw(graphics, spriteBatch);

            else if (!(timePressed.TotalMilliseconds < DummyTimeSpan.TotalMilliseconds) && !isGoingToMenu)
            {
                spriteBatch.Begin();

                //if (!didClick) { Game1.PlaySound("back"); didClick = true; }
                string output = "Press Any Key";
                SpriteFont font = fontLoader.GetFont("OpenSans", false, 14);
                spriteBatch.DrawString(font, output, new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight * 40 / 55), Color.White, 0, font.MeasureString(output) / 2, 1.2f * ((ResOffset.X + ResOffset.Y) / 2), SpriteEffects.None, 0);

                blackTex.SetData(new Color[] { new Color(0, 0, 0, PressAlpha) });
                spriteBatch.Draw(blackTex, new Rectangle(0, 0, 1000000, 20000000), Color.Black);
                spriteBatch.Draw(Title, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight * 9 / 10) / 2, null, Color.White, 0, new Vector2(650, 332) / 2, 1.3f * ((ResOffset.X + ResOffset.Y) / 2), SpriteEffects.None, 0);

                spriteBatch.End();
            }

            if (isGoingToMenu)
            {
                spriteBatch.Begin();
                if (logoAnimation.TotalMilliseconds >= timeBeforeMoving)
                {
                    logoAnim.Update(gameTime.ElapsedGameTime);
                }

                spriteBatch.Draw(logo, logoAnim.GetV2(0), null, Color.White * MathHelper.Clamp((float)logoAnimation.TotalMilliseconds / logoFadeInTime, 0, 1), 0, logoOrigin, logoAnim.GetV2(1), SpriteEffects.None, 0);
                spriteBatch.End();
            }

            graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
