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
using FishStory.Factories;
using FishStory.Entities;

namespace FishStory.Screens
{
    public partial class MainLevel
    {

        void CustomInitialize()
        {
            InitializeScript();

        }

        private void InitializeScript()
        {
            var If = script;
            var Do = script;

            If.Check(() => PlayerCharacterInstance.Y < -100);
            Do.Call(() =>
            {
                var npc = NPCFactory.CreateNew(20, -150);
                npc.TwineDialogId = nameof(GlobalContent.Dialog1);
                npc.Animation = NPC.Boy1;
            });

            //If.Check(() =>
            //{
            //    return PlayerDataManager.PlayerData.Has(ItemDefinition.Fishing_Rod) &&
            //        PlayerCharacterInstance.X < -100;
            //});

            //Do.Call(() =>
            //{
            //    PlayerDataManager.PlayerData.AwardItem(ItemDefinition.Low_Quality_Bait);
            //    FlatRedBall.Debugging.Debugger.CommandLineWrite("You got that worm!");

            //});



            //If.Check(() =>
            //{
            //    return PlayerDataManager.PlayerData.NpcRelationships["Dave"].EventsTriggered
            //        .Contains(5);
            //});
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
