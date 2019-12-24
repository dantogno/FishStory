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

namespace FishStory.Screens
{
    public partial class GameScreen
    {
        protected ScreenScript<GameScreen> script;

        void CustomInitialize()
        {
            script = new ScreenScript<GameScreen>(this);

            DialogBox.Visible = false;
        }

        void CustomActivity(bool firstTimeCalled)
        {
            script.Activity();

            CameraActivity();
        }

        private void CameraActivity()
        {
            var difference = (PlayerCharacterInstance.Position - Camera.Main.Position).ToVector2();
            Camera.Main.Velocity = difference.ToVector3();
        }

        void CustomDestroy()
        {
            WaterCollision.RemoveFromManagers();
            SolidCollision.RemoveFromManagers();

        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

    }
}
