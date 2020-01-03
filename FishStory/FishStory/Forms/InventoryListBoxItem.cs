using FishStory.GumRuntimes.DefaultForms;
using Gum.Wireframe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.Forms
{
    public class ItemWithCount
    {
        public string ItemName { get; set; }
        public int Count { get; set; }
        public int? SellPrice { get; set; }
        public InventoryListItemRuntime.ViewOrSell ViewOrSell{ get; set; }
    }

    public class InventoryListBoxItem : FlatRedBall.Forms.Controls.ListBoxItem
    {
        
        public InventoryListBoxItem(GraphicalUiElement visual) : base(visual)
        {

        }

        public override void UpdateToObject(object itemWithCount)
        {
            var item = itemWithCount as ItemWithCount;
            var storeListItem = Visual as GumRuntimes.DefaultForms.InventoryListItemRuntime;

            storeListItem.ItemName = item.ItemName;
            storeListItem.Stock = item.Count.ToString();
            storeListItem.Price = item.SellPrice?.ToString();
            storeListItem.CurrentViewOrSellState = item.ViewOrSell;
        }
    }
}
