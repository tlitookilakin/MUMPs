using AeroCore;
using AeroCore.Generics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MUMPs.models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
	[ModInit]
	internal class ActionTextBubble
	{
		private static readonly PerScreen<Dictionary<Point, int>> currentPage = new(() => new());
		private static readonly PerScreen<Dictionary<Point, string[]>> pages = new(() => new());
		private static readonly PerScreen<List<SpeechBubble>> bubbles = new(() => new());
		private static readonly LazyAsset<Dictionary<string, string>> mapStrings = 
			new(ModEntry.helper, static () => "Strings/StringsFromMaps", false);
		internal static void Init()
		{
			ModEntry.AeroAPI.RegisterAction("TextBubble", Handle, 4);
			ModEntry.OnDraw += Draw;
			ModEntry.OnCleanup += Cleanup;
			ModEntry.OnChangeLocation += ChangeLocation;
		}
		private static void ChangeLocation(GameLocation where)
		{
			Cleanup();
		}
		private static void Cleanup()
		{
			currentPage.Value.Clear();
			pages.Value.Clear();
			bubbles.Value.Clear();
		}
		private static void Handle(Farmer who, string what, Point tile, GameLocation where)
		{
			if (!pages.Value.TryGetValue(tile, out var dlg) && mapStrings.Value.TryGetValue(what, out var s))
				pages.Value.Add(tile, dlg = s.Split('/', StringSplitOptions.RemoveEmptyEntries));
			if (dlg is null || dlg.Length == 0)
			{
				// TODO use tokenized string 
				ModEntry.monitor.Log($"Map strings does not contain key '{what}'; could not display text bubble.", LogLevel.Warn);
				return;
			}
			if (!currentPage.Value.TryGetValue(tile, out int ind))
				ind = 0;
			bubbles.Value.Add(new(Game1.parseText(dlg[ind]), new(tile.X * 64f, tile.Y * 64f)));
			currentPage.Value[tile] = (ind + 1) % dlg.Length;
		}
		private static void Draw(SpriteBatch b)
		{
			var millis = Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			var bs = bubbles.Value;
			for(int i = bs.Count - 1; i >= 0; i--)
			{
				var bubble = bs[i];
				if (bubble.Timer <= 0)
					bs.RemoveAt(i);
				else
					bubble.Draw(b, millis);
			}
		}
	}
}
