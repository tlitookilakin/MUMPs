using AeroCore.Models;
using AeroCore.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace MUMPs.models
{
    internal class HorizonLayer
    {
        public float Depth { set; get; } = 0f;
        public HorizonImage Image { get; set; }
        public ParticleDefinition Particles { set; get; }
        public IParticleEmitter ParticleEmitter { set; get; }
        public int ParticleCapacity { set; get; }
        public string TileLayer { set; get; }

        private IParticleManager pman;
        private bool pinEmitter = false;

        public void Draw(SpriteBatch b, Point center, int millis)
        {
            Vector2 off = new((int)(center.X * (1f - Depth) + .5f), (int)(center.Y * (1f - Depth) + .5f));
            Image?.Draw(b, center, millis, Depth);
            Game1.currentLocation?.map?.GetLayer(TileLayer)?.Draw(Game1.mapDisplayDevice, Game1.viewport, new((int)off.X, (int)off.Y), false, 4);
            if (pman is not null)
            {
                pman.Offset = off;
                if (pinEmitter)
                    pman.Emitter.Region = new(
                        Game1.viewport.X - (int)pman.Offset.X - 64, Game1.viewport.Y - (int)pman.Offset.Y - 64,
                        Game1.viewport.Width + 128, Game1.viewport.Height + 128);
                pman.Tick(millis);
                pman.Draw(b);
            }
        }
        public void Init()
        {
            Image?.Reload();
            if (ParticleEmitter is null || Particles is null)
                return;
            pinEmitter = pinEmitter || ParticleEmitter.Region != default;
            pman ??= Particles.Create(ParticleEmitter, ParticleCapacity);
            pman.Cleanup(); // refresh in case this is a cached asset
        }
        public void Cleanup()
        {
            pman.Cleanup();
        }
    }
}
