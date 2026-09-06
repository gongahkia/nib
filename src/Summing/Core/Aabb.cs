using Microsoft.Xna.Framework;

namespace Summing.Core;

public readonly record struct Aabb(float X, float Y, float Width, float Height)
{
    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;
    public Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);
    public Aabb Offset(float x, float y) => new(X + x, Y + y, Width, Height);
    public bool Intersects(Aabb other) => Left < other.Right && Right > other.Left && Top < other.Bottom && Bottom > other.Top;
}
