using System;
using System.Collections.Generic;
using System.Linq;
using FishStory.DataTypes;
using FlatRedBall.Forms.Controls;
using FlatRedBall.Input;

namespace FishStory.GumRuntimes
{
    public partial class StoreRuntime
    {
        public IPressableInput CancelInput { get; internal set; }
        ListBox listBox;

        public ShopItem SelectedShopItem => listBox.SelectedObject as ShopItem;

        partial void CustomInitialize () 
        {
            listBox = 
                this.ListBoxInstance.FormsControl;

            listBox.ListBoxItemGumType = typeof(GumRuntimes.DefaultForms.StoreListItemRuntime);
            listBox.ListBoxItemFormsType = typeof(Forms.StoreListBoxItem);
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

        internal void PopulateFromStoreName(string storeName)
        {
            var shop = GlobalContent.Shop1;

            listBox.Items.Clear();

            foreach (var item in shop.Values)
            {
                listBox.Items.Add(item);
            }
        }
    }
}
