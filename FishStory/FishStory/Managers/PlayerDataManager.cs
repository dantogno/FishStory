using FishStory.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.Managers
{
    public static class PlayerDataManager
    {
        public static PlayerData PlayerData
        {
            get; private set;
        } = new PlayerData();
    }
}
