using AeroCore;
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

namespace MUMPs.Props
{
    [HarmonyPatch]
	[ModInit]
    class ActionGarbage
    {
        private static readonly PerScreen<Dictionary<GameLocation, HashSet<string>>> checkedCans = new(() => new());

		internal static void Init()
		{
			if (!ModEntry.helper.ModRegistry.IsLoaded("furyx639.GarbageDay"))
				ModEntry.AeroAPI.RegisterAction("Garbage", HandleAction);
		}
		private static void HandleAction(Farmer who, string action, Point tile)
        {
            if(who.currentLocation == null || who.currentLocation is Town)
                return;

            string id = action.Trim();

            if (!checkedCans.Value.TryGetValue(who.currentLocation, out var cans))
            {
                cans = new();
                checkedCans.Value.Add(who.currentLocation,cans);
            }
            if (!cans.Contains(id))
            {
                cans.Add(id);
                DoGarbage(who.currentLocation, tile.X, tile.Y, who, action);
            }
        }
		private static void Cleanup() => checkedCans.ResetAllScreens();
        private static void DoGarbage(GameLocation location, int x, int y, Farmer who, string index)
        {

			Random garbageRandom = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 777 + (index.GetHashCode() % 16) * 77);
			int prewarm = garbageRandom.Next(0, 100);
			for (int k = 0; k < prewarm; k++)
				garbageRandom.NextDouble();

			prewarm = garbageRandom.Next(0, 100);
			for (int j = 0; j < prewarm; j++)
				garbageRandom.NextDouble();

			Game1.stats.incrementStat("trashCansChecked", 1);
			int xSourceOffset = StardewValley.Utility.getSeasonNumber(Game1.currentSeason) * 17;
			bool mega = Game1.stats.getStat("trashCansChecked") > 20 && garbageRandom.NextDouble() < 0.01;
			bool doubleMega = Game1.stats.getStat("trashCansChecked") > 20 && garbageRandom.NextDouble() < 0.002;
			if (doubleMega)
				location.playSound("explosion");
			else if (mega)
				location.playSound("crit");

			List<TemporaryAnimatedSprite> trashCanSprites = new();
			trashCanSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(22 + xSourceOffset, 0, 16, 10), new Vector2(x, y) * 64f + new Vector2(0f, -6f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = (doubleMega ? 4000 : 1000),
				motion = (doubleMega ? new Vector2(4f, -20f) : new Vector2(0f, -8f + (mega ? (-7f) : ((float)(Game1.random.Next(-1, 3) + ((Game1.random.NextDouble() < 0.1) ? (-2) : 0)))))),
				rotationChange = (doubleMega ? 0.4f : 0f),
				acceleration = new Vector2(0f, 0.7f),
				yStopCoordinate = y * 64 + -24,
				layerDepth = (doubleMega ? 1f : ((float)((y + 1) * 64 + 2) / 10000f)),
				scale = 4f,
				Parent = location,
				shakeIntensity = (doubleMega ? 0f : 1f),
				reachedStopCoordinate = delegate
				{
					location.removeTemporarySpritesWithID(97654);
					location.playSound("thudStep");
					for (int l = 0; l < 3; l++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(x, y) * 64f + new Vector2(l * 6, -3 + Game1.random.Next(3)) * 4f, flipped: false, 0.02f, Color.DimGray)
						{
							alpha = 0.85f,
							motion = new Vector2(-0.6f + (float)l * 0.3f, -1f),
							acceleration = new Vector2(0.002f, 0f),
							interval = 99999f,
							layerDepth = (float)((y + 1) * 64 + 3) / 10000f,
							scale = 3f,
							scaleChange = 0.02f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = 50
						});
					}
				},
				id = 97654f
			});
			if (doubleMega)
				trashCanSprites.Last().reachedStopCoordinate = trashCanSprites.Last().bounce;

			trashCanSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(22 + xSourceOffset, 11, 16, 16), new Vector2(x, y) * 64f + new Vector2(0f, -5f) * 4f, flipped: false, 0f, Color.White)
			{
				interval = (doubleMega ? 999999 : 1000),
				layerDepth = ((y + 1) * 64 + 1) / 10000f,
				scale = 4f,
				id = 97654f
			});
			for (int i = 0; i < 5; i++)
			{
				trashCanSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(22 + Game1.random.Next(4) * 4, 32, 4, 4), new Vector2(x, y) * 64f + new Vector2(Game1.random.Next(13), -3 + Game1.random.Next(3)) * 4f, flipped: false, 0f, Color.White)
				{
					interval = 500f,
					motion = new Vector2(Game1.random.Next(-2, 3), -5f),
					acceleration = new Vector2(0f, 0.4f),
					layerDepth = ((y + 1) * 64 + 3) / 10000f,
					scale = 4f,
					color = StardewValley.Utility.getRandomRainbowColor(),
					delayBeforeAnimationStart = Game1.random.Next(100)
				});
			}
			var multiplayer = Utility.GetMultiplayer();
			multiplayer.broadcastSprites(location, trashCanSprites);
			location.playSound("trashcan");
            Character c = StardewValley.Utility.isThereAFarmerOrCharacterWithinDistance(new Vector2(x, y), 7, location);
			if (c != null && c is NPC && !(c is Horse))
			{
				multiplayer.globalChatInfoMessage("TrashCan", Game1.player.Name, c.Name);
				if (c.Name.Equals("Linus"))
				{
					c.doEmote(32);
					(c as NPC).setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus"), add: true, clearOnMovement: true);
					who.changeFriendship(5, c as NPC);
					multiplayer.globalChatInfoMessage("LinusTrashCan");
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
			if (doubleMega)
				who.addItemByMenuIfNecessary(new Hat(66));
			else if (mega || garbageRandom.NextDouble() < 0.2 + who.DailyLuck)
			{
				int item = 168;
				switch (garbageRandom.Next(10))
				{
					case 0:
						item = 168;
						break;
					case 1:
						item = 167;
						break;
					case 2:
						item = 170;
						break;
					case 3:
						item = 171;
						break;
					case 4:
						item = 172;
						break;
					case 5:
						item = 216;
						break;
					case 6:
						item = StardewValley.Utility.getRandomItemFromSeason(Game1.currentSeason, x * 653 + y * 777, forQuest: false);
						break;
					case 7:
						item = 403;
						break;
					case 8:
						item = 309 + garbageRandom.Next(3);
						break;
					case 9:
						item = 153;
						break;
				}
				if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
					item = 890;
				Vector2 origin = new Vector2(x + 0.5f, y - 1) * 64f;
				Game1.createItemDebris(new StardewValley.Object(item, 1), origin, 2, location, (int)origin.Y + 64);
			}
		}
    }
}
