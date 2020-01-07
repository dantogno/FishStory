using System;
using System.Collections.Generic;
using System.Linq;
using FishStory.DataTypes;
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

        partial void CustomInitialize () 
        {
            listBox = 
                this.ListBoxInstance.FormsControl;

            listBox.ListBoxItemGumType = typeof(GumRuntimes.DefaultForms.InventoryListItemRuntime);
            listBox.ListBoxItemFormsType = typeof(Forms.StoreListBoxItem);
            listBox.SelectionChanged += HandleListBoxSelectionChanged;

            this.CloseButton.FormsControl.Click += (not, used) => this.Visible = false;
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
        }
    }
}
