using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectPenguin
{
    public class PlayingStateClass
    {
        Camera camera;
        Room room;
        Teleporter teleporter;
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        ContentManager Content;
        public bool showBoundingBox = false;
        public static Vector2 screenCenter;
        RenderTarget2D rt;

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        public bool showFPS = false;

        TimeSpan timeBeforeStart;
        const int MiSe_BeforeStart = 10000;
        Texture2D BlackTexture;

        Dictionary<string, Key> UsedKeys = new Dictionary<string, Key>();

        BoundingBox b;
        Vector3 mirrorPos_Middle;

        DynamicAnimation blink = new DynamicAnimation(new float[] { 0 }, new float[] { 255 }, TimeSpan.FromMilliseconds(1000), true);



        public PlayingStateClass(GraphicsDeviceManager graphics, GraphicsDevice device, ContentManager Content)
        {
            UsedKeys.Add("i", new Key(Keys.I, Key.type.always));
            UsedKeys.Add("o", new Key(Keys.O, Key.type.always));
            UsedKeys.Add("u", new Key(Keys.U, Key.type.always));
            UsedKeys.Add("y", new Key(Keys.Y, Key.type.always));
            UsedKeys.Add("w", new Key(Keys.W, Key.type.never));
            UsedKeys.Add("a", new Key(Keys.A, Key.type.never));
            UsedKeys.Add("s", new Key(Keys.S, Key.type.never));
            UsedKeys.Add("d", new Key(Keys.D, Key.type.never));
            UsedKeys.Add("space", new Key(Keys.Space, Key.type.never));
            UsedKeys.Add("escape", new Key(Keys.Escape, Key.type.always));
            UsedKeys.Add("mouse", new Key(true));
            UsedKeys.Add("lmouse", new Key(false));


            BlackTexture = new Texture2D(graphics.GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            Color[] colorArray = new Color[BlackTexture.Width * BlackTexture.Height];
            for (int i = 0; i < colorArray.Length; i++)
                colorArray[i] = new Color(0, 0, 0, 0);
            BlackTexture.SetData(colorArray);


            screenCenter = new Vector2(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height) / 2;
            rt = new RenderTarget2D(device, (int)(graphics.PreferredBackBufferWidth), (int)(graphics.PreferredBackBufferHeight * 1.5), false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            camera = new Camera(graphics.GraphicsDevice, Content.Load<Model>("Models/PenguinFBX"), Content.Load<Effect>("RoomEffect"), false, false, true);
            room = new Room(Content.Load<Model>("Models/MainRoom"), Content.Load<Effect>("RoomEffect"), device);
            teleporter = new Teleporter(Content.Load<Model>("Models/teleporter"), new BasicEffect(device), room.bBoxManager.boundingBoxes, camera.characterOffset.Y);

            this.graphics = graphics;
            this.device = device;
            this.Content = Content;


            Vector3 mirrorPos_RightBottom = new Vector3(Math.Min(room.bBoxManager.boundingBoxes[8].Min.X, room.bBoxManager.boundingBoxes[8].Max.X), Math.Min(room.bBoxManager.boundingBoxes[8].Min.Y, room.bBoxManager.boundingBoxes[8].Max.Y), Math.Min(room.bBoxManager.boundingBoxes[8].Min.Z, room.bBoxManager.boundingBoxes[8].Max.Z));
            Vector3 mirrorPos_LeftTop = new Vector3(Math.Max(room.bBoxManager.boundingBoxes[8].Min.X, room.bBoxManager.boundingBoxes[8].Max.X), Math.Max(room.bBoxManager.boundingBoxes[8].Min.Y, room.bBoxManager.boundingBoxes[8].Max.Y), Math.Min(room.bBoxManager.boundingBoxes[8].Min.Z, room.bBoxManager.boundingBoxes[8].Max.Z));
            b = new BoundingBox(mirrorPos_RightBottom, mirrorPos_LeftTop);
            mirrorPos_Middle = new Vector3((room.bBoxManager.boundingBoxes[8].Min.X + room.bBoxManager.boundingBoxes[8].Max.X) / 2, (room.bBoxManager.boundingBoxes[8].Min.Y + room.bBoxManager.boundingBoxes[8].Max.Y) / 2, (room.bBoxManager.boundingBoxes[8].Min.Z + room.bBoxManager.boundingBoxes[8].Max.Z) / 2);


            camera.YrotationSpeed = (float)graphics.PreferredBackBufferWidth / 12000.0f;
            camera.XrotatiobSpeed = (float)graphics.PreferredBackBufferHeight / 13000.0f;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, KeyboardState lastKeyboardState, MouseState mouseState, MouseState lastMouseState, Game1 game1)
        {
            HandleInput(gameTime, keyboardState, lastKeyboardState, mouseState, game1);
            camera.UpdateView();
            teleporter.Update(gameTime);
            game1.IsMouseVisible = false;

            foreach (KeyValuePair<string, Key> key in UsedKeys)
            {
                if (key.Key != "mouse" && key.Key != "lmouse") key.Value.Update(keyboardState, lastKeyboardState);
                else key.Value.Update(mouseState, lastMouseState);
            }

            #region blink
            if (timeBeforeStart.TotalMilliseconds >= MiSe_BeforeStart)
            {
                blink.Update(gameTime.ElapsedGameTime);
            }
            else
            {
            }
            #endregion

            game1.Window.Title = camera.position + "    " + camera.yaw + "," + camera.pitch;
        }

        public void Draw(SpriteBatch spriteBatch, FontLoader fontLoader, GraphicsDeviceManager graphics, GameTime gametime)
        {
            #region mirror
            if (isOnScreen(b, camera.view * camera.projection))
            {
                device.SetRenderTarget(rt);
                device.Clear(Color.Black);

                Vector3 camerafinalPosition = camera.GetFinalPosition();

                Vector3 vectorToMirror = mirrorPos_Middle - camera.position;
                Vector3 mirrorReflectionVector = vectorToMirror - 2 * (Vector3.Dot(vectorToMirror, room.mirror.mirrorNormal)) * room.mirror.mirrorNormal;

                Matrix mirrorLookAt = Matrix.CreateLookAt(mirrorPos_Middle, mirrorReflectionVector, Vector3.Up) * Matrix.CreateScale(3, 1, 1);

                room.DrawRoom(mirrorLookAt, camera.projection, camera.position, camera.characterOffset, camera.isCrouched, showBoundingBox, camera.isAnimating);
                camera.DrawCharacter(mirrorLookAt);

                device.SetRenderTarget(null);
            }
            #endregion

            device.BlendState = BlendState.Opaque;
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            room.DrawRoom(camera.view, camera.projection, camera.position, camera.characterOffset, camera.isCrouched, showBoundingBox, camera.isAnimating);
            room.mirror.Draw((Texture2D)rt, camera.view, camera.projection);

            UpdateFrameRate(gametime);

            teleporter.Draw(camera.view, camera.projection, device);

            #region blink
            spriteBatch.Begin();
            spriteBatch.Draw(BlackTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White * ((float)MathHelper.Clamp(blink.GetFloat(0), 0, 255) / 255f));
            spriteBatch.End();
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            #endregion

            #region fps
            if (showFPS)
            {
                spriteBatch.Begin();
                string output = frameRate + " FPS";
                spriteBatch.DrawString(fontLoader.GetFont("OpenSans", false, 14), output, new Vector2(6, 3), Color.Black, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0);
                spriteBatch.End();

                device.BlendState = BlendState.Opaque;
                device.DepthStencilState = DepthStencilState.Default;
            }
            #endregion
        }


        public void UpdateFrameRate(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;
            frameCounter++;
            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }

        private void HandleInput(GameTime gameTime, KeyboardState keyboardState, KeyboardState lastKeyboardState, MouseState mouseState, Game1 game1)
        {
            //Debugging Stop Point
            if (keyboardState.IsKeyDown(Keys.P))
            { }


            #region keyboard

            if (UsedKeys["o"].isPressed())
                showFPS = !showFPS;
            if (UsedKeys["i"].isPressed())
                showBoundingBox = !showBoundingBox;
            if (UsedKeys["u"].isPressed())
                camera.canJump = !camera.canJump;
            if (UsedKeys["y"].isPressed())
                camera.canCrouch = !camera.canCrouch;

            #region game
            Vector3 moveVector = new Vector3(0, 0, 0);

            if (UsedKeys["w"].isPressed())
                moveVector += new Vector3(0, 0, -1);
            if (UsedKeys["s"].isPressed())
                moveVector += new Vector3(0, 0, 1);
            if (UsedKeys["d"].isPressed())
                moveVector += new Vector3(1, 0, 0);
            if (UsedKeys["a"].isPressed())
                moveVector += new Vector3(-1, 0, 0);

            moveVector *= (float)gameTime.ElapsedGameTime.TotalMilliseconds / 100.0f;

            if (camera.canCrouch && !camera.isJumping)
            {
                if (keyboardState.IsKeyDown(Keys.LeftControl))
                    camera.isCrouched = true;
                else
                    camera.isCrouched = false;
            }

            if (camera.canJump && UsedKeys["space"].isPressed() && !camera.isJumping && !camera.isCrouched)
            {
                camera.isJumping = true;
                camera.jumpspeed = 4;
            }
            camera.UpdatePosition(moveVector, room.bBoxManager.boundingBoxes, gameTime, this);
            #endregion

            #endregion

            #region mouse
            float amount = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            if (mouseState.X != screenCenter.X && !camera.isAnimating)
            {
                camera.yaw -= camera.YrotationSpeed * (mouseState.X - screenCenter.X) * amount;
            }
            if (mouseState.Y != screenCenter.Y && !camera.isAnimating)
            {
                camera.pitch -= camera.XrotatiobSpeed * (mouseState.Y - screenCenter.Y) * amount;
                if (camera.pitch > MathHelper.ToRadians(90))
                    camera.pitch = MathHelper.ToRadians(90);
                if (camera.pitch < MathHelper.ToRadians(-60))
                    camera.pitch = MathHelper.ToRadians(-60);
            }
            Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);

            if (UsedKeys["mouse"].isPressed())
            {
                if (teleporter.isOnPlayer) teleporter.Throw(camera.position, camera.yaw, camera.pitch);
                else teleporter.Teleport(ref camera.position);
            }
            if (UsedKeys["lmouse"].isPressed())
            {
                if (!teleporter.isOnPlayer) teleporter.ReturnToHand();
            }
            #endregion

            if (UsedKeys["escape"].isPressed())
                game1.ChangeGameState(GameState.EscMenu);
        }

        private bool isOnScreen(BoundingBox b, Matrix viewProj)
        {
            BoundingFrustum boundingFrustum;
            boundingFrustum = new BoundingFrustum(viewProj);

            return boundingFrustum.Contains(b) != ContainmentType.Disjoint;
        }
    }
}