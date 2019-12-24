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



        #endregion

        #region Initialize

        void CustomInitialize()
        {
            script = new ScreenScript<GameScreen>(this);

            DialogBox.Visible = false;

            InitializeCollision();
        }

        private void InitializeCollision()
        {
            PlayerCharacterInstanceActivityCollisionVsNPCListBodyCollision.CollisionOccurred +=
                HandlePlayerVsNpcActivityCollision;
        }

        private void HandlePlayerVsNpcActivityCollision(PlayerCharacter player, NPC npc)
        {
            player.NpcForAction = npc;
        }

        #endregion

        #region Activity

        void CustomActivity(bool firstTimeCalled)
        {
            script.Activity();

            CameraActivity();

            UiActivity();

            CollisionActivity();

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
                    DialogBox.Visible = true;
                    PlayerCharacterInstance.InputEnabled = false;
                }

            }
        }

        private void UiActivity()
        {
            if (DialogBox.Visible && PlayerCharacterInstance.TalkInput.WasJustPressed)
            {
                DialogBox.Visible = false;
            }
        }

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
