using System;
using System.Collections.Generic;

namespace MUMPs.models
{
    public class ForageData
    {
        // key: forage pool id, value: query string
        public Dictionary<string, string> Overrides { get; set; } = new();

        // key: item id, value: metadata
        public Dictionary<string, ForageItem> Items { get; set; } = new();
    }
}
