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
    class Teleporter
    {
        Model model;
        BasicEffect effect;
        Vector3 position;
        public bool isOnPlayer { get; private set; }
        Matrix[] transforms;
        BoundingSphere sphere;
        List<BoundingBox> boundingBoxes;
        float offset;
        Matrix scaleMatrix = Matrix.CreateScale(0.05f);
        const float sphereCenterOffset = 0.2f;

        const float Gravity = 0.05f;
        const float ThrowPower = 0.3f;
        float xVelocity;
        float yVelocity0;

        float throwingYaw;
        TimeSpan time;
        bool canX = true, canY = true, hasLowered = false;

        public Teleporter(Model model, BasicEffect effect, List<BoundingBox> boundingBoxes, float offset)
        {
            this.model = model;
            this.effect = effect;
            this.boundingBoxes = boundingBoxes;
            this.offset = offset;
            isOnPlayer = true;
            position = Vector3.Zero;

            transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            sphere = model.Meshes[0].BoundingSphere;
            sphere.Radius -= 0.22f;
            sphere.Center.Y += sphereCenterOffset;
            if (model.Meshes.Count > 1)
                foreach (ModelMesh mesh in model.Meshes)
                    sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);

            sphere = sphere.Transform(scaleMatrix);
        }

        public void SetRoomBoundingBoxes(List<BoundingBox> boundingBoxes)
        {
            this.boundingBoxes = boundingBoxes;
        }



        public void Update(GameTime gameTime)
        {
            if (!isOnPlayer)
            {
                int collisioned = -1;

                time += gameTime.ElapsedGameTime;
                float Time = (float)(time.TotalMilliseconds / 1000d);

                Vector3 rotationVector = Vector3.Transform(new Vector3(0, 0, -1), Matrix.CreateRotationY(throwingYaw));

                BoundingSphere xSphere = sphere.Transform(Matrix.CreateTranslation(position + (xVelocity * rotationVector)));
                BoundingSphere ySphere = sphere.Transform(Matrix.CreateTranslation(new Vector3(position.X, position.Y + (yVelocity0 - (Gravity * Time)), position.Z)));

                if (boundingBoxes[0].Contains(xSphere) != ContainmentType.Contains)
                {
                    //canX = false;
                    yVelocity0 /= 2;
                    xVelocity = -xVelocity / 50;
                }
                if (boundingBoxes[0].Contains(ySphere) != ContainmentType.Contains)
                {
                    canY = false;
                    collisioned = 0;
                }
                for (int i = 1; i < boundingBoxes.Count; i++)
                {
                    if (boundingBoxes[i].Contains(xSphere) != ContainmentType.Disjoint)
                    {
                        //canX = false;
                        throwingYaw = 
                        yVelocity0 /= 2;
                        xVelocity = -xVelocity / 50;
                    }
                    if (boundingBoxes[i].Contains(ySphere) != ContainmentType.Disjoint)
                    {
                        canY = false;
                        collisioned = i;
                    }
                }


                if (canY)
                {
                    position.Y += yVelocity0 - Gravity * Time;
                    if (canX) position += xVelocity * rotationVector;
                }
                else if (!hasLowered)
                {
                    xVelocity /= 3;
                    yVelocity0 = 0;

                    ySphere = sphere.Transform(Matrix.CreateTranslation(new Vector3(position.X, position.Y - Gravity, position.Z)));


                    if (collisioned == 0)
                    {
                        if (boundingBoxes[0].Contains(ySphere) == ContainmentType.Contains)
                        {
                            position.Y -= Gravity;
                            canY = true;
                        }
                    }
                    else
                        if (boundingBoxes[collisioned].Contains(ySphere) == ContainmentType.Disjoint)
                        {
                            position.Y -= Gravity;
                            canY = true;
                        }

                    hasLowered = true;
                }
            }
        }

        public void Throw(Vector3 sPosition, float yaw, float pitch)
        {
            position = sPosition;

            Vector3 rotationVector = Vector3.Transform(new Vector3(0, 0, -1), Matrix.CreateRotationY(yaw));
            position += rotationVector * 0.3f;
            position.Y -= 0.05f;

            position = (Vector3.Transform(position - sPosition, Matrix.CreateFromAxisAngle(Vector3.Transform(new Vector3(1, 0, 0), Matrix.CreateRotationY(yaw)), pitch)) + sPosition);

            yVelocity0 = (float)Math.Sin(pitch) * ThrowPower;
            xVelocity = (float)Math.Cos(pitch) * ThrowPower;

            throwingYaw = yaw;
            isOnPlayer = false;
            time = TimeSpan.Zero;
            canX = true;
            canY = true;
            hasLowered = false;
        }

        public void Teleport(ref Vector3 characterPosition)
        {
            characterPosition = new Vector3(position.X, position.Y + offset + sphereCenterOffset, position.Z);
            isOnPlayer = true;
            time = TimeSpan.Zero;

            canX = true;
            canY = true;
            hasLowered = false;
        }

        public void ReturnToHand()
        {
            isOnPlayer = true;
            time = TimeSpan.Zero;
            position = Vector3.Zero;

            canX = true;
            canY = true;
            hasLowered = false;
        }


        public void Draw(Matrix view, Matrix projection, GraphicsDevice device)
        {
            if (!isOnPlayer)
            {
                Matrix world = scaleMatrix * Matrix.CreateTranslation(position);

                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = transforms[mesh.ParentBone.Index] * world;
                        effect.View = view;
                        effect.Projection = projection;
                        effect.TextureEnabled = false;
                        effect.LightingEnabled = false;
                    }
                    mesh.Draw();
                }
                //BoundingSphereRenderer.Render(sphere.Transform(Matrix.CreateTranslation(position)), device, view, projection, Color.Red);
            }
        }
    }

    #region BoundingSphereRenderer
    //public static class BoundingSphereRenderer
    //{
    //    static VertexBuffer vertBuffer;
    //    static VertexDeclaration vertDecl;
    //    static BasicEffect effect;
    //    static int sphereResolution;

    //    /// <summary>
    //    /// Initializes the graphics objects for rendering the spheres. If this method isn't
    //    /// run manually, it will be called the first time you render a sphere.
    //    /// </summary>
    //    /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
    //    /// <param name="sphereResolution">The number of line segments
    //    ///     to use for each of the three circles.</param>
    //    public static void InitializeGraphics(GraphicsDevice graphicsDevice, int sphereResolution)
    //    {
    //        BoundingSphereRenderer.sphereResolution = sphereResolution;

    //        //vertDecl = new VertexDeclaration(
    //        effect = new BasicEffect(graphicsDevice);
    //        effect.LightingEnabled = false;
    //        effect.VertexColorEnabled = false;

    //        VertexPositionColor[] verts = new VertexPositionColor[(sphereResolution + 1) * 3];

    //        int index = 0;

    //        float step = MathHelper.TwoPi / (float)sphereResolution;

    //        //create the loop on the XY plane first
    //        for (float a = 0f; a <= MathHelper.TwoPi; a += step)
    //        {
    //            verts[index++] = new VertexPositionColor(
    //                new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f),
    //                Color.White);
    //        }

    //        //next on the XZ plane
    //        for (float a = 0f; a <= MathHelper.TwoPi; a += step)
    //        {
    //            verts[index++] = new VertexPositionColor(
    //                new Vector3((float)Math.Cos(a), 0f, (float)Math.Sin(a)),
    //                Color.White);
    //        }

    //        //finally on the YZ plane
    //        for (float a = 0f; a <= MathHelper.TwoPi; a += step)
    //        {
    //            verts[index++] = new VertexPositionColor(
    //                new Vector3(0f, (float)Math.Cos(a), (float)Math.Sin(a)),
    //                Color.White);
    //        }

    //        vertBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), verts.Length, BufferUsage.None);
    //        vertBuffer.SetData(verts);
    //    }

    //    /// <summary>
    //    /// Renders a bounding sphere using different colors for each axis.
    //    /// </summary>
    //    /// <param name="sphere">The sphere to render.</param>
    //    /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
    //    /// <param name="view">The current view matrix.</param>
    //    /// <param name="projection">The current projection matrix.</param>
    //    /// <param name="xyColor">The color for the XY circle.</param>
    //    /// <param name="xzColor">The color for the XZ circle.</param>
    //    /// <param name="yzColor">The color for the YZ circle.</param>
    //    public static void Render(
    //        BoundingSphere sphere,
    //        GraphicsDevice graphicsDevice,
    //        Matrix view,
    //        Matrix projection,
    //        Color xyColor,
    //        Color xzColor,
    //        Color yzColor)
    //    {
    //        if (vertBuffer == null)
    //            InitializeGraphics(graphicsDevice, 30);

    //        graphicsDevice.SetVertexBuffer(vertBuffer);

    //        effect.World =
    //            Matrix.CreateScale(sphere.Radius) *
    //            Matrix.CreateTranslation(sphere.Center);
    //        effect.View = view;
    //        effect.Projection = projection;
    //        effect.DiffuseColor = xyColor.ToVector3();

    //        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
    //        {
    //            pass.Apply();

    //            //render each circle individually
    //            graphicsDevice.DrawPrimitives(
    //                  PrimitiveType.LineStrip,
    //                  0,
    //                  sphereResolution);
    //            pass.Apply();
    //            effect.DiffuseColor = xzColor.ToVector3();
    //            graphicsDevice.DrawPrimitives(
    //                  PrimitiveType.LineStrip,
    //                  sphereResolution + 1,
    //                  sphereResolution);
    //            pass.Apply();
    //            effect.DiffuseColor = yzColor.ToVector3();
    //            graphicsDevice.DrawPrimitives(
    //                  PrimitiveType.LineStrip,
    //                  (sphereResolution + 1) * 2,
    //                  sphereResolution);
    //            pass.Apply();

    //        }

    //    }

    //    public static void Render(BoundingSphere[] spheres,
    //       GraphicsDevice graphicsDevice,
    //       Matrix view,
    //       Matrix projection,
    //       Color xyColor,
    //        Color xzColor,
    //        Color yzColor)
    //    {
    //        foreach (BoundingSphere sphere in spheres)
    //        {
    //            Render(sphere, graphicsDevice, view, projection, xyColor, xzColor, yzColor);
    //        }
    //    }

    //    public static void Render(BoundingSphere[] spheres,
    //        GraphicsDevice graphicsDevice,
    //        Matrix view,
    //        Matrix projection,
    //        Color color)
    //    {
    //        foreach (BoundingSphere sphere in spheres)
    //        {
    //            Render(sphere, graphicsDevice, view, projection, color);
    //        }
    //    }

    //    /// <summary>
    //    /// Renders a bounding sphere using a single color for all three axis.
    //    /// </summary>
    //    /// <param name="sphere">The sphere to render.</param>
    //    /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
    //    /// <param name="view">The current view matrix.</param>
    //    /// <param name="projection">The current projection matrix.</param>
    //    /// <param name="color">The color to use for rendering the circles.</param>
    //    public static void Render(
    //        BoundingSphere sphere,
    //        GraphicsDevice graphicsDevice,
    //        Matrix view,
    //        Matrix projection,
    //        Color color)
    //    {
    //        if (vertBuffer == null)
    //            InitializeGraphics(graphicsDevice, 30);

    //        graphicsDevice.SetVertexBuffer(vertBuffer);

    //        effect.World =
    //              Matrix.CreateScale(sphere.Radius) *
    //              Matrix.CreateTranslation(sphere.Center);
    //        effect.View = view;
    //        effect.Projection = projection;
    //        effect.DiffuseColor = color.ToVector3();

    //        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
    //        {
    //            pass.Apply();

    //            //render each circle individually
    //            graphicsDevice.DrawPrimitives(
    //                  PrimitiveType.LineStrip,
    //                  0,
    //                  sphereResolution);
    //            graphicsDevice.DrawPrimitives(
    //                  PrimitiveType.LineStrip,
    //                  sphereResolution + 1,
    //                  sphereResolution);
    //            graphicsDevice.DrawPrimitives(
    //                  PrimitiveType.LineStrip,
    //                  (sphereResolution + 1) * 2,
    //                  sphereResolution);

    //        }

    //    }
    //}
    #endregion
}
