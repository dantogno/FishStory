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
            this.RelativeZ += 1;
            
            ShadowSpriteInstance.RelativeY += 48;
            ShadowSpriteInstance.RelativeZ = -1;
        }

        private double lastTimeChangedDirection = 3f;
        private void CustomActivity()
        {
            if (lastTimeChangedDirection <= 0)
            {
                SwimAtRandom();
                lastTimeChangedDirection = FlatRedBallServices.Random.Next(5, 8);
            }
            else
            {
                lastTimeChangedDirection -= TimeManager.SecondDifference;
            }

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
        }

        private void ChangeSwimDirection(SwimDirection direction)
        {
            CurrentSwimDirectionState = direction;
            if (CurrentSwimDirectionState == SwimDirection.Down)
            {
                this.Velocity = new Microsoft.Xna.Framework.Vector3(0, -10, 0);
            }
            else if (CurrentSwimDirectionState == SwimDirection.Up)
            {
                this.Velocity = new Microsoft.Xna.Framework.Vector3(0, 10, 0); 
            }
            else if (CurrentSwimDirectionState == SwimDirection.Left)
            {
                this.Velocity = new Microsoft.Xna.Framework.Vector3(-10, 0, 0);
            }
            else if (CurrentSwimDirectionState == SwimDirection.Right)
            {
                this.Velocity = new Microsoft.Xna.Framework.Vector3(10, 0, 0); 
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
