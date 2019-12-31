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
using FlatRedBall.TileEntities;
using FishStory.Managers;
using FlatRedBall.Gui;
using FishStory.DataTypes;

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

            TileEntityInstantiator.CreateEntitiesFrom(Map);

            DialogBox.Visible = false;

            InitializeCamera();

            InitializeCollision();

            InitializeUi();

            InitializeRestartVariables();
        }

        private void InitializeCamera()
        {
            Camera.Main.X = PlayerCharacterInstance.X;
            Camera.Main.Y = PlayerCharacterInstance.Y;
        }

        private void InitializeCollision()
        {
            PlayerCharacterInstanceActivityCollisionVsNPCListBodyCollision.CollisionOccurred +=
                HandlePlayerVsNpcActivityCollision;
        }

        private void InitializeUi()
        {
            #region Initialize Dialog Box

            if (PlayerCharacterInstance.InputDevice is Keyboard keyboard)
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
            DialogBox.StoreShouldShow += HandleStoreShouldShow;
            DialogBox.DialogTagShown += HandleDialogTagShown;

            #endregion

            GameScreenGum.StoreInstance.CancelInput = PlayerCharacterInstance.CancelInput;
            GameScreenGum.StoreInstance.Visible = false;
            GameScreenGum.StoreInstance.BuyButtonClick += HandleBuyClicked;

            GameScreenGum.NotificationBoxInstance.UpdateVisibility();
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
        }

        #endregion

        #region Activity

        void CustomActivity(bool firstTimeCalled)
        {
            dialogTagsThisFrame.Clear();
            if(InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.RightButton))
            {
                RestartScreen(true);
            }
            CameraActivity();

            UiActivity();

            CollisionActivity();

            DebuggingActivity();

            // do script *after* the UI
            script.Activity();
        }

        private void DebuggingActivity()
        {
            var keyboard = InputManager.Keyboard;

            if(DebuggingVariables.AwardMoneyByPressingM && keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.M))
            {
                AwardMoney(50);
            }
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
                    if(DialogBox.TryShow(PlayerCharacterInstance.NpcForAction.TwineDialogId))
                    {
                        PlayerCharacterInstance.InputEnabled = false;
                    }
                }

            }
        }

        #region UI Activity

        private void UiActivity()
        {
            //if(InputManager.Keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.Space))
            //{
            //    GameScreenGum.NotificationBoxInstance.AddNotification($"You pushed space at {DateTime.Now.ToShortTimeString()}");
            //}

            DialogBox.CustomActivity();

            GameScreenGum.StoreInstance.CustomActivity();

            GameScreenGum.NotificationBoxInstance.CustomActivity();
        }

        private void AddNotification(string notification) =>
            GameScreenGum.NotificationBoxInstance.AddNotification(notification);
        private void HandleStoreShouldShow(string storeName)
        {
            var store = GameScreenGum.StoreInstance;
            store.Visible = true;
            store.PlayerMoneyText = "$" + PlayerDataManager.PlayerData.Money.ToString();

            store.PopulateFromStoreName(storeName);
        }

        private void HandleDialogBoxHide()
        {
            PlayerCharacterInstance.InputEnabled = true;
        }

        private void HandleDialogTagShown(string tag)
        {
            dialogTagsThisFrame.Add(tag);
        }

        private void HandleBuyClicked(IWindow window)
        {

            if(GameScreenGum.StoreInstance.SelectedShopItem == null)
            {
                AddNotification("Select item first, then click buy");
            }
            else
            {
                var itemToBuy = 
                    GlobalContent.ItemDefinition[GameScreenGum.StoreInstance.SelectedShopItem.Item];

                if(itemToBuy.PlayerBuyingCost > PlayerDataManager.PlayerData.Money)
                {
                    AddNotification("You do not have enough money");
                }
                else
                {
                    BuyItem(itemToBuy);
                }
            }
            //if(PlayerDataManager.PlayerData.Money >= itemToBuy.)
        }

        private void BuyItem(ItemDefinition itemToBuy)
        {
            AddNotification($"Bought {itemToBuy.Name}");
            PlayerDataManager.PlayerData.Money -= itemToBuy.PlayerBuyingCost;
            PlayerDataManager.PlayerData.AwardItem(itemToBuy.Name);

            GameScreenGum.StoreInstance.PlayerMoneyText = $"${PlayerDataManager.PlayerData.Money}";
        }

        #endregion

        #endregion

        #region Script-helping methods

        public bool HasTag(string tag) =>
            dialogTagsThisFrame.Contains(tag);

        public void AwardMoney(int amountOfMoney)
        {
            GameScreenGum.NotificationBoxInstance.AddNotification($"Earned ${amountOfMoney}");

            PlayerDataManager.PlayerData.Money += amountOfMoney;
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
