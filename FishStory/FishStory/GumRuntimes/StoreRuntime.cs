using System;
using System.Collections.Generic;
using System.Linq;
using FishStory.DataTypes;
using FishStory.GumRuntimes.DefaultForms;
using FishStory.Managers;
using FlatRedBall.Forms.Controls;
using FlatRedBall.Input;
using FlatRedBall.IO;

namespace FishStory.GumRuntimes
{
    public partial class StoreRuntime
    {
        public IPressableInput UpInput { get; set; }
        public IPressableInput DownInput { get; set; }
        public IPressableInput SelectInput { get; set; }
        public IPressableInput CancelInput { get; internal set; }
        public IPressableInput InventoryInput { get; internal set; }
        ListBox listBox;

        public Dictionary<string, ShopItem> CurrentStore;
        public List<string> ItemsBoughtFromThisStore;
        public int OptionCount => listBox.Items.Count();

        public ShopItem SelectedShopItem => listBox.SelectedObject as ShopItem;
        public int? SelectedIndex
        {
            get
            {
                var selectedOption = CurrentlySelectedItem;

                if (selectedOption == null)
                {
                    return null;
                }
                else
                {
                    return listBox.Items.IndexOf(selectedOption);
                }
            }
            set
            {
                if (value.HasValue && value.Value >= 0)
                {
                    listBox.SelectedObject = listBox.Items[value.Value];
                }
                else
                {
                    listBox.SelectedObject = null;
                }
            }
        }

        private void RefreshSelectionDisplay()
        {
            int itemCount = OptionCount;

            for (var i = 0; i < itemCount; i++)
            {
                if (ListBoxInstance.FormsControl.InnerPanel.Children[i] is InventoryListItemRuntime selectableItem)
                {
                    if (i == SelectedIndex)
                    {
                        selectableItem.CurrentListBoxItemCategoryState = InventoryListItemRuntime.ListBoxItemCategory.Selected;
                    }
                    else
                    {
                        selectableItem.CurrentListBoxItemCategoryState = InventoryListItemRuntime.ListBoxItemCategory.Enabled;
                    }
                }
            }
        }

        public ShopItem CurrentlySelectedItem
        {
            get => listBox.SelectedObject as ShopItem;
        }
        public ItemDefinition ItemToBuy => GlobalContent.ItemDefinition[SelectedShopItem.Item];

        #region Events

        public event Action Closed;

        #endregion

        partial void CustomInitialize () 
        {
            listBox =  this.ListBoxInstance.FormsControl;

            listBox.ListBoxItemGumType = typeof(GumRuntimes.DefaultForms.InventoryListItemRuntime);
            listBox.ListBoxItemFormsType = typeof(Forms.StoreListBoxItem);
            listBox.SelectionChanged += HandleListBoxSelectionChanged;

            this.CloseButton.FormsControl.Click += (not, used) => Close();
        }
        
        private void Close()
        {
            Visible = false;

            Closed?.Invoke();
        }

        private void HandleListBoxSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            RefreshSelectionDisplay();
            ListBoxInstance.FormsControl.ScrollIntoView(CurrentlySelectedItem);
            UpdateBuyButtonDisplay();
            SoundManager.Play(GlobalContent.StoreItemSelectSound);
        }

        private void UpdateBuyButtonDisplay()
        {
            if (SelectedShopItem is null || SelectedShopItem.Stock <= 0 || ItemToBuy.PlayerBuyingCost > PlayerDataManager.PlayerData.Money)
            {
                DisableBuyButton();
            }
            else
            {
                EnableBuyButton();
            }
        }

        private void DisableBuyButton()
        {
            this.BuyButton.CurrentButtonCategoryState = StoreBuyButtonRuntime.ButtonCategory.Disabled;
            BuyButton.Enabled = false;
        }

        private void EnableBuyButton()
        {
            this.BuyButton.CurrentButtonCategoryState = StoreBuyButtonRuntime.ButtonCategory.Enabled;
            BuyButton.Enabled = true;
        }

        public void CustomActivity()
        {
            if(Visible)
            {
                HandlePlayerInput();
            }
        }

        private void HandlePlayerInput()
        {
            if (CancelInput.WasJustPressed || InventoryInput.WasJustPressed)
            {
                Close();
            }
            else
            {
                if (UpInput.WasJustPressed)
                {
                    int index = SelectedIndex ?? 0;

                    if (index == 0)
                    {
                        SelectedIndex = OptionCount - 1;
                    }
                    else
                    {
                        if (!SelectedIndex.HasValue)
                        {
                            SelectedIndex = 1;
                        }
                        else
                        {
                            SelectedIndex--;
                        }
                    }
                    SoundManager.Play(GlobalContent.MenuMoveSound);
                }
                if (DownInput.WasJustPressed)
                {
                    int index = SelectedIndex ?? 0;

                    if (SelectedIndex == OptionCount - 1)
                    {
                        SelectedIndex = 0;
                    }
                    else
                    {
                        if (!SelectedIndex.HasValue)
                        {
                            SelectedIndex = 0;
                        }
                        else
                        {
                            SelectedIndex++;
                        }
                    }
                    SoundManager.Play(GlobalContent.MenuMoveSound);
                }
                if (SelectInput.WasJustPressed)
                {
                    if (BuyButton.Enabled)
                    {
                        BuyButton.CallClick();
                    }
                }
            }
        }

        internal void PopulateFromStoreName(string storeName, Dictionary<string, List<string>> itemsBoughtToday)
        {
            this.CurrentStore = GlobalContent.GetFile(storeName) as Dictionary<string, ShopItem>;

            if(!itemsBoughtToday.ContainsKey(storeName))
            {
                itemsBoughtToday.Add(storeName, new List<string>());
            }
            this.ItemsBoughtFromThisStore = itemsBoughtToday[storeName];

            RefreshStoreItems();
            UpdateBuyButtonDisplay();
        }

        public void RefreshStoreItems()
        {
            var selectedItem = listBox.SelectedObject as ShopItem;

            listBox.Items.Clear();

            foreach (var item in CurrentStore.Values)
            {

                var clone = FileManager.CloneObject(item);

                var timesThisItemWasBought = ItemsBoughtFromThisStore.Count(
                    it => it == item.Item);

                clone.Stock = clone.Stock - timesThisItemWasBought;

                listBox.Items.Add(clone);
            }
            if(selectedItem != null)
            {
                listBox.SelectedObject = listBox.Items.FirstOrDefault(item =>
                    (item as ShopItem).Item == selectedItem.Item);
            }

            var anyItemsAvailable = false;
            foreach (var child in this.ListBoxInstance.FormsControl.InnerPanel.Children)
            {
                if (child is InventoryListItemRuntime inventoryItem)
                {
                    if (inventoryItem.Stock == "0")
                    {
                        inventoryItem.CurrentAvailabilityState = InventoryListItemRuntime.Availability.SoldOut;
                    }
                    else if (string.IsNullOrWhiteSpace(inventoryItem.Price) == false && 
                            int.TryParse(inventoryItem.Price.Replace("$",""), out int price) && 
                            price > PlayerDataManager.PlayerData.Money)
                    {
                        inventoryItem.CurrentAvailabilityState = InventoryListItemRuntime.Availability.CantAfford;
                    }
                    else
                    {
                        inventoryItem.CurrentAvailabilityState = InventoryListItemRuntime.Availability.Available;
                        anyItemsAvailable = true;
                    }

                }
            }
            if (anyItemsAvailable == false)
            {
                DisableBuyButton();
            }
        }
    }
}
