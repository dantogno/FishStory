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

        public Dictionary<string, int> TimesFishIdentified
        {
            get; set;
        } = new Dictionary<string, int>();

        public int Money { get; set; } = 25;

        public Dictionary<string, NpcRelationship> NpcRelationships { get; set; } =
            new Dictionary<string, NpcRelationship>();

        public int CurrentDay { get; set; } = 1;

        public void AwardItem(string itemKey)
        {
            ItemInventory.Increment(itemKey);
        }

        public void RemoveItem(string itemKey)
        {
            ItemInventory[itemKey]--;
        }

        public bool Has(string itemKey, int desiredCount = 1)
        {
            return ItemInventory.Get(itemKey) >= desiredCount;
        }
    }

    public static class DictionaryExtensions
    {
        public static void Set(this Dictionary<string, int> dictionary, string key, int value)
        {
            dictionary[key] = value;
        }

        public static int Get(this Dictionary<string, int> dictionary, string key)
        {
            if (dictionary.ContainsKey(key) == false)
            {
                return 0;
            }
            else
            {
                return dictionary[key];
            }
        }

        public static void Increment(this Dictionary<string, int> dictionary, string key)
        {
            if (dictionary.ContainsKey(key) == false)
            {
                dictionary[key] = 1;
            }
            else
            {
                dictionary[key]++;
            }
        }

        public static void IncrementBy(this Dictionary<string, int> dictionary, string key, int value)
        {
            if (dictionary.ContainsKey(key) == false)
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary[key] += value;
            }
        }
    }
        
}
