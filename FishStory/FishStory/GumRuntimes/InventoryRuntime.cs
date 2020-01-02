using FishStory.Forms;
using FlatRedBall.Forms.Controls;
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

        public string SelectedItemName => (listBox.SelectedObject as ItemWithCount).ItemName;

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
                    var item = new ItemWithCount
                    {
                        ItemName = kvp.Key,
                        Count = kvp.Value
                    };
                    listBox.Items.Add(item);
                }

            }
            UpdateCurrentDescription();
        }

        #endregion
    }
}
