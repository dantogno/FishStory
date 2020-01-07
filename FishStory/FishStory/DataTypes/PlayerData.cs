using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.DataTypes
{
    public class PlayerData
    {
        public Dictionary<string, int> ItemInventory { get; set; } =
            new Dictionary<string, int>();

        public int Money { get; set; } = 25;

        public Dictionary<string, NpcRelationship> NpcRelationships { get; set; } =
            new Dictionary<string, NpcRelationship>();

        public int CurrentDay { get; set; } = 1;

        public void AwardItem(string itemKey)
        {
            if(ItemInventory.ContainsKey(itemKey) == false)
            {
                ItemInventory[itemKey] = 1;
            }
            else
            {
                ItemInventory[itemKey]++;
            }
        }

        public void RemoveItem(string itemKey)
        {
            ItemInventory[itemKey]--;
        }

        public bool Has(string itemKey, int desiredCount = 1)
        {
            var inventoryCount = 0;

            if(ItemInventory.ContainsKey(itemKey))
            {
                inventoryCount = ItemInventory[itemKey];
            }

            return inventoryCount >= desiredCount;
        }
        


    }
}
