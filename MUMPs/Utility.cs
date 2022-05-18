using AeroCore.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using xTile.Layers;

namespace MUMPs
{
    [ModInit]
    static class Utility
    {
        internal static IReflectedField<Multiplayer> mpField = null;
        internal static void Init()
        {
            mpField = ModEntry.helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
        }
        public static IList<T> TransformItems<T>(this IList<T> list, Func<T, T> transformer)
        {
            for(int i = 0; i < list.Count; i++)
            {
                list[i] = transformer(list[i]);
            }
            return list;
        }
        public static void BroadcastReloadRequest(string name)
        {
            models.MessageRepairEvent msg = new(name);
            ReceiveReloadRequest(msg);
            ModEntry.helper.Multiplayer.SendMessage(msg, "RepairEvent", new string[] { ModEntry.ModID });
        }
        public static void ReceiveReloadRequest(models.MessageRepairEvent ev)
        {
            if(ev.LocationName == Game1.currentLocation.name)
            {
                ReloadCurrentLocation(Game1.currentLocation.mapPath, Game1.player.getTileLocation(), ev.LocationName);
            }
        }
        public static void ReloadCurrentLocation(string path, Vector2 coords, string name)
        {
            ModEntry.helper.Content.InvalidateCache(path);
            if (Game1.currentLocation.mapPath == path)
                Utility.warpToTempMap("EventVoid", Game1.player);
            Game1.warpFarmer(name, (int)coords.X, (int)coords.Y, false);
        }
        public static Multiplayer GetMultiplayer()
        {
            return mpField.GetValue();
        }
        public static void warpToTempMap(string path, Farmer who)
        {
            GameLocation temp = new(PathUtilities.NormalizeAssetName("Maps/"+path), "Temp");
            temp.map.LoadTileSheets(Game1.mapDisplayDevice);
            if(path.Trim() == "EventVoid")
                Events.drawVoid.Value = true; //anti-flicker
            Event e = Game1.currentLocation.currentEvent;
            Game1.currentLocation.cleanupBeforePlayerExit();
            Game1.currentLocation.currentEvent = null;
            Game1.currentLightSources.Clear();
            Game1.currentLocation = temp;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.currentLocation.currentEvent = e;
            Game1.player.currentLocation = Game1.currentLocation;
            who.currentLocation = Game1.currentLocation;
            Game1.panScreen(0, 0);
        }

        //Used to get DGA item #
        public static int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
