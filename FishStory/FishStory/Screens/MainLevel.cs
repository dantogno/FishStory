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
using FlatRedBall.Scripting;

namespace FishStory.Screens
{
    public partial class MainLevel
    {
 
        private int TotalFishIdentified
        {
            get
            {
                int total = 0;
                foreach (var item in ItemDefinition.FishNames)
                {
                    total += PlayerDataManager.PlayerData.TimesFishIdentified.Get(item);
                }
                return total;
            }
        }
        void CustomInitialize()
        {
            InitializeScript();
        }

        private void InitializeScript()
        {
            var If = script;
            var Do = script;


            PlayerCharacterInstance.DirectionFacing = TopDownDirection.Left;

            PlayerDataManager.PlayerData.AwardItem(ItemDefinition.Small_Brown_Fish);
            PlayerDataManager.PlayerData.AwardItem(ItemDefinition.Small_Brown_Fish);
            PlayerDataManager.PlayerData.AwardItem(ItemDefinition.Small_Brown_Fish);
            
            // TODO: remove this. Can use this to debug different days.
            // PlayerDataManager.PlayerData.CurrentDay = 2;

            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 1);
            Do.Call(() => DoDay1Script(If, Do));
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 2);
            Do.Call(() => DoDay2Script(If, Do));
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 3);
            Do.Call(() => DoDay3Script(If, Do));
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 4);
            Do.Call(() => DoDay4Script(If, Do));
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 5);
            Do.Call(() => DoDay5Script(If, Do));
        }

        private void DoDay1Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            GameScreenGum.InputInstructionsInstance.Visible = true;
            var secondsToShowInputCallout = 7;
            this
                .Call(() => GameScreenGum.InputInstructionsInstance.Visible = false)
                .After(secondsToShowInputCallout);

            InGameDateTimeManager.SetTimeOfDay(TimeSpan.FromHours(12));

            #region Farmer
            //var farmer = NPCList.FindByName("Farmer");
            //farmer.CurrentChainName = "FishRight";
            //farmer.Position = new Microsoft.Xna.Framework.Vector3(PlayerCharacterInstance.Position.X, PlayerCharacterInstance.Position.Y, PlayerCharacterInstance.Position.Z);

            #endregion
            #region Identifier
            If.Check(() => HasTag("HasSeenIdentifierDay1"));
            #endregion
            #region Tycoon
            // He gives you the key if you have identified 3 fish
            int numFishRequiredForKey = 3;
            If.Check(() => HasTag("HasTalkedToTycoonDay1"));
            Do.Call(() =>
            {
                NPCList.FindByName("Tycoon").TwineDialogId = "TycoonNoFishNoKey";
            });
            If.Check(() => HasTag("HasTalkedToTycoonDay1") && TotalFishIdentified >= numFishRequiredForKey);
            Do.Call(() =>
            {
                NPCList.FindByName("Tycoon").TwineDialogId = "TycoonYesFishNoKey";
            });
            If.Check(() => HasTag("GiveTrailerKey"));
            Do.Call(() =>
            {
                PlayerDataManager.PlayerData.AwardItem(ItemDefinition.Trailer_Key);
                AddNotification("Recieved: Trailer Key");
            });
            If.Check(() => PlayerDataManager.PlayerData.Has(ItemDefinition.Trailer_Key));
            Do.Call(() =>
            {
                NPCList.FindByName("Tycoon").TwineDialogId = "TycoonYesKey";
            });
            #endregion
            #region Mayor
            // Mayor
            // TODO: This is annoying during testing, but turn it back on eventually!
            //If.Check(() => !HasTag("HasSeenWelcomeDialog") && PlayerCharacterInstance.X < 1070 );
            //Do.Call(() =>
            //{
            //    if (DialogBox.TryShow("WelcomeDialog"))
            //    {
            //        PlayerCharacterInstance.ObjectsBlockingInput.Add(DialogBox);
            //    }
            //});
            If.Check(() => HasTag("HasSeenWelcomeDialog"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName("Mayor");
                npc.TwineDialogId = nameof(GlobalContent.MayorAfterWelcome);
                PlayerDataManager.PlayerData.AwardItem("Festival Badge");
                PlayerDataManager.PlayerData.AwardItem("Festival Pamphlet");
                // Magic numbers to save time here... this is referenced in dialog as well.
                PlayerDataManager.PlayerData.Money -= 5;
                AddNotification("-$5.");
                AddNotification("Recieved: Festival Badge");
                AddNotification("Recieved: Festival Pamphlet");
            });
            #endregion
            #region FestivalCoordinator
            // FestivalCoordinator
            If.Check(() => HasTag("AwardFishingRod"));
            Do.Call(() =>
            {
                PlayerDataManager.PlayerData.AwardItem(ItemDefinition.Fishing_Rod);
                AddNotification("Received: Fishing Rod");
            });
            // Festival Coordinator gives player random bait each day.
            If.Check(() => HasTag("AwardDay1Bait"));
            Do.Call(() =>
            {
                var npc = NPCList.FindByName("FestivalCoordinator");
                npc.TwineDialogId = nameof(GlobalContent.FestivalCoordinatorDay1Brief);
                AwardRandomBait();
            });
            #endregion
            //TODO: What are NPCRelationships??
            //If.Check(() =>
            //{
            //    return PlayerDataManager.PlayerData.NpcRelationships["Dave"].EventsTriggered
            //        .Contains(5);
            //});
        }

        private void DoDay2Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            #region Mayor
            #endregion
            #region FestivalCoordinator
            #endregion
            #region Identifier
            #endregion
            #region Fishmonger
            #endregion
            #region FarmerSonBaitShop
            #endregion
            #region YoungManBaitShop
            #region BlackMarketShop
            #endregion
            #endregion
            #region #ElderlyMother
            #endregion
            #region Priestess
            var npc = this.NPCList.FindByName("Priestess");
            npc.TwineDialogId = nameof(GlobalContent.PriestessDay2);
            #endregion
            #region Nun
            #endregion
            #region Farmer
            var farmer = NPCList.FindByName("Farmer");
            farmer.CurrentChainName = "FishLeft";
            farmer.Position = new Microsoft.Xna.Framework.Vector3(315, -595, PlayerCharacterInstance.Position.Z);

            #endregion
            #region Tycoon
            #endregion
            #region TycoonDaughter
            #endregion
            #region Conservationist
            #endregion
            #region FishermanBald
            #endregion
            #region FishermanHair
            #endregion
        }
        private void DoDay3Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            #region Mayor
            #endregion
            #region FestivalCoordinator
            #endregion
            #region Identifier
            #endregion
            #region Fishmonger
            #endregion
            #region FarmerSonBaitShop
            #endregion
            #region YoungManBaitShop
            #region BlackMarketShop
            #endregion
            #endregion
            #region #ElderlyMother
            #endregion
            #region Priestess
            #endregion
            #region Nun
            #endregion
            #region Farmer
            var farmer = NPCList.FindByName("Farmer");
            farmer.CurrentChainName = "FishRight";
            farmer.Position = new Microsoft.Xna.Framework.Vector3(1885, -665, PlayerCharacterInstance.Position.Z);

            #endregion
            #region Tycoon
            #endregion
            #region TycoonDaughter
            #endregion
            #region Conservationist
            #endregion
            #region FishermanBald
            #endregion
            #region FishermanHair
            #endregion
        }
        private void DoDay4Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            #region Mayor
            #endregion
            #region FestivalCoordinator
            #endregion
            #region Identifier
            #endregion
            #region Fishmonger
            #endregion
            #region FarmerSonBaitShop
            #endregion
            #region YoungManBaitShop
            #region BlackMarketShop
            #endregion
            #endregion
            #region #ElderlyMother
            #endregion
            #region Priestess
            #endregion
            #region Nun
            #endregion
            #region Farmer
            var farmer = NPCList.FindByName("Farmer");
            farmer.CurrentChainName = "OccasionallyLookUpAndWipeSweat";
            farmer.Position = new Microsoft.Xna.Framework.Vector3(1885, -665, PlayerCharacterInstance.Position.Z);

            #endregion
            #region Tycoon
            #endregion
            #region TycoonDaughter
            #endregion
            #region Conservationist
            #endregion
            #region FishermanBald
            #endregion
            #region FishermanHair
            #endregion
        }
        private void DoDay5Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            #region Mayor
            #endregion
            #region FestivalCoordinator
            #endregion
            #region Identifier
            #endregion
            #region Fishmonger
            #endregion
            #region FarmerSonBaitShop
            #endregion
            #region YoungManBaitShop
            #region BlackMarketShop
            #endregion
            #endregion
            #region #ElderlyMother
            #endregion
            #region Priestess
            #endregion
            #region Nun
            #endregion
            #region Farmer
            var farmer = NPCList.FindByName("Farmer");
            farmer.CurrentChainName = "LookGuilty";
            farmer.Position = new Microsoft.Xna.Framework.Vector3(955, -574, PlayerCharacterInstance.Position.Z);

            #endregion
            #region Tycoon
            #endregion
            #region TycoonDaughter
            #endregion
            #region Conservationist
            #endregion
            #region FishermanBald
            #endregion
            #region FishermanHair
            #endregion
        }

        private void AwardRandomBait()
        {
            int index = FlatRedBallServices.Random.Next(0, ItemDefinition.BaitNames.Length - 1);
            PlayerDataManager.PlayerData.AwardItem(ItemDefinition.BaitNames[index]);
            AddNotification($"Recieved: {ItemDefinition.BaitNames[index]}");
        }

        void CustomActivity(bool firstTimeCalled)
        {
            // TODO: we could make debug variables for these in glue
            FlatRedBall.Debugging.Debugger.Write($"Player X: {PlayerCharacterInstance.X}, Player Y: {PlayerCharacterInstance.Y}");
            //FlatRedBall.Debugging.Debugger.Write($"Fish identified: {TotalFishIdentified}");
        }

        void CustomDestroy()
        {


        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

    }
}
