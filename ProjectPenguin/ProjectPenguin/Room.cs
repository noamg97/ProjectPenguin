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
    class Room
    {
        public Mirror mirror;
        public BoundingBoxManager bBoxManager;

        Model room;
        Effect roomEffect;
        Matrix roomWorldMatrix = Matrix.CreateTranslation(new Vector3(0, -6, 0)) * Matrix.CreateScale(0.3f);

        public static Vector3 lightPos = new Vector3(8f, 3, -0.5f);
        public const float lightPower = 1.3f;
        public const float ambientPower = -0.1f;

        List<Vector3> OriginalDiffuseColors = new List<Vector3>();
        Matrix[] bones;
        GraphicsDevice device;


        public Room(Model Room, Effect RoomEffect, GraphicsDevice device)
        {
            this.room = Room;
            this.roomEffect = RoomEffect;
            this.device = device;
            PresentationParameters pp = device.PresentationParameters;

            foreach (ModelMesh mesh in this.room.Meshes)
            {
                if (mesh.Effects.Count > 0)
                    if (mesh.Effects[0] is BasicEffect)
                        foreach (BasicEffect effect in mesh.Effects)
                            this.OriginalDiffuseColors.Add(effect.DiffuseColor);
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = this.roomEffect.Clone();
            }

            bones = new Matrix[room.Bones.Count];
            room.CopyAbsoluteBoneTransformsTo(bones);




            bBoxManager = new BoundingBoxManager(room, this.device, bones, roomWorldMatrix);
            mirror = new Mirror(bBoxManager.boundingBoxes, device);
        }

        public void DrawRoom(Matrix view, Matrix projection, Vector3 position, Vector3 characterOffset, bool isCrouched, bool showBoundingBox, bool DrawHat)
        {
            device.BlendState = BlendState.Opaque;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            
            roomEffect.CurrentTechnique = roomEffect.Techniques["Colored"];

            int effectCount = 0;
            foreach (ModelMesh mesh in room.Meshes)
            {
                if (mesh.Name != "hat" || DrawHat)
                {
                    foreach (Effect currentEffect in mesh.Effects)
                    {
                        Matrix worldMatrix = bones[mesh.ParentBone.Index] * roomWorldMatrix;
                        currentEffect.Parameters["xWorldViewProjection"].SetValue(worldMatrix * view * projection);
                        currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                        currentEffect.Parameters["xLightPos"].SetValue(lightPos);
                        currentEffect.Parameters["xLightPower"].SetValue(lightPower);
                        currentEffect.Parameters["xAmbient"].SetValue(ambientPower);
                        currentEffect.Parameters["DiffuseColor"].SetValue(OriginalDiffuseColors[effectCount++]);
                    }
                    mesh.Draw();
                }
                else effectCount++;
            }


            if (showBoundingBox)
            {
                bBoxManager.DrawBoundingBoxes(view, projection, position, characterOffset, isCrouched);
            }
        }
    }

    #region Mirror
    class Mirror
    {
        BasicEffect mirrorEffect;
        VertexPositionNormalTexture[] Vertices;
        short[] Indexes;
        public Vector3 mirrorNormal = new Vector3(0, 0, 1);
        private GraphicsDevice device;

        public Mirror(List<BoundingBox> boundingBoxes, GraphicsDevice device)
        {
            this.device = device;
            mirrorEffect = new BasicEffect(this.device);
            SetUpMirror(boundingBoxes);
        }

        private void SetUpMirror(List<BoundingBox> boundingBoxes)
        {
            Vector3 Origin = new Vector3((boundingBoxes[8].Min.X + boundingBoxes[8].Max.X) / 2, (boundingBoxes[8].Min.Y + boundingBoxes[8].Max.Y) / 2, (boundingBoxes[8].Min.Z + boundingBoxes[8].Max.Z) / 2);
            Origin.Z -= 0.015f;
            Origin.Y += 0.3f;
            Vector3 Up = Vector3.Up;
            float width = Math.Abs(boundingBoxes[8].Min.X - boundingBoxes[8].Max.X) - 0.01f;
            float height = Math.Abs(boundingBoxes[8].Min.Y - boundingBoxes[8].Max.Y);
            Vertices = new VertexPositionNormalTexture[4];
            Indexes = new short[6];


            Vector3 Left = Vector3.Cross(mirrorNormal, Up);
            Vector3 uppercenter = (Up * height / 2) + Origin;
            Vector3 UpperLeft = uppercenter + (Left * width / 2);
            Vector3 UpperRight = uppercenter - (Left * width / 2);
            Vector3 LowerLeft = UpperLeft - (Up * height);
            Vector3 LowerRight = UpperRight - (Up * height);

            Vector2 textureUpperLeft = new Vector2(1.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(0.0f, 0.0f);
            Vector2 textureLowerLeft = new Vector2(1.0f, 1.0f);
            Vector2 textureLowerRight = new Vector2(0.0f, 1.0f);
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = mirrorNormal;
            }

            Vertices[0].Position = LowerLeft;
            Vertices[0].TextureCoordinate = textureLowerLeft;
            Vertices[1].Position = UpperLeft;
            Vertices[1].TextureCoordinate = textureUpperLeft;
            Vertices[2].Position = LowerRight;
            Vertices[2].TextureCoordinate = textureLowerRight;
            Vertices[3].Position = UpperRight;
            Vertices[3].TextureCoordinate = textureUpperRight;

            Indexes[0] = 0;
            Indexes[1] = 1;
            Indexes[2] = 2;
            Indexes[3] = 2;
            Indexes[4] = 1;
            Indexes[5] = 3;
        }

        public void Draw(Texture2D rt, Matrix View, Matrix Projection)
        {
            device.BlendState = BlendState.Opaque;
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            mirrorEffect.World = Matrix.CreateRotationX(MathHelper.ToRadians(-5));
            mirrorEffect.View = View;
            mirrorEffect.Projection = Projection;
            mirrorEffect.TextureEnabled = true;
            mirrorEffect.Texture = rt;
            mirrorEffect.DiffuseColor = new Vector3(0.9f);

            VertexDeclaration vertexDeclaration = new VertexDeclaration(
                new VertexElement[]
                {
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                }
            );

            foreach (EffectPass pass in mirrorEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, Vertices, 0, 4, Indexes, 0, 2);
            }
        }
    }
    #endregion

    #region BoundingBoxManager
    class BoundingBoxManager
    {
        GraphicsDevice device;

        public BoundingBoxManager(Model room, GraphicsDevice device, Matrix[] bones, Matrix roomWorldMatrix)
        {
            this.device = device;
            boxEffect = new BasicEffect(this.device);

            int count = 0;
            foreach (ModelMesh mesh in room.Meshes)
            {
                if (mesh.BoundingSphere.Radius >= 1)
                {
                    if (count != 11 && count != 7 && count != 6 && count != 2)
                        boundingBoxes.Add(BuildBoundingBox(mesh, bones[mesh.ParentBone.Index] * roomWorldMatrix));
                    count++;
                }
            }
            boundingBoxes.Add(new BoundingBox(new Vector3(-5, -6, -3.5f), new Vector3(-1.8f, 16, -2.3f)));
        }

        private BoundingBox BuildBoundingBox(ModelMesh mesh, Matrix meshTransform)
        {
            Vector3 meshMax = new Vector3(float.MinValue);
            Vector3 meshMin = new Vector3(float.MaxValue);

            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                int stride = part.VertexBuffer.VertexDeclaration.VertexStride;

                VertexPositionNormalTexture[] vertexData = new VertexPositionNormalTexture[part.NumVertices];
                part.VertexBuffer.GetData(part.VertexOffset * stride, vertexData, 0, part.NumVertices, stride);

                Vector3 vertPosition = new Vector3();

                for (int i = 0; i < vertexData.Length; i++)
                {
                    vertPosition = vertexData[i].Position;

                    meshMin = Vector3.Min(meshMin, vertPosition);
                    meshMax = Vector3.Max(meshMax, vertPosition);
                }
            }

            meshMin = Vector3.Transform(meshMin, meshTransform);
            meshMax = Vector3.Transform(meshMax, meshTransform);

            BoundingBox box = new BoundingBox(meshMin, meshMax);
            return box;
        }


        short[] bBoxIndices = { 0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7 };
        BasicEffect boxEffect;
        public List<BoundingBox> boundingBoxes = new List<BoundingBox>();

        public void DrawBoundingBoxes(Matrix view, Matrix projection, Vector3 position, Vector3 characterOffset, bool isCrouched)
        {
            foreach (BoundingBox box in boundingBoxes)
            {
                Vector3[] corners = box.GetCorners();
                VertexPositionColor[] primitiveList = new VertexPositionColor[corners.Length];

                for (int i = 0; i < corners.Length; i++)
                    primitiveList[i] = new VertexPositionColor(corners[i], Color.White);

                boxEffect.World = Matrix.Identity;
                boxEffect.View = view;
                boxEffect.Projection = projection;
                boxEffect.TextureEnabled = false;
                boxEffect.LightingEnabled = false;

                foreach (EffectPass pass in boxEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    this.device.DrawUserIndexedPrimitives(
                        PrimitiveType.LineList, primitiveList, 0, 8, bBoxIndices, 0, 12);
                }
            }

            DrawCharacterBoundingBox(view, projection, position, characterOffset, isCrouched);
        }

        private void DrawCharacterBoundingBox(Matrix view, Matrix projection, Vector3 position, Vector3 characterOffset, bool isCrouched)
        {
            float characterCrounched_Y_Offset;
            if (isCrouched)
            {
                characterCrounched_Y_Offset = characterOffset.Y * 6 / 20;
            }
            else characterCrounched_Y_Offset = characterOffset.Y;


            Vector3 newOriginPoint = position;

            BoundingBox characterBox;
            if (isCrouched) characterBox = new BoundingBox(new Vector3(position.X - (characterOffset.X / 2), position.Y - characterOffset.Y, position.Z - (characterOffset.Z / 2)), new Vector3(position.X + (characterOffset.X / 2), position.Y - characterCrounched_Y_Offset + 0.14f, position.Z + (characterOffset.Z / 2)));
            else characterBox = new BoundingBox(new Vector3(position.X - (characterOffset.X / 2), position.Y - characterCrounched_Y_Offset, position.Z - (characterOffset.Z / 2)), new Vector3(position.X + (characterOffset.X / 2), position.Y + 0.14f, position.Z + (characterOffset.Z / 2)));


            Vector3[] cornerss = characterBox.GetCorners();
            VertexPositionColor[] primitiveListt = new VertexPositionColor[cornerss.Length];

            for (int i = 0; i < cornerss.Length; i++)
                primitiveListt[i] = new VertexPositionColor(cornerss[i], Color.Red);

            boxEffect.World = Matrix.Identity;
            boxEffect.View = view;
            boxEffect.Projection = projection;
            boxEffect.TextureEnabled = false;
            boxEffect.LightingEnabled = false;

            foreach (EffectPass pass in boxEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                this.device.DrawUserIndexedPrimitives(
                    PrimitiveType.LineList, primitiveListt, 0, 8,
                    bBoxIndices, 0, 12);
            }
        }
    }
    #endregion
}