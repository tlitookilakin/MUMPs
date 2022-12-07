using System;
using MUMPs.models;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using StardewModdingAPI.Utilities;
using Microsoft.Xna.Framework;
using AeroCore.Utils;
using AeroCore;

namespace MUMPs.Props
{
	[HarmonyPatch]
	[ModInit]
	class Parallax
	{
		private static readonly PerScreen<HorizonModel> currentBackground = new();
		private static readonly PerScreen<HorizonModel> currentForeground = new();
		private static readonly PerScreen<Vector2> backgroundOffset = new();
		private static readonly PerScreen<Vector2> foregroundOffset = new();

		internal static void Init()
		{
			ModEntry.OnChangeLocation += ChangeLocation;
			ModEntry.OnDraw += DrawAfter;
			ModEntry.OnCleanup += Cleanup;
		}
		private static HorizonModel getTemplate(string prop, out Vector2 offset)
		{
			string[] props = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			offset = Vector2.Zero;

			if (props.Length == 0)
				return null;

			if (props.ToVector2(out Vector2 vec, 1))
				offset = vec;
			else
				offset = Vector2.Zero;

			if (Assets.Backdrops.TryGetValue(props[0], out var ret))
				return ret;
			else
				return null;
		}
		private static void ChangeLocation(GameLocation loc, bool soft)
		{
			currentBackground.Value = null;
			currentForeground.Value = null;

			if(loc is null)
				return;

			currentBackground.Value = getTemplate(loc.getMapProperty("DepthBackground"), out Vector2 off);
			backgroundOffset.Value = off;
			currentForeground.Value = getTemplate(loc.getMapProperty("DepthForeground"), out off);
			foregroundOffset.Value = off;

			currentBackground.Value?.Init();
			currentForeground.Value?.Init();
		}

		[HarmonyPatch(typeof(GameLocation), "drawBackground")]
		[HarmonyPrefix]
		internal static void DrawBackgroundPrefix(ref SpriteBatch b) => currentBackground.Value?.Draw(b, false, backgroundOffset.Value);
		private static void DrawAfter(SpriteBatch b) => currentForeground.Value?.Draw(b, true, foregroundOffset.Value);
		private static void Cleanup()
		{
			currentBackground.ResetAllScreens();
			currentForeground.ResetAllScreens();
			backgroundOffset.ResetAllScreens();
			foregroundOffset.ResetAllScreens();
		}
	}
}
