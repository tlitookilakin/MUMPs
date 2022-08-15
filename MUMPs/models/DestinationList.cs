using System;
using System.Collections.Generic;

namespace MUMPs.models
{
    internal class DestinationList
    {
        public Dictionary<string, Destination> MineCartDestinations { get; set; } = new();
        public string MineCartCondition { get; set; } = string.Empty;
    }
}
