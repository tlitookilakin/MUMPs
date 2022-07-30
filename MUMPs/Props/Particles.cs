using AeroCore;
using AeroCore.Particles;
using AeroCore.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MUMPs.Props
{
    [ModInit]
    internal class Particles
    {
        private static readonly Rectangle defaultRect = new(0, 0, 16, 16);
        private static readonly PerScreen<List<IParticleManager>> particles = new(() => new());
        internal static void Init()
        {
            ModEntry.OnChangeLocation += ChangeLocation;
            ModEntry.OnCleanup += Cleanup;
            ModEntry.OnTick += Tick;
            ModEntry.OnDraw += Draw;
        }
        private static void Cleanup()
        {
            particles.ResetAllScreens();
        }
        private static void Draw(SpriteBatch b)
        {
            Vector2 offset = new(-Game1.viewport.X, -Game1.viewport.Y);
            foreach(var particle in particles.Value)
            {
                particle.Offset = offset;
                particle.Draw(b);
            }
        }
        private static void Tick()
        {
            int millis = Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            foreach(var particle in particles.Value)
                particle.Tick(millis);
        }
        private static void ChangeLocation(GameLocation loc)
        {
            List<IParticleManager> ps = new();
            foreach ((var tile, int x, int y) in loc.map.TilesInLayer("Back"))
            {
                if (!tile.TileHasProperty("Particles", out var prop))
                    continue;
                var split = prop.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 0)
                    continue;
                if (!split.ToRect(out var rect, 1))
                    rect = defaultRect;
                if (split.Length <= 6 || !int.TryParse(split[6], out int count))
                    count = 50;
                if (split.Length <= 7 || !int.TryParse(split[7], out int rate))
                    rate = 100;
                if (split.Length <= 8 || !int.TryParse(split[8], out int rateVar))
                    rateVar = 0;
                if (split.Length <= 9 || !int.TryParse(split[9], out int burst))
                    burst = 1;
                if (split.Length <= 10 || !int.TryParse(split[10], out int burstMax))
                    burstMax = 1;
                var man = ModEntry.AeroAPI.CreateParticleSystem(ModEntry.helper.GameContent, 
                    $"{ModEntry.ContentDir}/particles/{split[0]}", new Emitter() { 
                    Region = new(x * 64 + rect.X * 4, y * 64 + rect.Y * 4, rect.Width * 4, rect.Height * 4),
                    Rate = rate,
                    RateVariance = rateVar,
                    BurstMin = burst,
                    BurstMax = burstMax,
                    Radial = split.Length > 11
                }, count);
                if (man is null)
                    continue;
                ps.Add(man);
                man.Tick(0);
            }
            particles.Value = ps;
        }
    }
}
