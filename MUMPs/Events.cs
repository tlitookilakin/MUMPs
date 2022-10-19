using AeroCore;
using AeroCore.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs
{
	[ModInit]
	class Events
	{
		internal static readonly PerScreen<List<Action>> afterWarpActions = new(() => new());
		internal static readonly PerScreen<bool> reloadScreen = new(() => false);
		private static void DayStarted(object sender, DayStartedEventArgs ev)
		{
			if (!Context.IsMainPlayer)
				return;
				foreach (GameLocation loc in Game1.locations)
				{
					if (loc.Name != "Woods")
						Props.Stumps.SpawnMapStumps(loc);
					Patches.BlockedTileClearer.ClearBlockedTilesIn(loc);
					Props.Forage.ClearOnNewDay(loc);
				}
		}
		internal static void Init()
		{
			ModEntry.helper.Events.Multiplayer.ModMessageReceived += RecieveMessage;
			ModEntry.helper.Events.GameLoop.DayStarted += DayStarted;
		}
		private static void RecieveMessage(object sender, ModMessageReceivedEventArgs ev)
		{
			if (ev.FromModID != ModEntry.ModID)
				return;

			switch (ev.Type)
			{
				case "ReloadEvent":
					ReceiveReloadRequest(ev.ReadAs<string>()); break;
				default:
					ModEntry.monitor.Log("Unhandled message type: " + ev.Type, LogLevel.Warn); break;
			}
		}
		internal static void BroadcastReloadRequest(string name)
		{
			ReceiveReloadRequest(name);
			ModEntry.helper.Multiplayer.SendMessage(name, "RepairEvent", new string[] { ModEntry.ModID });
		}
		internal static void ReceiveReloadRequest(string name)
		{
			if (Game1.currentLocation.mapPath.Value != name)
				return;

			ModEntry.helper.GameContent.InvalidateCache(name);
			Maps.ReloadCurrentLocation();
		}
	}
}
