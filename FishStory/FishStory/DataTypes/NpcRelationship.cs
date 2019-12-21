using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.DataTypes
{
    public class NpcRelationship
    {
        public string NpcName { get; set; }
        public List<int> EventsTriggered { get; set; } =
            new List<int>();





    }
}
