using System;
using Microsoft.Xna.Framework;
using Summing.Core;

namespace Summing.Camera;

public sealed class Camera2D
{
    public Vector2 Position { get; private set; }
    public Vector2 LookAhead { get; private set; }
    public Matrix View => Matrix.CreateTranslation(
        -MathF.Floor(Position.X - GameConstants.VirtualWidth * 0.5f),
        -MathF.Floor(Position.Y - GameConstants.VirtualHeight * 0.5f), 0f);

    public void Snap(Vector2 target)
    {
        Position = target;
        LookAhead = Vector2.Zero;
    }

    public void Update(Vector2 target, Vector2 velocity, float dt)
    {
        var desiredLook = new Vector2(Math.Clamp(velocity.X * 0.22f, -52f, 52f), Math.Clamp(velocity.Y * 0.08f, -20f, 26f));
        LookAhead = Vector2.Lerp(LookAhead, desiredLook, 1f - MathF.Exp(-5f * dt));
        Position = Vector2.Lerp(Position, target + LookAhead, 1f - MathF.Exp(-7.5f * dt));
    }

    public Vector2 WorldToScreen(Vector2 world) => world - Position + new Vector2(GameConstants.VirtualWidth, GameConstants.VirtualHeight) * 0.5f;
}
