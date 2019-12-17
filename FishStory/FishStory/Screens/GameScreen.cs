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

namespace FishStory.Screens
{
    public partial class GameScreen
    {
        protected ScreenScript<GameScreen> script;

        void CustomInitialize()
        {
            script = new ScreenScript<GameScreen>(this);
        }

        void CustomActivity(bool firstTimeCalled)
        {
            script.Activity();
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
