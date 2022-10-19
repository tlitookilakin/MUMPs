using AeroCore.Utils;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MUMPs.models
{
	public class SpawnDataItem
	{
		public string Type { get; set; }
		public string ID { get; set; }
		public string Conditions { get; set; }
		public int Weight { get; set; } = 1;
		public Dictionary<string, object> additionalFields { get; set; }

		private bool typeExists = true;

		private static Dictionary<string, HashSet<string>> badFields = new();
		private static readonly Dictionary<string, Dictionary<string, FieldInfo>> fieldCache = new();
		private static readonly Dictionary<string, Dictionary<string, MethodInfo>> propertyCache = new();
		private static readonly Dictionary<string, Type> typeCache = new();

		public bool TrySpawnAt(GameLocation loc, Vector2 tile, Farmer who = null)
		{
			if (loc is null || !typeExists || Type is null || ID is null)
				return false;
			if (Conditions is not null && !ModEntry.AeroAPI.CheckConditions(Conditions, null, who, null, loc))
				return false;
			var type = GetTypeNamed(Type, out int SlimeVariant);
			if (type is null)
				return typeExists = false;

			Monster monster;
			try
			{
				if (type == typeof(Monster))
					monster = Activator.CreateInstance(type, new object[] {ID, tile }) as Monster;
				else
					monster = Activator.CreateInstance(type, new object[] {tile}) as Monster;
			} catch
			{
				return false;
			}
			if (monster is null)
				return false;
			ApplyFields(monster, type);
			if (monster is GreenSlime slime)
			{
				switch (SlimeVariant)
				{
					case 1: slime.makePrismatic(); break;
					case 2: slime.makeTigerSlime(); break;
				}
			}
			monster.modData[Props.SpawnMonsters.KillFlag] = "T";
			loc.addCharacter(monster);
			return true;
		}
		private static Type GetTypeNamed(string what, out int SlimeVariant)
		{
			SlimeVariant = 0;
			var ret = typeof(GreenSlime);
			if (what.Equals("prismaticslime", StringComparison.OrdinalIgnoreCase))
				SlimeVariant = 1;
			else if (what.Equals("tigerslime", StringComparison.OrdinalIgnoreCase))
				SlimeVariant = 2;
			else
				if (typeCache.TryGetValue(what, out ret))
					return ret;
				else
					ret = Reflection.TypeNamed(what.Contains(".") ? what : "StardewValley.Monsters." + what);
			typeCache.Add(what, ret);
			return ret;
		}
		private void ApplyFields(Monster monster, Type type)
		{
			if (additionalFields is null || additionalFields.Count < 1)
				return;
			if (!fieldCache.TryGetValue(Type, out var fcache))
				fieldCache.Add(Type, fcache = new());
			if (!propertyCache.TryGetValue(Type, out var pcache))
				propertyCache.Add(Type, pcache = new());
			if (!badFields.TryGetValue(Type, out var badCache))
				badFields.Add(Type, badCache = new());

			foreach((var name, var val) in additionalFields)
			{
				if (badCache.Contains(name))
					continue;
				MethodInfo prop = null;
				if (!fcache.TryGetValue(name, out var field) && !pcache.TryGetValue(name, out prop))
				{
					field = type.FieldNamed(name);
					prop = field is null ? type.PropertySetter(name) : null;
					if (field is null && prop is null)
					{
						badCache.Add(name);
						continue;
					}
					if (field is not null)
						if (field.FieldType.Assembly.FullName == "Netcode")
							prop = field.FieldType.PropertySetter("Value");
						else if (field.FieldType.IsAssignableFrom(type))
							fcache.Add(name, field);
						else
							continue;
					if (prop is not null)
						if (MethodAccepts(prop, type))
							pcache.Add(name, prop);
						else
							continue;
				}
				field?.SetValue(monster, val);
				prop?.Invoke(monster, new[] {val});
			}
		}
		private static bool MethodAccepts(MethodInfo method, Type type)
		{
			var args = method.GetParameters();
			return args.Length > 0 && args[0].ParameterType.IsAssignableFrom(type);
		}
	}
}
