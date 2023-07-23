using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MUMPs.models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using SObject = StardewValley.Object;

namespace MUMPs.Props
{
	[HarmonyPatch]
	[ModInit]
	class FishingArea
	{
		internal static readonly PerScreen<Dictionary<Rectangle, int>> idRegions = new(() => new());
		internal static readonly PerScreen<Dictionary<Rectangle, string>> locRegions = new(() => new());
		internal static readonly PerScreen<int> defaultRegion = new(() => -1);
		internal static readonly PerScreen<string> defaultRegionName = new();
		private static bool skipChecks = false;
		internal static readonly ConditionalWeakTable<FishingRod, SObject> caughtItems = new();
		private static readonly ILHelper fishPatch = new ILHelper(ModEntry.monitor, "GetFish")
			.Add(new CodeInstruction[]{
				new(OpCodes.Ldarg_S, 6),
				new(OpCodes.Ldarg_S, 7),
				new(OpCodes.Call, typeof(FishingArea).MethodNamed(nameof(SwapPool))),
				new(OpCodes.Starg_S, 7)
			})
			.Finish();
		private static readonly ILHelper rodFunction = new ILHelper(ModEntry.monitor, "Fishing rod DoFunction")
			.SkipTo(new CodeInstruction[] {
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldloc_S, (11, typeof(SObject))),
				new(OpCodes.Callvirt, typeof(Item).PropertyGetter(nameof(Item.ParentSheetIndex))),
				new(OpCodes.Ldc_I4_M1)
			})
			.Add(new CodeInstruction[]
			{
				new(OpCodes.Ldloc_S, 11),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Call, typeof(FishingArea).MethodNamed(nameof(StoreCaughtItem)))
			})
			.Finish();
		private static readonly ILHelper rodTick = new ILHelper(ModEntry.monitor, "Fishing rod tick")
			.SkipTo(new CodeInstruction[]
			{
				new(OpCodes.Ldnull),
				new(OpCodes.Stloc_S, (21, typeof(SObject)))
			})
			.Skip(2)
			.Add(new CodeInstruction[]
			{
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldloca_S, 21),
				new(OpCodes.Call, typeof(FishingArea).MethodNamed(nameof(TryGetStoredItem)))
			})
			.AddJump(OpCodes.Brtrue, "skip")
			.SkipTo(new CodeInstruction[]
			{
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldfld, typeof(FishingRod).FieldNamed(nameof(FishingRod.fromFishPond)))
			})
			.AddLabel("skip")
			.Finish();

		internal static void Init()
		{
			ModEntry.OnChangeLocation += ChangeLocation;
			ModEntry.OnCleanup += Cleanup;
		}
		private static void ChangeLocation(GameLocation loc, bool soft)
		{
			idRegions.Value.Clear();
			locRegions.Value.Clear();

			string[] data = Maps.MapPropertyArray(loc, "FishingAreaCorners");
			for (int i = 0; i + 4 < data.Length; i += 5)
			{
				if (data.FromCorners(out var rect, i))
				{
					if (data.Length > i + 5 && int.TryParse(data[i + 4], out int region))
					{
						idRegions.Value[rect] = region;
						i += 1;
					}
					locRegions.Value[rect] = data[i + 4];
				}
			}
			string[] defaults = Maps.MapPropertyArray(loc, "DefaultFishingArea");
				defaultRegionName.Value = defaults.Length > 0 ? defaults[0] : null;
			defaultRegion.Value = defaults.Length > 1 && int.TryParse(defaults[1], out int def) ?
				 def : -1;
			ModEntry.monitor.Log($"Fishing: Found {idRegions.Value.Count} ID regions and {locRegions.Value.Count} Location regions.", LogLevel.Trace);
		}
		private static void Cleanup()
		{
			idRegions.ResetAllScreens();
			locRegions.ResetAllScreens();
			defaultRegion.ResetAllScreens();
			defaultRegionName.ResetAllScreens();
		}

		[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.getFish))]
		[HarmonyPostfix]
		internal static SObject SwapOutput(SObject original, GameLocation __instance, int bait, int waterDepth, Farmer who, Vector2 bobberTile)
		{
			if (!skipChecks)
			{
				// get treasure
				var treasureProp = __instance.doesTileHaveProperty((int)bobberTile.X, (int)bobberTile.Y, "FishingTreasure", "Back");
				if (treasureProp is not null)
				{
					var split = treasureProp.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (split.Length >= 1 && split[0].TryGetItem(out Item fished))
					{
						Farmer address = split.Length < 3 ? Game1.MasterPlayer : who; // whether instanced or not
						if (split.Length < 2)
						{
							return ModEntry.AeroAPI.WrapItem(fished);
						}
						else if (!address.hasOrWillReceiveMail(split[1]))
						{
							if (split.Length > 2)
								address.mailForTomorrow.Add(split[1]);
							else
								address.mailReceived.Add(split[1]);
							return ModEntry.AeroAPI.WrapItem(fished);
						}
					}
				}
			}
			return original;
		}

		[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.getFish))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> GetFish(IEnumerable<CodeInstruction> instructions) => fishPatch.Run(instructions);

		[HarmonyPatch(typeof(FishingRod), nameof(FishingRod.DoFunction))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> RodFunction(IEnumerable<CodeInstruction> source) => rodFunction.Run(source);

		[HarmonyPatch(typeof(FishingRod), nameof(FishingRod.tickUpdate))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> RodTick(IEnumerable<CodeInstruction> source, ILGenerator gen) => rodTick.Run(source, gen);

		[HarmonyPatch(typeof(Farm), nameof(Farm.getFish))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> GetFarmFish(IEnumerable<CodeInstruction> instructions)
		{
			CodeInstruction prev = null;
			var getFish = typeof(GameLocation).MethodNamed(nameof(GameLocation.getFish));
			var swapFish = typeof(FishingArea).MethodNamed(nameof(SwapPool));
			foreach (var code in instructions)
			{
				if (prev is not null) 
				{
					if (code.opcode == OpCodes.Call && (code.operand as MethodInfo) == getFish)
					{
						yield return new(OpCodes.Ldarg_S, 6);
						yield return prev;
						yield return new(OpCodes.Call, swapFish);
						yield return code;
					} else
					{
						yield return prev;
						yield return code;
					}
					prev = null;
				}
				else if (code.opcode == OpCodes.Ldstr)
				{
					prev = code;
				}
				else
				{
					yield return code;
				}
			}
		}
		private static string SwapPool(Vector2 bobber, string location)
		{
			skipChecks = false;
			if (location != null)
				return location;

			string ret = defaultRegionName.Value;
			foreach ((var region, string loc) in locRegions.Value)
			{
				if (region.Contains(bobber))
				{
					ret = loc;
					break;
				}
			}
			skipChecks = ret is not null;
			return ret;
		}

		[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.getFishingLocation))]
		[HarmonyPostfix]
		internal static int GetFishingLocationPatch(int result, Vector2 tile)
		{
			foreach ((var region, int id) in idRegions.Value)
				if (region.Contains(tile))
					return id;
			return defaultRegion.Value == -1 ? result : defaultRegion.Value;
		}
		internal static void StoreCaughtItem(SObject obj, FishingRod rod)
		{
			if (ModEntry.AeroAPI.IsWrappedItem(obj))
				caughtItems.AddOrUpdate(rod, obj);
		}
		internal static bool TryGetStoredItem(FishingRod rod, ref SObject obj)
		{
			var ret = caughtItems.TryGetValue(rod, out var res);
			caughtItems.Remove(rod);
			if (ret)
				obj = res;
			return ret;
		}
	}
}
