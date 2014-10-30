using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PenisPotato.Graphics
{
    public class Grid
    {
        public GridEffect.PointMass[,] points;
        public GridEffect.Spring[] springs;

        public Vector2 screenSize;

        public Grid(Rectangle size, Vector2 spacing)
        {
            var springList = new List<GridEffect.Spring>();
            screenSize = new Vector2(size.Width, size.Height);

            int numColumns = (int)(size.Width / spacing.X) + 1;
            int numRows = (int)(size.Height / spacing.Y) + 1;
            points = new GridEffect.PointMass[numColumns, numRows];

            // these fixed points will be used to anchor the grid to fixed positions on the screen
            GridEffect.PointMass[,] fixedPoints = new GridEffect.PointMass[numColumns, numRows];

            // create the point masses
            int column = 0, row = 0;
            for (float y = size.Top; y <= size.Bottom; y += spacing.Y)
            {
                for (float x = size.Left; x <= size.Right; x += spacing.X)
                {
                    points[column, row] = new GridEffect.PointMass(new Vector3(x, y, 0), 1);
                    fixedPoints[column, row] = new GridEffect.PointMass(new Vector3(x, y, 0), 0);
                    column++;
                }
                row++;
                column = 0;
            }

            // link the point masses with springs
            for (int y = 0; y < numRows; y++)
                for (int x = 0; x < numColumns; x++)
                {
                    if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1)    // anchor the border of the grid 
                        springList.Add(new GridEffect.Spring(fixedPoints[x, y], points[x, y], 0.1f, 0.1f));
                    else if (x % 3 == 0 && y % 3 == 0)                                  // loosely anchor 1/9th of the point masses 
                        springList.Add(new GridEffect.Spring(fixedPoints[x, y], points[x, y], 0.002f, 0.02f));

                    const float stiffness = 0.28f;
                    const float damping = 0.06f;
                    if (x > 0)
                        springList.Add(new GridEffect.Spring(points[x - 1, y], points[x, y], stiffness, damping));
                    if (y > 0)
                        springList.Add(new GridEffect.Spring(points[x, y - 1], points[x, y], stiffness, damping));
                }

            springs = springList.ToArray();
        }

        public void Update()
        {
            foreach (var spring in springs)
                spring.Update();

            foreach (var mass in points)
                mass.Update();
        }

        public void ApplyDirectedForce(Vector3 force, Vector3 position, float radius)
        {
            foreach (var mass in points)
                if (Vector3.DistanceSquared(position, mass.Position) < radius * radius)
                    mass.ApplyForce(10 * force / (10 + Vector3.Distance(position, mass.Position)));
        }

        public void ApplyImplosiveForce(float force, Vector3 position, float radius)
        {
            foreach (var mass in points)
            {
                float dist2 = Vector3.DistanceSquared(position, mass.Position);
                if (dist2 < radius * radius)
                {
                    mass.ApplyForce(10 * force * (position - mass.Position) / (100 + dist2));
                    mass.IncreaseDamping(0.6f);
                }
            }
        }

        public void ApplyExplosiveForce(float force, Vector3 position, float radius)
        {
            foreach (var mass in points)
            {
                float dist2 = Vector3.DistanceSquared(position, mass.Position);
                if (dist2 < radius * radius)
                {
                    mass.ApplyForce(100 * force * (mass.Position - position) / (10000 + dist2));
                    mass.IncreaseDamping(0.6f);
                }
            }
        }

        public Vector2 ToVec2(Vector3 v)
        {
            // do a perspective projection
            float factor = (v.Z + 2000) / 2000;
            return (new Vector2(v.X, v.Y) - screenSize / 2f) * factor + screenSize / 2;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int width = points.GetLength(0);
            int height = points.GetLength(1);
            Color color = new Color(30, 30, 139, 85);   // dark blue

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 left = new Vector2(), up = new Vector2(); Vector2 p = ToVec2(points[x, y].Position); if (x > 1)
                    {
                        left = ToVec2(points[x - 1, y].Position);
                        float thickness = y % 3 == 1 ? 3f : 1f;
                        spriteBatch.DrawLine(left, p, color, thickness);
                    }
                    if (y > 1)
                    {
                        up = ToVec2(points[x, y - 1].Position);
                        float thickness = x % 3 == 1 ? 3f : 1f;
                        spriteBatch.DrawLine(up, p, color, thickness);
                    }
                }
            }
        }
    }
}
