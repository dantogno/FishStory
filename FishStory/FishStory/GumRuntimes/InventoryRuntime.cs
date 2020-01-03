using FishStory.Forms;
using FlatRedBall.Forms.Controls;
using FlatRedBall.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FishStory.GumRuntimes
{
    public partial class InventoryRuntime
    {
        #region Fields/Properties

        ListBox listBox;

        public Action SellClicked;

        public IPressableInput CancelInput { get; internal set; }
        public IPressableInput InventoryInput { get; internal set; }


        public string SelectedItemName
        {
            get => (listBox.SelectedObject as ItemWithCount)?.ItemName;
            set
            {
                listBox.SelectedObject = listBox.Items
                    .FirstOrDefault(item => ((ItemWithCount)item).ItemName == value);
            }
        }
        #endregion

        #region Initialize

        partial void CustomInitialize () 
        {
            listBox = this.ListBoxInstance.FormsControl;

            listBox.ListBoxItemGumType = typeof(GumRuntimes.DefaultForms.InventoryListItemRuntime);
            listBox.ListBoxItemFormsType = typeof(Forms.InventoryListBoxItem);
            listBox.SelectionChanged += HandleListBoxSelectionChanged;

            this.CloseButton.FormsControl.Click += (not, used) => this.Visible = false;

            this.SellButton.FormsControl.Click += (not, used) => SellClicked();
        }

        #endregion

        #region Activity

        public void CustomActivity()
        {
            if (Visible)
            {
                if (CancelInput.WasJustPressed)
                {
                    Visible = false;
                }
                if(CurrentViewOrSellState == ViewOrSell.View && InventoryInput.WasJustPressed)
                {
                    Visible = false;
                }
            }
        }

        private void HandleListBoxSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            UpdateCurrentDescription();
        }

        private void UpdateCurrentDescription()
        {
            var selectedItem = listBox.SelectedObject as ItemWithCount;

            DataTypes.ItemDefinition item = null;

            if (selectedItem != null)
            {
                item = GlobalContent.ItemDefinition[selectedItem.ItemName];

            }
            this.CurrentDescription = item?.Description;
        }

        internal void FillWithInventory(Dictionary<string, int> itemDictionary)
        {
            listBox.Items.Clear();
            foreach(var kvp in itemDictionary)
            {
                if(kvp.Value > 0)
                {
                    var item = GlobalContent.ItemDefinition[kvp.Key];

                    var shouldShow = item.PlayerSellingCost > 0 ||
                        CurrentViewOrSellState == ViewOrSell.View;

                    if(shouldShow)
                    {
                        var itemWithCount = new ItemWithCount
                        {
                            ItemName = kvp.Key,
                            Count = kvp.Value,
                            SellPrice = item.PlayerSellingCost,
                            ViewOrSell = this.CurrentViewOrSellState == ViewOrSell.View 
                                ? DefaultForms.InventoryListItemRuntime.ViewOrSell.View 
                                : DefaultForms.InventoryListItemRuntime.ViewOrSell.Sell
                        };
                        listBox.Items.Add(itemWithCount);
                    }
                }
            }
            UpdateCurrentDescription();
        }

        #endregion
    }
}
