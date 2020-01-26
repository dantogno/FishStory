using FishStory.Forms;
using FlatRedBall.Forms.Controls;
using FlatRedBall.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FishStory.GumRuntimes
{
    #region Enums

    public enum InventoryRestrictions
    {
        NoRestrictions,
        IdentifiedFishOnly
    }

    #endregion

    public partial class InventoryRuntime
    {
        #region Fields/Properties

        ListBox listBox;

        public Action SellClicked;

        public IPressableInput CancelInput { get; internal set; }
        public IPressableInput InventoryInput { get; internal set; }

        public float LastSellPriceMultiplier { get; private set; }
        public InventoryRestrictions InventoryRestrictions { get; private set; }
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

        #region Events

        public event Action Closed;

        #endregion

        #region Initialize

        partial void CustomInitialize () 
        {
            listBox = this.ListBoxInstance.FormsControl;

            listBox.ListBoxItemGumType = typeof(GumRuntimes.DefaultForms.InventoryListItemRuntime);
            listBox.ListBoxItemFormsType = typeof(Forms.InventoryListBoxItem);
            listBox.SelectionChanged += HandleListBoxSelectionChanged;

            this.CloseButton.FormsControl.Click += (not, used) => Close();

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
                    Close();
                }
                if (CurrentViewOrSellState == ViewOrSell.View && InventoryInput.WasJustPressed)
                {
                    Close();
                }
                if (SellButton.Visible != (CurrentViewOrSellState == ViewOrSell.SellToBlackMarket || CurrentViewOrSellState == ViewOrSell.SellToStore))
                {
                    SellButton.Visible = (CurrentViewOrSellState == ViewOrSell.SellToBlackMarket || CurrentViewOrSellState == ViewOrSell.SellToStore);
                }

            }
        }

        private void Close()
        {
            Visible = false;
            
            Closed?.Invoke();
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

        internal void FillWithInventory(Dictionary<string, int> itemDictionary, 
            float sellPriceMultiplier, InventoryRestrictions inventoryRestrictions)
        {
            LastSellPriceMultiplier = sellPriceMultiplier;
            InventoryRestrictions = inventoryRestrictions;
            listBox.Items.Clear();
            foreach(var kvp in itemDictionary)
            {
                if(kvp.Value > 0)
                {
                    var item = GlobalContent.ItemDefinition[kvp.Key];


                    bool shouldShow = false;
                    if(CurrentViewOrSellState == ViewOrSell.View)
                    {
                        shouldShow = true;
                    }
                    else
                    {
                        if(item.PlayerSellingCost > 0)
                        {
                            switch(inventoryRestrictions)
                            {
                                case InventoryRestrictions.NoRestrictions:
                                    shouldShow = true;
                                    break;
                                case InventoryRestrictions.IdentifiedFishOnly:
                                    shouldShow = item.IsFish;
                                    // todo - need to see if identified
                                    break;
                            }
                        }
                    }



                    if(shouldShow)
                    {
                        var itemWithCount = new ItemWithCount
                        {
                            ItemName = kvp.Key,
                            Count = kvp.Value,
                            SellPrice = (int)(item.PlayerSellingCost * sellPriceMultiplier),
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
