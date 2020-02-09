using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.DataTypes
{
    public partial class ItemDefinition
    {
        public int TotalCaught { get; set; }
        public static string[] FishNames => GlobalContent.ItemDefinition
            .Where(item => item.Value.IsFish)
            .Select(item => item.Value.Name).ToArray();
        public static string[] BaitNames => GlobalContent.ItemDefinition
            .Where(item => item.Value.IsBait)
            .Select(item => item.Value.Name).ToArray();
    }
}
