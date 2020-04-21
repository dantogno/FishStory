using System;
using System.Collections.Generic;
using System.Linq;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Scripting;
using Microsoft.Xna.Framework;
using FishStory.Entities;
using FlatRedBall.TileEntities;
using FishStory.Managers;
using FlatRedBall.Gui;
using FishStory.DataTypes;
using FishStory.GumRuntimes;
using static DialogTreePlugin.SaveClasses.DialogTreeRaw;
using DialogTreePlugin.SaveClasses;
using static FishStory.Entities.PropObject;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace FishStory.Screens
{
    public partial class GameScreen
    {
        #region Structs

        struct FishWeight
        {
            public string Fish;
            public float Weight;

            public override string ToString()
            {
                return $"{Fish} {Weight}";
            }
        }

        #endregion

        #region Fields/Properties

        protected ScreenScript<GameScreen> script;

        List<string> dialogTagsThisFrame = new List<string>();
        private bool isBeingForcedToSleep;

        Dictionary<string, List<string>> ItemsBought = new Dictionary<string, List<string>>();

        private bool FadeInComplete => FadeInSprite.Alpha <= 0f;
        private StoreRuntime StoreInstance => GameScreenGum.StoreInstance;
        private InventoryRuntime InventoryInstance => GameScreenGum.InventoryInstance;

        #endregion

        #region Initialize

        void CustomInitialize()
        {
            script = new ScreenScript<GameScreen>(this);
            
            InitializeEntitiesFromMap();
            //Map.AddToManagers(WorldLayer);

            DialogBox.Visible = false;
            DialoguePortrait.Visible = false;
            DialogBox.GameScreenInstance = this;
            InitializePlayer();

            InitializeCamera();

            SpriteManager.OrderedSortType = FlatRedBall.Graphics.SortType.ZSecondaryParentY;

            InitializeCollision();

            InitializeUi();

            InitializeDarkness();

            InitializeRestartVariables();


#if DEBUG
            DebugInitialize();
#endif
        }

        private void InitializePlayer()
        {
            PlayerCharacterInstance.FishLost += HandleFishLost;
            PlayerCharacterInstance.Lantern.Z = 1; // above the player, so always on top
            PlayerCharacterInstance.Lantern.SetLayers(LightEffectsLayer);
            PlayerCharacterInstance.MoveDisplayElementsToUiLayer(UILayer);
        }
        private void InitializeDarkness()
        {
            BaseNightTimeColor.ColorOperation = FlatRedBall.Graphics.ColorOperation.Color;

            //WorldLayer.RenderTarget = WorldRenderTarget;
            LightEffectsLayer.RenderTarget = NightDarknessRenderTarget;
            //BackgroundLayer.RenderTarget = BackgroundRenderTarget;
            DarknessOverlaySprite.Texture = NightDarknessRenderTarget;

            DarknessOverlaySprite.BlendOperation = FlatRedBall.Graphics.BlendOperation.Modulate;
        }

#if DEBUG
        private void DebugInitialize()
        {
            if (DebuggingVariables.ShowFishLootZones)
            {
                foreach(var zone in this.FishingZoneList)
                {
                    zone.Collision.Visible = true;
                }
            }


            void Award(string item) => PlayerDataManager.PlayerData.AwardItem(item);
            if (DebuggingVariables.AwardTonsOfBait)
            {
                Award(DataTypes.ItemDefinition.Sardine);
                Award(DataTypes.ItemDefinition.Sardine);
                Award(DataTypes.ItemDefinition.Sardine);

                Award(DataTypes.ItemDefinition.Blood_Worm);
                Award(DataTypes.ItemDefinition.Blood_Worm);
                Award(DataTypes.ItemDefinition.Blood_Worm);

                Award(DataTypes.ItemDefinition.Squid);
                Award(DataTypes.ItemDefinition.Squid);
                Award(DataTypes.ItemDefinition.Little_Bonito);
                Award(DataTypes.ItemDefinition.Little_Bonito);
            }

            if(DebuggingVariables.AwardUnidentifiedFish || DebuggingVariables.NextDayWillTriggerEnding)
            {
                foreach(var item in GlobalContent.ItemDefinition.Where(item => !string.IsNullOrEmpty(item.Value.AssociatedItem)))
                {
                    Award(item.Key);
                }
            }

            if (DebuggingVariables.NextDayWillTriggerEnding)
                HandleIdentify();

            if (DebuggingVariables.AwardIdentifiedFish)
            {
                foreach (var item in GlobalContent.ItemDefinition.Where(item => item.Value.IsFish))
                {
                    Award(item.Key);
                }
            }

            if(DebuggingVariables.AwardFishingPole)
            {
                Award(ItemDefinition.Fishing_Rod);
            }

            var testBlackMarketDialog = GetRootObject("Sell At Black Market Dialog",
                new List<string>
                {
                    "KabukiQuantum|Sell"
                });

            testBlackMarketDialog.passages.Add(new Passage
            {
                pid = "1",
                text = "sell=blackmarket"
            });



            var testFishMonger = GetRootObject("Fishmonger dialog",
                new List<string>
                {
                    "Sell",
                    "Cancel"
                });

            testFishMonger.passages.Add(new Passage
            {
                pid = "1",
                text = "sell=fishmonger"
            });

            var testIdentifier = GetRootObject("Identifier dialog",
                new List<string>
                {
                    "Identify",
                    "Cancel"
                });

            testIdentifier.passages.Add(new Passage
            {
                pid = "1",
                text = "id="
            });
        }
#endif

        private void InitializeEntitiesFromMap()
        {
            foreach (var layer in Map.MapLayers)
            {
                if (layer.Name == "Labels")
                {
                    layer.Visible = false;
                }
            }

            TileEntityInstantiator.CreateEntitiesFrom(Map);

            foreach(var npc in NPCList)
            {
                npc.Z = PlayerCharacterInstance.Z; // same as player so they sort
                //npc.MoveToLayer(WorldLayer);
                npc.SpawnPosition = new Vector3(npc.X, npc.Y, npc.Z);
                // David's hack to support invisible NPCs (for signs, etc.): DumpSign is the default, and it is not used.
                // Thus if an NPC is not given an animation in Tiled, it will use DumpSign.
                // Designers will not assign an animation if the intent is that the sprite is hidden.
                // So all the DumpSigns should be visible = false.
                if (npc.Animation == NPC.DumpSign)
                {
                    npc.SpriteInstance.Visible = false;
                }
                else
                    npc.MoveDisplayElementsToUiLayer(UILayer);
            }
            foreach (var propObject in PropObjectList)
            {
                propObject.Z = PlayerCharacterInstance.Z; // same as player so they sort
                propObject.SetLayers(LightEffectsLayer);
            }
        }

        private void InitializeCamera()
        {
            Camera.Main.X = PlayerCharacterInstance.X;
            Camera.Main.Y = PlayerCharacterInstance.Y;

            Camera.Main.SetBordersAtZ(Map.X, Map.Y - Map.Height, Map.X + Map.Width, Map.Y, 0);
        }

        private void InitializeCollision()
        {
            PlayerCharacterInstanceActivityCollisionVsNPCListBodyCollision.CollisionOccurred +=
                HandlePlayerVsNpcActivityCollision;
        }

        private void InitializeUi()
        {
            #region Dialog Box

            if (PlayerCharacterInstance.InputDevice is Keyboard keyboard)
            {
                DialogBox.UpInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Up)
                    .Or(keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.W));

                DialogBox.DownInput = keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.Down)
                    .Or(keyboard.GetKey(Microsoft.Xna.Framework.Input.Keys.S));

                GameScreenGum.InputInstructionsInstance.CurrentInputTypeState = InputInstructionsRuntime.InputType.Keyboard;
            }
            else if (PlayerCharacterInstance.InputDevice is Xbox360GamePad gamepad)
            {
                DialogBox.UpInput = gamepad.GetButton(Xbox360GamePad.Button.DPadUp)
                    .Or(gamepad.LeftStick.UpAsButton);

                DialogBox.DownInput = gamepad.GetButton(Xbox360GamePad.Button.DPadDown)
                    .Or(gamepad.LeftStick.DownAsButton);

                GameScreenGum.InputInstructionsInstance.CurrentInputTypeState = InputInstructionsRuntime.InputType.XboxController;
            }

            DialogBox.SelectInput = PlayerCharacterInstance.TalkInput;

            DialogBox.AfterHide += HandleDialogBoxHide;
            DialogBox.StoreShouldShow += HandleStoreShouldShow;
            DialogBox.SellingShouldShow += HandleSellingShouldShow;
            DialogBox.IdentifyPerformed += HandleIdentify;
            DialogBox.DialogTagShown += HandleDialogTagShown;

            #endregion

            #region Store
            StoreInstance.CancelInput = PlayerCharacterInstance.CancelInput;
            StoreInstance.Visible = false;
            StoreInstance.BuyButtonClick += HandleBuyClicked;
            StoreInstance.Closed += HandleStoreClosed;
            StoreInstance.CancelInput = PlayerCharacterInstance.CancelInput;
            StoreInstance.InventoryInput = PlayerCharacterInstance.InventoryInput;
            StoreInstance.UpInput = PlayerCharacterInstance.UpInput;
            StoreInstance.DownInput = PlayerCharacterInstance.DownInput;
            StoreInstance.SelectInput = PlayerCharacterInstance.TalkInput;
            #endregion

            #region Inventory
            InventoryInstance.Visible = false;
            InventoryInstance.SellClicked += HandleSellClicked;
            InventoryInstance.Closed += HandleInventoryClosed;
            InventoryInstance.CancelInput = PlayerCharacterInstance.CancelInput;
            InventoryInstance.InventoryInput = PlayerCharacterInstance.InventoryInput;
            InventoryInstance.UpInput = PlayerCharacterInstance.UpInput;
            InventoryInstance.DownInput = PlayerCharacterInstance.DownInput;
            InventoryInstance.SelectInput = PlayerCharacterInstance.TalkInput;
            #endregion

            #region Pause Screen
            GameScreenGum.PauseMenuInstance.UpInput = PlayerCharacterInstance.UpInput;
            GameScreenGum.PauseMenuInstance.DownInput = PlayerCharacterInstance.DownInput;
            GameScreenGum.PauseMenuInstance.SelectInput = PlayerCharacterInstance.TalkInput;
            GameScreenGum.PauseMenuInstance.CancelInput = PlayerCharacterInstance.CancelInput;
            GameScreenGum.PauseMenuInstance.Closed += PauseMenuInstance_Closed;
            #endregion
            GameScreenGum.NotificationBoxInstance.UpdateVisibility();

            GameScreenGum.MoveToFrbLayer(UILayer, UILayerGum);
        }

        private void PauseMenuInstance_Closed()
        {
            GameScreenGum.PauseMenuInstance.Visible = false;
            UnpauseThisScreen();
        }

        private void HandleStoreClosed()
        {
            PlayerCharacterInstance.ObjectsBlockingInput.Remove(GameScreenGum.StoreInstance);
            SoundManager.Play(GlobalContent.StoreCloseSound);
            UnpauseThisScreen();
        }

        private void HandlePlayerVsNpcActivityCollision(PlayerCharacter player, NPC npc)
        {
            player.NpcForAction = npc;
        }

        private void InitializeRestartVariables()
        {
            RestartVariables.Add(
                $"this.{nameof(PlayerCharacterInstance)}.{nameof(PlayerCharacterInstance.X)}");
            RestartVariables.Add(
                $"this.{nameof(PlayerCharacterInstance)}.{nameof(PlayerCharacterInstance.Y)}");

            RestartVariables.Add($"Camera.Main.X");
            RestartVariables.Add($"Camera.Main.Y");

            //RestartVariables.Add($"this.{nameof(FadeInSprite)}.{nameof(FadeInSprite.Alpha)}");
        }

        #endregion

        #region Activity

        private SoundEffectInstance boatHornSound = null;
        void CustomActivity(bool firstTimeCalled)
        {
#if DEBUG
            if (DebuggingVariables.ShouldSkipFadeInWithBoatSound && !FadeInComplete)
            {
                FadeInSprite.Alpha = 0f;
            }
#endif
            if (FadeInComplete == false)
            {
                if (firstTimeCalled)
                {
                    InGameDateTimeManager.Activity(firstTimeCalled);
                    boatHornSound = SoundManager.Play(GlobalContent.BoatHornSound, volume: 1f);
                }
                else if (boatHornSound == null || boatHornSound.IsDisposed || boatHornSound.State != SoundState.Playing)
                {
                    FadeInSprite.Alpha -= 0.01f;
                }
            }
            else
            {
                // No longer clearing because we need to know if tags have ever been seen
                // dialogTagsThisFrame.Clear();
                if (InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.RightButton))
                {
                    RestartScreen(true);
                }
                CameraActivity();

                if (!IsPaused && DialogBox.Visible == false)
                {
                    InGameDateTimeManager.Activity(firstTimeCalled);
                }
                DarknessOverlaySprite.Alpha = 1 - InGameDateTimeManager.SunlightEffectiveness;
                UpdatePropObjectsLights();

                UiActivity();

                PlayerCollisionActivity();
#if DEBUG
                DebuggingActivity();
#endif


                // do script *after* the UI
                Map?.AnimateSelf();

                script.Activity();

                PlayTimeBasedSounds();

                PlaySongForDay();

                if (InGameDateTimeManager.TimeOfDay.Hours == (int)HourOnClockPlayerForcedSleepIn24H && !isBeingForcedToSleep)
                {
                    ForcePlayerToSleep();
                }
            }
        }

        private void PlayTimeBasedSounds()
        {
            if ((InGameDateTimeManager.TimeOfDay.Hours > HourOnClockSunRisesIn24H && InGameDateTimeManager.TimeOfDay.Hours < HourOnClockSunRisesIn24H + 3) ||
                (InGameDateTimeManager.TimeOfDay.Hours > HourOnClockSunSetsIn24H - 3 && InGameDateTimeManager.TimeOfDay.Hours < HourOnClockSunSetsIn24H))
            {
                PlayBirdSong();
            }

            var isNightTimeAmbiencePlaying = SoundManager.IsPlaying(GlobalContent.NightTimeLoopSound);
            if ((InGameDateTimeManager.TimeOfDay.Hours > HourOnClockSunSetsIn24H || InGameDateTimeManager.TimeOfDay.Hours < HourOnClockPlayerForcedSleepIn24H))
            {
                if (isNightTimeAmbiencePlaying == false)
                {
                    SoundManager.Play(GlobalContent.NightTimeLoopSound, shouldLoop: true);
                }
            }
            else if (isNightTimeAmbiencePlaying)
            {
                SoundManager.Stop(GlobalContent.NightTimeLoopSound);
            }

        }

        double secondsUntilNextEffect = 5.0;
        int lastPlayedEffectNumber = 1;
        bool lastPlayAttemptWasSuccess = true;
        private void PlayBirdSong()
        {

            if (secondsUntilNextEffect > 0)
            {
                if (IsPaused == false)
                {
                    secondsUntilNextEffect -= TimeManager.SecondDifference;
                }
            }
            else
            {
                if (lastPlayAttemptWasSuccess)
                {
                    lastPlayedEffectNumber = FlatRedBallServices.Random.Next(1, 5);
                }
                var soundType = "Songbird";
                var stringName = $"{soundType}{lastPlayedEffectNumber}Sound";
                var soundEffectAsObject = GlobalContent.GetFile(stringName);
                if (soundEffectAsObject is SoundEffect soundEffect)
                {
                    lastPlayAttemptWasSuccess = SoundManager.PlayIfNotPlaying(soundEffect, soundType);
                    secondsUntilNextEffect = FlatRedBallServices.Random.Between(2, 10);
                }
            } 
        }

        private void PlaySongForDay()
        {
            Song songToPlayForDay;
            switch (PlayerDataManager.PlayerData.CurrentDay)
            {
                case 1:
                    songToPlayForDay = GlobalContent.music_calm_tree_of_life; break;
                case 2:
                    songToPlayForDay = GlobalContent.music_calm_green_lake_serenade; break;
                case 3:
                    songToPlayForDay = GlobalContent.music_oriental_sunrise; break;
                //case 4:
                //songToPlayForDay = GlobalContent.music_calm_tree_of_life; break;
                //case 5:
                //songToPlayForDay = GlobalContent.music_calm_tree_of_life; break;
                default:
                    songToPlayForDay = GlobalContent.music_calm_green_lake_serenade; break;
            }

            if (MusicManager.IsSongPlaying == false || MusicManager.CurrentSong != songToPlayForDay)
            {
                MusicManager.PlaySong(songToPlayForDay, forceRestart: true);
            }

            var minutesWhenSongMutes = (HourOnClockSunSetsIn24H * 60);
            if ((InGameDateTimeManager.TimeOfDay.Hours > HourOnClockSunSetsIn24H - 3 && InGameDateTimeManager.TimeOfDay.Hours < HourOnClockSunSetsIn24H))
            {
                
                MusicManager.MusicVolumeLevel = MusicManager.DefaultMusicLevel * (minutesWhenSongMutes - (InGameDateTimeManager.TimeOfDay.Hours*60 + InGameDateTimeManager.TimeOfDay.Minutes))/ 180f;
            }
            else if (hasEndingStarted && MusicManager.IsSongPlaying)
            {
                if (MusicManager.MusicVolumeLevel > 0)
                {
                    MusicManager.MusicVolumeLevel -= 0.3f;
                }
                else
                {
                    MusicManager.Stop();
                }
            }
            else if (MusicManager.MusicVolumeLevel != MusicManager.DefaultMusicLevel &&
                    InGameDateTimeManager.TimeOfDay.Hours < HourOnClockSunSetsIn24H && 
                    InGameDateTimeManager.TimeOfDay.Hours > HourOnClockPlayerForcedSleepIn24H)
            {
                MusicManager.MusicVolumeLevel = MusicManager.DefaultMusicLevel;
            }

        }
        protected void PlayLightShimmerAnimation()
        {
            GameScreenGum.ToBlackAnimation.Stop();
            EndingScreenTransitionInstance.GlowPulseAnimation.Play();
            EndingScreenTransitionInstance.LightTwinkleAnimation.Play();
            GameScreenGum.CurrentOverlayAnimationState = GumRuntimes.GameScreenGumRuntime.OverlayAnimation.NoOverlay;
        }

        protected bool hasEndingStarted;
#if DEBUG
        private void DebuggingActivity()
        {
            var keyboard = InputManager.Keyboard;

            if(DebuggingVariables.AwardMoneyByPressingM && keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.M))
            {
                AwardMoney(50);
            }
            if(DebuggingVariables.SkipDayWithCtrlD && keyboard.IsCtrlDown && 
                keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.D))
            {
                if (!DebuggingVariables.NextDayWillTriggerPostEnding)
                    GoToNewDay();
                else
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
                }                
            }
            if (DebuggingVariables.SkipDayWithCtrlD && keyboard.IsCtrlDown &&
                keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.R))
            {
                RestartScreen(false);
            }

            if(DebuggingVariables.PlusMinusControlsHour)
            {
                if(keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.OemPlus))
                {
                    InGameDateTimeManager.SetTimeOfDay(InGameDateTimeManager.TimeOfDay + TimeSpan.FromHours(1));
                }
                if (keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.OemMinus))
                {
                    InGameDateTimeManager.SetTimeOfDay(InGameDateTimeManager.TimeOfDay - TimeSpan.FromHours(1));

                }

            }
        }
#endif

        void CameraActivity()
        {
            var difference = (PlayerCharacterInstance.Position - Camera.Main.Position).ToVector2();
            Camera.Main.Velocity = difference.ToVector3();

        }

        private void PlayerCollisionActivity()
        {
            if (PlayerCharacterInstance.TalkInput.WasJustPressed && PlayerCharacterInstance.InputEnabled)
            {
                PlayerCharacterInstance.NpcForAction = null;

                PlayerCharacterInstanceActivityCollisionVsNPCListBodyCollision.DoCollisions();

                if (PlayerCharacterInstance.NpcForAction != null)
                {
                    var npc = PlayerCharacterInstance.NpcForAction;
                    var npcTextureRectangle = npc.GetTextureRectangle();
                    if(npc.DirectlySetDialog != null)
                    {
                        if (DialogBox.TryShow(npc.DirectlySetDialog))
                        {
                            DialoguePortrait.SetTextureCoordinates(npcTextureRectangle);
                            // For invisible NPCs, we don't want to show the portrait.
                            DialoguePortrait.Visible = npc.SpriteInstance.Visible;
                            PlayerCharacterInstance.ObjectsBlockingInput.Add(DialogBox);
                            npc.HandleDialogueSeen();
                        }

                    }
                    else
                    {
                        if (DialogBox.TryShow(npc.TwineDialogId))
                        {
                            DialoguePortrait.SetTextureCoordinates(npcTextureRectangle);
                            // For invisible NPCs, we don't want to show the portrait.
                            DialoguePortrait.Visible = npc.SpriteInstance.Visible;
                            PlayerCharacterInstance.ObjectsBlockingInput.Add(DialogBox);
                            npc.HandleDialogueSeen();
                        }
                    }
                }

                if(PlayerCharacterInstance.ObjectsBlockingInput.Count == 0 &&
                    PlayerCharacterInstance.NpcForAction == null &&
                    PlayerCharacterInstanceActivityCollisionVsPlayerHouseDoorList.DoCollisions())
                {
                    string text;
                    List<string> options = new List<string>();
                    if (PlayerDataManager.PlayerData.Has(ItemDefinition.Trailer_Key))
                    {
                        
                        text = "Call it a day?";
                        options.Add("Yes");
                        options.Add("No");
                    }
                    else
                    {
                        text = "\"Petterson Properties: Luxury mobile home for rent.\"";                        
                    }

                    var rootObject = GetRootObject(text, options);
                    DialogBox.TryShow(rootObject, HandleDoorOptionSelected);
                    PlayerCharacterInstance.ObjectsBlockingInput.Add(DialogBox);
                }

                if(PlayerCharacterInstance.ObjectsBlockingInput.Count == 0 && 
                    PlayerCharacterInstanceActivityCollisionVsFishIdentifiedSignList.DoCollisions())
                {
                    string text;
                    var identifiedDictionary = PlayerDataManager.PlayerData.TimesFishIdentified;
                    var hasIdentifiedAny = identifiedDictionary.Values.Any(item => item > 0);
                    if(!hasIdentifiedAny)
                    {
                        text = "\"203rd Ensomme Fishing Festival recorded fish counts will be posted here throughout the day!\"";
                    }
                    else
                    {
                        text = "\"203rd Ensomme Fishing Festival Recorded fish:";
                        foreach(var kvp in identifiedDictionary)
                        {
                            text += $"\n{kvp.Key} {kvp.Value}";
                        }
                        text += "\"";                        
                    }
                    var rootObject = GetRootObject(text, new List<string>());
                    DialogBox.TryShow(rootObject);
                    PlayerCharacterInstance.ObjectsBlockingInput.Add(DialogBox);
                }
            }


            DoFishingActivity();
        }

        public void SetDialoguePortraitFor(NPC npc)
        {
            var npcTextureRectangle = npc.GetTextureRectangle();
            DialoguePortrait.SetTextureCoordinates(npcTextureRectangle);
            DialoguePortrait.Visible = npc.SpriteInstance.Visible;
        }

        private void HandleDoorOptionSelected(DialogTreeRaw.Link selectedLink)
        {
            DialogBox.TryHide();
            PlayerCharacterInstance.ObjectsBlockingInput.Remove(DialogBox);
            if(selectedLink.name == "Yes")
            {
                GoToNewDay();
            }

        }

        private void ForcePlayerToSleep()
        {
            isBeingForcedToSleep = true;
            AddNotification("It's getting late. You can't keep your eyes open much longer...");
            float delayBeforeStartingNewDaySequence = 5;
            this.Call(() => GoToNewDay()).After(delayBeforeStartingNewDaySequence);
        }

        private void DoFishingActivity()
        {
            if (PlayerDataManager.PlayerData.Has(ItemDefinition.Fishing_Rod))
            {
                if (PlayerCharacterInstance.IsFishing == false && PlayerCharacterInstance.TalkInput.WasJustPressed && PlayerCharacterInstance.ObjectsBlockingInput.Any() == false &&
               PlayerCharacterInstanceFishingCollisionVsWaterCollision.DoCollisions())
                {
                    var baitSelection = GetBaitRootObject();

                    if (baitSelection == null)
                    {
                        AddNotification("Can't fish - no bait");
                    }
                    else
                    {
                        if (DialogBox.TryShow(baitSelection, HandleFishingLinkSelected))
                        {
                            PlayerCharacterInstance.ObjectsBlockingInput.Add(DialogBox);
                        }
                    }
                }
                else if (PlayerCharacterInstance.IsFishing && PlayerCharacterInstance.LastTimeFishingStarted != TimeManager.CurrentScreenTime)
                {
                    if (PlayerCharacterInstance.TalkInput.WasJustPressed && PlayerCharacterInstance.IsFishOnLine)
                    {
                        HandleFishCaught();
                        PlayerCharacterInstance.StopFishing();
                    } 
                    else if (PlayerCharacterInstance.TalkInput.WasJustPressed || PlayerCharacterInstance.CancelInput.WasJustPressed)
                    { 
                        PlayerCharacterInstance.StopFishing();
                        AddNotification("You cut your bait.");
                    }
                }
            }           
        }

        private void HandleFishCaught()
        {
            string fishCaught = GetFishCaught(PlayerCharacterInstance.CurrentBait);
            PlayerDataManager.PlayerData.AwardItem(fishCaught);
            AddNotification($"Caught {fishCaught}");
            SoundManager.Play(GlobalContent.FishCatchSound);
            // increment the number caught so that unique items can be ignored. 
            GlobalContent.ItemDefinition[fishCaught].TotalCaught++;
        }

        private void UpdatePropObjectsLights()
        {
            var lightShouldBeOn = InGameDateTimeManager.TimeOfDay.TotalHours >= HourOnClockLightPostsTurnOnIn24H ||
                                InGameDateTimeManager.TimeOfDay.TotalHours < HourOnClockLightPostsTurnOffIn24H;
            var lights = PropObjectList.Where(po => po.CurrentPropNameState == PropName.StreetLight || po.CurrentPropNameState == PropName.TriStreetLight);
            foreach (var lightSource in lights)
            {
                if (lightShouldBeOn && lightSource.CurrentChainName != "On")
                {
                    lightSource.ShowLight();

                }
                else if (!lightShouldBeOn && lightSource.CurrentChainName != "Off")
                {
                    lightSource.HideLight();
                }
            }
            if (PlayerCharacterInstance.Lantern.SpriteInstanceVisible != lightShouldBeOn)
            {
                PlayerCharacterInstance.Lantern.SpriteInstanceVisible = lightShouldBeOn;
                PlayerCharacterInstance.Lantern.LightSpriteInstanceVisible = lightShouldBeOn;
            }
        }

        private string GetFishCaught(string baitType)
        {
            var lootTable = GlobalContent.DefaultLootTable;

            // see if the player is colliding with any fishing zones
            var collidedFishingZone = this.FishingZoneList.FirstOrDefault(item => 
                item.IsActive &&
                item.CollideAgainst(PlayerCharacterInstance.BodyCollision));
            if(collidedFishingZone != null)
            {
                if(string.IsNullOrEmpty(collidedFishingZone.LootTable))
                {
                    throw new Exception("The current zone does not have a specified LootTable, it should");
                }
                lootTable = (Dictionary<string, FishingLootTable>)GlobalContent.GetFile(collidedFishingZone.LootTable);

                if(lootTable == null)
                {
                    throw new Exception($"The loot table {collidedFishingZone.LootTable} either doesn't exist or it doesn't use FishingLootTable created class");
                }
            }
            // David: I'm putting some "temp" (maybe?) code in here to address 
            // https://github.com/dantogno/FishStory/issues/135
            switch (baitType)
            {
                case ItemDefinition.Blood_Worm:
                    baitType = "BloodWorm";
                    break;
                case ItemDefinition.Little_Bonito:
                    baitType = "LittleBonito";
                    break;
                case ItemDefinition.Strange_Bait:
                    baitType = "StrangeBait";
                    break;
                default:
                    break;
            }
            var field = typeof(FishingLootTable).GetField(baitType);

            if(field == null)
            {
                throw new InvalidOperationException($"Trying to find a row for bait {baitType} but couldn't find it");
            }

            List<FishWeight> fishWeights = new List<FishWeight>();

            foreach(var kvp in lootTable)
            {
                var weightAsObject = field.GetValue(kvp.Value);
                int weight = 0;
                if (weightAsObject != null)
                {
                    weight = (int)weightAsObject;
                }
                // if the item is unique, and has already been caught, set the weight to 0 so it doesn't get caught again.
                if (GlobalContent.ItemDefinition[kvp.Key].IsUnique && GlobalContent.ItemDefinition[kvp.Key].TotalCaught > 0)
                    weight = 0;
                if(weight != 0)
                {
                    var fishWeight = new FishWeight
                    {
                        Fish = kvp.Key,
                        Weight = weight
                    };
                    fishWeights.Add(fishWeight);
                }
            }

            var sum = fishWeights.Sum(item => item.Weight);

            var randomValue = FlatRedBallServices.Random.Between(0, sum);

            var sumSoFar = 0.0f;

            foreach(var item in fishWeights)
            {
                sumSoFar += item.Weight;

                if(randomValue < sumSoFar)
                {
                    return item.Fish;
                }
            }

            return fishWeights.Last().Fish;
        }

        private void HandleFishingLinkSelected(DialogTreeRaw.Link selectedLink)
        {
            var text = selectedLink.name;

            PlayerCharacterInstance.ObjectsBlockingInput.Remove(DialogBox);
            DialogBox.TryHide();
            if(text == "Cancel")
            {
                // do nothing
            }
            else
            {
                var baitType = text.Substring(0, text.LastIndexOf(" ("));

                PlayerDataManager.PlayerData.RemoveItem(baitType);
                AddNotification($"Used {baitType}");

                PlayerCharacterInstance.StartFishing(baitType);
                SoundManager.Play(GlobalContent.FishingCastSound);
            }
        }

        private RootObject GetBaitRootObject()
        {
            var inventory = PlayerDataManager.PlayerData.ItemInventory;

            var baitItems = inventory
                .Where(item => item.Value > 0)
                .Where(item => GlobalContent.ItemDefinition[item.Key].IsBait)
                .ToList();
            if(baitItems.Count == 0)
            {
                return null;
            }
            else
            {
                string text = "Select Bait";
                List<string> options = new List<string>();

                foreach (var item in baitItems)
                {
                    options.Add($"{item.Key} ({item.Value})");
                }
                options.Add("Cancel");

                return GetRootObject(text, options);
            }
        }

        private RootObject GetRootObject(string text, List<string> options)
        {

            var rootObject = new RootObject();
            rootObject.startnode = "start";

            List<Passage> passages = new List<Passage>();

            var mainPassage = new Passage();
            mainPassage.name = "start";
            mainPassage.pid = "start";
            mainPassage.text = text;


            var links = new List<DialogTreePlugin.SaveClasses.DialogTreeRaw.Link>();

            int id = 1;
            foreach (var option in options)
            {
                var link = new DialogTreePlugin.SaveClasses.DialogTreeRaw.Link();
                link.name = option;
                link.pid = id.ToString();
                links.Add(link);
                id++;
            }

            mainPassage.links = links.ToArray();

            passages.Add(mainPassage);

            rootObject.passages = passages;

            return rootObject;
        }

        private void HandleFishLost()
        {
            AddNotification("Fish got away with bait");
            SoundManager.Play(GlobalContent.FishGotAwaySound);
        }

        public void GoToNewDay()
        {
            // stop player from moving
            PlayerCharacterInstance.ObjectsBlockingInput.Add(GameScreenGum.OverlayInstance);

            FadeToBlack();

            // reset purchases
            this.ItemsBought.Clear();

            // research tracked day
            InGameDateTimeManager.ResetDay();
            isBeingForcedToSleep = false;

            // Move player to their trailer
            this.Call(() =>
            {
                PlayerCharacterInstance.X = 695;
                PlayerCharacterInstance.Y = -855;
                PlayerCharacterInstance.DirectionFacing = TopDownDirection.Right;
            }).After(GameScreenGum.ToBlackAnimation.Length);

            
            // The transition from day 3 to 4 is a special case... (the ending)
            if (PlayerDataManager.PlayerData.CurrentDay != 3)
            {
                float delayBeforeShowingNotifications = 0.5f;
                this.Call(() =>
                {
                    float delayBetweenFadeOutAndFadeIn = 2f;
                    this.Call(FadeIn).After(GameScreenGum.ToBlackAnimation.Length + delayBetweenFadeOutAndFadeIn);
                    // allow player movement, display notification
                    PlayerCharacterInstance.ObjectsBlockingInput.Remove(GameScreenGum.OverlayInstance);

                    PlayerDataManager.PlayerData.CurrentDay++;
                    isBeingForcedToSleep = false;

                    var numberOfFishSpoiled = PlayerDataManager.PlayerData.SpoilItemsAndReturnCount();
                    if (numberOfFishSpoiled > 0)
                    {
                        SoundManager.Play(GlobalContent.FishSpoiledSound);
                        AddNotification($"The {numberOfFishSpoiled} fish you caught yesterday went bad.");
                    }

                    AddNotification($"Fishing Festival: Day {PlayerDataManager.PlayerData.CurrentDay}");

                }).After(GameScreenGum.ToBlackAnimation.Length + GameScreenGum.ToTransparentAnimation.Length + delayBeforeShowingNotifications);
            }
            // David: TODO: handle this in the level script? That way I have refs to the ifs and dos
            else
            {
                PlayerDataManager.PlayerData.CurrentDay++;
                isBeingForcedToSleep = false;
            }
        }


#region UI Activity

        private void UiActivity()
        {
            //if(InputManager.Keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.Space))
            //{
            //    GameScreenGum.NotificationBoxInstance.AddNotification($"You pushed space at {DateTime.Now.ToShortTimeString()}");
            //}
            var anyInterfaceVisible = DialogBox.Visible || InventoryInstance.Visible|| StoreInstance.Visible;
            if (!IsPaused && PlayerCharacterInstance.PauseInput.WasJustPressed)
            {
                GameScreenGum.PauseMenuInstance.Visible = true;
                PauseThisScreen();
            }

            if (IsPaused && !anyInterfaceVisible)
            {
                //Pause screen interface
                GameScreenGum.PauseMenuInstance.CustomActivity();
            }
            else
            {
                DialogBox.CustomActivity();

                GameScreenGum.StoreInstance.CustomActivity();

                GameScreenGum.NotificationBoxInstance.CustomActivity();

                if (InventoryInstance.Visible == false && PlayerCharacterInstance.InventoryInput.WasJustPressed && PlayerCharacterInstance.ObjectsBlockingInput.Any() == false)
                {
                    ShowInventory(InventoryRuntime.ViewOrSell.View, 1.0f, InventoryRestrictions.NoRestrictions);
                }
                else
                {
                    InventoryInstance.CustomActivity();
                }

                DayAndTimeDisplayInstance.UpdateTime(InGameDateTimeManager.OurInGameDay);
            }
        }

  

        protected void AddNotification(string notification) =>
            GameScreenGum.NotificationBoxInstance.AddNotification(notification);

        private void HandleStoreShouldShow(string storeName)
        {
            var store = GameScreenGum.StoreInstance;
            store.Visible = true;
            store.PlayerMoneyText = "$" + PlayerDataManager.PlayerData.Money.ToString();

            store.PopulateFromStoreName(storeName, ItemsBought);
            PauseThisScreen();
        }

        private void HandleSellingShouldShow(string sellerName)
        {
            sellerName = sellerName?.ToLowerInvariant();
            var isBlackMarket = sellerName == "blackmarket";
            var isFishStore = sellerName == "fishmonger";

            var sellPriceMultiplier = isBlackMarket ? 
                NPC.BlackMarketSellPriceMultiplier : 1.0f;
            var inventoryRestrictions = isFishStore ?
                InventoryRestrictions.IdentifiedFishOnly :
                InventoryRestrictions.NoRestrictions;

            InventoryRuntime.ViewOrSell sellType;

            if (isBlackMarket)
            {
                SoundManager.Play(GlobalContent.BlackMarketStoreOpenSound);
                sellType = InventoryRuntime.ViewOrSell.SellToBlackMarket;
            }
            else
            {
                SoundManager.Play(GlobalContent.StoreOpenSound);
                sellType = InventoryRuntime.ViewOrSell.SellToStore;
            }

            ShowInventory(sellType, sellPriceMultiplier, inventoryRestrictions);
        }
        //todo - need to actually call this if text is "id="
        private void HandleIdentify()
        {
            var itemDefinitions = GlobalContent.ItemDefinition;

            bool IsUnidentified(string itemName)
            {
                return !string.IsNullOrEmpty(itemDefinitions[itemName].AssociatedItem);
            }

            var toConvert = PlayerDataManager.PlayerData.ItemInventory
                .Where(item => IsUnidentified(item.Key))
                .ToArray();

            Dictionary<string, int> newItemCounts = new Dictionary<string, int>();

            foreach(var item in toConvert)
            {
                var itemName = item.Key;

                var itemDefinition = GlobalContent.ItemDefinition[itemName];

                var newItemName = itemDefinition.AssociatedItem;

                for(int i = 0; i < item.Value; i++)
                {
                    PlayerDataManager.PlayerData.AwardItem(newItemName);
                    PlayerDataManager.PlayerData.RemoveItem(itemName);
                }

                if (item.Value > 0)
                {
                    newItemCounts[newItemName] = item.Value;
                    PlayerDataManager.PlayerData.TimesFishIdentified.IncrementBy(newItemName, item.Value);
                }
            }


            foreach(var newItemKvp in newItemCounts)
            {
                AddNotification($"Identified {newItemKvp.Key} ({newItemKvp.Value})");
            }
            if (newItemCounts.Any() == false)
            {
                AddNotification("You have nothing to identify.");
            }
            SoundManager.Play(GlobalContent.FishIdentificationSound);
        }

        private void HandleSellClicked()
        {
            var inventory = GameScreenGum.InventoryInstance;
            var selectedItemName = inventory.SelectedItemName;

            if(!string.IsNullOrEmpty(selectedItemName))
            {
                var item = GlobalContent.ItemDefinition[selectedItemName];

                if(item.PlayerSellingCost <= 0)
                {
                    AddNotification("Item cannot be sold");
                }
                else
                {
                    // remove the item from inventory
                    PlayerDataManager.PlayerData.RemoveItem(selectedItemName);

                    // award money
                    PlayerDataManager.PlayerData.Money += 
                        (int)(item.PlayerSellingCost * inventory.LastSellPriceMultiplier);

                    // refresh the UI
                    inventory.FillWithInventory(
                        PlayerDataManager.PlayerData.ItemInventory,
                        inventory.LastSellPriceMultiplier,
                        inventory.InventoryRestrictions

                        );
                    inventory.PlayerMoneyText = "$" + PlayerDataManager.PlayerData.Money.ToString();

                    inventory.SelectedItemName = selectedItemName;

                    // Show the notification
                    AddNotification($"Sold {selectedItemName}");
                }
            }
            SoundManager.Play(GlobalContent.StoreBuySound);
        }

        private void HandleInventoryClosed()
        {
            PlayerCharacterInstance.ObjectsBlockingInput.Remove(GameScreenGum.InventoryInstance);
            if (GameScreenGum.InventoryInstance.CurrentViewOrSellState == InventoryRuntime.ViewOrSell.View)
            {
                SoundManager.Play(GlobalContent.InventoryCloseSound);
            }
            UnpauseThisScreen();
        }

        private void HandleDialogBoxHide()
        {
            if(PlayerCharacterInstance.ObjectsBlockingInput.Contains(DialogBox))
            {
                PlayerCharacterInstance.ObjectsBlockingInput.Remove(DialogBox);
            }
            DialoguePortrait.Visible = false;
        }

        private void HandleDialogTagShown(string tag)
        {
            dialogTagsThisFrame.Add(tag);
        }

        private void HandleBuyClicked(IWindow window)
        {
            var store = GameScreenGum.StoreInstance;
            if (store.SelectedShopItem == null)
            {
                AddNotification("Select item first, then click buy");
            }
            else if(store.SelectedShopItem.Stock == 0)
            {
                AddNotification("This item is sold out");
            }
            else
            {
                var itemToBuy = 
                    GlobalContent.ItemDefinition[store.SelectedShopItem.Item];

                if(itemToBuy.PlayerBuyingCost > PlayerDataManager.PlayerData.Money)
                {
                    AddNotification("You do not have enough money");
                }
                else
                {
                    store.ItemsBoughtFromThisStore.Add(itemToBuy.Name);

                    BuyItem(itemToBuy);

                    store.RefreshStoreItems();
                }
            }
        }

        private void BuyItem(ItemDefinition itemToBuy)
        {
            AddNotification($"Bought {itemToBuy.Name}");
            PlayerDataManager.PlayerData.Money -= itemToBuy.PlayerBuyingCost;
            PlayerDataManager.PlayerData.AwardItem(itemToBuy.Name);

            GameScreenGum.StoreInstance.PlayerMoneyText = $"${PlayerDataManager.PlayerData.Money}";
            SoundManager.Play(GlobalContent.StoreBuySound);
        }

        private void ShowInventory(InventoryRuntime.ViewOrSell state, 
            float sellPriceMultiplier, InventoryRestrictions inventoryRestrictions)
        {
            var inventory = GameScreenGum.InventoryInstance;

            inventory.Visible = true;
            inventory.CurrentViewOrSellState = state;
            inventory.FillWithInventory(PlayerDataManager.PlayerData.ItemInventory, 
                sellPriceMultiplier, inventoryRestrictions);
            inventory.PlayerMoneyText = "$" + PlayerDataManager.PlayerData.Money.ToString();

            PlayerCharacterInstance.ObjectsBlockingInput.Add(inventory);

            SoundManager.Play(GlobalContent.InventoryOpenSound);
            PauseThisScreen();
        }

#endregion

#endregion

#region Script-helping methods
        public void RemoveTag(string tag)
        {
            dialogTagsThisFrame.Remove(tag);
        }
        public bool HasTag(string tag) =>
            dialogTagsThisFrame.Contains(tag);

        public void AwardMoney(int amountOfMoney)
        {
            GameScreenGum.NotificationBoxInstance.AddNotification($"Earned ${amountOfMoney}");

            PlayerDataManager.PlayerData.Money += amountOfMoney;
        }

        protected void FadeIn()
        {
            // fade UI in
            GameScreenGum.ToTransparentAnimation.Play();
        }

        protected static void FadeToBlack()
        {
            GameScreenGum.ToBlackAnimation.Play();
        }

        protected bool IsNpcOnScreen(string npcName)
        {
            var npc = NPCList.FirstOrDefault(item => item.Name == npcName);

            if(npc == null)
            {
                throw new Exception($"Could not find NPC with name {npcName}");
            }

            return npc.IsOnScreen();
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
