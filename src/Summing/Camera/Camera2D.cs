using System;
using Microsoft.Xna.Framework;
using Summing.Core;

namespace Summing.Camera;

public sealed class Camera2D
{
    private float _shakeStrength;
    private float _shakeClock;

    public Vector2 Position { get; private set; }
    public Vector2 LookAhead { get; private set; }
    public Vector2 ShakeOffset { get; private set; }
    public float ShakeStrength => _shakeStrength;
    public Matrix View => Matrix.CreateTranslation(
        -MathF.Floor(Position.X + ShakeOffset.X - GameConstants.VirtualWidth * 0.5f),
        -MathF.Floor(Position.Y + ShakeOffset.Y - GameConstants.VirtualHeight * 0.5f), 0f);

    public void Snap(Vector2 target)
    {
        Position = target;
        LookAhead = Vector2.Zero;
        ShakeOffset = Vector2.Zero;
        _shakeStrength = 0f;
    }

    public void Update(Vector2 target, Vector2 velocity, float dt)
    {
        var desiredLook = new Vector2(Math.Clamp(velocity.X * 0.22f, -52f, 52f), Math.Clamp(velocity.Y * 0.08f, -20f, 26f));
        LookAhead = Vector2.Lerp(LookAhead, desiredLook, 1f - MathF.Exp(-5f * dt));
        Position = Vector2.Lerp(Position, target + LookAhead, 1f - MathF.Exp(-7.5f * dt));
        _shakeClock += dt;
        _shakeStrength = MathF.Max(0f, _shakeStrength - 27f * dt);
        var falloff = _shakeStrength * _shakeStrength / 14f;
        ShakeOffset = new Vector2(MathF.Sin(_shakeClock * 107f), MathF.Sin(_shakeClock * 149f + 1.7f)) * falloff;
    }

    public void AddShake(float strength) => _shakeStrength = MathF.Max(_shakeStrength, Math.Clamp(strength, 0f, 14f));

    public void Pan(Vector2 delta) => Position += delta;

    public Vector2 WorldToScreen(Vector2 world) => world - Position - ShakeOffset + new Vector2(GameConstants.VirtualWidth, GameConstants.VirtualHeight) * 0.5f;
    public Vector2 ScreenToWorld(Vector2 screen) => screen + Position + ShakeOffset - new Vector2(GameConstants.VirtualWidth, GameConstants.VirtualHeight) * 0.5f;
}
