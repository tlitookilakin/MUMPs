using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MUMPs.models
{
    public class Destination
    {
        public string Location { get; set; }
        public Point Tile { get; set; }
        public int Direction { get; set; }
        public string DisplayName { get; set; }
        public string Network { get; set; }
        public string Condition { get; set; }
    }
}
