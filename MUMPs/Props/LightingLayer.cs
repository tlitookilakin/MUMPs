using AeroCore.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using xTile.Layers;
using xTile.Tiles;
using AeroCore.Utils;
using AeroCore.API;
using AeroCore;
using xTile.ObjectModel;

namespace MUMPs.Props
{
	[ModInit]
	class LightingLayer
	{
		private static Dictionary<TileSheet, Texture2D> sheets;

		internal static void Init()
		{
			ModEntry.helper.Events.GameLoop.SaveLoaded += (s, ev) => Reload();
			ModEntry.AeroAPI.LightingEvent += Draw;
		}
		internal static void Reload()
		{
			if (ModEntry.helper.ModRegistry.IsLoaded("Platonymous.Toolkit"))
				try {
					sheets = (Dictionary<TileSheet, Texture2D>)AccessTools.TypeByName("PyDisplayDevice").
						GetField("m_tileSheetTextures2", BindingFlags.NonPublic | BindingFlags.Instance).
						GetValue(Game1.mapDisplayDevice);
					return;
				} catch (ArgumentException)
				{
					ModEntry.monitor.Log("Failed to retrieve hijacked pytk display device, attempting default", LogLevel.Debug);
				}
			sheets = (Dictionary<TileSheet, Texture2D>)AccessTools.TypeByName("SXnaDisplayDevice").
				GetField("m_tileSheetTextures", BindingFlags.NonPublic | BindingFlags.Instance).
				GetValue(Game1.mapDisplayDevice);
		}
		private static void Draw(ILightingEventArgs ev)
		{
			if (Game1.currentLocation?.map is null)
				return;
			List<Layer> layers = new();
			foreach(var l in Game1.currentLocation.map.Layers)
				if(l.Id.StartsWith("Lighting"))
					layers.Add(l);
			if (layers.Count == 0)
				return;

			var batch = Game1.spriteBatch;
			Color color = ev.intensity * Color.White;
			float scale = ev.scale * 4f;
			int tilesize = (int)(scale * 16f);
			var port = Game1.viewport.ToRect();
			Point offset = new((int)(port.X % 64 * ev.scale), (int)(port.Y % 64 * ev.scale));
			port = new(port.X / 64, port.Y / 64, port.Width / 64 + 1, port.Height / 64 + 1);

			for(int x = 0; x < port.Width; x++)
				for(int y = 0; y < port.Height; y++)
					foreach(var layer in layers)
						DrawTile(batch, layer.Tiles[new(x + port.X, y + port.Y)], x * tilesize - offset.X, y * tilesize - offset.Y, scale, color);
		}
		private static void DrawTile(SpriteBatch b, Tile tile, int x, int y, float scale, Color color)
		{
			if(tile != null)
			{
				Texture2D tex = sheets[tile.TileSheet];
				var bounds = tile.TileSheet.GetTileImageBounds(tile.TileIndex).ToRect();
				var origin = new Vector2(bounds.Width / 2f, bounds.Height / 2f);
				if (!tex.IsDisposed)
					b.Draw(tex, new Vector2(x + origin.X * scale, y + origin.Y * scale), bounds,
						color, GetRotation(tile), origin, scale, GetSpriteEffects(tile), .9f);
			}
		}

		// copied from SMAPI. Thanks pathos :P

		/// <summary>Get the sprite effects to apply for a tile.</summary>
		/// <param name="tile">The tile being drawn.</param>
		private static SpriteEffects GetSpriteEffects(Tile tile)
			=> tile.Properties.TryGetValue("@Flip", out PropertyValue propertyValue) && int.TryParse(propertyValue, out int value)
				? (SpriteEffects)value
				: SpriteEffects.None;

		/// <summary>Get the draw rotation to apply for a tile.</summary>
		/// <param name="tile">The tile being drawn.</param>
		private static float GetRotation(Tile tile)
		{
			if (!tile.Properties.TryGetValue("@Rotation", out PropertyValue propertyValue) || !int.TryParse(propertyValue, out int value))
				return 0;

			value %= 360;
			if (value == 0)
				return 0;

			return (float)(Math.PI / (180.0 / value));
		}
	}
}
