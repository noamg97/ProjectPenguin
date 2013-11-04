using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ProjectPenguin
{
    public enum GameState
    {
        Menu,
        Playing,
        EscMenu,
        Loading,
        TitleScreen
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        bool isFullScreen = false;
        public static bool SkipAnimation = true;
        GameState CurrentGameState = GameState.Playing;


        PlayingStateClass playing;
        MainMenuStateClass menu;
        TitleScreenClass titleScreen;
        LoadingScreenClass loading;

        FontLoader fontLoader;

        GraphicsDeviceManager graphics;

        KeyboardState keyboardState;
        KeyboardState lastKeyboardState;
        MouseState mouseState;
        MouseState lastMouseState;

        private static SoundEffect buttonSound;
        private static SoundEffect buttonClickSound;
        private static SoundEffect backSound;


        SpriteBatch spriteBatch;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            if (!this.isFullScreen)
            {
                graphics.PreferredBackBufferWidth = 1440;
                graphics.PreferredBackBufferHeight = 810;
            }
            else
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.ToggleFullScreen();
            }
            graphics.PreferMultiSampling = true;
            this.IsMouseVisible = true;
            this.Window.AllowUserResizing = false;

            graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fontLoader = new FontLoader(Content);
            playing = new PlayingStateClass(graphics, GraphicsDevice, Content);
            titleScreen = new TitleScreenClass(Content, GraphicsDevice, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
            loading = new LoadingScreenClass(Content, null, false);

            Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            menu = new MainMenuStateClass(Content, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), true);

            ChangeGameState(CurrentGameState);

            buttonSound = Content.Load<SoundEffect>("Sounds/button");
            buttonClickSound = Content.Load<SoundEffect>("Sounds/buttonClick");
            backSound = Content.Load<SoundEffect>("Sounds/back");
        }

        protected override void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            switch (CurrentGameState)
            {
                case GameState.EscMenu:
                case GameState.Menu: menu.Update(gameTime, this, mouseState, lastMouseState, keyboardState, lastKeyboardState); Window.Title = mouseState.X + " : " + mouseState.Y;

                    break;
                case GameState.Playing: playing.Update(gameTime, keyboardState, lastKeyboardState, mouseState, lastMouseState, this);
                    break;
                case GameState.Loading: loading.Update(gameTime, this); Window.Title = mouseState.X + " : " + mouseState.Y;

                    break;
                case GameState.TitleScreen: titleScreen.Update(gameTime, this, mouseState, keyboardState); Window.Title = mouseState.X + " : " + mouseState.Y;

                    break;
            }
            lastKeyboardState = keyboardState;
            lastMouseState = mouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (CurrentGameState)
            {
                case GameState.EscMenu:
                case GameState.Menu: menu.Draw(spriteBatch, GraphicsDevice);
                    break;
                case GameState.Playing: playing.Draw(spriteBatch, fontLoader, graphics, gameTime);
                    break;
                case GameState.Loading: loading.Draw(graphics, spriteBatch);
                    break;
                case GameState.TitleScreen: titleScreen.Draw(spriteBatch, graphics, gameTime, fontLoader);
                    break;
            }


            base.Draw(gameTime);
        }

        public void ChangeGameState(GameState newState)
        {
            //if (newState == GameState.TitleScreen && titleScreen == null) titleScreen = new TitleScreenClass(Content, GraphicsDevice, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
            //if (CurrentGameState == GameState.TitleScreen && newState != GameState.TitleScreen) titleScreen = null;

            //if (newState == GameState.Loading && loading == null) loading = new LoadingScreenClass(Content, null, false);
            //if (CurrentGameState == GameState.Loading && newState != GameState.Loading) loading = null;


            //if (newState == GameState.Menu || newState == GameState.EscMenu)
            //{
            //    if (menu == null)
            //        menu = new MainMenuStateClass(Content, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), true);
            //}
            ////else if (menu != null) menu.backGroundMusic.Stop();
            //if (CurrentGameState == GameState.Menu || CurrentGameState == GameState.EscMenu)
            //    if (newState != GameState.Menu && newState != GameState.EscMenu)
            //        menu = null;


            //if (newState != GameState.Playing) playing.showFPS = false;



            if (newState == GameState.Menu || newState == GameState.EscMenu)
            {
                if (newState == GameState.EscMenu) PlaySound("back");
                //menu.backGroundMusic.Play();
                menu.isMainMenu = (newState == GameState.Menu);
            }

            CurrentGameState = newState;
        }

        public static void PlaySound(string Name)
        {
            switch (Name.ToLower())
            {
                case "button": buttonSound.Play(0.6f, 0, 0); return;
                case "click": buttonClickSound.Play(0.9f, 0, 0); return;
                case "back": backSound.Play(0.8f, -0.2f, 0); return;
            }
            throw new Exception("Invalid Sound Name");
        }

        public void ExitGame()
        {
            //if (System.Windows.Forms.MessageBox.Show("Are You Sure You Want To Exit?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            this.Exit();
        }

        public void RestartGame()
        {
            playing = new PlayingStateClass(graphics, GraphicsDevice, new ContentManager(Content.ServiceProvider, Content.RootDirectory));
        }


        //Keyboard
        public static bool isPressed(KeyboardState state, KeyboardState lastState, Keys key)
        {
            return state.IsKeyDown(key) && lastState.IsKeyUp(key);
        }
        public static bool isPressed_Inverted(KeyboardState state, KeyboardState lastState, Keys key)
        {
            return state.IsKeyUp(key) && lastState.IsKeyDown(key);
        }
        //Mouse
        public static bool isPressed(MouseState state, MouseState lastState, string btn)
        {
            switch (btn.ToUpper())
            {
                case "L": return state.LeftButton == ButtonState.Pressed && lastState.LeftButton == ButtonState.Released;
                case "R": return state.RightButton == ButtonState.Pressed && lastState.RightButton == ButtonState.Released;
                case "M": return state.MiddleButton == ButtonState.Pressed && lastState.MiddleButton == ButtonState.Released;
            }
            return false;
        }
        public static bool isPressed_Inverted(MouseState state, MouseState lastState, string btn)
        {
            switch (btn.ToUpper())
            {
                case "L": return lastState.LeftButton == ButtonState.Pressed && state.LeftButton == ButtonState.Released;
                case "R": return lastState.RightButton == ButtonState.Pressed && state.RightButton == ButtonState.Released;
                case "M": return lastState.MiddleButton == ButtonState.Pressed && state.MiddleButton == ButtonState.Released;
            }
            return false;
        }
        public static bool isHover(MouseState state, Rectangle pos)
        {
            return pos.Contains(state.X, state.Y);
        }
    }

    #region Key
    public class Key
    {
        Keys name;
        bool hasBeenPressed = false;
        bool toReturn = false;
        public enum type
        {
            never, onlyFirstTime, always
        }
        bool isFirstTime;
        type typ;
        bool isLefttMouse;


        /// <summary>
        /// Use This For Tracking Keyboard Clicks.
        /// </summary>
        /// <param name="Name">The Key To Keep Track Off</param>
        public Key(Keys Name, type typ)
        {
            this.name = Name;
            this.typ = typ;
            isFirstTime = typ == type.onlyFirstTime;
        }

        /// <summary>
        /// Use Only For Mouse Click.
        /// </summary>
        public Key(bool isLeftMouse)
        {
            this.isLefttMouse = isLeftMouse;
        }

        public void Update(KeyboardState keystate, KeyboardState lastKeyState)
        {
            if (typ == type.always || isFirstTime)
            {
                if (Game1.isPressed_Inverted(keystate, lastKeyState, name))
                    if (hasBeenPressed)
                    {
                        toReturn = true;
                        hasBeenPressed = false;
                        isFirstTime = false;
                    }
                    else toReturn = false;
                else toReturn = false;

                if (!hasBeenPressed) hasBeenPressed = Game1.isPressed(keystate, lastKeyState, name);
            }
            else
            {
                toReturn = keystate.IsKeyDown(name);
            }
        }

        public void Update(MouseState state, MouseState lastState)
        {
            if (isLefttMouse)
            {
                if (Game1.isPressed_Inverted(state, lastState, "l"))
                    if (hasBeenPressed)
                    {
                        toReturn = true;
                        hasBeenPressed = false;
                    }
                    else toReturn = false;
                else toReturn = false;
            }
            else
            {
                if (Game1.isPressed_Inverted(state, lastState, "r"))
                    if (hasBeenPressed)
                    {
                        toReturn = true;
                        hasBeenPressed = false;
                    }
                    else toReturn = false;
                else toReturn = false;
            }

            if (!hasBeenPressed) hasBeenPressed = Game1.isPressed(state, lastState, isLefttMouse ? "l" : "r");
        }

        public bool isPressed()
        {
            return toReturn;
        }
    }
    #endregion

    #region fontLoader
    public class FontLoader
    {
        struct font
        {
            public SpriteFont sprite;
            public string name;
            public bool isBold;
            public int size;
        }

        List<font> fontsList = new List<font>();
        ContentManager content;

        public FontLoader(ContentManager content)
        {
            this.content = content;
        }

        public SpriteFont GetFont(string name, bool isBold, int size)
        {
            foreach (font f in fontsList)
                if (f.name.Equals(name) && f.isBold == isBold && f.size == size)
                    return f.sprite;


            font nFont = new font();
            nFont.sprite = content.Load<SpriteFont>("Fonts/" + name + "_" + ((isBold) ? "1" : "0") + "_" + size);
            nFont.name = name;
            nFont.isBold = isBold;
            nFont.size = size;

            fontsList.Add(nFont);

            return nFont.sprite;
        }
    }
    #endregion

    #region DynamicAnimation
    public class DynamicAnimation
    {
        TimeSpan duration;
        bool loop;
        public bool hasFinished { get; private set; }
        TimeSpan elapsedTime = TimeSpan.FromSeconds(0);

        Vector3[] v3Start, v3End;
        Vector2[] v2Start, v2End;
        float[] fStart, fEnd;

        Vector3[] v3;
        Vector2[] v2;
        float[] f;



        public DynamicAnimation(Vector3[] StartV3, Vector3[] EndV3, TimeSpan Duration, bool Loop)
        {
            this.v3Start = StartV3;
            this.v3End = EndV3;
            this.v3 = new Vector3[StartV3.Length];
            SetArray(StartV3);
            this.duration = Duration;
            this.loop = Loop;
            hasFinished = false;
        }

        public DynamicAnimation(Vector2[] StartV2, Vector2[] EndV2, TimeSpan Duration, bool Loop)
        {
            this.v2Start = StartV2;
            this.v2End = EndV2;
            this.v2 = new Vector2[StartV2.Length];
            SetArray(StartV2);
            this.duration = Duration;
            this.loop = Loop;
            hasFinished = false;
        }

        public DynamicAnimation(float[] StartFloat, float[] EndFloat, TimeSpan Duration, bool Loop)
        {
            this.fStart = StartFloat;
            this.fEnd = EndFloat;
            this.f = new float[StartFloat.Length];
            SetArray(StartFloat);
            this.duration = Duration;
            this.loop = Loop;
            hasFinished = false;
        }

        public DynamicAnimation(Vector3[] StartV3, Vector3[] EndV3, Vector2[] StartV2, Vector2[] EndV2, TimeSpan Duration, bool Loop)
        {
            this.v3Start = StartV3;
            this.v3End = EndV3;
            this.v2Start = StartV2;
            this.v2End = EndV2;

            this.v3 = new Vector3[StartV3.Length];
            SetArray(StartV3);
            this.v2 = new Vector2[StartV2.Length];
            SetArray(StartV2);

            this.duration = Duration;
            this.loop = Loop;
            hasFinished = false;
        }

        public DynamicAnimation(Vector3[] StartV3, Vector3[] EndV3, float[] StartFloat, float[] EndFloat, TimeSpan Duration, bool Loop)
        {
            this.v3Start = StartV3;
            this.v3End = EndV3;
            this.fStart = StartFloat;
            this.fEnd = EndFloat;

            this.v3 = new Vector3[StartV3.Length];
            SetArray(StartV3);
            this.f = new float[StartFloat.Length];
            SetArray(StartFloat);

            this.duration = Duration;
            this.loop = Loop;
            hasFinished = false;
        }

        public DynamicAnimation(Vector2[] StartV2, Vector2[] EndV2, float[] StartFloat, float[] EndFloat, TimeSpan Duration, bool Loop)
        {
            this.v2Start = StartV2;
            this.v2End = EndV2;
            this.fStart = StartFloat;
            this.fEnd = EndFloat;

            this.v2 = new Vector2[StartV2.Length];
            SetArray(StartV2);
            this.f = new float[StartFloat.Length];
            SetArray(StartFloat);

            this.duration = Duration;
            this.loop = Loop;
            hasFinished = false;
        }

        public DynamicAnimation(Vector3[] StartV3, Vector3[] EndV3, Vector2[] StartV2, Vector2[] EndV2, float[] StartFloat, float[] EndFloat, TimeSpan Duration, bool Loop)
        {
            this.v3Start = StartV3;
            this.v3End = EndV3;
            this.v2Start = StartV2;
            this.v2End = EndV2;
            this.fStart = StartFloat;
            this.fEnd = EndFloat;

            this.v3 = new Vector3[StartV3.Length];
            SetArray(StartV3);
            this.v2 = new Vector2[StartV2.Length];
            SetArray(StartV2);
            this.f = new float[StartFloat.Length];
            SetArray(StartFloat);

            this.duration = Duration;
            this.loop = Loop;
            hasFinished = false;
        }

        private void SetArray(Vector3[] s)
        {
            for (int i = 0; i < v3.Length; i++)
                v3[i] = s[i];
        }
        private void SetArray(Vector2[] s)
        {
            for (int i = 0; i < v2.Length; i++)
                v2[i] = s[i];
        }
        private void SetArray(float[] s)
        {
            for (int i = 0; i < f.Length; i++)
                f[i] = s[i];
        }



        public void Update(TimeSpan Elapsed)
        {
            this.elapsedTime += Elapsed;

            double amt = elapsedTime.TotalMilliseconds / duration.TotalMilliseconds;

            if (elapsedTime.TotalMilliseconds >= duration.TotalMilliseconds)
                hasFinished = true;

            if (loop)
                while (amt > 1)
                    amt -= 1;
            else
                amt = MathHelper.Clamp((float)amt, 0, 1);


            if (v3 != null)
                for (int i = 0; i < v3.Length; i++)
                    v3[i] = Vector3.Lerp(v3Start[i], v3End[i], (float)amt);

            if (v2 != null)
                for (int i = 0; i < v2.Length; i++)
                    v2[i] = Vector2.Lerp(v2Start[i], v2End[i], (float)amt);

            if (f != null)
                for (int i = 0; i < f.Length; i++)
                    f[i] = MathHelper.Lerp(fStart[i], fEnd[i], (float)amt);
        }


        public Vector3 GetV3(int index)
        {
            return v3[index];
        }

        public Vector2 GetV2(int index)
        {
            return v2[index];
        }

        public float GetFloat(int index)
        {
            return f[index];
        }
    }
    #endregion

    #region Entery Point
    static class Program
    {
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())//System.Windows.Forms.MessageBox.Show("Open In Full Screen ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes))
            {
                game.Run();
            }
        }
    }
    #endregion
}