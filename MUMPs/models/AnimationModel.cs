using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
namespace MUMPs.models
{
    public class AnimationModel
    {
        public int HFrames { set; get; } = 1;
        public int VFrames { set; get; } = 1;
        public int Speed { set; get; } = 100;
        public List<int> Delays { set; get; }

        private int timeSinceLast = 0;
        private int frame = 0;
        public Rectangle GetSource(Rectangle region, int millis = 0)
        {
            if (millis > 0)
                Animate(millis);
            return new(new(frame % HFrames, frame / HFrames), new(region.Width / HFrames, region.Height / VFrames));
        }
        public void Animate(int millis)
        {
            timeSinceLast += millis;
            int time = (Delays != null && Delays.Count > frame && Delays[frame] > 0) ? Delays[frame] : Speed;
            if (timeSinceLast >= time)
            {
                timeSinceLast -= time;
                frame = (frame + 1) % (HFrames * VFrames); //wrap
            }
        }
    }
}
