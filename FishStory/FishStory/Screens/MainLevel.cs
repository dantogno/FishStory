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
using FlatRedBall.Math;

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
        public const string PlayerCharacter = "PlayerCharacter";
        public static Dictionary<string, string> DisplayNames = new Dictionary<string, string>()
        {
            {BlackMarketShop, "Elias" },
            {Mayor, "Mayor Olsen" },
            {FestivalCoordinator, "Anthony" },
            {Identifier, "Jakob" },
            {Fishmonger, "Oscar" },
            {FarmerSonBaitShop, "Emil" },
            {YoungManBaitShop, "William" },
            {ElderlyMother, "Nora" },
            {Priestess, Priestess },
            {Nun, "Cinthia" },
            {Farmer, "Issac" },
            {Tycoon, "Mr. Petterson" },
            {TycoonDaughter, "Emily" },
            {Conservationist, "Sofia" },
            {FishermanBald, "Larry" },
            {FishermanHair, "Roger" }
        };
        public static Dictionary<string, string> ChosenLines = new Dictionary<string, string>()
        {
            {BlackMarketShop, "Yer' all a bunch of cowards! Why don’t you take care of me yourselves, instead of letting your petty sea-god do all yer' dirty work for you?!" },
            {Mayor, "I... I don’t know what to say. Just... make it quick, okay? And Anthony... look after everyone once I’m gone. Make sure our way of life is protected." },
            {FestivalCoordinator, "This is the stranger's fault! She came in here and ruined the fishing ratios! This is unfair!" },
            {Identifier, "It's my time… And I do believe it's been my time for quite a while now. I'm ready. I do not shrink away from the abyss." },
            {Fishmonger, "Oscar" },
            {FarmerSonBaitShop, "Dad?! Dad, where are you? You told me it wouldn't be me! You said you would protect me. You promised!" },
            {YoungManBaitShop, "Wait! Before I go, someone has to check in on my mother every day. She forgets to eat sometimes, and I don’t want her to think that I've forgotten about her, even when I'm gone. Will someone promise me they'll do it? Please?!" },
            {ElderlyMother, "Well, I suppose it was meant to be. Soon I will be with Ingrid again... I will go without making a fuss. Just promise that someone will look after William once I'm gone." },
            {Priestess, Priestess },
            {Nun, "This is best for everyone. Every year since my daughter made the journey, I prayed that the sea would take me next. I suppose my prayers have finally been answered." },
            {Farmer, "How are you all gonna survive without me, huh?! This is a mistake! You won't last two winters!" },
            {Tycoon, "You can't do this! It was never supposed to be me! I made this town what it is! It will crumble into the ocean without my support!" },
            {TycoonDaughter, "No, no... This is wrong! take my father instead! He's the one who deserves it! What have I ever done, aside from what I'm told?! What did I do to deserve this?!" },
            {Conservationist, "Sofia" },
            {FishermanBald, "Bah, thereain’tnothingleftformeonthisearthanyway… atleastI'llbewiththefishes." },
            {FishermanHair, "Fine, I'll go! At least once I'm gone, I won’t have to hear you all yammering away no more!" }
        };
    }
    public partial class MainLevel
    {
        /// <summary>
        /// Tycoon requires this many fish before giving key.
        /// </summary>
        private int numFishRequiredForKey = 3;
        private const int outOfWorldX = 9000;
        private const int outOfWorldY = 9000;
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

        private Dictionary<string, double> CharacterBedTimes = new Dictionary<string, double>()
        {
            {CharacterNames.BlackMarketShop, 24},
            {CharacterNames.Conservationist, 19 },
            {CharacterNames.ElderlyMother, 16 },
            {CharacterNames.Farmer, 18 },
            {CharacterNames.FarmerSonBaitShop, 18 },
            {CharacterNames.FestivalCoordinator, InGameDateTimeManager.HourToFreezeTimeIfPlayerNeedsKeyOnDay1 + 1 },
            {CharacterNames.FishermanBald, 17 },
            {CharacterNames.FishermanHair, 22 },
            {CharacterNames.Fishmonger, InGameDateTimeManager.HourToFreezeTimeIfPlayerNeedsKeyOnDay1 - 1},
            {CharacterNames.Identifier, InGameDateTimeManager.HourToFreezeTimeIfPlayerNeedsKeyOnDay1 + 2},
            {CharacterNames.Mayor, 19 },
            {CharacterNames.Nun, 17 },
            {CharacterNames.Priestess, 23 },
            {CharacterNames.Tycoon, InGameDateTimeManager.HourToFreezeTimeIfPlayerNeedsKeyOnDay1 + 1},
            {CharacterNames.TycoonDaughter, 22 },
            {CharacterNames.YoungManBaitShop, InGameDateTimeManager.HourToFreezeTimeIfPlayerNeedsKeyOnDay1 + 3}
        };
        private Dictionary<string, double> CharacterWakeTimes = new Dictionary<string, double>()
        {
            {CharacterNames.BlackMarketShop, 17},
            {CharacterNames.Conservationist, 9 },
            {CharacterNames.ElderlyMother, 11 },
            {CharacterNames.Farmer, 6 },
            {CharacterNames.FarmerSonBaitShop, 8 },
            {CharacterNames.FestivalCoordinator, 9 },
            {CharacterNames.FishermanBald, 11 },
            {CharacterNames.FishermanHair, 8 },
            {CharacterNames.Fishmonger, 10 },
            {CharacterNames.Identifier, 9},
            {CharacterNames.Mayor, 11 },
            {CharacterNames.Nun, 9 },
            {CharacterNames.Priestess, 15 },
            {CharacterNames.Tycoon, 10},
            {CharacterNames.TycoonDaughter, 12 },
            {CharacterNames.YoungManBaitShop, 11}
        };
        void CustomInitialize()
        {
            InitializeScript();
        }

        private void InitializeNPCEmoteIcons()
        {
            foreach (var npc in NPCList)
            {
                npc.InitializeEmoteIcon();
            }
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
        public static List<string> GetKeysWithTopValues(Dictionary<string, int> dictionary, int numberOfKeysToReturn)
        {
            var list = dictionary.ToList();
            list.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            var keys = list.Select((kvp) => kvp.Key).Reverse();
            return keys.Take(numberOfKeysToReturn).ToList();
        }
        public static string CharacterToSacrifice { get; private set; }
        /// <summary>
        /// These characters represen specific traits / classes. 
        /// On day 2, if this class is threatened based on the number and type
        /// of fish caught, these characters will have alternative dialog.
        /// </summary>
        private Dictionary<string, string> classRepresentatives = new Dictionary<string, string>()
        {
            // male
            {ItemDefinition.King_Mackerel, CharacterNames.Fishmonger},
            // female
            {ItemDefinition.Ladyfish, CharacterNames.Mayor },
            // old
            { ItemDefinition.Rougheye_Rockfish, CharacterNames.ElderlyMother },
            // young
            { ItemDefinition.Shrimp, CharacterNames.Farmer },
            // bald
            { ItemDefinition.Monkfish, CharacterNames.Identifier },
            // facial hair
            { ItemDefinition.Goatfish, CharacterNames.Tycoon },
            // mother
            { ItemDefinition.Giant_Octopus, CharacterNames.Nun },
            // father
            { ItemDefinition.Seahorse, CharacterNames.TycoonDaughter },
            // blonde hair
            {ItemDefinition.Trumpetfish, CharacterNames.YoungManBaitShop },
            // dark hair
            {ItemDefinition.Cobia, CharacterNames.FestivalCoordinator },
            // brown hair
            {ItemDefinition.Brown_Rockfish, CharacterNames.BlackMarketShop }
        };

        private bool DoesPlayerHaveNoBaitAndNoMoneyAndNoFish =>
            PlayerDataManager.PlayerData.Money < GlobalContent.ItemDefinition[ItemDefinition.Blood_Worm].PlayerBuyingCost
                && !DoesPlayerHaveBait && !DoesPlayerHaveFish;

        private bool DoesPlayerHaveBait => PlayerDataManager.PlayerData.ItemInventory
                    .Where((kvp) => GlobalContent.ItemDefinition[kvp.Key].IsBait && kvp.Value > 0).Any();

        private bool DoesPlayerHaveFish => PlayerDataManager.PlayerData.ItemInventory
                    .Where((kvp) => GlobalContent.ItemDefinition[kvp.Key].IsFish && kvp.Value > 0).Any();

        private string GetCharacterForSacrifice()
        {
            string characterToSacrifice = CharacterNames.PlayerCharacter;

            var dictionary = GetNumberOfFishAssociatedWithCharacters();
            characterToSacrifice = GetKeyWithHighestValue(dictionary);

            return characterToSacrifice;
        }


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
                        NPCList.FindByName(CharacterNames.FestivalCoordinator).SetDialogue(nameof(GlobalContent.FestivalCoordinatorNobaitNoMoney));
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
                            NPCList.FindByName(CharacterNames.FestivalCoordinator).SetDialogue(nameof(GlobalContent.FestivalCoordinatorDay1Brief));
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
            GameScreenGum.InputInstructionsInstance.Visible = false;

            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 1);
            Do.Call(() => DoDay1Script(If, Do));
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 2);
            Do.Call(() => DoDay2Script(If, Do));
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 3);
            Do.Call(() => DoDay3Script(If, Do));
            If.Check(() => PlayerDataManager.PlayerData.CurrentDay == 4);
            Do.Call(() => DoDay4Script(If, Do));            
        }
        private void HandleCharacterBedTimes(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            foreach (var kvp in CharacterBedTimes)
            {

                var npc = NPCList.FindByName(kvp.Key);
                If.Check(() =>
                {
                    return InGameDateTimeManager.OurInGameDay.Hour >= kvp.Value
                        && !npc.IsOnScreen();
                });
                Do.Call(() =>
                {
                    npc.X = outOfWorldX;
                    npc.Y = outOfWorldY;
                });
            }
        }
        private void HandleCharacterWakeTimes(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            foreach (var kvp in CharacterWakeTimes)
            {

                var npc = NPCList.FindByName(kvp.Key);
                npc.X = outOfWorldX;
                npc.Y = outOfWorldY;

                If.Check(() =>
                {
                    return (InGameDateTimeManager.OurInGameDay.Hour >= kvp.Value
                        && !npc.WillBeOnScreenAtPosition(npc.SpawnPosition.X, npc.SpawnPosition.Y))
                        || kvp.Value < HourOnClockPlayerWakesIn24H;
                });
                Do.Call(() =>
                {
                    npc.Position = npc.SpawnPosition;
                });
            }
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
                    NPCList.FindByName(classRepresentatives[item]).SetDialogue(classRepresentatives[item] + "Day2AltTrait", EmotiveIcon.IconDisplay.Exclamation);
                });
            }
        }

        private void DoDay1Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            InitializeNPCEmoteIcons();
            PlayerCharacterInstance.DirectionFacing = TopDownDirection.Left;
            GameScreenGum.InputInstructionsInstance.Visible = true;
            var secondsToShowInputCallout = 14;
            this
                .Call(() => GameScreenGum.InputInstructionsInstance.Visible = false)
                .After(secondsToShowInputCallout);
            
            #region Priestess
            If.Check(() => HasTag("HasSeenPriestessDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Priestess);
                npc.SetDialogue(nameof(GlobalContent.PriestessDay1Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region ElderlyMother
            If.Check(() => HasTag("HasSeenElderlyMotherDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.ElderlyMother);
                npc.SetDialogue(nameof(GlobalContent.ElderlyMotherDay1Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Nun
            If.Check(() => HasTag("HasSeenNunDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Nun);
                npc.SetDialogue(nameof(GlobalContent.NunDay1Brief), EmotiveIcon.IconDisplay.None);
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
                npc.SetDialogue(nameof(GlobalContent.FarmerDay1Brief), EmotiveIcon.IconDisplay.None);
            });

            #endregion
            #region Identifier
            NPCList.FindByName(CharacterNames.Identifier).SetDialogue(nameof(GlobalContent.IdentifierDay1), EmotiveIcon.IconDisplay.Exclamation);

            If.Check(() => HasTag("HasSeenIdentifierDay1"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.Identifier).SetDialogue(nameof(GlobalContent.IdentifierDay1Brief), EmotiveIcon.IconDisplay.Exclamation);
            });
            If.Check(() => TotalFishIdentified >= numFishRequiredForKey);
            Do.Call(()=> NPCList.FindByName(CharacterNames.Identifier).SetDialogue(nameof(GlobalContent.IdentifierDay1Brief), EmotiveIcon.IconDisplay.None));
            #endregion
            #region Fishmonger
            If.Check(() => HasTag("HasTalkedToFishMongerDay1"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.Fishmonger).SetDialogue(nameof(GlobalContent.FishMongerDay1Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Tycoon
            NPCList.FindByName(CharacterNames.Tycoon).SetDialogue(nameof(GlobalContent.TycoonIntro), EmotiveIcon.IconDisplay.Exclamation);
            If.Check(() => HasTag("HasTalkedToTycoonDay1"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.Tycoon).SetDialogue("TycoonNoFishNoKey", EmotiveIcon.IconDisplay.Exclamation);
            });
            If.Check(() => HasTag("HasTalkedToTycoonDay1") && TotalFishIdentified >= numFishRequiredForKey);
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.Tycoon).SetDialogue("TycoonYesFishNoKey", EmotiveIcon.IconDisplay.Exclamation);
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
                NPCList.FindByName(CharacterNames.Tycoon).SetDialogue("TycoonYesKey", EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region TycoonDaughter
            If.Check(() => HasTag("HasSeenTycoonDaughterDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
                npc.SetDialogue(nameof(GlobalContent.TycoonDaughterDay1Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Conservationist
            If.Check(() => HasTag("HasSeenConservationistDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Conservationist);
                npc.SetDialogue(nameof(GlobalContent.ConservationistDay1Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region FishermanBald
            If.Check(() => HasTag("HasSeenFishermanBaldDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanBald);
                npc.SetDialogue(nameof(GlobalContent.FishermanBaldDay1Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region FishermanHair
            If.Check(() => HasTag("HasSeenFishermanHairDay1"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanHair);
                npc.SetDialogue(nameof(GlobalContent.FishermanHairDay1Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Mayor
            if (!DebuggingVariables.ShouldSkipDay1MayorIntro)
            {
                this.NPCList.FindByName(CharacterNames.Mayor).SetDialogue(nameof(GlobalContent.WelcomeDialog), EmotiveIcon.IconDisplay.Exclamation);
                If.Check(() => !HasTag("HasSeenWelcomeDialog") && PlayerCharacterInstance.X < 1525);
                Do.Call(() =>
                {
                    SetDialoguePortraitFor(NPCList.FindByName(CharacterNames.Mayor));
                    if (DialogBox.TryShow(nameof(GlobalContent.WelcomeDialog)))
                    {
                        SetDialoguePortraitFor(NPCList.FindByName(CharacterNames.Mayor));
                        PlayerCharacterInstance.ObjectsBlockingInput.Add(DialogBox);
                    }
                });
            }

            If.Check(() => HasTag("HasSeenWelcomeDialog"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Mayor);
                npc.SetDialogue(nameof(GlobalContent.MayorAfterWelcome), EmotiveIcon.IconDisplay.None);
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
            this.NPCList.FindByName(CharacterNames.FestivalCoordinator).SetDialogue(nameof(GlobalContent.FestivalCoordinatorDay1), EmotiveIcon.IconDisplay.Exclamation);
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
                npc.SetDialogue(nameof(GlobalContent.FestivalCoordinatorDay1Brief), EmotiveIcon.IconDisplay.None);
                AwardRandomBait();
            });
            #endregion
            #region BlackMarketShop
            var blackMarketShop = NPCList.FindByName(CharacterNames.BlackMarketShop);
            blackMarketShop.X = outOfWorldX;
            blackMarketShop.Y = outOfWorldY;

            If.Check(() =>
            {
               return InGameDateTimeManager.OurInGameDay.Hour > CharacterWakeTimes[CharacterNames.BlackMarketShop] 
                && PlayerDataManager.PlayerData.Has(ItemDefinition.Trailer_Key)
                && !blackMarketShop.WillBeOnScreenAtPosition(blackMarketShop.SpawnPosition.X, blackMarketShop.SpawnPosition.Y);
            });
            Do.Call(() =>
            {
                blackMarketShop.Position = blackMarketShop.SpawnPosition;
                this.NPCList.FindByName(CharacterNames.BlackMarketShop).SetDialogue(nameof(GlobalContent.BlackMarketShopDay1Brief), EmotiveIcon.IconDisplay.Exclamation);
            });
            If.Check(() =>
            {
                return HasTag("HasSeenBlackMarketShopDay1");
            });
            Do.Call(() =>
            {
                blackMarketShop.SetDialogue(nameof(GlobalContent.BlackMarketShopDay1Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region YoungManBaitShop  
            If.Check(() => HasTag("HasSeenFancyBaitShopDialog"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.YoungManBaitShop).SetDialogue(nameof(GlobalContent.FancyBaitShopDay1Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion

            HandleCharacterBedTimes(If, Do);
            // Can't do this in Day 1 or important characters may not be there when the game starts!
            // HandleCharacterWakeTimes(If, Do);

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
            mayor.SetDialogue(nameof(GlobalContent.MayorDay2));
            mayor.SpawnPosition = new Microsoft.Xna.Framework.Vector3(967, -593, PlayerCharacterInstance.Z);
            If.Check(() => HasTag("HasSeenMayorDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Mayor);
                npc.SetDialogue(nameof(GlobalContent.MayorDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region FestivalCoordinator
            NPCList.FindByName(CharacterNames.FestivalCoordinator).SetDialogue(nameof(GlobalContent.FestivalCoordinatorDay2), EmotiveIcon.IconDisplay.Exclamation);
            If.Check(() => HasTag("Day2FreeBait"));
            Do.Call(() =>
            {
                var npc = NPCList.FindByName(CharacterNames.FestivalCoordinator);
                npc.SetDialogue(nameof(GlobalContent.FestivalCoordinatorDay2Brief), EmotiveIcon.IconDisplay.None);
                AwardRandomBait();
            });
            #endregion
            #region Identifier
            NPCList.FindByName(CharacterNames.Identifier).SetDialogue(nameof(GlobalContent.IdentifierDay2));
            #endregion
            #region Fishmonger
            NPCList.FindByName(CharacterNames.Fishmonger).SetDialogue(nameof(GlobalContent.FishMongerDay2));
            If.Check(() => HasTag("HasSeenFishmongerDay2Alt"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.Fishmonger).SetDialogue(nameof(GlobalContent.FishMongerDay2));
            });
            #endregion
            #region FarmerSonBaitShop
            NPCList.FindByName(CharacterNames.FarmerSonBaitShop).SetDialogue(nameof(GlobalContent.FarmerSonBaitShopDialogDay2));
            #endregion
            #region YoungManBaitShop 
            this.NPCList.FindByName(CharacterNames.YoungManBaitShop).SetDialogue(nameof(GlobalContent.YoungManBaitShopDay2));
            If.Check(() => HasTag("HasSeenYoungManBaitShopDay2AltTrait"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.YoungManBaitShop).SetDialogue(nameof(GlobalContent.YoungManBaitShopDay2));
            });
            #endregion
            #region BlackMarketShop
            NPCList.FindByName(CharacterNames.BlackMarketShop).SetDialogue(nameof(GlobalContent.BlackMarketShopDay2));
            If.Check(() =>
            {
                return HasTag("HasSeenBlackMarketShopDay2Alt");
            });
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.BlackMarketShop).SetDialogue(nameof(GlobalContent.BlackMarketShopDay2));
            });
            If.Check(() =>
            {
                return HasTag("HasSeenBlackMarketShopDay2");
            });
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.BlackMarketShop).SetDialogue(nameof(GlobalContent.BlackMarketShopDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region ElderlyMother
            var elderlyMother = this.NPCList.FindByName(CharacterNames.ElderlyMother);
            elderlyMother.SetDialogue(nameof(GlobalContent.ElderlyMotherDay2));
            elderlyMother.SpawnPosition = new Microsoft.Xna.Framework.Vector3(1548, -568, PlayerCharacterInstance.Z);

            If.Check(() => HasTag("HasSeenElderlyMotherDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.ElderlyMother);
                npc.SetDialogue(nameof(GlobalContent.ElderlyMotherDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Priestess
            var priestess = this.NPCList.FindByName(CharacterNames.Priestess);
            priestess.SetDialogue(nameof(GlobalContent.PriestessDay2));

            If.Check(() => HasTag("HasSeenPriestessDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Priestess);
                npc.SetDialogue(nameof(GlobalContent.PriestessDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Nun
            var nun = this.NPCList.FindByName(CharacterNames.Nun);
            nun.SetDialogue(nameof(GlobalContent.NunDay2));

            If.Check(() => HasTag("HasSeenNunDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Nun);
                npc.SetDialogue(nameof(GlobalContent.NunDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Farmer
            var farmer = NPCList.FindByName(CharacterNames.Farmer);
            farmer.CurrentChainName = "FishLeft";
            farmer.SpawnPosition = new Microsoft.Xna.Framework.Vector3(315, -595, PlayerCharacterInstance.Position.Z);
            farmer.SetDialogue(nameof(GlobalContent.FarmerDay2));

            If.Check(() => HasTag("HasSeenFarmerDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Farmer);
                npc.SetDialogue(nameof(GlobalContent.FarmerDay2Brief), EmotiveIcon.IconDisplay.None);
            });

            #endregion
            #region Tycoon
            var tycoon = this.NPCList.FindByName(CharacterNames.Tycoon);
            tycoon.SetDialogue(nameof(GlobalContent.TycoonDay2));

            If.Check(() => HasTag("HasSeenTycoonDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Tycoon);
                npc.SetDialogue(nameof(GlobalContent.TycoonDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region TycoonDaughter
            var tycoonDaughter = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
            tycoonDaughter.SpawnPosition = new Microsoft.Xna.Framework.Vector3(715, -340, PlayerCharacterInstance.Position.Z);
            tycoonDaughter.SetDialogue(nameof(GlobalContent.TycoonDaughterDay2));

            If.Check(() => HasTag("HasSeenTycoonDaughterDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
                npc.SetDialogue(nameof(GlobalContent.TycoonDaughterDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Conservationist
            var conservationist = this.NPCList.FindByName(CharacterNames.Conservationist);
            conservationist.SetDialogue(nameof(GlobalContent.ConservationistDay2));
            conservationist.SpawnPosition = new Microsoft.Xna.Framework.Vector3(745, -880, PlayerCharacterInstance.Z);
            If.Check(() => HasTag("HasSeenConservationistDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Conservationist);
                npc.SetDialogue(nameof(GlobalContent.ConservationistDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region FishermanBald
            var baldFisher = this.NPCList.FindByName(CharacterNames.FishermanBald);
            baldFisher.SetDialogue(nameof(GlobalContent.FishermanBaldDay2));

            If.Check(() => HasTag("HasSeenFishermanBaldDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanBald);
                npc.SetDialogue(nameof(GlobalContent.FishermanBaldDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region FishermanHair
            var hairFisher = this.NPCList.FindByName(CharacterNames.FishermanHair);
            hairFisher.SetDialogue(nameof(GlobalContent.FishermanHairDay2));

            If.Check(() => HasTag("HasSeenFishermanHairDay2"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanHair);
                npc.SetDialogue(nameof(GlobalContent.FishermanHairDay2Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            HandleDay2TraitAlternateDialogForClassRepresentatives(If, Do);

            HandleCharacterBedTimes(If, Do);
            HandleCharacterWakeTimes(If, Do);
        }
        private void DoDay3Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            #region Mayor
            var mayor = this.NPCList.FindByName(CharacterNames.Mayor);
            mayor.SetDialogue(nameof(GlobalContent.MayorDay3));
            mayor.SpawnPosition = new Microsoft.Xna.Framework.Vector3(1422, -620, PlayerCharacterInstance.Z);

            If.Check(() => HasTag("HasSeenMayorDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Mayor);
                npc.SetDialogue(nameof(GlobalContent.MayorDay3Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region FestivalCoordinator
            NPCList.FindByName(CharacterNames.FestivalCoordinator).SetDialogue(nameof(GlobalContent.FestivalCoordinatorDay3), EmotiveIcon.IconDisplay.Exclamation);
            If.Check(() => HasTag("Day3FreeBait"));
            Do.Call(() =>
            {
                var npc = NPCList.FindByName(CharacterNames.FestivalCoordinator);
                npc.SetDialogue(nameof(GlobalContent.FestivalCoordinatorDay2Brief), EmotiveIcon.IconDisplay.None);
                PlayerDataManager.PlayerData.AwardItem(ItemDefinition.Strange_Bait);
            });
            #endregion
            #region Identifier
            NPCList.FindByName(CharacterNames.Identifier).SetDialogue(nameof(GlobalContent.IdentifierDay3));
            #endregion
            #region Fishmonger
            NPCList.FindByName(CharacterNames.Fishmonger).SetDialogue(nameof(GlobalContent.FishMongerDay3));
            #endregion
            #region FarmerSonBaitShop
            NPCList.FindByName(CharacterNames.FarmerSonBaitShop).SetDialogue(nameof(GlobalContent.FarmerSonBaitShopDialogDay3));
            #endregion
            #region YoungManBaitShop
            this.NPCList.FindByName(CharacterNames.YoungManBaitShop).SetDialogue(nameof(GlobalContent.YoungManBaitShopDay3));
            #endregion
            #region BlackMarketShop
            NPCList.FindByName(CharacterNames.BlackMarketShop).SetDialogue(nameof(GlobalContent.BlackMarketShopDay3));
            If.Check(() => HasTag("HasSeenBlackMarketShopDay3"));
            Do.Call(() =>
            {
                NPCList.FindByName(CharacterNames.FishermanHair).SetDialogue(nameof(GlobalContent.BlackMarketShopDay3Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region ElderlyMother
            var elderlyMother = this.NPCList.FindByName(CharacterNames.ElderlyMother);
            elderlyMother.SetDialogue(nameof(GlobalContent.ElderlyMotherDay3));

            If.Check(() => HasTag("HasSeenElderlyMotherDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.ElderlyMother);
                npc.SetDialogue(nameof(GlobalContent.ElderlyMotherDay3Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Priestess
            var priestess = this.NPCList.FindByName(CharacterNames.Priestess);
            priestess.SetDialogue(nameof(GlobalContent.PriestessDay3));

            If.Check(() => HasTag("HasSeenPriestessDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Priestess);
                npc.SetDialogue(nameof(GlobalContent.PriestessDay3Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Nun
            var nun = this.NPCList.FindByName(CharacterNames.Nun);
            nun.SetDialogue(nameof(GlobalContent.NunDay3));

            If.Check(() => HasTag("HasSeenNunDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Nun);
                npc.SetDialogue(nameof(GlobalContent.NunDay3Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Farmer
            var farmer = NPCList.FindByName(CharacterNames.Farmer);
            farmer.CurrentChainName = "FishRight";
            farmer.SpawnPosition = new Microsoft.Xna.Framework.Vector3(1576, -729, PlayerCharacterInstance.Position.Z);
            farmer.SetDialogue(nameof(GlobalContent.FarmerDay3));

            If.Check(() => HasTag("HasSeenFarmerDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Farmer);
                npc.SetDialogue(nameof(GlobalContent.FarmerDay3Brief), EmotiveIcon.IconDisplay.None);
            });

            #endregion
            #region Tycoon
            var tycoon = this.NPCList.FindByName(CharacterNames.Tycoon);
            tycoon.SetDialogue(nameof(GlobalContent.TycoonDay3));

            If.Check(() => HasTag("HasSeenTycoonDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.Tycoon);
                npc.SetDialogue(nameof(GlobalContent.TycoonDay3Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region TycoonDaughter
            var tycoonDaughter = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
            tycoonDaughter.SetDialogue(nameof(GlobalContent.TycoonDaughterDay3));

            If.Check(() => HasTag("HasSeenTycoonDaughterDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.TycoonDaughter);
                npc.SetDialogue(nameof(GlobalContent.TycoonDaughterDay3Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region Conservationist
            //ded
            If.Check(() =>
            {
                return true;
            });
            Do.Call(() =>
            {
                var conservationist = NPCList.FindByName(CharacterNames.Conservationist);
                NPCList.Remove(conservationist);
                conservationist.Destroy();
            });
            #endregion
            #region FishermanBald
            var bald = this.NPCList.FindByName(CharacterNames.FishermanBald);
            bald.SetDialogue(nameof(GlobalContent.FishermanBaldDay3));

            If.Check(() => HasTag("HasSeenFishermanBaldDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanBald);
                npc.SetDialogue(nameof(GlobalContent.FishermanBaldDay3Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion
            #region FishermanHair
            var hair = this.NPCList.FindByName(CharacterNames.FishermanHair);
            hair.SetDialogue(nameof(GlobalContent.FishermanHairDay3), EmotiveIcon.IconDisplay.None);

            If.Check(() => HasTag("HasSeenFishermanHairDay3"));
            Do.Call(() =>
            {
                var npc = this.NPCList.FindByName(CharacterNames.FishermanHair);
                npc.SetDialogue(nameof(GlobalContent.FishermanHairDay3Brief), EmotiveIcon.IconDisplay.None);
            });
            #endregion

            HandleCharacterBedTimes(If, Do);
            HandleCharacterWakeTimes(If, Do);
        }
        private void DoDay4Script(ScreenScript<GameScreen> If, ScreenScript<GameScreen> Do)
        {
            if (DebuggingVariables.NextDayWillTriggerEnding)
            {
                
            }
            foreach (var item in NPCList)
            {
                item.SetEmoteIcon(EmotiveIcon.IconDisplay.None);
            }
            InGameDateTimeManager.SetTimeOfDay(TimeSpan.FromHours(3));
            InGameDateTimeManager.ShouldTimePass = false;

            CharacterToSacrifice = GetCharacterForSacrifice();
            bool isPlayerSacrificed = CharacterToSacrifice == CharacterNames.PlayerCharacter;
            if (!isPlayerSacrificed)
                NPCList.FindByName(CharacterToSacrifice).CurrentChainName = "Idle";
            string officiant = CharacterToSacrifice == CharacterNames.Priestess ? CharacterNames.Nun : CharacterNames.Priestess;

            NPC[] cloakedNPCs = null;
            var escortGuard1 = NPCList.FindByName(CharacterToSacrifice == CharacterNames.Farmer ? CharacterNames.Tycoon : CharacterNames.Farmer);
            var escortGuard2 = NPCList.FindByName(CharacterToSacrifice == CharacterNames.FishermanBald ? CharacterNames.Tycoon : CharacterNames.FishermanBald);
            this.Call(() =>
            {
                // Change all the npcs but the priestess and the person getting sacrificed
                    cloakedNPCs = NPCList.Where((npc) => npc.Name != officiant
                    && !npc.Name.Contains("Sign") && !npc.Name.Contains("Board")
                    && npc.Name != CharacterToSacrifice).ToArray();
                foreach (var npc in cloakedNPCs)
                {
                    npc.Animation = NPC.CloakedGuy;
                    npc.CurrentChainName = "Idle";
                }
                // Knock in the middle of the night. player clicks through dialog, then fade back in
                float delayBeforeKnock = 1;
                this.Call(() => { DialogBox.TryShow(nameof(GlobalContent.Day4Intro)); }).After(delayBeforeKnock);


                escortGuard1.Position = new Microsoft.Xna.Framework.Vector3(729, -834, PlayerCharacterInstance.Z);
                // escortGuard1.CurrentChainName = "WalkLeft";
                escortGuard2.Position = new Microsoft.Xna.Framework.Vector3(696, -834, PlayerCharacterInstance.Z);

            }).After(GameScreenGum.ToBlackAnimation.Length);
 
             
            If.Check(() =>
            {
                return HasTag("HasSeenKnocking") && !DialogBox.Visible;
            });
            Do.Call(() =>
            {
                FadeIn();
                SetDialoguePortraitFor(escortGuard1);
                this.Call(() => { DialogBox.TryShow(nameof(GlobalContent.Day4Intro2)); }).After(GameScreenGum.ToBlackAnimation.Length);
            });

            If.Check(() =>
            {
                return HasTag("HasSeenIntro2") && !DialogBox.Visible;
            });
            Do.Call(() =>
            {
                FadeToBlack();
                float extraFadeDelay = 1;
                float delayBeforeOfficiantSpeaks = GameScreenGum.ToBlackAnimation.Length + extraFadeDelay + 1;
                this.Call(FadeIn).After(GameScreenGum.ToBlackAnimation.Length + extraFadeDelay);
                float distanceBetweenCharactersX = 16;
                float distanceBetweenCharactersY = 15;
                float column1X = 1064;
                float column2X = 1102;
                float row1Y = -1049;
                int numPerRow = 9;
                this.Call(() =>
                {
                    for (int i = 0; i < numPerRow; i++)
                    { 
                        cloakedNPCs[i].X = column1X + i * distanceBetweenCharactersX;
                        cloakedNPCs[i].Y = row1Y;
                    }
                    for (int i = numPerRow; i < cloakedNPCs.Count(); i++)
                    {
                        cloakedNPCs[i].X = column2X + (i - numPerRow) * distanceBetweenCharactersX;
                        cloakedNPCs[i].Y = row1Y - distanceBetweenCharactersY;
                    }
                    NPCList.FindByName(officiant).Position = new Microsoft.Xna.Framework.Vector3(1114, -1112, PlayerCharacterInstance.Z);
                    if (isPlayerSacrificed)
                        PlayerCharacterInstance.Position = new Microsoft.Xna.Framework.Vector3(1140, -1112, PlayerCharacterInstance.Z);
                    else
                    {
                        PlayerCharacterInstance.Position = new Microsoft.Xna.Framework.Vector3(1127, -1088, PlayerCharacterInstance.Z);
                        NPCList.FindByName(CharacterToSacrifice).Position = new Microsoft.Xna.Framework.Vector3(1140, -1112, PlayerCharacterInstance.Z);
                    }
                    PlayerCharacterInstance.DirectionFacing = TopDownDirection.Down;
                }).After(GameScreenGum.ToBlackAnimation.Length);
                this.Call(() =>
                {
                    SetDialoguePortraitFor(NPCList.FindByName(officiant));
                    if (isPlayerSacrificed)
                        DialogBox.TryShow(nameof(GlobalContent.Day4PlayerChosenEnding));
                    else if(CharacterToSacrifice == CharacterNames.Priestess)
                        DialogBox.TryShow(nameof(GlobalContent.Day4PriestessChosenEnding));
                    else
                        DialogBox.TryShow(nameof(GlobalContent.Day4BasicEnding));
                }).After(delayBeforeOfficiantSpeaks);

                If.Check(() =>
                {
                    return HasTag("ShowOfficiantPortrait");
                });
                Do.Call(() =>
                {
                    SetDialoguePortraitFor(NPCList.FindByName(officiant));
                });

                If.Check(() =>
                {
                    return !DialogBox.Visible && HasTag("Ending");
                });
                Do.Call(() =>
                {
                    float delayBeforeDrowningSound = GameScreenGum.ToBlackAnimation.Length + 3;
                    var drowningSoundDuration = GlobalContent.DrowningSound.Duration.Seconds;
                    float delayBeforeLoadingCreditsScreen = (float)GlobalContent.DrowningSound.Duration.TotalSeconds - 3;
                    FadeToBlack();
                    DayAndTimeDisplayIsVisible = false;
                    hasEndingStarted = true;

                    EndingScreenTransitionInstance.CurrentFadeTransitionState = GumRuntimes.EndingScreenTransitionRuntime.FadeTransition.Out;
                    EndingScreenTransitionInstance.Visible = true;

                    GameScreenGum.ToBlackAnimation.Play();

                    this.Call(() => EndingScreenTransitionInstance.FadeInAnimation.Play())
                        .After(GameScreenGum.ToBlackAnimation.Length);
                    this.Call(() => PlayLightShimmerAnimation())
                        .After(EndingScreenTransitionInstance.FadeInAnimation.Length + GameScreenGum.ToBlackAnimation.Length);
                    this.Call(() => SoundManager.Play(GlobalContent.DrowningSound, volume: 1f))
                        .After(delayBeforeDrowningSound);
                    this.Call(() => { EndingScreenTransitionInstance.GlowPulseAnimation.Stop(); EndingScreenTransitionInstance.FadeOutAnimation.Play(); })
                        .After(delayBeforeDrowningSound + drowningSoundDuration - EndingScreenTransitionInstance.FadeOutAnimation.Length);
                    this.Call(() =>
                    {
                        MoveToScreen(nameof(CreditsScreen));
                    }).After(delayBeforeDrowningSound + drowningSoundDuration);
                });
            });
        }

        private void AwardRandomBait()
        {
            int index = FlatRedBallServices.Random.Next(0, ItemDefinition.BaitNames.Length - 1);
            PlayerDataManager.PlayerData.AwardItem(ItemDefinition.BaitNames[index]);
            AddNotification($"Recieved: {ItemDefinition.BaitNames[index]}");
        }

        void CustomActivity(bool firstTimeCalled)
        {
            if (DebuggingVariables.DisplayPlayerCoordinates)
                FlatRedBall.Debugging.Debugger.Write($"Player X: {PlayerCharacterInstance.X}, Player Y: {PlayerCharacterInstance.Y}");
            //FlatRedBall.Debugging.Debugger.Write($"Fish identified: {TotalFishIdentified}");
            EveryFrameScriptLogic();
        }
        private void HandleEndingMusicFadeOut()
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
