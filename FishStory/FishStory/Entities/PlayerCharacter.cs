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

        public IPressableInput TalkInput;

        public NPC NpcForAction { get; set; }

        #region Initialize

        private void CustomInitialize()
        {
            collisionOffset = ActivityCollision.RelativeX;
            this.PossibleDirections = PossibleDirections.EightWay;
        }

        partial void CustomInitializeTopDownInput()
        {
            if(InputDevice is Keyboard keyboard)
            {
                TalkInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Space);
            }
            else if(InputDevice is Xbox360GamePad gamepad)
            {
                TalkInput = gamepad.GetButton(Xbox360GamePad.Button.A);
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
