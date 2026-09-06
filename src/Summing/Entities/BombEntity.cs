using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Summing.Core;
using Summing.World;

namespace Summing.Entities;

public sealed class BombEntity
{
    private const float Radius = 6f;
    public BombEntity(Vector2 position, Vector2 velocity)
    {
        Position = position;
        Velocity = velocity;
    }

    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }
    public float Fuse { get; private set; } = 2.15f;
    public bool Alive { get; private set; } = true;

    public bool Update(TileWorld world, float dt)
    {
        if (!Alive) return false;
        Fuse -= dt;
        Velocity += new Vector2(0f, 760f * dt);
        Velocity = new Vector2(Velocity.X * (1f - 0.08f * dt), MathF.Min(360f, Velocity.Y));
        MoveAxis(world, Velocity.X * dt, true);
        MoveAxis(world, Velocity.Y * dt, false);
        if (Fuse > 0f) return false;
        Alive = false;
        return true;
    }

    public void Draw(SpriteBatch batch, Texture2D pixel)
    {
        var flash = Fuse < 0.55f && ((int)(Fuse * 20f) & 1) == 0;
        var color = flash ? new Color(237, 206, 142) : new Color(42, 43, 50);
        batch.Draw(pixel, new Rectangle((int)(Position.X - Radius), (int)(Position.Y - Radius), 12, 12), color);
        batch.Draw(pixel, new Rectangle((int)Position.X - 1, (int)Position.Y - 10, 3, 4), new Color(210, 92, 65));
    }

    private void MoveAxis(TileWorld world, float amount, bool horizontal)
    {
        var candidate = Position + (horizontal ? new Vector2(amount, 0f) : new Vector2(0f, amount));
        var bounds = new Aabb(candidate.X - Radius, candidate.Y - Radius, Radius * 2f, Radius * 2f);
        if (!world.OverlapsSolid(bounds))
        {
            Position = candidate;
            return;
        }
        if (horizontal) Velocity = new Vector2(-Velocity.X * 0.42f, Velocity.Y);
        else Velocity = new Vector2(Velocity.X * 0.74f, -Velocity.Y * 0.32f);
    }
}
