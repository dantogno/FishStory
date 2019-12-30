using FishStory.DataTypes;
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

        }

        public override void UpdateToObject(object shopAsObject)
        {
            var shop = shopAsObject as Shop;
            var item = GlobalContent.ItemDefinition[shop.Item];
            var storeListItem = Visual as GumRuntimes.DefaultForms.StoreListItemRuntime;

            storeListItem.PriceText = item.PlayerBuyingCost.ToString() ;
            storeListItem.ItemName = item.Name;
        }
    }
}
