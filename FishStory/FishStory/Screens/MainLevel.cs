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
    public static class CharacterNames
    {
        public const string BlackMarketShop = "BlackMarketShop";
        public const string Mayor = "Mayor";
        public const string FestivalCoordinator = "FestivalCoordinator";
        public const string Identifier = "Identifier";
        public const string Fishmonger = "Fishmonger";
        public const string FarmerSonBaitShop = "FarmerSonBaitShop";
        public const string YoungManBaitShop = "YoungManBaitShop";
        public const string ElderlyMother = "ElderlyMother";
        public const string Priestess = "Priestess";
        public const string Nun = "Nun";
        public const string Farmer = "Farmer";
        public const string Tycoon = "Tycoon";
        public const string TycoonDaughter = "TycoonDaughter";
        public const string Conservationist = "Conservationist";
        public const string FishermanBald = "FishermanBald";
        public const string FishermanHair = "FishermanHair";
    }
    public partial class MainLevel
    {
        /// <summary>
        /// Tycoon requires this many fish before giving key.
        /// </summary>
        private int numFishRequiredForKey = 3;
        bool isWaitingToGiveFreeBaitInConversation = false;
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
        /// <summary>
        /// Calculate how many identified fish are associated with each character.
        /// </summary>
        /// <returns>Dictionary with character names as keys and number of fish as values.</returns>
        private Dictionary<string, int> GetNumberOfFishAssociatedWithCharacters()
        {
            Dictionary<string, int> numberOfFishAssociatedWithCharacters
                = new Dictionary<string, int>();

            foreach (var fishName in ItemDefinition.FishNames)
            {
                for (int i = 0; i < PlayerDataManager.PlayerData.TimesFishIdentified.Get(fishName); i++)
                {
                    foreach (var characterName in GlobalContent.ItemDefinition[fishName].AssociatedCharacters)
                    {
                        if (numberOfFishAssociatedWithCharacters.ContainsKey(characterName))
                        {
                            numberOfFishAssociatedWithCharacters[characterName]++;
                        }
                        else
                        {
                            numberOfFishAssociatedWithCharacters.Add(characterName, 0);
                        }
                    }
                }
            }
            return numberOfFishAssociatedWithCharacters;
        }
        /// <summary>
        /// Take a dictionary of string keys and int values and return the key with the highest value.
        /// </summary>
        private string GetKeyWithHighestValue(Dictionary<string, int> dictionary)
        {
            return dictionary.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        }

        /// <summary>
        /// Returns a list of string keys with the top int values in a Dictionary.
        /// For use when determining the characters to use alternative dialog in day 2.
        /// </summary>
        /// <param name="numberOfKeysToReturn">How many keys (characters to use alt dialog) in the list returned.</param>
        private List<string> GetKeysWithTopValues(Dictionary<string, int> dictionary, int numberOfKeysToReturn)
        {
            var list = dictionary.ToList();
            list.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            var keys = list.Select((kvp) => kvp.Key);
            return keys.Take(numberOfKeysToReturn).ToList();
        }

        /// <summary>
        /// These characters represen specific traits / classes. 
        /// On day 2, if this class is threatened based on the number and type
        /// of fish caught, these characters will have alternative dialog.
        /// </summary>
        private Dictionary<string, string> classRepresentatives = new Dictionary<string, string>()
        {
            // male
            {ItemDefinition.King_Mackerel, "Fishmonger"},
            // female
            {ItemDefinition.Ladyfish, "Mayor" },
            // old
            { ItemDefinition.Rougheye_Rockfish, "ElderlyMother" },
            // young
            { ItemDefinition.Shrimp, "Farmer" },
            // bald
            { ItemDefinition.Monkfish, "Identifier" },
            // facial hair
            { ItemDefinition.Goatfish, "Tycoon" },
            // mother
            { ItemDefinition.Giant_Octopus, "Nun" },
            // father
            { ItemDefinition.Seahorse, "TycoonDaughter" },
            // blonde hair
            {ItemDefinition.Trumpetfish, "YoungManBaitShop"  },
            // dark hair
            {ItemDefinition.Cobia, "FestivalCoordinator" },
            // brown hair
            {ItemDefinition.Brown_Rockfish, "BlackMarketShop" }
        };

        private bool DoesPlayerHaveNoBaitAndNoMoneyAndNoFish =>
            PlayerDataManager.PlayerData.Money < GlobalContent.ItemDefinition[ItemDefinition.Blood_Worm].PlayerBuyingCost
                && !DoesPlayerHaveBait && !DoesPlayerHaveFish;

        private bool DoesPlayerHaveBait => PlayerDataManager.PlayerData.ItemInventory
                    .Where((kvp) => GlobalContent.ItemDefinition[kvp.Key].IsBait && kvp.Value > 0).Any();

        private bool DoesPlayerHaveFish => PlayerDataManager.PlayerData.ItemInventory
                    .Where((kvp) => GlobalContent.ItemDefinition[kvp.Key].IsFish && kvp.Value > 0).Any();


        private void EveryFrameScriptLogic()
        {
            
            switch (PlayerDataManager.PlayerData.CurrentDay)
            {
                case 1:
                    bool doesPlayerNeedFreeBait = !PlayerDataManager.PlayerData.Has(ItemDefinition.Trailer_Key)
                        && DoesPlayerHaveNoBaitAndNoMoneyAndNoFish && TotalFishIdentified < numFishRequiredForKey
                        && HasTag("AwardDay1Bait") && !PlayerCharacterInstance.IsFishing;
                    if (doesPlayerNeedFreeBait && !isWaitingToGiveFreeBaitInConversation)
                    {
                        NPCList.FindByName(CharacterNames.FestivalCoordinator).TwineDialogId = nameof(GlobalContent.FestivalCoordinatorNobaitNoMoney);
                        // the dialog that gives the free bait has this tag.
                        RemoveTag("AwardFreebieBait");
                        isWaitingToGiveFreeBaitInConversation = true;
                    }
                    if (isWaitingToGiveFreeBaitInConversation)
                    {
                        if (HasTag("AwardFreebieBait"))
                        {
                            AwardRandomBait();
                            isWaitingToGiveFreeBaitInConversation = false;
                            NPCList.FindByName(CharacterNames.FestivalCoordinator).TwineDialogId = nameof(GlobalContent.FestivalCoordinatorDay1Brief);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private void InitializeScript()
        {
            var If = script;
            var Do = script;
                
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 1);
            Do.Call(() => DoDay1Script(If, Do));
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 2);
            Do.Call(() => DoDay2Script(If, Do));
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 3);
            Do.Call(() => DoDay3Script(If, Do));
        }

        private void HandleDay2TraitAlternateDialogForClassRepresentatives(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            int numberOfClassRepresentativesToUseAltDay2Dialog = 4;
            var mostIdentifiedFish = GetKeysWithTopValues(PlayerDataManager.PlayerData.TimesFishIdentified, numberOfClassRepresentativesToUseAltDay2Dialog);

            If.Check(()=> { return true; });
            foreach (var item in mostIdentifiedFish)
            {
                Do.Call(() =>
                {
                    NPCList.FindByName(classRepresentatives[item]).TwineDialogId = classRepresentatives[item] + "Day2AltTrait";
                });
            }
        }

        private void DoDay1Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            PlayerCharacterInstance.DirectionFacing = TopDownDirection.Left;
            GameScreenGum.InputInstructionsInstance.Visible = true;
            var secondsToShowInputCallout = 7;
            this
                .Call(() => GameScreenGum.InputInstructionsInstance.Visible = false)
                .After(secondsToShowInputCallout);

            InGameDateTimeManager.SetTimeOfDay(TimeSpan.FromHours(12));

            #region Priestess
            If.Check(() => HasTag("HasSeenPriestessDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Priestess);
                npc.TwineDialogId = nameof(GlobalContent.PriestessDay1Brief);
            });
            #endregion
            #region ElderlyMother
            If.Check(() => HasTag("HasSeenElderlyMotherDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.ElderlyMother);
                npc.TwineDialogId = nameof(GlobalContent.ElderlyMotherDay1Brief);
            });
            #endregion
            #region Nun
            If.Check(() => HasTag("HasSeenNunDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Nun);
                npc.TwineDialogId = nameof(GlobalContent.NunDay1Brief);
            });
            #endregion
            #region Farmer
            //var farmer = NPCList.FindByName("Farmer");
            //farmer.CurrentChainName = "FishRight";
            //farmer.Position = new Microsoft.Xna.Framework.Vector3(PlayerCharacterInstance.Position.X, PlayerCharacterInstance.Position.Y, PlayerCharacterInstance.Position.Z);

            If.Check(() => HasTag("HasSeenFarmerDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Farmer);
                npc.TwineDialogId = nameof(GlobalContent.FarmerDay1Brief);
            });

            #endregion
            #region Identifier
            If.Check(() => HasTag("HasSeenIdentifierDay1"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.Identifier).TwineDialogId = nameof(GlobalContent.IdentifierDay1Brief);
            });
            #endregion
            #region Fishmonger
            If.Check(() => HasTag("HasTalkedToFishMongerDay1"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.Fishmonger).TwineDialogId = nameof(GlobalContent.FishMongerDay1Brief);
            });
            #endregion
            #region Tycoon
            If.Check(() => HasTag("HasTalkedToTycoonDay1"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.Tycoon).TwineDialogId = "TycoonNoFishNoKey";
            });
            If.Check(() => HasTag("HasTalkedToTycoonDay1") && TotalFishIdentified >= numFishRequiredForKey);
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.Tycoon).TwineDialogId = "TycoonYesFishNoKey";
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
                NPCList.FindByName(CharacterNames.Tycoon).TwineDialogId = "TycoonYesKey";
            });
            #endregion
            #region TycoonDaughter
            If.Check(() => HasTag("HasSeenTycoonDaughterDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
                npc.TwineDialogId = nameof(GlobalContent.TycoonDaughterDay1Brief);
            });
            #endregion
            #region Conservationist
            If.Check(() => HasTag("HasSeenConservationistDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Conservationist);
                npc.TwineDialogId = nameof(GlobalContent.ConservationistDay1Brief);
            });
            #endregion
            #region FishermanBald
            If.Check(() => HasTag("HasSeenFishermanBaldDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanBald);
                npc.TwineDialogId = nameof(GlobalContent.FishermanBaldDay1Brief);
            });
            #endregion
            #region FishermanHair
            If.Check(() => HasTag("HasSeenFishermanHairDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanHair);
                npc.TwineDialogId = nameof(GlobalContent.FishermanHairDay1Brief);
            });
            #endregion
            #region Mayor
            // Mayor
            // TODO: This is annoying during testing, but turn it back on eventually!
            If.Check(() => !HasTag("HasSeenWelcomeDialog") && PlayerCharacterInstance.X < 1525);
            Do.Call(() =>
            {
                if (DialogBox.TryShow("WelcomeDialog"))
                {
                    PlayerCharacterInstance.ObjectsBlockingInput.Add(DialogBox);
                }
            });
            If.Check(() => HasTag("HasSeenWelcomeDialog"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Mayor);
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
                var npc = NPCList.FindByName(CharacterNames.FestivalCoordinator);
                npc.TwineDialogId = nameof(GlobalContent.FestivalCoordinatorDay1Brief);
                AwardRandomBait();
            });
            #endregion
            #region BlackMarketShop
            If.Check(() =>
            {
                return HasTag("HasSeenBlackMarketShopDay1");
            });
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.BlackMarketShop).TwineDialogId = nameof(GlobalContent.BlackMarketShopDay1Brief);
            });
            #endregion
            #region YoungManBaitShop  
            If.Check(() => HasTag("HasSeenFancyBaitShopDialog"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.YoungManBaitShop).TwineDialogId = nameof(GlobalContent.FancyBaitShopDay1Brief);
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
            var mayor = this.NPCList.FindByName(CharacterNames.Mayor);
            mayor.TwineDialogId = nameof(GlobalContent.MayorDay2);

            If.Check(() => HasTag("HasSeenMayorDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Mayor);
                npc.TwineDialogId = nameof(GlobalContent.MayorDay2Brief);
            });
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
            #endregion
            #region BlackMarketShop
            NPCList.FindByName(CharacterNames.BlackMarketShop).TwineDialogId = nameof(GlobalContent.BlackMarketShopDay2);
            If.Check(() =>
            {
                return HasTag("HasSeenBlackMarketShopDay2");
            });
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.BlackMarketShop).TwineDialogId = nameof(GlobalContent.BlackMarketShopDay2Brief);
            });
            #endregion
            #region ElderlyMother
            var elderlyMother = this.NPCList.FindByName(CharacterNames.ElderlyMother);
            elderlyMother.TwineDialogId = nameof(GlobalContent.ElderlyMotherDay2);

            If.Check(() => HasTag("HasSeenElderlyMotherDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.ElderlyMother);
                npc.TwineDialogId = nameof(GlobalContent.ElderlyMotherDay2Brief);
            });
            #endregion
            #region Priestess
            var priestess = this.NPCList.FindByName(CharacterNames.Priestess);
            priestess.TwineDialogId = nameof(GlobalContent.PriestessDay2);

            If.Check(() => HasTag("HasSeenPriestessDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Priestess);
                npc.TwineDialogId = nameof(GlobalContent.PriestessDay2Brief);
            });
            #endregion
            #region Nun
            var nun = this.NPCList.FindByName(CharacterNames.Nun);
            nun.TwineDialogId = nameof(GlobalContent.NunDay2);

            If.Check(() => HasTag("HasSeenNunDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Nun);
                npc.TwineDialogId = nameof(GlobalContent.NunDay2Brief);
            });
            #endregion
            #region Farmer
            var farmer = NPCList.FindByName(CharacterNames.Farmer);
            farmer.CurrentChainName = "FishLeft";
            farmer.Position = new Microsoft.Xna.Framework.Vector3(315, -595, PlayerCharacterInstance.Position.Z);
            farmer.TwineDialogId = nameof(GlobalContent.FarmerDay2);

            If.Check(() => HasTag("HasSeenFarmerDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Farmer);
                npc.TwineDialogId = nameof(GlobalContent.FarmerDay2Brief);
            });

            #endregion
            #region Tycoon
            var tycoon = this.NPCList.FindByName(CharacterNames.Tycoon);
            tycoon.TwineDialogId = nameof(GlobalContent.TycoonDay2);

            If.Check(() => HasTag("HasSeenTycoonDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Tycoon);
                npc.TwineDialogId = nameof(GlobalContent.TycoonDay2Brief);
            });
            #endregion
            #region TycoonDaughter
            var tycoonDaughter = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
            tycoonDaughter.Position = new Microsoft.Xna.Framework.Vector3(715, -340, PlayerCharacterInstance.Position.Z);
            tycoonDaughter.TwineDialogId = nameof(GlobalContent.TycoonDaughterDay2);

            If.Check(() => HasTag("HasSeenTycoonDaughterDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
                npc.TwineDialogId = nameof(GlobalContent.TycoonDaughterDay2Brief);
            });
            #endregion
            #region Conservationist
            var conservationist = this.NPCList.FindByName(CharacterNames.Conservationist);
            conservationist.TwineDialogId = nameof(GlobalContent.ConservationistDay2);

            If.Check(() => HasTag("HasSeenConservationistDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Conservationist);
                npc.TwineDialogId = nameof(GlobalContent.ConservationistDay2Brief);
            });
            #endregion
            #region FishermanBald
            var baldFisher = this.NPCList.FindByName(CharacterNames.FishermanBald);
            baldFisher.TwineDialogId = nameof(GlobalContent.FishermanBaldDay2);

            If.Check(() => HasTag("HasSeenFishermanBaldDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanBald);
                npc.TwineDialogId = nameof(GlobalContent.FishermanBaldDay2Brief);
            });
            #endregion
            #region FishermanHair
            var hairFisher = this.NPCList.FindByName(CharacterNames.FishermanHair);
            hairFisher.TwineDialogId = nameof(GlobalContent.FishermanHairDay2);

            If.Check(() => HasTag("HasSeenFishermanHairDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanHair);
                npc.TwineDialogId = nameof(GlobalContent.FishermanHairDay2Brief);
            });
            #endregion
            HandleDay2TraitAlternateDialogForClassRepresentatives(If, Do);
        }
        private void DoDay3Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            #region Mayor
            var mayor = this.NPCList.FindByName(CharacterNames.Mayor);
            mayor.TwineDialogId = nameof(GlobalContent.MayorDay3);

            If.Check(() => HasTag("HasSeenMayorDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Mayor);
                npc.TwineDialogId = nameof(GlobalContent.MayorDay3Brief);
            });
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
            #region ElderlyMother
            var elderlyMother = this.NPCList.FindByName(CharacterNames.ElderlyMother);
            elderlyMother.TwineDialogId = nameof(GlobalContent.ElderlyMotherDay3);

            If.Check(() => HasTag("HasSeenElderlyMotherDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.ElderlyMother);
                npc.TwineDialogId = nameof(GlobalContent.ElderlyMotherDay3Brief);
            });
            #endregion
            #region Priestess
            var priestess = this.NPCList.FindByName(CharacterNames.Priestess);
            priestess.TwineDialogId = nameof(GlobalContent.PriestessDay3);

            If.Check(() => HasTag("HasSeenPriestessDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Priestess);
                npc.TwineDialogId = nameof(GlobalContent.PriestessDay3Brief);
            });
            #endregion
            #region Nun
            var nun = this.NPCList.FindByName(CharacterNames.Nun);
            nun.TwineDialogId = nameof(GlobalContent.NunDay3);

            If.Check(() => HasTag("HasSeenNunDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Nun);
                npc.TwineDialogId = nameof(GlobalContent.NunDay3Brief);
            });
            #endregion
            #region Farmer
            var farmer = NPCList.FindByName(CharacterNames.Farmer);
            farmer.CurrentChainName = "FishRight";
            farmer.Position = new Microsoft.Xna.Framework.Vector3(1885, -665, PlayerCharacterInstance.Position.Z);
            farmer.TwineDialogId = nameof(GlobalContent.FarmerDay3);

            If.Check(() => HasTag("HasSeenFarmerDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Farmer);
                npc.TwineDialogId = nameof(GlobalContent.FarmerDay3Brief);
            });

            #endregion
            #region Tycoon
            var tycoon = this.NPCList.FindByName(CharacterNames.Tycoon);
            tycoon.TwineDialogId = nameof(GlobalContent.TycoonDay3);

            If.Check(() => HasTag("HasSeenTycoonDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Tycoon);
                npc.TwineDialogId = nameof(GlobalContent.TycoonDay3Brief);
            });
            #endregion
            #region TycoonDaughter
            var tycoonDaughter = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
            tycoonDaughter.TwineDialogId = nameof(GlobalContent.TycoonDaughterDay3);

            If.Check(() => HasTag("HasSeenTycoonDaughterDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
                npc.TwineDialogId = nameof(GlobalContent.TycoonDaughterDay3Brief);
            });
            #endregion
            #region Conservationist
            //ded
            #endregion
            #region FishermanBald
            var bald = this.NPCList.FindByName(CharacterNames.FishermanBald);
            bald.TwineDialogId = nameof(GlobalContent.FishermanBaldDay3);

            If.Check(() => HasTag("HasSeenFishermanBaldDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanBald);
                npc.TwineDialogId = nameof(GlobalContent.FishermanBaldDay3Brief);
            });
            #endregion
            #region FishermanHair
            var hair = this.NPCList.FindByName(CharacterNames.FishermanHair);
            hair.TwineDialogId = nameof(GlobalContent.FishermanHairDay3);

            If.Check(() => HasTag("HasSeenFishermanHairDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanHair);
                npc.TwineDialogId = nameof(GlobalContent.FishermanHairDay3Brief);
            });
            #endregion
        }
        /// <summary>
        /// Leaving one empty one here as a template if we need it.
        /// </summary>
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
            #region BlackMarketShop
            #endregion
            #region YoungManBaitShop

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
            EveryFrameScriptLogic();
        }

        void CustomDestroy()
        {


        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

    }
}
