using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Graphics;

namespace FishStory.Entities
{
    public partial class IntroFish
    {
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        private void CustomInitialize()
        {
            //this.RelativeZ += 1;
            
            ShadowSpriteInstance.RelativeY += 72;
            //ShadowSpriteInstance.RelativeZ = -1;

            SwimAtRandom();
        }

        private double lastTimeChangedDirection = 3f;
        private void CustomActivity()
        {
            if (lastTimeChangedDirection <= 0)
            {
                SwimAtRandom();
                lastTimeChangedDirection = FlatRedBallServices.Random.Next(2, 6);
            }
            else if (IsOnScreen() == false)
            {
                SwimTowardsScreen();
            }
            else
            {
                lastTimeChangedDirection -= TimeManager.SecondDifference;
            }
        }

        public bool IsOnScreen()
        {
            var camera = Camera.Main;
            var isOffScreen = FishSpriteInstance.X > camera.X + camera.OrthogonalWidth / 2 + FishSpriteInstance.Width / 2 ||
                FishSpriteInstance.X < camera.X - camera.OrthogonalWidth / 2 - FishSpriteInstance.Width / 2 ||
                FishSpriteInstance.Y > camera.Y + camera.OrthogonalHeight / 2 + FishSpriteInstance.Height / 2 ||
                FishSpriteInstance.Y < camera.Y - camera.OrthogonalHeight / 2 - FishSpriteInstance.Height / 2;
            return !isOffScreen;
        }

        private void SwimTowardsScreen()
        {
            var camera = Camera.Main;
            if (FishSpriteInstance.X > camera.X + camera.OrthogonalWidth / 2 + FishSpriteInstance.Width / 2)
            {
                ChangeSwimDirection(SwimDirection.Left);
            }
            else if (FishSpriteInstance.X < camera.X - camera.OrthogonalWidth / 2 - FishSpriteInstance.Width / 2)
            {
                ChangeSwimDirection(SwimDirection.Right);
            }
            else if (FishSpriteInstance.Y > camera.Y + camera.OrthogonalHeight / 2 + FishSpriteInstance.Height / 2)
            {
                ChangeSwimDirection(SwimDirection.Down);
            }
            else if (FishSpriteInstance.Y < camera.Y - camera.OrthogonalHeight / 2 - FishSpriteInstance.Height / 2)
            {
                ChangeSwimDirection(SwimDirection.Up);
            }
            lastTimeChangedDirection = FlatRedBallServices.Random.Next(5, 8);
        }

        private void SwimAtRandom()
        {
            var randomDirection = (int)FlatRedBallServices.Random.Next(0, 5);
            var swimDirection = SwimDirection.Down;
            switch (randomDirection)
            {
                case 0: swimDirection = SwimDirection.Down; break;
                case 1: swimDirection = SwimDirection.Up; break;
                case 2: swimDirection = SwimDirection.Left; break;
                case 3: swimDirection = SwimDirection.Right; break;
                default: swimDirection = SwimDirection.Down; break;
            }
            ChangeSwimDirection(swimDirection);
            Drag = 0.2f;
        }

        private void ChangeSwimDirection(SwimDirection direction)
        {
            CurrentSwimDirectionState = direction;
            if (CurrentSwimDirectionState == SwimDirection.Down)
            {
                this.Acceleration = new Microsoft.Xna.Framework.Vector3(0, -15, 0);
            }
            else if (CurrentSwimDirectionState == SwimDirection.Up)
            {
                this.Acceleration = new Microsoft.Xna.Framework.Vector3(0, 15, 0); 
            }
            else if (CurrentSwimDirectionState == SwimDirection.Left)
            {
                this.Acceleration = new Microsoft.Xna.Framework.Vector3(-15, 0, 0);
            }
            else if (CurrentSwimDirectionState == SwimDirection.Right)
            {
                this.Acceleration = new Microsoft.Xna.Framework.Vector3(15, 0, 0); 
            }        
        }

        private void CustomDestroy()
        {


        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        public void SetLayers(Layer lightEffectLayer)
        {
            //FishLightSpriteInstance.TextureScale = NormalizedLightRadius;
            
            SpriteManager.AddToLayer(FishLightSpriteInstance, lightEffectLayer);
        }
    }
}
