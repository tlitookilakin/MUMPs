using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.models
{
    internal class DestinationList
    {
        public Dictionary<string, Destination> Destinations { get; set; } = new();
        public string Condition { get; set; } = string.Empty;
    }
}
