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
using FishStory.Managers;
using FishStory.DataTypes;

namespace FishStory.Screens
{
    public partial class TestLevel
    {

        void CustomInitialize()
        {
            InitializeScript();

        }

        private void InitializeScript()
        {
            var If = script;
            var Do = script;

            If.Check(() => PlayerCharacterInstance.X > 100);
            Do.Call(() =>
            {
                PlayerDataManager.PlayerData.AwardItem(ItemDefinition.Fishing_Rod);
                FlatRedBall.Debugging.Debugger.CommandLineWrite("You got the fishing rod!");
            });

            If.Check(() =>
            {
                return PlayerDataManager.PlayerData.Has(ItemDefinition.Fishing_Rod) &&
                    PlayerCharacterInstance.X < -100;
            });

            Do.Call(() =>
            {
                PlayerDataManager.PlayerData.AwardItem(ItemDefinition.Low_Quality_Bait);
                FlatRedBall.Debugging.Debugger.CommandLineWrite("You got that worm!");

            });
        }

        void CustomActivity(bool firstTimeCalled)
        {


        }

        void CustomDestroy()
        {


        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

    }
}
