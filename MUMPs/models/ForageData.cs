using AeroCore;
using AeroCore.Generics;
using AeroCore.Utils;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SObject = StardewValley.Object;

namespace MUMPs.models
{
    [ModInit]
    public class ForageData
    {
        private readonly Dictionary<string, WeightedArray<SObject>> data = new(StringComparer.OrdinalIgnoreCase);
        private bool generated = false;
        private static readonly Dictionary<string, SObject> idCache = new();

        // key: query string, value: forage pool id
        public Dictionary<string, string> Overrides { get; set; } = new();

        // key: item id, value: metadata
        public Dictionary<string, ForageItem> Items { get; set; } = new();

        public Dictionary<string, WeightedArray<SObject>> GetForage()
        {
            if (generated)
                return data;
            data.Clear();
            generated = true;
            Dictionary<string, List<SObject>> items = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, List<int>> weights = new(StringComparer.OrdinalIgnoreCase);
            foreach((var id, var data) in Items)
            {
                string sid = id.Trim();
                if (!idCache.TryGetValue(sid, out var obj))
                    if (sid.TryGetItem(out var item))
                        idCache.Add(sid, obj = ModEntry.AeroAPI.WrapItem(item, true));
                    else
                        idCache.Add(sid, obj = null);

                if (obj is null)
                {
                    ModEntry.monitor.Log($"Failed to generate forage, item with id '{sid}' does not exist");
                    continue;
                }
                if (!ModEntry.AeroAPI.CheckConditions(data.Condition, target_item: obj))
                    continue;

                var gt = data.GroundType.Trim();
                if (!items.ContainsKey(gt))
                {
                    items.Add(gt, new());
                    weights.Add(gt, new());
                }
                items[gt].Add(obj);
                weights[gt].Add(data.Weight);
            }
            foreach(var ground in items.Keys)
                data.Add(ground, new(items[ground], weights[ground]));
            return data;
        }
        internal static void Init()
        {
            ModEntry.helper.Events.GameLoop.DayEnding += (s, e) => NewDay();
        }
        private static void NewDay()
        {
            idCache.Clear();
            foreach (var obj in Assets.Forage.Values)
                obj.generated = false;
        }
    }
}
