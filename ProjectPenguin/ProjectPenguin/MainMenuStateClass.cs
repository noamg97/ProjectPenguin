using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectPenguin
{
    class MainMenuStateClass
    {
        #region Text Positions
        Rectangle[] TextPositions = new Rectangle[]{
            new Rectangle(728, 434, 362, 80),
            new Rectangle(728, 514, 362, 72),
            new Rectangle(728, 586, 362, 76),
            new Rectangle(728, 662, 362, 76),
            new Rectangle(728, 738, 362, 76),
        };
        #endregion

        Texture2D back, logo, _1, _2, _3, _4, _5;
        Texture2D textContinue, textNewGame, textEsc;
        Rectangle screen;
        public bool showNewGame;
        int selected = 1;
        const float menuScale = 0.4f;
        Vector2 resOffset;
        public bool isMainMenu = true; // or pause menu
        TimeSpan time, soundTime;
        //public SoundEffectInstance backGroundMusic;
        public const float BackgroundMusicVolume = 0.2f;
        DynamicAnimation backGroundAlphaAnimation = new DynamicAnimation(new Vector2[] { Vector2.Zero }, new Vector2[] { Vector2.One }, TimeSpan.FromMilliseconds(500), false);
        static bool isFirstTime = true;

        Dictionary<string, Key> UsedKeys = new Dictionary<string, Key>();


        public MainMenuStateClass(ContentManager content, Vector2 gameScreenSize, bool showNew)
        {
            UsedKeys.Add("enter", new Key(Keys.Enter, Key.type.always));
            UsedKeys.Add("up", new Key(Keys.Up, Key.type.never));
            UsedKeys.Add("down", new Key(Keys.Down, Key.type.never));
            UsedKeys.Add("escape", new Key(Keys.Escape, Key.type.always));
            UsedKeys.Add("mouse", new Key(true));

            resOffset = new Vector2(gameScreenSize.X / 1920, gameScreenSize.Y / 1080);
            screen = new Rectangle(0, 0, (int)gameScreenSize.X, (int)gameScreenSize.Y);
            showNewGame = showNew;

            //backGroundMusic = content.Load<SoundEffect>("Sounds/wind02").CreateInstance();
            //backGroundMusic.IsLooped = true;
            //backGroundMusic.Volume = BackgroundMusicVolume;

            back = content.Load<Texture2D>("Images/Menus/MenuBack");
            logo = content.Load<Texture2D>("Images/Logo");

            _1 = content.Load<Texture2D>("Images/Menus/1");
            _2 = content.Load<Texture2D>("Images/Menus/2");
            _3 = content.Load<Texture2D>("Images/Menus/3");
            _4 = content.Load<Texture2D>("Images/Menus/4");
            _5 = content.Load<Texture2D>("Images/Menus/5");

            textContinue = content.Load<Texture2D>("Images/Menus/continue");
            textNewGame = content.Load<Texture2D>("Images/Menus/newgame");
            textEsc = content.Load<Texture2D>("Images/Menus/esc");


            for (int i = 0; i < TextPositions.Length; i++)
            {
                TextPositions[i].Width = (int)(((float)TextPositions[i].Width) / (1920 / gameScreenSize.X) * (0.4f / menuScale));
                TextPositions[i].Height = (int)(((float)TextPositions[i].Height) / (1080 / gameScreenSize.Y) * (0.4f / menuScale));
                TextPositions[i].X = (int)(((float)TextPositions[i].X) / (1920 / gameScreenSize.X) * (0.4f / menuScale));
                TextPositions[i].Y = (int)(((float)TextPositions[i].Y) / (1080 / gameScreenSize.Y) * (0.4f / menuScale));
            }
        }

        public void Update(GameTime gameTime, Game1 game1, MouseState state, MouseState lState, KeyboardState keystate, KeyboardState lastKeyState)
        {
            game1.IsMouseVisible = true;
            time += gameTime.ElapsedGameTime;
            soundTime += gameTime.ElapsedGameTime;
            backGroundAlphaAnimation.Update(gameTime.ElapsedGameTime);


            int lastSelected = selected;

            #region input
            if (UsedKeys["down"].isPressed() && time.TotalMilliseconds >= 200)
            {
                selected++;
                if (selected > 5)
                {
                    if (time.TotalMilliseconds >= 300)
                    {
                        selected = 1;
                        time = TimeSpan.Zero;
                    }
                    else selected = 5;
                }
                else
                    time = TimeSpan.Zero;
            }
            else if (UsedKeys["up"].isPressed() && time.TotalMilliseconds >= 200)
            {
                selected--;
                if (selected < 1)
                {
                    if (time.TotalMilliseconds >= 300)
                    {
                        selected = 5;
                        time = TimeSpan.Zero;
                    }
                    else selected = 1;
                }
                else
                    time = TimeSpan.Zero;
            }

            for (int i = 0; i < TextPositions.Length; i++)
                if (Game1.isHover(state, TextPositions[i]))
                {
                    if (state.X != lState.X || state.Y != lState.Y)
                    {
                        selected = i + 1;
                        time = TimeSpan.Zero;
                    }
                    if (UsedKeys["mouse"].isPressed())
                    {
                        Game1.PlaySound("click");
                        switch (selected)
                        {
                            case 1:
                                if (!isMainMenu)
                                {
                                    Game1.PlaySound("back");
                                    game1.ChangeGameState(GameState.Playing);
                                }
                                else
                                {
                                    if (!isFirstTime) { game1.RestartGame(); isFirstTime = false; }
                                    game1.ChangeGameState(GameState.Loading);
                                }
                                //backGroundMusic.Stop();
                                break;
                            case 4: if (!isMainMenu) game1.ChangeGameState(GameState.Menu);
                                break;
                            case 5: game1.ExitGame();
                                break;
                        }
                    }
                }

            if (UsedKeys["enter"].isPressed())
            {
                if (!(selected == 1 && !isMainMenu)) Game1.PlaySound("click");
                switch (selected)
                {
                    case 1:
                        if (!isMainMenu)
                        {
                            Game1.PlaySound("back");
                            game1.ChangeGameState(GameState.Playing);
                        }
                        else
                        {
                            if (!isFirstTime) { game1.RestartGame(); isFirstTime = false; }
                            game1.ChangeGameState(GameState.Loading);
                        }
                        //backGroundMusic.Stop();
                        break;
                    case 4: if (!isMainMenu) game1.ChangeGameState(GameState.Menu);
                        break;
                    case 5: game1.ExitGame();
                        break;
                }
            }
            #endregion

            if (soundTime.TotalMilliseconds >= 20)
                if (lastSelected != selected)
                {
                    Game1.PlaySound("button");
                    soundTime = TimeSpan.Zero;
                }

            if (!isMainMenu)
                if (UsedKeys["escape"].isPressed())
                {
                    Game1.PlaySound("back");
                    game1.ChangeGameState(GameState.Playing);
                }
            foreach (KeyValuePair<string, Key> key in UsedKeys)
            {
                if (key.Key != "mouse") key.Value.Update(keystate, lastKeyState);
                else key.Value.Update(state, lState);
            }
        }

        public void Draw(SpriteBatch sb, GraphicsDevice device)
        {
            sb.Begin();
            //sb.Draw(back, new Rectangle(0, 0, screen.Width, screen.Height), null, Color.White * backGroundAlphaAnimation.Resize.X, 0, Vector2.Zero, SpriteEffects.None, 1);
            sb.Draw(back, new Rectangle(0, 0, screen.Width / 2, screen.Height / 2), null, Color.White * backGroundAlphaAnimation.GetV2(0).X * 0.35f, 0, Vector2.Zero, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 1);
            sb.Draw(back, new Rectangle(screen.Width / 2, 0, screen.Width / 2, screen.Height / 2), null, Color.White * backGroundAlphaAnimation.GetV2(0).X * 0.35f, 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);
            sb.Draw(back, new Rectangle(0, screen.Height / 2, screen.Width / 2, screen.Height / 2), null, Color.White * backGroundAlphaAnimation.GetV2(0).X * 0.35f, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 1);
            sb.Draw(back, new Rectangle(screen.Width / 2, screen.Height / 2, screen.Width / 2, screen.Height / 2), null, Color.White * backGroundAlphaAnimation.GetV2(0).X * 0.35f, 0, Vector2.Zero, SpriteEffects.None, 1);

            sb.Draw(logo, new Vector2(TitleScreenClass.endLogoPositionSize.X, TitleScreenClass.endLogoPositionSize.Y), null, Color.White, 0, TitleScreenClass.logoOrigin, new Vector2(TitleScreenClass.endLogoPositionSize.Z, TitleScreenClass.endLogoPositionSize.W), SpriteEffects.None, 0);


            switch (selected)
            {
                case 1: sb.Draw(_1, new Vector2(screen.Width, screen.Height) / 2, null, Color.White * backGroundAlphaAnimation.GetV2(0).X, 0, new Vector2(1920, 1080), menuScale * resOffset, SpriteEffects.None, 0); break;
                case 2: sb.Draw(_2, new Vector2(screen.Width, screen.Height) / 2, null, Color.White * backGroundAlphaAnimation.GetV2(0).X, 0, new Vector2(1920, 1080), menuScale * resOffset, SpriteEffects.None, 0); break;
                case 3: sb.Draw(_3, new Vector2(screen.Width, screen.Height) / 2, null, Color.White * backGroundAlphaAnimation.GetV2(0).X, 0, new Vector2(1920, 1080), menuScale * resOffset, SpriteEffects.None, 0); break;
                case 4: sb.Draw(_4, new Vector2(screen.Width, screen.Height) / 2, null, Color.White * backGroundAlphaAnimation.GetV2(0).X, 0, new Vector2(1920, 1080), menuScale * resOffset, SpriteEffects.None, 0); break;
                case 5: sb.Draw(_5, new Vector2(screen.Width, screen.Height) / 2, null, Color.White * backGroundAlphaAnimation.GetV2(0).X, 0, new Vector2(1920, 1080), menuScale * resOffset, SpriteEffects.None, 0); break;
            }

            if (!isMainMenu) sb.Draw(textEsc, new Vector2(screen.Width, screen.Height) / 2, null, Color.White * backGroundAlphaAnimation.GetV2(0).X, 0, new Vector2(1920, 1080), menuScale * resOffset, SpriteEffects.None, 0);
            else sb.Draw(showNewGame ? textNewGame : textContinue, new Vector2(screen.Width, screen.Height) / 2, null, Color.White * backGroundAlphaAnimation.GetV2(0).X, 0, new Vector2(1920, 1080), menuScale * resOffset, SpriteEffects.None, 0);




            sb.End();
        }
    }
}
