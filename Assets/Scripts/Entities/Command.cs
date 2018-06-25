using StarterBot.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace StarterBot.Entities
{
    public class Command
    {
        public int Round { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public BuildingType? Type { get; set; }        

        public override string ToString()
        {
            if (X == null && Y == null && Type == null)
            {
                return "";
            }

            return $"{X},{Y},{(int)Type}";
        }
    }
}
