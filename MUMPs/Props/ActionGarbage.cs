using AeroCore;
using AeroCore.Utils;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using SUtility = StardewValley.Utility;

namespace MUMPs.Props
{
    [HarmonyPatch]
	[ModInit]
    class ActionGarbage
    {
        private static readonly PerScreen<Dictionary<GameLocation, HashSet<Point>>> checkedCans = new(() => new());

		internal static void Init()
		{
			if (!ModEntry.helper.ModRegistry.IsLoaded("furyx639.GarbageDay"))
				ModEntry.AeroAPI.RegisterAction("Garbage", HandleAction);
			ModEntry.OnCleanup += Cleanup;
		}
		private static void HandleAction(Farmer who, string action, Point tile, GameLocation where)
        {
            if(who.currentLocation == null || who.currentLocation is Town)
                return;

            if (!checkedCans.Value.TryGetValue(who.currentLocation, out var cans))
            {
                cans = new();
                checkedCans.Value.Add(who.currentLocation,cans);
            }
            if (!cans.Contains(tile))
            {
                cans.Add(tile);
                DoGarbage(who.currentLocation, tile.X, tile.Y, who, action);
            }
        }
		private static void Cleanup() => checkedCans.ResetAllScreens();
        private static void DoGarbage(GameLocation location, int x, int y, Farmer who, string action)
        {
			string[] item_lists_split = action.Split('/');
			string[] can_data_split = item_lists_split[0].Split(' ');
			double double_mega_chance = -1.0;
			double mega_chance = -1.0;
			if (can_data_split.Length > 2 && !double.TryParse(can_data_split[2], out double_mega_chance))
			{
				double_mega_chance = -1.0;
			}
			if (can_data_split.Length > 3 && !double.TryParse(can_data_split[3], out mega_chance))
			{
				mega_chance = -1.0;
			}
			int location_index = Game1.locations.IndexOf(location);
			Random garbage_random = new((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 777 + location_index * 10000 + x + y * 1000);
			int prewarm = garbage_random.Next(0, 100);
			for (int m = 0; m < prewarm; m++)
				garbage_random.NextDouble();
			prewarm = garbage_random.Next(0, 100);
			for (int l = 0; l < prewarm; l++)
				garbage_random.NextDouble();
			Game1.stats.incrementStat("trashCansChecked", 1);
			int xSourceOffset = location.GetSeasonIndexForLocation() * 17;
			if (mega_chance < 0.0)
				mega_chance = 0.1;
			if (double_mega_chance < 0.0)
				double_mega_chance = 0.002;
			bool mega = false;
			bool double_mega = false;
			if (Game1.stats.getStat("trashCansChecked") > 20)
			{
				if (garbage_random.NextDouble() < mega_chance)
					mega = true;
				if (garbage_random.NextDouble() < double_mega_chance)
					double_mega = true;
			}
			if (double_mega)
				location.playSound("explosion");
			else if (mega)
				location.playSound("crit");
			List<TemporaryAnimatedSprite> trashCanSprites = new();
			trashCanSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(22 + xSourceOffset, 0, 16, 10), new Vector2(x, y) * 64f + new Vector2(0f, -6f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = double_mega ? 4000 : 1000,
				motion = double_mega ? new Vector2(4f, -20f) : new Vector2(0f, -8f + (mega ? (-7f) : ((Game1.random.Next(-1, 3) + ((Game1.random.NextDouble() < 0.1) ? (-2) : 0))))),
				rotationChange = double_mega ? 0.4f : 0f,
				acceleration = new Vector2(0f, 0.7f),
				yStopCoordinate = y * 64 + -24,
				layerDepth = double_mega ? 1f : (((y + 1) * 64 + 2) / 10000f),
				scale = 4f,
				Parent = location,
				shakeIntensity = double_mega ? 0f : 1f,
				reachedStopCoordinate = delegate
				{
					location.removeTemporarySpritesWithID(97654);
					location.playSound("thudStep");
					for (int num = 0; num < 3; num++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(x, y) * 64f + new Vector2(num * 6, -3 + Game1.random.Next(3)) * 4f, flipped: false, 0.02f, Color.DimGray)
						{
							alpha = 0.85f,
							motion = new Vector2(-0.6f + num * 0.3f, -1f),
							acceleration = new Vector2(0.002f, 0f),
							interval = 99999f,
							layerDepth = ((y + 1) * 64 + 3) / 10000f,
							scale = 3f,
							scaleChange = 0.02f,
							rotationChange = Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = 50
						});
					}
				},
				id = 97654f
			});
			if (double_mega)
				trashCanSprites.Last().reachedStopCoordinate = trashCanSprites.Last().bounce;
			trashCanSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(22 + xSourceOffset, 11, 16, 16), new Vector2(x, y) * 64f + new Vector2(0f, -5f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = double_mega ? 999999 : 1000,
				layerDepth = ((y + 1) * 64 + 1) / 10000f,
				scale = 4f,
				id = 97654f
			});
			for (int k = 0; k < 5; k++)
			{
				trashCanSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(22 + Game1.random.Next(4) * 4, 32, 4, 4), new Vector2(x, y) * 64f + new Vector2(Game1.random.Next(13), -3 + Game1.random.Next(3)) * 4f, flipped: false, 0f, Color.White)
				{
					interval = 500f,
					motion = new Vector2(Game1.random.Next(-2, 3), -5f),
					acceleration = new Vector2(0f, 0.4f),
					layerDepth = ((y + 1) * 64 + 3) / 10000f,
					scale = 4f,
					color = SUtility.getRandomRainbowColor(),
					delayBeforeAnimationStart = Game1.random.Next(100)
				});
			}
			Reflection.Multiplayer.broadcastSprites(location, trashCanSprites);
			location.playSound("trashcan");
			Character c = SUtility.isThereAFarmerOrCharacterWithinDistance(new Vector2(x, y), 7, location);
			if (c != null && c is NPC && !(c is Horse))
			{
				Reflection.Multiplayer.globalChatInfoMessage("TrashCan", Game1.player.Name, c.Name);
				if (c.Name.Equals("Linus"))
				{
					c.doEmote(32);
					(c as NPC).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus"), add: true, clearOnMovement: true);
					who.changeFriendship(5, c as NPC);
					Reflection.Multiplayer.globalChatInfoMessage("LinusTrashCan");
				}
				else if ((c as NPC).Age == 2)
				{
					c.doEmote(28);
					(c as NPC).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Child"), add: true, clearOnMovement: true);
					who.changeFriendship(-25, c as NPC);
				}
				else if ((c as NPC).Age == 1)
				{
					c.doEmote(8);
					(c as NPC).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Teen"), add: true, clearOnMovement: true);
					who.changeFriendship(-25, c as NPC);
				}
				else
				{
					c.doEmote(12);
					(c as NPC).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult"), add: true, clearOnMovement: true);
					who.changeFriendship(-25, c as NPC);
				}
				Game1.drawDialogue(c as NPC);
			}
			string item = null;
			int stack = 1;
			if (double_mega)
			{
				if (item_lists_split.Length <= 1 || item_lists_split[1] == "")
				{
					who.addItemByMenuIfNecessary(new Hat(66));
				}
				else
				{
					string[] item_list_split3 = item_lists_split[1].Split(' ');
					int index3 = garbage_random.Next(0, item_list_split3.Length / 2);
					item = item_list_split3[index3 * 2];
					if (!int.TryParse(item_list_split3[index3 * 2 + 1], out stack))
						stack = 1;
				}
			}
			else if (mega && item_lists_split.Length > 2 && item_lists_split[2] != "")
			{
				string[] item_list_split2 = item_lists_split[2].Split(' ');
				int index2 = garbage_random.Next(0, item_list_split2.Length / 2);
				item = item_list_split2[index2 * 2];
				if (!int.TryParse(item_list_split2[index2 * 2 + 1], out stack))
					stack = 1;
			}
			else
			{
				if (mega || garbage_random.NextDouble() < 0.2 + who.DailyLuck)
				{
					item = "168";
					switch ((!mega) ? garbage_random.Next(10) : garbage_random.Next(5, 10))
					{
						case 0: item = "168"; break;
						case 1: item = "167"; break;
						case 2: item = "170"; break;
						case 3: item = "171"; break;
						case 4: item = "172"; break;
						case 5: item = "216"; break;
						case 6:
							item = SUtility.getRandomItemFromSeason(location.GetSeasonForLocation(), x * 653 + y * 777, forQuest: false).ToString();
							break;
						case 7: item = "403"; break;
						case 8: item = (309 + garbage_random.Next(3)).ToString(); break;
						case 9: item = "153"; break;
					}
					if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
						item = "890";
				}
				if (item_lists_split.Length > 3)
				{
					for (int j = 3; j < item_lists_split.Length; j++)
					{
						string[] item_list_split = item_lists_split[j].Split(' ');
						if (item_list_split.Length < 4)
							continue;
                        if (!double.TryParse(item_list_split[0], out double chance))
                            continue;
                        if (item_list_split[1] == "true")
							chance += who.DailyLuck;
						if (garbage_random.NextDouble() < chance || mega)
						{
							int index = garbage_random.Next(1, item_list_split.Length / 2);
							item = item_list_split[index * 2];
							if (!int.TryParse(item_list_split[index * 2 + 1], out stack))
								stack = 1;
							break;
						}
					}
				}
			}
			if (item != null && item.TryGetItem(out Item loot))
			{
				Vector2 origin = new Vector2(x + 0.5f, y - 1) * 64f;
				for (int i = 0; i < stack; i++)
					Game1.createItemDebris(loot, origin, 2, location, (int)origin.Y + 64);
			}
		}
    }
}
