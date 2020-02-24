using FishStory.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.DataTypes
{
    public class PlayerData
    {
        public PlayerData()
        {
            if (DebuggingVariables.NextDayWillTriggerEnding)
                CurrentDay = 3;
        }
        public Dictionary<string, int> ItemInventory { get; set; } =
            new Dictionary<string, int>();

        public Dictionary<string, int> TimesFishIdentified
        {
            get; set;
        } = new Dictionary<string, int>();

        public int Money { get; set; } = 35;

        public Dictionary<string, NpcRelationship> NpcRelationships { get; set; } =
            new Dictionary<string, NpcRelationship>();

        /// <summary>
        /// This is only incremented after the go to bed / wake up sequence.
        /// In other words, if the player stays up to 12AM without going to bed, it's still Day 1.
        /// </summary>
        public int CurrentDay { get; set; } 

        public void AwardItem(string itemKey)
        {
            ItemInventory.Increment(itemKey);
        }

        public void RemoveItem(string itemKey)
        {
            ItemInventory[itemKey]--;
        }

        public bool Has(string itemKey)
        {
            return ItemInventory.Get(itemKey) > 0;
        }

        public bool Has(string itemKey, int desiredAmount)
        {
            return ItemInventory.Get(itemKey) >= desiredAmount;
        }
        public int SpoilItemsAndReturnCount()
        {
            if (DebuggingVariables.FishDoNotGoBadAtEndOfTheDay)
            {
                return 0;
            }

            var itemKeys = ItemInventory.Keys.ToArray();
            var numberKeys = itemKeys.Count();
            var numberSpoiledItems = 0;

            for (int i = numberKeys - 1; i >= 0; i--)
            {
                var keyToCheckForFish = itemKeys[i];
                bool isFish = GlobalContent.ItemDefinition[keyToCheckForFish].IsFish;
                if (isFish)
                {
                    numberSpoiledItems += ItemInventory[keyToCheckForFish];
                    ItemInventory.RemoveAll(keyToCheckForFish);
                }
            }

            return numberSpoiledItems;
        }
    }

    public static class DictionaryExtensions
    {
        public static void Set(this Dictionary<string, int> dictionary, string key, int value)
        {
            dictionary[key] = value;
        }

        public static void RemoveAll(this Dictionary<string, int> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);
            }
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
