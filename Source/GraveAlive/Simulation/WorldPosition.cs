using System;

namespace GraveAlive.Simulation
{
    public struct WorldPosition
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public WorldPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float DistanceSquaredTo(WorldPosition other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            float dz = Z - other.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        public WorldPosition Offset(float x, float y, float z)
        {
            return new WorldPosition(X + x, Y + y, Z + z);
        }

        public override string ToString()
        {
            return string.Format("({0:0.0}, {1:0.0}, {2:0.0})", X, Y, Z);
        }
    }
}
