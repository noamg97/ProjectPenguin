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
    public class Camera
    {
        private GraphicsDevice device;

        public bool canJump;
        public bool canCrouch;
        public bool isSlow;

        Model Character;
        //Effect effect;
        //List<Vector3> OriginalDiffuseColors = new List<Vector3>();


        public Matrix view;
        public Matrix projection;
        public float YrotationSpeed;
        public float XrotatiobSpeed;
        public float yaw = 0;
        public float pitch = 0;
        public bool isCrouched = false;
        public Vector3 position = new Vector3(0, -0.4f, 0);
        public Vector3 characterOffset = new Vector3(1f, 1.79f, 1f);
        private float characterCrounched_Y_Offset;
        float cameraSpeed = 0.2f;
        private Int16 crouchCount = 0;

        public bool isJumping = false;
        public float jumpspeed = 0;

        public bool isAnimating = true;
        short animationCount = 0;
        List<DynamicAnimation> startAnimation = new List<DynamicAnimation>();
        TimeSpan TimeAfterAnimationOver = TimeSpan.Zero;


        public Camera(GraphicsDevice device, Model Character, Effect effect, bool canJump, bool canCrouch, bool isSlow)
        {
            this.device = device;
            characterCrounched_Y_Offset = characterOffset.Y / 2;
            this.canJump = canJump;
            this.canCrouch = canCrouch;
            this.isSlow = isSlow;
            this.Character = Character;
            //this.effect = effect;

            //foreach (ModelMesh mesh in Character.Meshes)
            //{
            //    foreach (BasicEffect bEffect in mesh.Effects)
            //        this.OriginalDiffuseColors.Add(bEffect.DiffuseColor);
            //    foreach (ModelMeshPart meshPart in mesh.MeshParts)
            //        meshPart.Effect = this.effect.Clone();
            //}


            float aspectRatio = (float)device.Viewport.Width / (float)device.Viewport.Height;
            projection = Matrix.CreatePerspectiveFieldOfView((float)((double)MathHelper.Pi / 3d), aspectRatio, 0.1f, 200000f);



            if (isSlow) cameraSpeed = 0.1f;

            if (!Game1.SkipAnimation)
            {
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.1f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.1f, -1.7f) }, new Vector2[] { new Vector2(3.11f, 1.2f) }, new Vector2[] { new Vector2(3.11f, 1.2f) }, TimeSpan.FromMilliseconds(10050), false));
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.1f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(3.11f, 1.2f) }, new Vector2[] { new Vector2(3.11f, 1.2f) }, TimeSpan.FromMilliseconds(500), false));
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(3.11f, 1.2f) }, new Vector2[] { new Vector2(4.66f, 1.0f) }, TimeSpan.FromMilliseconds(2500), false));
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(4.66f, 1.0f) }, new Vector2[] { new Vector2(4.66f, 0.28f) }, TimeSpan.FromMilliseconds(2300), false));
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(4.66f, 0.28f) }, new Vector2[] { new Vector2(4.66f, 0.076f) }, TimeSpan.FromMilliseconds(700), false));


                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(4.66f, 0.076f) }, new Vector2[] { new Vector2(4.66f, 0.076f) }, TimeSpan.FromMilliseconds(400), false));
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(4.66f, 0.076f) }, new Vector2[] { new Vector2(4.38f, 0.1f) }, TimeSpan.FromMilliseconds(1500), false));
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(4.38f, 0.1f) }, new Vector2[] { new Vector2(4.38f, 0.1f) }, TimeSpan.FromMilliseconds(1000), false));
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(4.38f, 0.1f) }, new Vector2[] { new Vector2(4.91f, 0.108f) }, TimeSpan.FromMilliseconds(1650), false));
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(4.91f, 0.108f) }, new Vector2[] { new Vector2(4.91f, 0.108f) }, TimeSpan.FromMilliseconds(1100), false));
                startAnimation.Add(new DynamicAnimation(new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector3[] { new Vector3(-2.7f, -1.0f, -1.7f) }, new Vector2[] { new Vector2(4.91f, 0.108f) }, new Vector2[] { new Vector2(4.91f, 0.106f) }, TimeSpan.FromMilliseconds(1000), false));
            }
            else isAnimating = false;
        }

        public void UpdateView()
        {
            Vector3 finalPosition = GetFinalPosition();

            Matrix cameraRotation = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = finalPosition + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            view = Matrix.CreateLookAt(finalPosition, cameraFinalTarget, cameraRotatedUpVector);
        }

        public void UpdatePosition(Vector3 moveVector, List<BoundingBox> boundingBoxes, GameTime gameTime, PlayingStateClass parent)
        {
            #region animation
            if (!Game1.SkipAnimation && !startAnimation[animationCount].hasFinished)
            {
                startAnimation[animationCount].Update(gameTime.ElapsedGameTime);
                yaw = startAnimation[animationCount].GetV2(0).X;
                pitch = startAnimation[animationCount].GetV2(0).Y;
                position = startAnimation[animationCount].GetV3(0);
                moveVector = Vector3.Zero;
            }
            else
                if (animationCount == startAnimation.Count - 1)
                    isAnimating = false;
                else
                    animationCount++;

            if (!isAnimating && TimeAfterAnimationOver.TotalMilliseconds != -100) TimeAfterAnimationOver += gameTime.ElapsedGameTime;
            if (TimeAfterAnimationOver.TotalMilliseconds >= 10)
            {
                position = new Vector3(-0.5f, 0, -0.2f);
                yaw = 0;
                pitch = -0.28f;
                TimeAfterAnimationOver = TimeSpan.FromMilliseconds(-100);
            }
            #endregion

            if (!isAnimating)
            {
                #region Bounderies
                moveVector.Y += jumpspeed / 40.0f;
                jumpspeed -= 0.3f;
                if (jumpspeed <= -4) jumpspeed = -4;

                if (!moveVector.Equals(Vector3.Zero))
                {
                    Matrix cameraRotation = Matrix.CreateRotationY(yaw);
                    Vector3 rotatedVector = Vector3.Transform(moveVector, cameraRotation);

                    if (isCrouched)
                    {
                        cameraSpeed /= 1.8f;
                        characterCrounched_Y_Offset = characterOffset.Y * crouchCount / 20;
                    }
                    //if (!isCrouched && Math.Abs(characterCrounched_Y_Offset / characterOffset.Y) >= 0.1f)
                    //    if (!Check_CanGetUp(boundingBoxes))
                    //        isCrouched = true;
                    if (!isCrouched) characterCrounched_Y_Offset = characterOffset.Y;

                    bool canX = true;
                    bool canY = true;
                    bool canZ = true;

                    BoundingBox characterBoxX;
                    BoundingBox characterBoxZ;
                    BoundingBox characterBoxY;

                    Vector3 newOriginPoint = position + new Vector3((cameraSpeed * rotatedVector.X), 0, 0);
                    if (isCrouched) characterBoxX = new BoundingBox(new Vector3(newOriginPoint.X - (characterOffset.X / 2), newOriginPoint.Y - characterOffset.Y, newOriginPoint.Z - (characterOffset.Z / 2)), new Vector3(newOriginPoint.X + (characterOffset.X / 2), newOriginPoint.Y - characterCrounched_Y_Offset + 0.14f, newOriginPoint.Z + (characterOffset.Z / 2)));
                    else characterBoxX = new BoundingBox(new Vector3(newOriginPoint.X - (characterOffset.X / 2), newOriginPoint.Y - characterCrounched_Y_Offset, newOriginPoint.Z - (characterOffset.Z / 2)), new Vector3(newOriginPoint.X + (characterOffset.X / 2), newOriginPoint.Y + 0.14f, newOriginPoint.Z + (characterOffset.Z / 2)));

                    newOriginPoint = position + new Vector3(0, 0, (cameraSpeed * rotatedVector.Z));
                    if (isCrouched) characterBoxZ = new BoundingBox(new Vector3(newOriginPoint.X - (characterOffset.X / 2), newOriginPoint.Y - characterOffset.Y, newOriginPoint.Z - (characterOffset.Z / 2)), new Vector3(newOriginPoint.X + (characterOffset.X / 2), newOriginPoint.Y - characterCrounched_Y_Offset + 0.14f, newOriginPoint.Z + (characterOffset.Z / 2)));
                    else characterBoxZ = new BoundingBox(new Vector3(newOriginPoint.X - (characterOffset.X / 2), newOriginPoint.Y - characterCrounched_Y_Offset, newOriginPoint.Z - (characterOffset.Z / 2)), new Vector3(newOriginPoint.X + (characterOffset.X / 2), newOriginPoint.Y + 0.14f, newOriginPoint.Z + (characterOffset.Z / 2)));

                    newOriginPoint = position + new Vector3(0, rotatedVector.Y, 0);
                    if (isCrouched) characterBoxY = new BoundingBox(new Vector3(newOriginPoint.X - (characterOffset.X / 2), newOriginPoint.Y - characterOffset.Y, newOriginPoint.Z - (characterOffset.Z / 2)), new Vector3(newOriginPoint.X + (characterOffset.X / 2), newOriginPoint.Y - characterCrounched_Y_Offset + 0.14f, newOriginPoint.Z + (characterOffset.Z / 2)));
                    else characterBoxY = new BoundingBox(new Vector3(newOriginPoint.X - (characterOffset.X / 2), newOriginPoint.Y - characterCrounched_Y_Offset, newOriginPoint.Z - (characterOffset.Z / 2)), new Vector3(newOriginPoint.X + (characterOffset.X / 2), newOriginPoint.Y + 0.14f, newOriginPoint.Z + (characterOffset.Z / 2)));

                    if (boundingBoxes[0].Contains(characterBoxX) != ContainmentType.Contains) canX = false;
                    if (boundingBoxes[0].Contains(characterBoxZ) != ContainmentType.Contains) canZ = false;
                    if (boundingBoxes[0].Contains(characterBoxY) != ContainmentType.Contains) canY = false;


                    for (int i = 1; i < boundingBoxes.Count; i++)
                    {
                        if (boundingBoxes[i].Contains(characterBoxX) != ContainmentType.Disjoint) canX = false;

                        if (boundingBoxes[i].Contains(characterBoxZ) != ContainmentType.Disjoint) canZ = false;

                        if (boundingBoxes[i].Contains(characterBoxY) != ContainmentType.Disjoint) canY = false;
                    }

                    if (canX) position.X += cameraSpeed * rotatedVector.X;
                    if (canZ) position.Z += cameraSpeed * rotatedVector.Z;
                    if (canY) position.Y += rotatedVector.Y;
                    else
                    {
                        if (rotatedVector.Y < 0 && isJumping)
                            if (jumpspeed >= 0) { jumpspeed = -0.01f; position.Y -= 0.01f; }
                            else isJumping = false;
                    }
                    if (isCrouched) cameraSpeed *= 1.8f;
                }
                #endregion
            }
        }

        public Vector3 GetFinalPosition()
        {
            Vector3 finalPosition = position;
            if (isCrouched && !isAnimating)
            {
                if (crouchCount < 7) crouchCount += 1;
                finalPosition.Y -= (characterOffset.Y * crouchCount / 20);
            }
            else crouchCount = 0;


            return finalPosition;
        }

        private bool Check_CanGetUp(List<BoundingBox> boundingBoxes)
        {
            Vector3 newOriginPoint = position;
            BoundingBox cBox = new BoundingBox(new Vector3(position.X - (characterOffset.X / 2), position.Y - characterCrounched_Y_Offset, position.Z - (characterOffset.Z / 2)), new Vector3(position.X + (characterOffset.X / 2), position.Y + 0.14f, position.Z + (characterOffset.Z / 2)));
            bool ret = true;


            ContainmentType doesCont = boundingBoxes[0].Contains(cBox);
            if (doesCont != ContainmentType.Contains) ret = false;

            for (int i = 1; i < boundingBoxes.Count; i++)
            {
                doesCont = boundingBoxes[i].Contains(cBox);
                if ((doesCont == ContainmentType.Contains || doesCont == ContainmentType.Intersects))
                    ret = false;
            }


            return ret;
        }

        //public void DrawCharacterY(Matrix viewMatrix)
        //{
        //    effect.CurrentTechnique = effect.Techniques["Colored"];

        //    Matrix[] transforms = new Matrix[Character.Bones.Count];
        //    Character.CopyAbsoluteBoneTransformsTo(transforms);

        //    Matrix world = Matrix.CreateScale(0.08f) * Matrix.CreateRotationY(MathHelper.ToRadians(180) + yaw) * Matrix.CreateTranslation(new Vector3(position.X, position.Y - characterOffset.Y, position.Z));

        //    device.RasterizerState = RasterizerState.CullNone;
        //    int effectCount = 0;
        //    foreach (ModelMesh mesh in Character.Meshes)
        //    {
        //        foreach (Effect currentEffect in mesh.Effects)
        //        {
        //            Matrix worldMatrix = transforms[mesh.ParentBone.Index] * world;
        //            currentEffect.Parameters["xWorldViewProjection"].SetValue(worldMatrix * viewMatrix* projection);
        //            currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
        //            currentEffect.Parameters["xLightPos"].SetValue(Room.lightPos);
        //            currentEffect.Parameters["xLightPower"].SetValue(Room.lightPower);
        //            currentEffect.Parameters["xAmbient"].SetValue(Room.ambientPower);
        //            currentEffect.Parameters["DiffuseColor"].SetValue(OriginalDiffuseColors[effectCount++]);
        //        }
        //        mesh.Draw();
        //    }
        //    device.RasterizerState = RasterizerState.CullCounterClockwise;
        //}

        //Draw Character With Basic Effect

        public void DrawCharacter(Matrix viewMatrix)
        {

            Matrix[] transforms = new Matrix[Character.Bones.Count];
            Character.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix CharacterWorld = Matrix.CreateScale(0.08f) * Matrix.CreateRotationY(MathHelper.ToRadians(180) + yaw) * Matrix.CreateTranslation(new Vector3(position.X, position.Y - characterOffset.Y, position.Z));

            device.RasterizerState = RasterizerState.CullNone;
            device.BlendState = BlendState.Opaque;
            foreach (ModelMesh mesh in Character.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * CharacterWorld;
                    effect.View = viewMatrix;
                    effect.Projection = projection;
                    effect.TextureEnabled = false;
                    effect.LightingEnabled = false;
                }
                mesh.Draw();
            }
            device.RasterizerState = RasterizerState.CullCounterClockwise;
        }
    }
}
