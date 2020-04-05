using FishStory.Forms;
using FishStory.GumRuntimes.DefaultForms;
using FishStory.Managers;
using FlatRedBall.Forms.Controls;
using FlatRedBall.Input;
using Gum.Wireframe;
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

        private ListBox listBox;
        private ScrollBar VerticalScrollBar => ListBoxInstance?.ScrollBar?.FormsControl;

        public Action SellClicked;

        public IPressableInput UpInput { get; set; }
        public IPressableInput DownInput { get; set; }
        public IPressableInput SelectInput { get; set; }

        public IPressableInput CancelInput { get; internal set; }
        public IPressableInput InventoryInput { get; internal set; }

        public float LastSellPriceMultiplier { get; private set; }
        public InventoryRestrictions InventoryRestrictions { get; private set; }

        public int OptionCount => listBox.Items.Count();
        public string SelectedItemName
        {
            get => CurrentlySelectedItem?.ItemName;
            set
            {
                listBox.SelectedObject = listBox.Items
                    .FirstOrDefault(item => ((ItemWithCount)item).ItemName == value);
            }
        }
        public int? SelectedIndex
        {
            get
            {
                ItemWithCount selectedOption = CurrentlySelectedItem;

                if (selectedOption == null)
                {
                    if (listBox.Items?.Count() == 1)
                    {
                        return 0;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return listBox.Items.IndexOf(selectedOption);
                }
            }
            set
            {
                if (value.HasValue)
                {
                    listBox.SelectedObject = listBox.Items[value.Value];
                }
                else
                {
                    if (listBox.SelectedObject != null)
                        (listBox.SelectedObject as InventoryListItemRuntime).CurrentListBoxItemCategoryState = InventoryListItemRuntime.ListBoxItemCategory.Enabled;

                    listBox.SelectedObject = null;
                }

                int index = 0;
                foreach (var item in listBox.Items)
                {
                    if (item is InventoryListItemRuntime selectableItem)
                    {
                        if (index++ == value)
                        {
                            selectableItem.CurrentListBoxItemCategoryState = InventoryListItemRuntime.ListBoxItemCategory.Selected;
                        }
                        else
                        {
                            selectableItem.CurrentListBoxItemCategoryState = InventoryListItemRuntime.ListBoxItemCategory.Enabled;
                        }
                        index++;
                    }
                }
                if (CurrentlySelectedItem!= null)
                    ListBoxInstance.FormsControl.ScrollIntoView(CurrentlySelectedItem);
            }
        }

        public ItemWithCount CurrentlySelectedItem
        {
            get => listBox.SelectedObject as ItemWithCount;
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
                HandlePlayerInput();
                UpdateSellButtonDisplay();
            }
        }

        private void HandlePlayerInput()
        {
            if (CancelInput.WasJustPressed ||
                (CurrentViewOrSellState == ViewOrSell.View && InventoryInput.WasJustPressed))
            {
                Close();
            }
            else
            {
                if (UpInput.WasJustPressed)
                {
                    if (!SelectedIndex.HasValue)
                        SelectedIndex = 0;

                    if (SelectedIndex == 0)
                    {
                        SelectedIndex = OptionCount - 1;
                    }
                    else
                    {
                        SelectedIndex--;
                    }
                    SoundManager.Play(GlobalContent.MenuMoveSound);
                }
                if (DownInput.WasJustPressed)
                {
                    if (!SelectedIndex.HasValue)
                        SelectedIndex = 0;

                    if (SelectedIndex == OptionCount - 1)
                    {
                        SelectedIndex = 0;
                    }
                    else
                    {
                        SelectedIndex++;
                    }
                    SoundManager.Play(GlobalContent.MenuMoveSound);
                }
                if (SelectInput.WasJustPressed)
                {
                    if (CurrentViewOrSellState != ViewOrSell.View)
                    {
                        SellButton.CallClick();
                    }
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
            UpdateSellButtonDisplay();
            SoundManager.Play(GlobalContent.ItemSelectedSound);
        }

        private void UpdateSellButtonDisplay()
        {
            if (SellButton.Visible != (CurrentViewOrSellState == ViewOrSell.SellToBlackMarket || CurrentViewOrSellState == ViewOrSell.SellToStore))
            {
                SellButton.Visible = (CurrentViewOrSellState == ViewOrSell.SellToBlackMarket || CurrentViewOrSellState == ViewOrSell.SellToStore);
            }

            if (CurrentViewOrSellState != ViewOrSell.View)
            {
                if (CurrentlySelectedItem is null || CurrentlySelectedItem.Count <= 0)
                {
                    DisableSellButton();
                }
                else
                {
                    EnableSellButton();
                }
            }
        }

        private void DisableSellButton()
        {
            this.SellButton.CurrentButtonCategoryState = StoreBuyButtonRuntime.ButtonCategory.Disabled;
            SellButton.Enabled = false;
        }

        private void EnableSellButton()
        {
            this.SellButton.CurrentButtonCategoryState = StoreBuyButtonRuntime.ButtonCategory.Enabled;
            SellButton.Enabled = true;
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
                                    shouldShow = item.IsFish && string.IsNullOrEmpty(item.AssociatedItem);                                     
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
