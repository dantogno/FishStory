using FishStory.DataTypes;
using FishStory.Managers;
using Gum.Wireframe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.Forms
{
    public class StoreListBoxItem : FlatRedBall.Forms.Controls.ListBoxItem
    {
        public StoreListBoxItem(GraphicalUiElement visual) : base(visual)
        {
            Selected += HandleSelected;
        }

        private void HandleSelected(object sender, EventArgs e)
        {
            if (IsSelected)
            {
                SoundManager.Play(GlobalContent.ItemSelectedSound);
            }
            else
            {
                SoundManager.Play(GlobalContent.ItemDeselectedSound);
            }
        }

        public override void UpdateToObject(object shopAsObject)
        {
            var shop = shopAsObject as ShopItem;
            var item = GlobalContent.ItemDefinition[shop.Item];
            var storeListItem = Visual as GumRuntimes.DefaultForms.InventoryListItemRuntime;

            storeListItem.Price = "$" + item.PlayerBuyingCost.ToString() ;
            storeListItem.Stock = shop.Stock.ToString();
            storeListItem.ItemName = item.Name;
            storeListItem.CurrentViewOrSellState = GumRuntimes.DefaultForms.InventoryListItemRuntime.ViewOrSell.Sell;
        }
    }
}
