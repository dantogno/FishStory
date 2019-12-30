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
        #region Fields/Properties

        float actionCollisionOffset;

        float fishingCollisionOffset;
        float fishingCollisionUnrotatedWidth;
        float fishingCollisionUnrotatedHeight;

        public IPressableInput TalkInput;
        public IPressableInput CancelInput;

        public NPC NpcForAction { get; set; }

        #endregion

        #region Initialize

        private void CustomInitialize()
        {
            actionCollisionOffset = ActivityCollision.RelativeX;

            fishingCollisionOffset = FishingCollision.RelativeX;
            fishingCollisionUnrotatedWidth = FishingCollision.Width ;
            fishingCollisionUnrotatedHeight = FishingCollision.Height;

            this.PossibleDirections = PossibleDirections.EightWay;

            this.AnimationControllerInstance.Layers.Add(mTopDownAnimationLayer);
        }

        partial void CustomInitializeTopDownInput()
        {
            if(InputDevice is Keyboard keyboard)
            {
                TalkInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Space);
                CancelInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Escape);
            }
            else if(InputDevice is Xbox360GamePad gamepad)
            {
                TalkInput = gamepad.GetButton(Xbox360GamePad.Button.A);
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Custom Activity

        private void CustomActivity()
        {
            UpdateActivityCollisionPosition();

        }

        private void UpdateActivityCollisionPosition()
        {
            var actionVector = this.DirectionFacing.ToVector() * actionCollisionOffset;
            this.ActivityCollision.RelativePosition = actionVector;

            var fishingVector = this.DirectionFacing.ToVector() * fishingCollisionOffset;

            this.FishingCollision.RelativePosition = fishingVector;

            if (DirectionFacing == TopDownDirection.Left ||
                DirectionFacing == TopDownDirection.Right)
            {
                FishingCollision.Width = fishingCollisionUnrotatedWidth;
                FishingCollision.Height = fishingCollisionUnrotatedHeight;
            }
            else
            {
                FishingCollision.Width = fishingCollisionUnrotatedHeight;
                FishingCollision.Height = fishingCollisionUnrotatedWidth;
            }
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
