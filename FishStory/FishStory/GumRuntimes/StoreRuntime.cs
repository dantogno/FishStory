using System;
using System.Collections.Generic;
using System.Linq;
using FlatRedBall.Forms.Controls;
using FlatRedBall.Input;

namespace FishStory.GumRuntimes
{
    public partial class StoreRuntime
    {
        public IPressableInput CancelInput { get; internal set; }
        ListBox listBox;



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

            foreach(var item in shop.Values)
            {
                listBox.Items.Add(item);
            }
        }
    }
}
