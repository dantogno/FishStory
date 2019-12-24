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

namespace FishStory.Entities
{
    public partial class PlayerCharacter
    {
        float collisionOffset;

        #region Initialize

        private void CustomInitialize()
        {
            collisionOffset = ActivityCollision.RelativeX;
            this.PossibleDirections = PossibleDirections.EightWay;
        }

        #endregion

        #region Custom Activity

        private void CustomActivity()
        {
            UpdateActivityCollisionPosition();

        }

        private void UpdateActivityCollisionPosition()
        {
            var vector = this.DirectionFacing.ToVector() * collisionOffset;

            this.ActivityCollision.RelativePosition = vector;

        }

        #endregion

        private void CustomDestroy()
        {


        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
    }
}
