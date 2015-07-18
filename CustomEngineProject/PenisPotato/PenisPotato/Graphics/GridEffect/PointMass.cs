using System;
using Microsoft.Xna.Framework;

namespace PenisPotato.Graphics.GridEffect
{
    public class PointMass
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float InverseMass;

        private Vector3 acceleration;
        private float damping = 0.98f;

        public PointMass(Vector3 position, float invMass)
        {
            Position = position;
            InverseMass = invMass;
        }

        public void ApplyForce(Vector3 force)
        {
            acceleration += force * InverseMass;
        }

        public void IncreaseDamping(float factor)
        {
            damping *= factor;
        }

        public void Update()
        {
            Velocity += acceleration;
            Position += Velocity;
            acceleration = Vector3.Zero;
            if (Velocity.LengthSquared() < 0.001f * 0.001f)
                Velocity = Vector3.Zero;

            Velocity *= damping;
            damping = 0.98f;
        }
    }

    public struct Spring
    {
        public PointMass End1;
        public PointMass End2;
        public float TargetLength;
        public float Stiffness;
        public float Damping;

        public Spring(PointMass end1, PointMass end2, float stiffness, float damping)
        {
            End1 = end1;
            End2 = end2;
            Stiffness = stiffness;
            Damping = damping;
            TargetLength = Vector3.Distance(end1.Position, end2.Position) * 0.95f;
        }

        public void Update()
        {
            var x = End1.Position - End2.Position;

            float length = x.Length();
            // these springs can only pull, not push
            if (length <= TargetLength)
                return;

            x = (x / length) * (length - TargetLength);
            var dv = End2.Velocity - End1.Velocity;
            var force = Stiffness * x - dv * Damping;

            End1.ApplyForce(-force);
            End2.ApplyForce(force);
        }
    }
}
