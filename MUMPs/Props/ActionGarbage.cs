using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MUMPs.models;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MUMPs.Props
{
	[HarmonyPatch]
	[ModInit]
	class ActionGarbage
	{
		private static readonly PerScreen<HashSet<string>> checkedCans = new(() => new());

		internal static void Init()
		{
			if (!ModEntry.helper.ModRegistry.IsLoaded("furyx639.GarbageDay"))
				ModEntry.AeroAPI.RegisterAction("Garbage", HandleAction);
			ModEntry.OnCleanup += Cleanup;
		}
		private static void HandleAction(Farmer who, string action, Point tile, GameLocation where)
			=> DoGarbage(who.currentLocation, new(tile.X, tile.Y), who, action);
		private static void Cleanup() => checkedCans.ResetAllScreens();
		private static void DoGarbage(GameLocation location, Vector2 tile, Farmer who, string id)
		{
			if (string.IsNullOrWhiteSpace(id))
				return;
			if (!checkedCans.Value.Add(id))
				return;
			var allData = Assets.Garbage;
			if (!allData.GarbageCans.TryGetValue(id, out var data))
				data = null;
			float baseChance = (data != null && data.BaseChance > 0f) ? data.BaseChance : allData.DefaultBaseChance;
			baseChance += (float)who.DailyLuck;
			Random garbageRandom = new((int)(Game1.stats.DaysPlayed + Game1.uniqueIDForThisGame / 2uL)
				+ 777 + id.GetDeterministicHashCode());
			int prewarm = garbageRandom.Next(0, 100);
			for (int k = 0; k < prewarm; k++)
				garbageRandom.NextDouble();
			prewarm = garbageRandom.Next(0, 100);
			for (int j = 0; j < prewarm; j++)
				garbageRandom.NextDouble();
			GarbageItemData selected = null;
			Item item = null;
			bool baseChancePassed = garbageRandom.NextDouble() > (double)baseChance;
			List<GarbageItemData>[] array = new List<GarbageItemData>[3]
			{
				allData.BeforeAll,
				data?.Items,
				allData.AfterAll
			};
			foreach (List<GarbageItemData> itemList in array)
			{
				if (itemList is null)
					continue;
				foreach (GarbageItemData entry in itemList)
				{
					if (string.IsNullOrWhiteSpace(entry.ID))
					{
						ModEntry.monitor.Log("Garbage: ignored item entry with no ID field.");
					}
					else if ((baseChancePassed || entry.IgnoreBaseChance) && ModEntry.AeroAPI.CheckConditions(entry.Condition))
					{
						List<string> randomItemId = entry.RandomItemId;
						string key = (randomItemId != null && randomItemId.Any()) ? Utility.GetRandom(entry.RandomItemId, garbageRandom) : entry.ItemId;
						if (key.TryGetItem(out var result))
						{
							selected = entry;
							item = result;
							break;
						}
					}
				}
				if (selected is not null)
					break;
			}
			bool doubleMega = selected?.IsDoubleMegaSuccess ?? false;
			bool mega = !doubleMega && (selected?.IsMegaSuccess ?? false);
			if (doubleMega)
				location.playSound("explosion");
			else if (mega)
				location.playSound("crit");
			location.playSound("trashcan");
			_ = tile;
			int tileY = (int)tile.Y;
			int xSourceOffset = location.GetSeasonIndexForLocation() * 17;
			TemporaryAnimatedSprite lidSprite = new("LooseSprites\\Cursors2", new Rectangle(22 + xSourceOffset, 0, 16, 10), tile * 64f + new Vector2(0f, -6f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = doubleMega ? 4000 : 1000,
				motion = doubleMega ? new Vector2(4f, -20f) : new Vector2(0f, -8f + (mega ? (-7f) : garbageRandom.Next(-1, 3) + ((garbageRandom.NextDouble() < 0.1) ? (-2) : 0))),
				rotationChange = (doubleMega ? 0.4f : 0f),
				acceleration = new Vector2(0f, 0.7f),
				yStopCoordinate = tileY * 64 + -24,
				layerDepth = doubleMega ? 1f : (((tileY + 1) * 64 + 2) / 10000f),
				scale = 4f,
				Parent = location,
				shakeIntensity = doubleMega ? 0f : 1f,
				reachedStopCoordinate = delegate
				{
					location.removeTemporarySpritesWithID(97654);
					location.playSound("thudStep");
					for (int m = 0; m < 3; m++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), tile * 64f + new Vector2(m * 6, -3 + garbageRandom.Next(3)) * 4f, flipped: false, 0.02f, Color.DimGray)
						{
							alpha = 0.85f,
							motion = new Vector2(-0.6f + m * 0.3f, -1f),
							acceleration = new Vector2(0.002f, 0f),
							interval = 99999f,
							layerDepth = ((tileY + 1) * 64 + 3) / 10000f,
							scale = 3f,
							scaleChange = 0.02f,
							rotationChange = garbageRandom.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = 50
						});
					}
				},
				id = 97654f
			};
			TemporaryAnimatedSprite bodySprite = new("LooseSprites\\Cursors2", new Rectangle(22 + xSourceOffset, 11, 16, 16), tile * 64f + new Vector2(0f, -5f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = doubleMega ? 999999 : 1000,
				layerDepth = ((tileY + 1) * 64 + 1) / 10000f,
				scale = 4f,
				id = 97654f
			};
			if (doubleMega)
				lidSprite.reachedStopCoordinate = lidSprite.bounce;
			List<TemporaryAnimatedSprite> trashCanSprites = new(){ lidSprite, bodySprite };
			for (int i = 0; i < 5; i++)
			{
				TemporaryAnimatedSprite particleSprite = new("LooseSprites\\Cursors2", new Rectangle(22 + garbageRandom.Next(4) * 4, 32, 4, 4), tile * 64f + new Vector2(Game1.random.Next(13), -3 + Game1.random.Next(3)) * 4f, flipped: false, 0f, Color.White)
				{
					interval = 500f,
					motion = new Vector2(garbageRandom.Next(-2, 3), -5f),
					acceleration = new(0f, 0.4f),
					layerDepth = ((tileY + 1) * 64 + 3) / 10000f,
					scale = 4f,
					color = Utility.getRandomRainbowColor(garbageRandom),
					delayBeforeAnimationStart = garbageRandom.Next(100)
				};
				trashCanSprites.Add(particleSprite);
			}
			Reflection.Multiplayer.broadcastSprites(location, trashCanSprites);
			if (Utility.isThereAFarmerOrCharacterWithinDistance(tile, 7, location) is NPC npc && npc is not Horse)
			{
				Reflection.Multiplayer.globalChatInfoMessage("TrashCan", who.Name, npc.Name);
				if (npc.Name.Equals("Linus"))
				{
					npc.doEmote(32);
					npc.setNewDialogue("Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus", add: true, clearOnMovement: true);
					who.changeFriendship(5, npc);
					Reflection.Multiplayer.globalChatInfoMessage("LinusTrashCan");
				}
				else if (npc.Age == 2)
				{
					npc.doEmote(28);
					npc.setNewDialogue("Data\\ExtraDialogue:Town_DumpsterDiveComment_Child", add: true, clearOnMovement: true);
					who.changeFriendship(-25, npc);
				}
				else if (npc.Age == 1)
				{
					npc.doEmote(8);
					npc.setNewDialogue("Data\\ExtraDialogue:Town_DumpsterDiveComment_Teen", add: true, clearOnMovement: true);
					who.changeFriendship(-25, npc);
				}
				else
				{
					npc.doEmote(12);
					npc.setNewDialogue("Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult", add: true, clearOnMovement: true);
					who.changeFriendship(-25, npc);
				}
				Game1.drawDialogue(npc);
			}
			Game1.stats.incrementStat("trashCansChecked", 1);
			if (selected is not null)
			{
				item.Stack = selected.Stack;
				if (item is StardewValley.Object obj)
				{
					obj.IsRecipe = selected.IsRecipe;
					obj.Quality = selected.Quality;
				}
				if (selected.AddToInventoryDirectly)
				{
					who.addItemByMenuIfNecessary(item);
				}
				else
				{
					Vector2 origin = new Vector2(tile.X + 0.5f, tile.Y - 1f) * 64f;
					Game1.createItemDebris(item, origin, 2, location, (int)origin.Y + 64);
				}
			}
		}
	}
}
