using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PenisPotato.Graphics.ParticleEffects
{
    public enum ParticleType { None, Enemy, Bullet, IgnoreGravity }

    public struct ParticleState
    {
        public Vector2 Velocity;
        public ParticleType Type;
        public float LengthMultiplier;

        public static void UpdateParticle(ParticleManager<ParticleState>.Particle particle)
        {
            var vel = particle.State.Velocity;

            particle.Position += vel;
            particle.Orientation = vel.ToAngle();

            //var pos = particle.Position;
            //int left = (int)particle.OriginalPos.X - 128;
            //int right = (int)particle.OriginalPos.X + 128;
            //int top = (int)particle.OriginalPos.Y - 128;
            //int bottom = (int)particle.OriginalPos.Y + 128;

            //// collide with the edges of the screen
            //if (pos.X < left) vel.X = Math.Abs(vel.X);
            //else if (pos.X > right)
            //    vel.X = -Math.Abs(vel.X);
            //if (pos.Y < top) vel.Y = Math.Abs(vel.Y);
            //else if (pos.Y > bottom)
            //    vel.Y = -Math.Abs(vel.Y);

            float speed = vel.Length();
            float alpha = Math.Min(1, Math.Min(particle.PercentLife * 2, speed * 1f));
            alpha *= alpha;

            particle.Color.A = (byte)(255 * alpha);

            particle.Scale.X = particle.State.LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * speed + 0.1f), alpha);

            // denormalized floats cause significant performance issues
            if (Math.Abs(vel.X) + Math.Abs(vel.Y) < 0.00000000001f)
                vel = Vector2.Zero;

            vel *= 0.97f;       // particles gradually slow down
            particle.State.Velocity = vel;
        }
    }
}
