using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Localization;
using FlatRedBall.Scripting;
using Microsoft.Xna.Framework;
using FishStory.Entities;

namespace FishStory.Screens
{
    public partial class GameScreen
    {
        #region Fields/Properties

        protected ScreenScript<GameScreen> script;

        List<string> dialogTagsThisFrame = new List<string>();

        #endregion

        #region Initialize

        void CustomInitialize()
        {
            script = new ScreenScript<GameScreen>(this);

            DialogBox.Visible = false;

            InitializeCollision();

            InitializeUi();
        }

        private void InitializeCollision()
        {
            PlayerCharacterInstanceActivityCollisionVsNPCListBodyCollision.CollisionOccurred +=
                HandlePlayerVsNpcActivityCollision;
        }

        private void InitializeUi()
        {
            if(PlayerCharacterInstance.InputDevice is Keyboard keyboard)
            {
                DialogBox.UpInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Up)
                    .Or(keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.W));

                DialogBox.DownInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Down)
                    .Or(keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.S));
            }
            else
            {
                throw new NotImplementedException();
            }

            DialogBox.SelectInput = PlayerCharacterInstance.TalkInput;

            DialogBox.AfterHide += HandleDialogBoxHide;

            DialogBox.DialogTagShown += HandleDialogTagShown;
        }

        private void HandlePlayerVsNpcActivityCollision(PlayerCharacter player, NPC npc)
        {
            player.NpcForAction = npc;
        }

        #endregion

        #region Activity

        void CustomActivity(bool firstTimeCalled)
        {
            dialogTagsThisFrame.Clear();

            CameraActivity();

            UiActivity();

            CollisionActivity();

            // do script *after* the UI
            script.Activity();
        }

        void CameraActivity()
        {
            var difference = (PlayerCharacterInstance.Position - Camera.Main.Position).ToVector2();
            Camera.Main.Velocity = difference.ToVector3();
        }

        private void CollisionActivity()
        {
            if(PlayerCharacterInstance.TalkInput?.WasJustPressed == true && DialogBox.Visible == false)
            {
                PlayerCharacterInstance.NpcForAction = null;

                PlayerCharacterInstanceActivityCollisionVsNPCListBodyCollision.DoCollisions();

                if(PlayerCharacterInstance.NpcForAction != null)
                {
                    if(DialogBox.TryShow(nameof(GlobalContent.Dialog1)))
                    {
                        PlayerCharacterInstance.InputEnabled = false;
                    }
                }

            }
        }

        private void UiActivity()
        {

            DialogBox.CustomActivity();
        }

        private void HandleDialogBoxHide()
        {
            PlayerCharacterInstance.InputEnabled = true;
        }

        private void HandleDialogTagShown(string tag)
        {
            dialogTagsThisFrame.Add(tag);
        }

        #endregion

        #region Script-helping methods

        public bool HasTag(string tag) =>
            dialogTagsThisFrame.Contains(tag);

        #endregion

        #region Destroy

        void CustomDestroy()
        {
            WaterCollision.RemoveFromManagers();
            SolidCollision.RemoveFromManagers();

        }

        #endregion

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

    }
}
