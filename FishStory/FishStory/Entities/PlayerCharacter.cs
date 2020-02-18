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
using FishStory.Managers;
using FlatRedBall.Graphics;

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
        public IPressableInput InventoryInput;

        public NPC NpcForAction { get; set; }

        public bool IsFishing => this.CurrentMovement?.Name == "Fishing";

        double? nextFishTime;
        public bool IsFishOnLine { get; private set; }
        bool hasShownExclamation = false;

        public double LastTimeFishingStarted { get; set; }

        public List<object> ObjectsBlockingInput { get; private set; } = new List<object>();

        public event Action FishLost;

        public string CurrentBait { get; private set; }

        public PropObject Lantern => PropObjectLanternInstance;

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

            this.PropObjectLanternInstance.CurrentPropNameState = PropObject.PropName.Lantern;
        }

        partial void CustomInitializeTopDownInput()
        {
            if(InputDevice is Keyboard keyboard)
            {
                TalkInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Space);
                CancelInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Escape);
                InventoryInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.I);
            }
            else if(InputDevice is Xbox360GamePad gamepad)
            {
                TalkInput = gamepad.GetButton(Xbox360GamePad.Button.A);
                CancelInput = gamepad.GetButton(Xbox360GamePad.Button.B);
                InventoryInput = gamepad.GetButton(Xbox360GamePad.Button.X);
            }
        }

        public void MoveDisplayElementsToUiLayer(Layer uiLayer)
        {
            ExclamationIconInstance.MoveToLayer(uiLayer);
        }

        #endregion

        #region Custom Activity

        private void CustomActivity()
        {
            UpdateActivityCollisionPosition();

            InputEnabled = ObjectsBlockingInput.Count == 0;

            if(IsFishing)
            {
                DoFishingActivity();
            }

        }

        private void DoFishingActivity()
        {
            if(TimeManager.CurrentScreenTime > nextFishTime && hasShownExclamation == false)
            {
                PerformFishIsAvailableLogic();
            }
        }

        private void PerformFishIsAvailableLogic()
        {
            hasShownExclamation = true;
            ExclamationIconInstance.CurrentIconDisplayState = EmotiveIcon.IconDisplay.Exclamation;
            ExclamationIconInstance.Visible = true;
            ExclamationIconInstance.BeginAnimations();

            IsFishOnLine = true;
            SoundManager.Play(GlobalContent.FishOnLineSound);
            this.Call(StopFishAvailable)
                .After(TimeFishExclamationShows);
        }

        private void StopFishAvailable()
        {
            if(IsFishOnLine)
            {
                ExclamationIconInstance.Visible = false;
                IsFishOnLine = false;

                hasShownExclamation = false;
                StopFishing();
                FishLost?.Invoke();

                ExclamationIconInstance.CurrentIconDisplayState = EmotiveIcon.IconDisplay.Explitive;
                ExclamationIconInstance.Visible = true;
                ExclamationIconInstance.BeginAnimations(shouldHideAfter: true);
            }

        }

        private void UpdateActivityCollisionPosition()
        {
            var actionVector = this.DirectionFacing.ToVector() * actionCollisionOffset;
            this.ActivityCollision.RelativePosition = actionVector;

            var fishingVector = this.DirectionFacing.ToVector() * fishingCollisionOffset;

            this.FishingCollision.RelativePosition = fishingVector;

            if (this.CurrentAnimationChainName.Contains("Left") ||
                this.CurrentAnimationChainName.Contains("Right"))
            {
                FishingCollision.Width = fishingCollisionUnrotatedWidth;a
                FishingCollision.Height = fishingCollisionUnrotatedHeight;
            }
            else
            {
                FishingCollision.Width = fishingCollisionUnrotatedHeight;
                FishingCollision.Height = fishingCollisionUnrotatedWidth;
            }
        }

        public void StartFishing(string baitType)
        {
            this.CurrentBait = baitType;
            this.CurrentMovement = TopDownValues["Fishing"];
            LastTimeFishingStarted = TimeManager.CurrentScreenTime;
            SetNextFishTime();
        }

        private void SetNextFishTime()
        {
            const float minTimeForFish = 2f;
            const float maxTimeForFish = 12;

            var randomTime = FlatRedBallServices.Random.Between(minTimeForFish, maxTimeForFish);

            nextFishTime = TimeManager.CurrentScreenTime + randomTime;
            hasShownExclamation = false;
            ExclamationIconInstance.Visible = false;
        }

        public void StopFishing()
        {
            this.CurrentMovement = TopDownValues["Default"];
            nextFishTime = null;
            IsFishOnLine = false;
            ExclamationIconInstance.Visible = false;
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
