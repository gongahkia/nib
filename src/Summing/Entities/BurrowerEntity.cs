using System;
using Microsoft.Xna.Framework;
using Summing.Core;
using Summing.Generation;
using Summing.Player;
using Summing.World;

namespace Summing.Entities;

public sealed class BurrowerEntity : IExplosionTarget
{
    private float _digTimer;
    private float _contactCooldown;
    private float _hurtFlash;
    private int _animationTick;

    public BurrowerEntity(WorldFeature source)
    {
        Position = source.Position + new Vector2(0f, -12f);
    }

    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }
    public bool Alive => Health > 0;
    public bool Alerted { get; private set; }
    public int Health { get; private set; } = 3;
    public int AnimationFrame => _animationTick / 7 % 4;
    public bool HurtFlash => _hurtFlash > 0f;
    public event Action<Point>? DugTile;
    public event Action? Attacked;

    public void Update(PlayerController player, TileWorld world, float speedMultiplier, int contactDamage, float dt)
    {
        if (!Alive) return;
        _animationTick++;
        _digTimer = MathF.Max(0f, _digTimer - dt);
        _contactCooldown = MathF.Max(0f, _contactCooldown - dt);
        _hurtFlash = MathF.Max(0f, _hurtFlash - dt);
        var toPlayer = player.Bounds.Center - Position;
        if (!Alerted && MathF.Abs(toPlayer.X) < 310f && MathF.Abs(toPlayer.Y) < 220f) Alerted = true;
        if (!Alerted) return;

        var intercept = player.Bounds.Center + player.Velocity * 0.28f;
        var direction = intercept - Position;
        if (direction.LengthSquared() > 1f) direction.Normalize();
        var speed = 64f * speedMultiplier;
        Velocity = Vector2.Lerp(Velocity, direction * speed, 1f - MathF.Exp(-4.5f * dt));
        var next = Position + Velocity * dt;
        var nextTile = world.WorldToTile(next);
        var terrain = world.GetTile(nextTile.X, nextTile.Y);
        if (terrain.Solid)
        {
            if (_digTimer <= 0f)
            {
                _digTimer = 0.18f / speedMultiplier;
                world.DamageTile(nextTile.X, nextTile.Y, 1, "burrower");
                DugTile?.Invoke(nextTile);
            }
            if (world.GetTile(nextTile.X, nextTile.Y).Solid)
            {
                Velocity *= 0.3f;
                return;
            }
        }
        Position = next;

        if (_contactCooldown <= 0f && Vector2.DistanceSquared(Position, player.Bounds.Center) < 25f * 25f)
        {
            var away = player.Bounds.Center - Position;
            if (away.LengthSquared() < 1f) away = Vector2.UnitY * -1f;
            away.Normalize();
            player.ApplyDamage(contactDamage, away * 215f + new Vector2(0f, -85f), "burrower");
            _contactCooldown = 1.15f;
            Attacked?.Invoke();
        }
    }

    public void ApplyExplosion(Explosion explosion)
    {
        if (!Alive) return;
        var distance = Vector2.Distance(Position, explosion.Position);
        if (distance > explosion.Radius + 14f) return;
        Health = Math.Max(0, Health - explosion.Damage);
        var away = Position - explosion.Position;
        if (away.LengthSquared() < 1f) away = -Vector2.UnitY;
        away.Normalize();
        Velocity += away * 245f;
        _hurtFlash = 0.18f;
    }
}
