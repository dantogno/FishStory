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
using FishStory.GumRuntimes;

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

            InitializeEntitiesFromMap();

            DialogBox.Visible = false;

            InitializeCamera();

            SpriteManager.OrderedSortType = FlatRedBall.Graphics.SortType.ZSecondaryParentY;

            InitializeCollision();

            InitializeUi();

            InitializeRestartVariables();
        }

        private void InitializeEntitiesFromMap()
        {
            TileEntityInstantiator.CreateEntitiesFrom(Map);

            foreach(var npc in NPCList)
            {
                npc.Z = 0; // same as player so they sort
            }
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
            #region Dialog Box

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
            DialogBox.SellingShouldShow += HandleSellingShouldShow;
            DialogBox.DialogTagShown += HandleDialogTagShown;

            #endregion

            #region Store
            GameScreenGum.StoreInstance.CancelInput = PlayerCharacterInstance.CancelInput;
            GameScreenGum.StoreInstance.Visible = false;
            GameScreenGum.StoreInstance.BuyButtonClick += HandleBuyClicked;
            #endregion

            #region Inventory

            var inventory = GameScreenGum.InventoryInstance;
            inventory.Visible = false;
            inventory.SellClicked += HandleSellClicked;
            inventory.CancelInput = PlayerCharacterInstance.CancelInput;
            inventory.InventoryInput = PlayerCharacterInstance.InventoryInput;
            #endregion

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
            if(PlayerCharacterInstance.TalkInput.WasJustPressed && DialogBox.Visible == false)
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

            if(PlayerCharacterInstance.IsFishing == false && PlayerCharacterInstance.TalkInput.WasJustPressed && PlayerCharacterInstanceFishingCollisionVsWaterCollision.DoCollisions())
            {
                PlayerCharacterInstance.StartFishing();
            }
            else if(PlayerCharacterInstance.IsFishing && PlayerCharacterInstance.TalkInput.WasJustPressed)
            {
                if(PlayerCharacterInstance.IsFishOnLine)
                {
                    string fishCaught = ItemDefinition.Regular_Fish;
                    PlayerDataManager.PlayerData.AwardItem(fishCaught);
                    AddNotification($"Caught {fishCaught}");
                }
                PlayerCharacterInstance.StopFishing();
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

            InventoryUiActivity();
        }

        protected void AddNotification(string notification) =>
            GameScreenGum.NotificationBoxInstance.AddNotification(notification);

        private void HandleStoreShouldShow(string storeName)
        {
            var store = GameScreenGum.StoreInstance;
            store.Visible = true;
            store.PlayerMoneyText = "$" + PlayerDataManager.PlayerData.Money.ToString();

            store.PopulateFromStoreName(storeName);
        }

        private void HandleSellingShouldShow()
        {
            ShowInventory(InventoryRuntime.ViewOrSell.Sell);
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
                    PlayerDataManager.PlayerData.Money += item.PlayerSellingCost;

                    // refresh the UI
                    inventory.FillWithInventory(PlayerDataManager.PlayerData.ItemInventory);
                    inventory.PlayerMoneyText = "$" + PlayerDataManager.PlayerData.Money.ToString();

                    inventory.SelectedItemName = selectedItemName;

                    // Show the notification
                    AddNotification($"Sold {selectedItemName}");
                }
            }
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

        private void InventoryUiActivity()
        {
            var inventory = GameScreenGum.InventoryInstance;
            if(inventory.Visible)
            {
                inventory.CustomActivity();
            }
            else if (inventory.Visible == false && PlayerCharacterInstance.InventoryInput.WasJustPressed)
            {
                ShowInventory(InventoryRuntime.ViewOrSell.View);
            }
        }

        private static void ShowInventory(InventoryRuntime.ViewOrSell state)
        {
            var inventory = GameScreenGum.InventoryInstance;

            inventory.Visible = true;
            inventory.CurrentViewOrSellState = state;
            inventory.FillWithInventory(PlayerDataManager.PlayerData.ItemInventory);
            inventory.PlayerMoneyText = "$" + PlayerDataManager.PlayerData.Money.ToString();
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
