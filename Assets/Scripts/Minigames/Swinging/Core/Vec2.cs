using System;

// We need this since the asmdef has no engine references.
namespace Minigames.Swinging.Core {
    public readonly struct Vec2 {
        public readonly float X;
        public readonly float Y;

        public Vec2(float x, float y) {
            X = x;
            Y = y;
        }
        
        public float Length => MathF.Sqrt(X * X + Y * Y);
        
        public float Dot(Vec2 other) => X * other.X + Y * other.Y;

        public Vec2 Normalized
        {
            get
            {
                float length = Length;
                return length > 0.0001f ? new Vec2(X / length, Y / length) : default;
            }
        }
        
        public static Vec2 operator +(Vec2 leftVector, Vec2 rightVector) => new(leftVector.X + rightVector.X, leftVector.Y + rightVector.Y);
        public static Vec2 operator -(Vec2 leftVector, Vec2 rightVector) => new(leftVector.X - rightVector.X, leftVector.Y - rightVector.Y);
        public static Vec2 operator *(Vec2 vector, float scalar) => new(vector.X * scalar, vector.Y * scalar);
        public static Vec2 operator *(float scalar, Vec2 vector) => new(vector.X * scalar, vector.Y * scalar);
    }
}