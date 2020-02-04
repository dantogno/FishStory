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
        public IPressableInput CancelInput { get; internal set; }
        ListBox listBox;

        public Dictionary<string, ShopItem> CurrentStore;
        public List<string> ItemsBoughtFromThisStore;

        public ShopItem SelectedShopItem => listBox.SelectedObject as ShopItem;

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

        }

        public void CustomActivity()
        {
            if(Visible)
            {
                if(CancelInput.WasJustPressed)
                {
                    Visible = false;
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
                    }

                }
            }
        }
    }
}
